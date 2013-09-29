using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.IO;
using System.Threading;


namespace SkeletalTracking
{
    /// <summary>
    /// The BVHWriter class takes bone rotations and writes them to a BVH file. The data is written to 
    /// disk when the CloseAndWrite() method is called. Until then it is kept in the BVHFile class. If the file
    /// does not exist, a new file can be created but once again it is not written until CloseAndWrite() is 
    /// called making it the most important method to call in the class.
    /// </summary>
    class BVHWriter
    {
        private String p_Filename;
        private BVHFile p_BVHFile;
        private bool isOpen;

        /// <summary>
        /// Constructs a new BVHWriter initializing it as not being open.
        /// </summary>
        /// <param name="filename">The name of the bvh file minus the .bvh extension</param>
        public BVHWriter(String filename)
        {
            this.p_BVHFile = new BVHFile();
            this.p_Filename = filename+".bvh";
            this.isOpen = false;
        }

        #region Open/Close methods
        /// <summary>
        /// Sees if the current writer is open.
        /// </summary>
        /// <returns>True if open, false otherwise</returns>
        public bool IsOpen()
        {
            return isOpen;
        }

        /// <summary>
        /// Opens the BVHWriter if not already open. Otherwise it does nothing.
        /// </summary>
        public void Open()
        {
            if (!isOpen)
            {
                if (!BVHFileExists())
                    this.CreateNewFile();
                isOpen = true;
            }
        }

        /// <summary>
        /// Closes the BVHWriter and writes all appended motion frames to disk.
        /// </summary>
        public void CloseAndWrite()
        {
            if (isOpen)
            {
                //Writes to disk
                FileStream p_FileStream = File.Open(p_Filename, FileMode.Append, FileAccess.Write);
                Byte[] encodedBVH = Encoding.ASCII.GetBytes(p_BVHFile.ToString());
                p_FileStream.Write(encodedBVH, 0, encodedBVH.Length);
                p_FileStream.Close();
                
                //Sets the file to closed and erases all temp data in the BVHFile
                isOpen = false;
                p_BVHFile = new BVHFile();
            }
        }
        #endregion

        #region Getters/Setters
        /// <summary>
        /// Gets the name of the BVHFile
        /// </summary>
        /// <returns>The name of the BVHFile (with file extension)</returns>
        public String GetFileName()
        {
            return p_Filename;
        }

        /// <summary>
        /// Sets the name of the BVHFile
        /// </summary>
        /// <param name="filename">The name of the BVHFile minus the file extension</param>
        public void SetFileName(String filename)
        {
            this.p_Filename = filename + ".bvh";
        }
        #endregion

        #region Append Frame
        /// <summary>
        /// Appends a frame of motion data to the BVHFile
        /// 
        /// PRECONDITION: The frame must be parsed according to BVH file format.
        /// </summary>
        /// <param name="frame">The frame to be appended to the BVHFile</param>
        public void AppendFrame(String frame)
        {
            p_BVHFile.AppendFrame(frame);
        }
        #endregion

        #region File Existence Methods
        /// <summary>
        /// Creates a new BVH file.
        /// </summary>
        public void CreateNewFile()
        {
            StreamWriter s_writer = File.CreateText(p_Filename);
            s_writer.Close();
        }

        /// <summary>
        /// Sees if the BVHFile exists on disk
        /// </summary>
        /// <returns>True if the file exists, false otherwise</returns>
        public bool BVHFileExists()
        {
            return File.Exists(p_Filename);
        }
        #endregion
    }
}
