using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using OpenMetaverse;

namespace SkeletalTracking
{
    using System.Windows.Media;
    using Microsoft.Kinect;
    /// <summary>
    /// Interaction logic for MainWindow.xaml. The Main window for the SkeletalTracking program.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Field Variables
        /// <summary>
        /// The width of the rendering window
        /// </summary>
        public const int RenderWidth = 640;

        /// <summary>
        /// The height of the rendering window
        /// </summary>
        public const int RenderHeight = 480;

        /// <summary>
        /// The height of the xz and yz render windows
        /// </summary>
        public const int SideRenderHeight = 240;

        /// <summary>
        /// The width of the xz and yz render windows
        /// </summary>
        public const int SideRenderWidth = 320;

        /// <summary>
        /// The radius of the point drawn to represent skeletal position
        /// </summary>
        public const int SkeletonPositionPointRadius = 3;

        /// <summary>
        /// The context of the drawing panel. Can either be the main panel, the xz-panel, or the, yz-panel
        /// </summary>
        public enum context{normal, xz, yz};

        /// <summary>
        /// The index of the currently tracked bone in the drop down menu
        /// </summary>
        private int TrackedBoneIndex = -1;

        /// <summary>
        /// The drawing groups for the skeleton output window
        /// </summary>
        private DrawingGroup drawingGroup;
        private DrawingGroup drawingGroupXZ;
        private DrawingGroup drawingGroupYZ;

        /// <summary>
        /// The connected Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// The pen used for the program
        /// </summary>
        private Pen pen;

        /// <summary>
        /// The BoneTree used for computing rotation angles and storing them in a BVHFile
        /// </summary>
        private BoneTree boneTree;
        #endregion

        #region Initialization of Window
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            boneSelectionBox.ItemsSource = TrackableBones.boneNames;
            boneTree = new BoneTree();

            //set brush
            Brush brush = Brushes.Green;

            //Sets the drawing group for the panel
            this.drawingGroup = new DrawingGroup();
            this.image.Source = new DrawingImage(drawingGroup);

            this.drawingGroupXZ = new DrawingGroup();
            this.imageXZ.Source = new DrawingImage(drawingGroupXZ);

            this.drawingGroupYZ = new DrawingGroup();
            this.imageYZ.Source = new DrawingImage(drawingGroupYZ);

            //TODO: Fix this and make it a popup window to choose a connection
            foreach (KinectSensor p_sensor in KinectSensor.KinectSensors)
            {
                if (p_sensor.Status == KinectStatus.Connected)
                {
                    this.sensor = p_sensor;
                    break;
                }
            }

            if (this.sensor != null)
            {
                //Enable Skeleton Streaming
                this.sensor.SkeletonStream.Enable();

                //Add event handler to Skeleton Frame
                this.sensor.SkeletonFrameReady += this.SkeletonFrameReadyHandler;

                try
                {
                    //Start the sensor
                    this.sensor.Start();
                }
                catch (System.IO.IOException)
                {
                    //Catches disconnection
                    this.sensor = null;
                }
            }
        }
        #endregion

        #region Drawing/Writing Events and Methods
        public void SkeletonFrameReadyHandler(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skelArr = null;
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skelArr = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skelArr);
                }
            }

            using (DrawingContext dc = this.drawingGroup.Open())
            {
                drawDrawingGroups(dc, skelArr, RenderWidth, RenderHeight, context.normal);
            }
            using (DrawingContext dc0 = this.drawingGroupXZ.Open())
            {
                drawDrawingGroups(dc0, skelArr, SideRenderWidth, SideRenderHeight, context.xz);
            }
            using (DrawingContext dc1 = this.drawingGroupYZ.Open())
            {
                drawDrawingGroups(dc1, skelArr, SideRenderWidth, SideRenderHeight, context.yz);
            }
        }

        private void drawDrawingGroups(DrawingContext dc, Skeleton[] skelArr, int r_width, int r_height, 
            context con)
        {
            dc.DrawRectangle(
                    Brushes.Black,
                    null,
                    new Rect(0.0, 0.0, r_width, r_height)
                    );

            if (skelArr != null)
            {
                foreach (Skeleton skelFrame in skelArr)
                {
                    this.drawSkeletonFrame(skelFrame, dc, con);
                }
            }

            // prevents drawing outside of the render area
            if(con == context.normal)
                this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, r_width, r_height));
            else if(con == context.xz)
                this.drawingGroupXZ.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, r_width, r_height));
            else if(con == context.yz)
                this.drawingGroupYZ.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, r_width, r_height));
        }

        private void drawSkeletonFrame(Skeleton skeletalFrame, DrawingContext dc, context con)
        {
            if (skeletalFrame.TrackingState == SkeletonTrackingState.Tracked)
            {
                //Update rotation data using current skeleton frame and streams it if streaming is enabled
                this.boneTree.SendUpdatedLocations(skeletalFrame);
                //Writes the rotation angles to the screen for selected body part
                this.WriteEulerAnglesToScreen();

                //Draw Bones to the screen
                this.drawBones(skeletalFrame, dc, con);
                foreach (Joint joint in skeletalFrame.Joints)
                {
                    if (joint.TrackingState == JointTrackingState.Tracked || joint.TrackingState == JointTrackingState.Inferred)
                    {
                        dc.DrawEllipse(Brushes.White, null, this.mapPointToDepth(joint.Position, con),
                            SkeletonPositionPointRadius, SkeletonPositionPointRadius);
                    }
                }
            }
            if (skeletalFrame.TrackingState == SkeletonTrackingState.PositionOnly)
            {
                dc.DrawEllipse(Brushes.White, null, mapPointToDepth(skeletalFrame.Position, con),
                    SkeletonPositionPointRadius, SkeletonPositionPointRadius);
            }
        }

        private void drawBones(Skeleton skeleton, DrawingContext dc, context con)
        {
            //Draw feet
            drawBone(skeleton.Joints[JointType.FootLeft], skeleton.Joints[JointType.AnkleLeft], dc, con);
            drawBone(skeleton.Joints[JointType.FootRight], skeleton.Joints[JointType.AnkleRight], dc, con);
            
            //Draw legs
            drawBone(skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.KneeLeft], dc, con);
            drawBone(skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.KneeRight], dc, con);
            drawBone(skeleton.Joints[JointType.KneeLeft], skeleton.Joints[JointType.HipLeft], dc, con);
            drawBone(skeleton.Joints[JointType.KneeRight], skeleton.Joints[JointType.HipRight], dc, con);
            drawBone(skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.HipCenter], dc, con);
            drawBone(skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.HipCenter], dc, con);

            //Draw Torso
            drawBone(skeleton.Joints[JointType.HipCenter], skeleton.Joints[JointType.Spine], dc, con);
            drawBone(skeleton.Joints[JointType.Spine], skeleton.Joints[JointType.ShoulderCenter], dc, con);
            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderLeft], dc, con);
            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.ShoulderRight], dc, con);

            //Draw Arms
            drawBone(skeleton.Joints[JointType.ShoulderLeft], skeleton.Joints[JointType.ElbowLeft], dc, con);
            drawBone(skeleton.Joints[JointType.ShoulderRight], skeleton.Joints[JointType.ElbowRight], dc, con);
            drawBone(skeleton.Joints[JointType.ElbowLeft], skeleton.Joints[JointType.WristLeft], dc, con);
            drawBone(skeleton.Joints[JointType.ElbowRight], skeleton.Joints[JointType.WristRight], dc, con);
            drawBone(skeleton.Joints[JointType.WristLeft], skeleton.Joints[JointType.HandLeft], dc, con);
            drawBone(skeleton.Joints[JointType.WristRight], skeleton.Joints[JointType.HandRight], dc, con);

            //Draw Head
            drawBone(skeleton.Joints[JointType.ShoulderCenter], skeleton.Joints[JointType.Head], dc, con);
        }

        private void drawBone(Joint j0, Joint j1, DrawingContext dc, context con)
        {
            pen = new Pen(Brushes.Green, 2.0);
            if(j0.TrackingState == JointTrackingState.Tracked && j1.TrackingState == JointTrackingState.Tracked)
                dc.DrawLine(pen, this.mapPointToDepth(j0.Position, con), this.mapPointToDepth(j1.Position, con));
        }

        private Point mapPointToDepth(SkeletonPoint skeletonPosition, context con)
        {
            DepthImagePoint depthPoint;
            Point p = new Point(0.0, 0.0);
            if (con == context.normal)
            {
                depthPoint = this.sensor.MapSkeletonPointToDepth(skeletonPosition,
                    DepthImageFormat.Resolution640x480Fps30);
                p = new Point(depthPoint.X, depthPoint.Y);
            }
            else if (con == context.xz)
            {
                float parsedX = SideRenderHeight - (skeletonPosition.X*100 + SideRenderHeight / 2);
                float parsedZ = skeletonPosition.Z * 100 - SideRenderWidth / 3;
                p = new Point(parsedZ, parsedX);
            }
            else if (con == context.yz)
            {
                float parsedY = SideRenderHeight - (skeletonPosition.Y*100 + SideRenderHeight / 2);
                float parsedZ = skeletonPosition.Z * 100 - SideRenderWidth / 3;
                p = new Point(parsedZ, parsedY);
            }

            return p;
        }

        private void WriteEulerAnglesToScreen()
        {
            if (TrackedBoneIndex == -1)
                return;
            xLabel.Content = boneTree.FindXRot(TrackableBones.boneNames[TrackedBoneIndex]) + "";
            yLabel.Content = boneTree.FindYRot(TrackableBones.boneNames[TrackedBoneIndex]) + "";
            zLabel.Content = boneTree.FindZRot(TrackableBones.boneNames[TrackedBoneIndex]) + "";
        }
        #endregion

        #region Event Handlers for the UI
        private void checkBoxSeated_Checked(object sender, RoutedEventArgs e)
        {
            if (this.checkBoxSeated.IsChecked.Value)
            {
                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            }
            else
            {
                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if(boneTree.isWritingToBVHFile())
                boneTree.FinishWritingToBVH();
        }

        private void recordButton_Click(object sender, RoutedEventArgs e)
        {
            if(!boneTree.isWritingToBVHFile())
                boneTree.StartWritingToBVH();
        }

        private void boneSelectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrackedBoneIndex = (boneSelectionBox.SelectedIndex);
        }

        private void btnVidStreaming_Click(object sender, RoutedEventArgs e)
        {
            if (boneTree.bvhStreamer.IsStreaming())
            {
                boneTree.bvhStreamer.StopStreaming();
                btnVidStreaming.Content = "Start";
            }
            else
            {
                //if (!this.boneTree.bvhStreamer.IsLoggedIn())
                //{
                //    this.boneTree.bvhStreamer.ShowLoginWindow();
                //}
                if(this.boneTree.bvhStreamer.IsLoggedIn())
                {
                    boneTree.bvhStreamer.StartStreaming();
                    btnVidStreaming.Content = "Stop";
                }
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (this.boneTree.bvhStreamer.IsLoggedIn())
            {
                this.boneTree.bvhStreamer.Logout();
                this.LoginLogoutItem.Header = "_Login";
            }
            else
            {
                this.boneTree.bvhStreamer.ShowLoginWindow();
                this.LoginLogoutItem.Header = "_Logout";
            }
        }
        #endregion

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
