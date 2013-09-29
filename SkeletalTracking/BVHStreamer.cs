using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Assets;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace SkeletalTracking
{
    public class BVHStreamer
    {
        public const String FILENAME = "streaming";
        public const int FILE_BUFFER_SIZE = 500; //try changing

        private static int s_FileNameCount = 0;

        private GridClient p_Client;
        private bool p_Streaming;
        private BVHFile p_BVHFile;
        private BVHPlayAnimationControl p_AnimationControl;
        private bool p_FirstUpload;

        public BVHStreamer()
        {
            this.p_FirstUpload = true;
            this.p_Client = new GridClient();
            this.p_Streaming = false;
            this.p_AnimationControl = new BVHPlayAnimationControl((int)(FILE_BUFFER_SIZE * BVHFile.DEFAULT_FRAME_RATE * 1000), ref p_Client);
        }

        #region Streaming Methods
        public bool IsStreaming()
        {
            return this.p_Streaming;
        }

        public void StartStreaming()
        {
            if (!this.IsLoggedIn())
                this.ShowLoginWindow();
            if (this.IsLoggedIn())
            {
                this.p_Streaming = true;
                this.p_BVHFile = new BVHFile(FILENAME + s_FileNameCount);
                s_FileNameCount++;
            }
        }

        public void StopStreaming()
        {
            //Stop streaming if currently streaming
            if (this.p_Streaming)
            {
                this.p_Streaming = false;
                p_AnimationControl.StopPlaying();
                //Wait on play animation to finish up before proceeding.
                WaitHandle.WaitAll(new ManualResetEvent[] { p_AnimationControl.ResetEvent }, 10000);
            }
        }

        public void SendFrame(String frame)
        {
            this.p_BVHFile.AppendFrame(frame);
            if (this.p_BVHFile.GetNumOfFrames() >= BVHStreamer.FILE_BUFFER_SIZE)
            {
                this.SendFileToServer(p_BVHFile);
            }
        }

        public void SendFileToServer(BVHFile bvhFile)
        {
            //The current UUID is now the previous UUID
            //p_PreviousUploadUUID = p_CurrentUploadUUID;
            UUID p_CurrentUploadUUID = UUID.Random();

            //Testing, write to file for analysis
            //File.WriteAllText(p_BVHFile.FileName, p_BVHFile.ToString());

            //Serialize the animation data
            BVHAnimationEncoder encoder = new BVHAnimationEncoder(p_BVHFile.ToDefaultAnimationMetaData());

            //Testing...Write bin to file for analysis
            //if (File.Exists("streaming1.bin")) { File.Delete("streaming.bin"); }
            //File.WriteAllBytes("streaming1.bin", encoder.EncodedData);

            //Turn it into a new Animation Asset
            AssetAnimation animation = new AssetAnimation(p_CurrentUploadUUID, encoder.EncodedData);
            animation.Temporary = true;

            //Upload the encoded animation
            p_CurrentUploadUUID = this.p_Client.Assets.RequestUpload(animation, false);

            //Upload the animation file to user's inventory
            ThreadPool.QueueUserWorkItem((threadContext) => this.p_Client.Inventory.RequestCreateItem(
                    this.p_Client.Inventory.FindFolderForType(AssetType.Animation),
                    bvhFile.FileName, bvhFile.FileName, AssetType.Animation,
                    p_CurrentUploadUUID, InventoryType.Animation, PermissionMask.All,
                    SuccessfulUploadCallback)
                );

            //Reset the file info and update the file name
            p_BVHFile = new BVHFile(BVHStreamer.FILENAME + s_FileNameCount);
            s_FileNameCount++;

            //this.StopStreaming();
        }

        public void SuccessfulUploadCallback(bool success, InventoryItem item)
        {
            if (success)
            {
                Console.WriteLine("Inventory Asset UID: " + item.AssetUUID);
                p_AnimationControl.EnqueueAnimation(item.AssetUUID);
                Object threadLock = new Object();
                lock (threadLock)
                {
                    if (p_FirstUpload) 
                    { 
                        p_AnimationControl.StartPlaying();
                        p_FirstUpload = false;
                    }
                }
            }
            Thread.Yield();
        }

        #endregion
        #region Login/Logout Methods for Open Sim
        public bool IsLoggedIn()
        {
            return this.p_Client.Network.Connected;
        }

        public void ShowLoginWindow()
        {
            if (!this.IsLoggedIn())
            {
                LoginForm loginWindow = new LoginForm(this.p_Client);
                loginWindow.Show();
            }
        }

        public void Logout()
        {
            if (this.IsLoggedIn())
            {
                this.p_Client.Network.Logout();
            }
        }
        #endregion
    }

    /// <summary>
    /// Controls the playing of the animations once they are streamed to the server.
    /// </summary>
    public class BVHPlayAnimationControl
    {
        private const int TIMEOUT = 2000;

        private LinkedList<UUID> p_PlayBuffer;
        //private int p_AnimsInBuffer;
        private bool p_Playing;
        private GridClient p_Client;
        private ManualResetEvent p_ResetEvent;

        /// <summary>
        /// The reset event that specifies the thread has safely stopped.
        /// </summary>
        public ManualResetEvent ResetEvent
        {
            get
            {
                return p_ResetEvent;
            }
        }

        /// <summary>
        /// The length of the animations in milliseconds.
        /// </summary>
        public int AnimationLength;
        public bool Playing
        {
            get
            {
                return p_Playing;
            }
        }

        /// <summary>
        /// Constructs a new instance of BVHPlayAnimationControl and initializes variables with default values.
        /// </summary>
        /// <param name="animationLength">Length of the animation files in milliseconds.</param>
        /// <param name="client">The GridClient used by the BVHStreamer.</param>
        public BVHPlayAnimationControl(int animationLength, ref GridClient client)
        {
            this.p_ResetEvent = new ManualResetEvent(false);
            this.p_PlayBuffer = new LinkedList<UUID>();
            //this.p_AnimsInBuffer = 0;
            this.p_Playing = false;
            this.AnimationLength = animationLength;
            this.p_Client = client;
        }

        /// <summary>
        /// Adds an animation to the playing queue.
        /// </summary>
        /// <param name="assetUID">The uuid of the animation to be added to the queue</param>
        public void EnqueueAnimation(UUID assetUID)
        {
            p_PlayBuffer.AddLast(assetUID);
            //p_AnimsInBuffer++;
        }

        /// <summary>
        /// Starts playing the queued animations one at a time on a seperate thread.
        /// </summary>
        public void StartPlaying()
        {
            p_Playing = true;
            new Thread(startPlaying).Start();
        }

        /// <summary>
        /// Starts playing the queued animations one at a time.
        /// </summary>
        private void startPlaying()
        {
            UUID currentlyPlaying = UUID.Zero;

            while (Playing)
            {
                if (p_PlayBuffer.Count <= 0)
                {
                    Thread.Sleep(TIMEOUT);
                }
                else
                {
                    //Gets a new UUID
                    // uploadedUUID = new UUID("a0e08d1b-1f4b-5bae-8942-25ea578c80f6");
                    object threadlock = new object();
                    int sleepTime = 0;
                    Stopwatch sw = Stopwatch.StartNew();

                    lock (threadlock)
                    {
                        UUID uploadedUUID = p_PlayBuffer.First.Value;

                        //Start playing the animation
                        this.p_Client.Self.AnimationStart(uploadedUUID, true);

                        //Only stop and remove animations if queued
                        if (!currentlyPlaying.Equals(UUID.Zero))
                        {
                            this.p_Client.Self.AnimationStop(currentlyPlaying, true);
                            ThreadPool.QueueUserWorkItem((ThreadContext) => this.p_Client.Inventory.RemoveItem(currentlyPlaying));
                        }
                        p_PlayBuffer.RemoveFirst();

                        currentlyPlaying = uploadedUUID;
                    }
                    //Let other threads run
                    Thread.Yield();

                    sw.Stop();
                    sleepTime = AnimationLength - (int)sw.ElapsedMilliseconds;
                    if (sleepTime > 0) { Thread.Sleep(sleepTime); }
                }    
            }

            cleanUp(currentlyPlaying);
        }

        /// <summary>
        /// Stops the currently playing animation and removes all streamed assets.
        /// </summary>
        /// <param name="currentlyPlaying">The UUID of the currently playing animation</param>
        private void cleanUp(UUID currentlyPlaying)
        {
            //Cleanup
            this.p_Client.Self.AnimationStop(currentlyPlaying, true);
            while (p_PlayBuffer.Count > 0)
            {
                this.p_Client.Inventory.RemoveItem(p_PlayBuffer.First.Value);
                this.p_PlayBuffer.RemoveFirst();
            }
            p_ResetEvent.Set();
        }

        /// <summary>
        /// Schedules the thread to stop playing on next its next iteration
        /// </summary>
        public void StopPlaying()
        {
            p_Playing = false;
        }
    }
}
