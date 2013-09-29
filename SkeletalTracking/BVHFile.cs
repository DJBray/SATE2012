using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;

namespace SkeletalTracking
{
    public class BVHFile
    {
        public const float DEFAULT_FRAME_RATE = 0.012f;
        #region DEFAULT_FRAME declaration
        private const String DEFAULT_FRAME = "0.000000 40.00000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 0.000000 \n";
        #endregion

        public readonly String MOTION;
        public String FileName;
        public Single FrameRate;

        private BVHBoneTree boneTree;
        private int numOfFrames;
        private LinkedList<String> frames;

        //private const String p_NUM_FRAMES_CONST = "&numframes&";

        /// <summary>
        /// Constructs a new BVHFile with the default defined heiarchy and motion, initialized with a starting frame.
        /// </summary>
        public BVHFile()
        {
            this.numOfFrames = 0;
            this.frames = new LinkedList<String>();

            this.SetToDefaultMapping();
        }

        /// <summary>
        /// Initializes a default BVHFile and gives it the provided filename
        /// </summary>
        /// <param name="filename">The filename of this BVHFile</param>
        public BVHFile(String filename) : this()
        {
            this.FileName = filename;
        }

        #region Default Mapping
        /// <summary>
        /// Sets the structure of the BVHBoneTree to the default BVH File Structure.
        /// It is highly disadvised to use another file structure.
        /// </summary>
        public void SetToDefaultMapping()
        {
            //Construct the default bone structure.
            boneTree = new BVHBoneTree();

            //Bones
            BVHBoneTree.BVHBoneNode hip = new BVHBoneTree.BVHBoneNode("hip");
            BVHBoneTree.BVHBoneNode abdomen = new BVHBoneTree.BVHBoneNode("abdomen");
            BVHBoneTree.BVHBoneNode chest = new BVHBoneTree.BVHBoneNode("chest");
            BVHBoneTree.BVHBoneNode neck = new BVHBoneTree.BVHBoneNode("neck");
            BVHBoneTree.BVHBoneNode head = new BVHBoneTree.BVHBoneNode("head");
            BVHBoneTree.BVHBoneNode lCollar = new BVHBoneTree.BVHBoneNode("lCollar");
            BVHBoneTree.BVHBoneNode lShldr = new BVHBoneTree.BVHBoneNode("lShldr");
            BVHBoneTree.BVHBoneNode lForeArm = new BVHBoneTree.BVHBoneNode("lForeArm");
            BVHBoneTree.BVHBoneNode lHand = new BVHBoneTree.BVHBoneNode("lHand");
            BVHBoneTree.BVHBoneNode rCollar = new BVHBoneTree.BVHBoneNode("rCollar");
            BVHBoneTree.BVHBoneNode rShldr = new BVHBoneTree.BVHBoneNode("rShldr");
            BVHBoneTree.BVHBoneNode rForeArm = new BVHBoneTree.BVHBoneNode("rForeArm");
            BVHBoneTree.BVHBoneNode rHand = new BVHBoneTree.BVHBoneNode("rHand");
            BVHBoneTree.BVHBoneNode lThigh = new BVHBoneTree.BVHBoneNode("lThigh");
            BVHBoneTree.BVHBoneNode lShin = new BVHBoneTree.BVHBoneNode("lShin");
            BVHBoneTree.BVHBoneNode lFoot = new BVHBoneTree.BVHBoneNode("lFoot");
            BVHBoneTree.BVHBoneNode rThigh = new BVHBoneTree.BVHBoneNode("rThigh");
            BVHBoneTree.BVHBoneNode rShin = new BVHBoneTree.BVHBoneNode("rShin");
            BVHBoneTree.BVHBoneNode rFoot = new BVHBoneTree.BVHBoneNode("rFoot");
           
            //Endsites
            BVHBoneTree.BVHBoneNode headEndsite = new BVHBoneTree.BVHBoneNode();
            BVHBoneTree.BVHBoneNode lHandEndsite = new BVHBoneTree.BVHBoneNode();
            BVHBoneTree.BVHBoneNode rHandEndsite = new BVHBoneTree.BVHBoneNode();
            BVHBoneTree.BVHBoneNode lFootEndsite = new BVHBoneTree.BVHBoneNode();
            BVHBoneTree.BVHBoneNode rFootEndsite = new BVHBoneTree.BVHBoneNode();

            //Declare types
            hip.Type = NodeType.Root;
            abdomen.Type = NodeType.XZY;
            chest.Type = NodeType.XZY;
            neck.Type = NodeType.XZY;
            head.Type = NodeType.XZY;
            lCollar.Type = NodeType.YZX;
            lShldr.Type = NodeType.ZYX;
            lForeArm.Type = NodeType.YZX;
            lHand.Type = NodeType.ZYX;
            rCollar.Type = NodeType.YZX;
            rShldr.Type = NodeType.ZYX;
            rForeArm.Type = NodeType.YZX;
            rHand.Type = NodeType.ZYX;
            lThigh.Type = NodeType.XZY;
            lShin.Type = NodeType.XZY;
            lFoot.Type = NodeType.XYZ;
            rThigh.Type = NodeType.XZY;
            rShin.Type = NodeType.XZY;
            rFoot.Type = NodeType.XYZ;

            //Declare end types
            headEndsite.Type = NodeType.EndSite;
            lHandEndsite.Type = NodeType.EndSite;
            rHandEndsite.Type = NodeType.EndSite;
            lFootEndsite.Type = NodeType.EndSite;
            rFootEndsite.Type = NodeType.EndSite;

            //Set Offsets
            hip.Offset = new Vector3(0.000000, 0.000000, 0.000000);
            abdomen.Offset = new Vector3(0.000000, 3.422050, 0.000000);
            chest.Offset = new Vector3(0.000000, 8.486693, -0.6844110);
            neck.Offset = new Vector3(0.000000, 10.26616, -0.2737640);
            head.Offset = new Vector3(0.000000, 3.148285, 0.000000);
            lCollar.Offset = new Vector3(3.422053, 6.707223, -0.8212930);
            lShldr.Offset = new Vector3(3.285171, 0.000000, 0.000000);
            lForeArm.Offset = new Vector3(10.12928, 0.000000, 0.000000);
            lHand.Offset = new Vector3(8.486692, 0.000000, 0.000000);
            rCollar.Offset = new Vector3(-3.558935, 6.707223, -0.8212930);
            rShldr.Offset = new Vector3(-3.148289, 0.000000, 0.000000);
            rForeArm.Offset = new Vector3(-10.26616, 0.000000, 0.000000);
            rHand.Offset = new Vector3(-8.349810, 0.000000, 0.000000);
            lThigh.Offset = new Vector3(5.338403, -1.642589, 1.368821);
            lShin.Offset = new Vector3(-2.053232, -20.12167, 0.000000);
            lFoot.Offset = new Vector3(0.000000, -19.30038, -1.231939);
            rThigh.Offset = new Vector3(-5.338403, -1.642589, 1.368821);
            rShin.Offset = new Vector3(2.053232, -20.12167, 0.000000);
            rFoot.Offset = new Vector3(0.000000, -19.30038, -1.231939);
            headEndsite.Offset = new Vector3(0.000000, 3.148289, 0.000000);
            lHandEndsite.Offset = new Vector3(4.106464, 0.000000, 0.000000);
            rHandEndsite.Offset = new Vector3(-4.106464, 0.000000, 0.000000);
            lFootEndsite.Offset = new Vector3(0.000000, -2.463878, 4.653993);
            rFootEndsite.Offset = new Vector3(0.000000, -2.463878, 4.653993);

            //The hip is the root
            boneTree.Root = hip;

            //Connect upper body
             boneTree.AddNode(abdomen, hip.BoneName);
              boneTree.AddNode(chest, abdomen.BoneName);
               boneTree.AddNode(neck, chest.BoneName);
                boneTree.AddNode(head, neck.BoneName);
                 boneTree.AddNode(headEndsite, head.BoneName);
            //Connect left arm
               boneTree.AddNode(lCollar, chest.BoneName);
                boneTree.AddNode(lShldr, lCollar.BoneName);
                 boneTree.AddNode(lForeArm, lShldr.BoneName);
                  boneTree.AddNode(lHand, lForeArm.BoneName);
                   boneTree.AddNode(lHandEndsite, lHand.BoneName);
            //Connect right arm
               boneTree.AddNode(rCollar, chest.BoneName);
                boneTree.AddNode(rShldr, rCollar.BoneName);
                 boneTree.AddNode(rForeArm, rShldr.BoneName);
                  boneTree.AddNode(rHand, rForeArm.BoneName);
                   boneTree.AddNode(rHandEndsite, rHand.BoneName);
            //Connect left leg
             boneTree.AddNode(lThigh, hip.BoneName);
              boneTree.AddNode(lShin, lThigh.BoneName);
               boneTree.AddNode(lFoot, lShin.BoneName);
                boneTree.AddNode(lFootEndsite, lFoot.BoneName);
            //Connect right leg
             boneTree.AddNode(rThigh, hip.BoneName);
              boneTree.AddNode(rShin, rThigh.BoneName);
               boneTree.AddNode(rFoot, rShin.BoneName);
                boneTree.AddNode(rFootEndsite, rFoot.BoneName);
            

            //Automatically starts with 1 starting frame
            this.numOfFrames++;
            this.frames.AddFirst(DEFAULT_FRAME);

            //Default frame rate
            this.FrameRate = BVHFile.DEFAULT_FRAME_RATE;
        }
        #endregion

        /// <summary>
        /// Returns the number of frames currently in this BVHFile
        /// </summary>
        /// <returns>The number of frames currently in the file.</returns>
        public int GetNumOfFrames()
        {
            return this.numOfFrames;
        }

        /// <summary>
        /// Adds on a frame to the current BVH file.
        /// 
        /// PRECONDITION: Frame must end in a new line AND is has as many values as the tree has 
        /// position + rotation variables.
        /// </summary>
        /// <param name="frame">The string representation of the frame to be added</param>
        public void AppendFrame(String frame)
        {
            frames.AddLast(frame);
            numOfFrames++;
        }

        /// <summary>
        /// Returns a frame at the specified index. Returns null if index is not in a valid range.
        /// 
        /// NOTE: This method runs in O(n) time.
        /// </summary>
        /// <param name="index">The index of the frame to be found in the Motion section.</param>
        /// <returns>The Frame at the specified index</returns>
        public String GetFrame(int index)
        {
            if (index >= frames.Count || index < 0) { return null; }
            //Return the starting frame if 0.
            if (index == 0) { return DEFAULT_FRAME; }

            LinkedListNode<String> iter = frames.First;
            for (int i = 0; i < index-1; i++)
            {
                iter = iter.Next;
            }
            return iter.Value;
        }

        /// <summary>
        /// Pops the first frame (not including the initialization frame).
        /// </summary>
        /// <returns>The first non-initialization frame</returns>
        public String PopFirstFrame()
        {
            if (frames.First == null) { return null; }

            String f = frames.First.Value;
            frames.RemoveFirst();
            numOfFrames--;
            return f;
        }

        /// <summary>
        /// Return the BVHFile as a String. Returns the Heiarchy and the Motion followed
        /// by all the current frames stored in the file.
        /// </summary>
        /// <returns>The string value of the file.</returns>
        public override String ToString()
        {
            //Creates the Heirarychy section
            String output = "HIERARCHY\n";
            output += boneTree.ToString();
            //Appends the Motion section
            output += "MOTION\nFrames: " + numOfFrames + "\n";
            output += "Frame Time: " + FrameRate + "\n";

            //Appends the actual frames
            foreach (String frame in frames)
            {
                output += frame;
            }
            return output;
        }

        /// <summary>
        /// Converts the String representation of this file to a Byte array
        /// </summary>
        /// <returns>The Byte representation of the file</returns>
        public Byte[] ToBytes()
        {
            return Encoding.ASCII.GetBytes(this.ToString());
        }

        /// <summary>
        /// Parses the BVHFile into an AnimationMetaData class
        /// </summary>
        /// <returns>The AnimationMetaData for this BVHFile</returns>
        public BVHAnimationEncoder.AnimationMetaData ToDefaultAnimationMetaData()
        {
            BVHAnimationEncoder.AnimationMetaData meta = new BVHAnimationEncoder.AnimationMetaData();
            meta.EaseInTime = 0.3f; //Ease in for 0.3 seconds
            meta.EaseOutTime = 0.3f; //Ease out for 0.3 seconds
            meta.ExpressionName = "";
            meta.HandPose = 1; //Same as HandPose.Relaxed
            meta.InPoint = 0.0f; //Cropped starting point is at 0.0s
            meta.BoneCount = Convert.ToUInt32(boneTree.Count); // num of bones
            meta.Length = FrameRate * numOfFrames; //Ending Point in seconds
            meta.Loop = false;
            meta.OutPoint = meta.Length; //Cropped stopping point is out at 0.0s
            meta.Priority = 2; //Default priority

            meta.Bones = BoneTreeToBinBVHJoints(meta.Priority); //set the Bone Property for the meta data field.

            return meta;
        }

        /// <summary>
        /// Convert the bone tree contained in this bvh file into a binBVHJoint array
        /// </summary>
        /// <param name="priority">The priority level to be given to the individual joints for the animaiton</param>
        /// <returns>The binBVHJoint array representing the bone tree heirarchy of this file. This does not contain any rotations.</returns>
        public binBVHJoint[] BoneTreeToBinBVHJoints(int priority)
        {
            //Get the number of frames into a local variable so it won't decrement while we pop the frames
            int numFrames = numOfFrames;

            //Convert the bone tree to a binBVHJoint array
            binBVHJoint[] bvhBinJoints = boneTree.ToBinBVHJointArray();

            //The root is the only one with position keys
            bvhBinJoints[0].positionkeys = new binBVHJointKey[numFrames];

            //Initialize each bone with rotation keys
            for (int i = 0; i < boneTree.Count; i++)
            {
                bvhBinJoints[i].Priority = priority;
                bvhBinJoints[i].rotationkeys = new binBVHJointKey[numFrames];
            }

            //Loop through each frame
            for (int i = 0; i < numFrames; i++)
			{
                //Pop the first frame out
                String firstFrame = PopFirstFrame();
                //Tokenize the frame into the float representations
                String[] nums = firstFrame.Split(' ');

                //Read in the first 3 elements as the position
                bvhBinJoints[0].positionkeys[i].key_element = new OpenMetaverse.Vector3(Single.Parse(nums[0]), Single.Parse(nums[1]), Single.Parse(nums[2]));
                bvhBinJoints[0].positionkeys[i].time = (FrameRate) * (i+1); //the frame rate * the num of frames indicates the starting time. (+1 so it isn't 0)

                //Loop through every three elements in the array and assign it to the tree array
                for (int j = 0; j < nums.Length-4; j+=3) //(we already grabbed the first 3 elements in nums and the last element in each row is \n so we exclude that too)
                {
                    //Remember, we're doing rotation keys now so the first bone starts at 0 and increments by 1 while parameters come in in threes starting at index 3
                    bvhBinJoints[j / 3].rotationkeys[i].key_element = new OpenMetaverse.Vector3(Single.Parse(nums[j+3]), Single.Parse(nums[j + 4]), Single.Parse(nums[j + 5]));
                    bvhBinJoints[j / 3].rotationkeys[i].time = (FrameRate) * (i+1);
                }
			}

            return bvhBinJoints;
        }

        /// <summary>
        /// A tree class that represents the heirarchy of the BVH file
        /// </summary>
        public class BVHBoneTree
        {
            private int count;
            private BVHBoneNode root;

            /// <summary>
            /// The root node for the tree
            /// </summary>
            public BVHBoneNode Root
            {
                get
                {
                    return root;
                }
                set
                {
                    root = value;
                    if (count == 0 && value.Type != NodeType.Null) { count = 1; }
                    else if (value.Type == NodeType.Null) { count = 0; }
                }
            }
            /// <summary>
            /// The number of Nodes in the tree
            /// </summary>
            public int Count
            {
                get
                {
                    return count;
                }
            }

            /// <summary>
            /// Initialize a new, empty bone tree
            /// </summary>
            public BVHBoneTree()
            {
                count = 0;
            }

            /// <summary>
            /// Initialize the tree with a root.
            /// </summary>
            /// <param name="root">The Root for the bone tree</param>
            public BVHBoneTree(BVHBoneNode root)
            {
                Root = root;
                count = 1;
            }

            /// <summary>
            /// Does a pre-order traversal of the tree and shows the heirarchy of the tree.
            /// </summary>
            /// <returns>The string value showing the heirarchy of this tree</returns>
            public override string ToString()
            {
                return getString("", Root);
            }

            /// <summary>
            /// The recursive call to ToString
            /// </summary>
            /// <param name="tabs">The indentations to use to show contained children</param>
            /// <param name="currNode">The current node in the recursive call</param>
            /// <returns>The string value showing the heirarchy of this tree</returns>
            private string getString(string tabs, BVHBoneNode currNode)
            {
                //Return if currNode is of type null.
                if (currNode.Type == NodeType.Null) { return "";}

                String output = "";
                String t; //The string value for the type

                //Sets the string value of the type 
                if (currNode.Type == NodeType.Root) { t = "ROOT "; }
                else if (currNode.Type == NodeType.EndSite) { t = "End Site "; }
                else { t = "JOINT "; }

                output += tabs + t + currNode.BoneName + "\n";
                output += tabs + "{\n";
                output += tabs + "\t" + "OFFSET " 
                    + String.Format("{0:f6}", (float)currNode.Offset.X) + " " 
                    + String.Format("{0:f6}", (float)currNode.Offset.Y) + " " 
                    + String.Format("{0:f6}", (float)currNode.Offset.Z) + "\n";
                //Adds channels if it isn't an endsite
                if (currNode.Type != NodeType.EndSite)
                {
                    output += tabs + "\t" + "CHANNELS " + currNode.Channels.Length + " ";
                    foreach (String item in currNode.Channels)
                    {
                        output += item + " ";
                    }
                    output += "\n";
                    foreach (BVHBoneNode child in currNode.Children)
                    {
                        output += getString(tabs + "\t", child);
                    }
                }
                output += tabs + "}\n";
                return output;
            }

            /// <summary>
            /// Attempts to find the bone with the given bone name in the tree.
            /// </summary>
            /// <param name="boneName">The name of the bone to be found</param>
            /// <returns>The found BVHBoneNode. If not found it will return a BVHBoneNode with field "IsNulled" equal to true</returns>
            public BVHBoneNode Find(String boneName)
            {
                return find(boneName, Root);
            }

            /// <summary>
            /// Recursive version of the find function.
            /// </summary>
            /// <param name="boneName">The name of the bone to be found</param>
            /// <param name="currNode">The current node of the recursive call. Use Root for external calls</param>
            /// <returns>The found BVHBoneNode. If not found it will return a BVHBoneNode with field "IsNulled" equal to true</returns>
            private BVHBoneNode find(String boneName, BVHBoneNode currNode)
            {
                //See if the current node is the sought node
                if (currNode.BoneName.Equals(boneName))
                {
                    //Found
                    return currNode;
                }
                else
                {
                    //Otherwise search the children to see if they are the parent or are ancestors of the desired parent.
                    foreach (BVHBoneNode child in currNode.Children)
                    {
                        //Searches SubTrees for the boneName
                        BVHBoneNode found = find(boneName, child);
                        //If the found node is not null then this is the desired return value
                        if (found.Type != NodeType.Null) { return found; }
                    }
                    //Otherwise this subtree can't find the parent and returns a nulled BVHBoneNode.
                    BVHBoneNode nulledNode = new BVHBoneNode();
                    nulledNode.Type = NodeType.Null;
                    return nulledNode;
                }
            }

            /// <summary>
            /// Sees if the Tree contains a node with the given bone name.
            /// </summary>
            /// <param name="boneName">The name of the bone to be found</param>
            /// <returns>True if the tree contains that bone, false otherwise</returns>
            public bool Contains(String boneName)
            {
                //Returns true if the find function can be called and a non-nulled node is returned.
                return (find(boneName, Root).Type != NodeType.Null);
            }

            /// <summary>
            /// Adds a new child to the tree with the specified parent.
            /// </summary>
            /// <param name="node">The child node to be added to the tree</param>
            /// <param name="parentName">Name of the parent bone matching Avatar_Skeleton.xml format</param>
            /// <returns>True if successfully added, else it couldn't find the parent bone</returns>
            public bool AddNode(BVHBoneNode node, String parentName)
            {
                //Attempt to find the parent
                BVHBoneNode foundParent = find(parentName, Root);
                if (foundParent.Type != NodeType.Null)
                {
                    //If the parent is actually found, add the node and update the count
                    foundParent.Children.Add(node);
                    if (node.Type != NodeType.EndSite) { count++; }
                    return true;
                }
                //Parent not found, node not added, so return false
                return false;
            }

            /// <summary>
            /// Turns the Bone Tree into a binBHVJoint array
            /// </summary>
            /// <returns>The binBVHJoint array containing the bone tree info.</returns>
            public binBVHJoint[] ToBinBVHJointArray()
            {
                LinkedList<binBVHJoint> l = new LinkedList<binBVHJoint>();
                ToBinBVHJoint(ref l, Root);
                return l.ToArray();
            }

            /// <summary>
            /// Recursive call to change the tree into a linked list of BVHJoints.
            /// Might be better if this was an array, not linked list.
            /// </summary>
            /// <param name="joints">The destination to store the joints</param>
            /// <param name="node">The current location in the recursive call</param>
            private void ToBinBVHJoint(ref LinkedList<binBVHJoint> joints, BVHBoneNode node)
            {
                //If the current node is not null and isn't and end site, add it to the list
                if (node.Type != NodeType.Null && node.Type != NodeType.EndSite)
                {
                    binBVHJoint joint = new binBVHJoint();
                    joint.Name = node.XMLBoneName;
                    joints.AddLast(joint);

                    //recurse on the children
                    foreach (BVHBoneNode child in node.Children)
                    {
                        ToBinBVHJoint(ref joints, child);
                    }
                }
            }

            /// <summary>
            /// Contains all the data about a given Bone in the Bone tree.
            /// </summary>
            public class BVHBoneNode
            {
                private String xmlBoneName;
                private String[] channels;
                private String boneName;
                private NodeType type;

                /// <summary>
                /// The Children of the current BVHBoneNode
                /// </summary>
                public List<BVHBoneNode> Children;
                /// <summary>
                /// A Vector3 representing the position offset values for this bone
                /// </summary>
                public Vector3 Offset;
                public String XMLBoneName
                {
                    get
                    {
                        return xmlBoneName;
                    }
                }
                /// <summary>
                /// The Channels for this bone node (currently expected to be either 0, 3 or 6)
                /// </summary>
                public String[] Channels
                {
                    get
                    {
                        return channels;
                    }
                }
                /// <summary>
                /// The name of the bone. Parses the value automatically to XML format
                /// </summary>
                public String BoneName
                {
                    get
                    {
                        return boneName;
                    }
                    set
                    {
                        xmlBoneName = BVHToXMLBoneMapping.GetMappingForBVH(value);
                        boneName = value;
                    }
                }
                /// <summary>
                /// Gets the type of the Node.
                /// </summary>
                public NodeType Type
                {
                    get
                    {
                        return type;
                    }
                    set
                    {
                        if (value == NodeType.Root)
                        {
                            channels = new String[] { 
                                "Xposition", "Yposition", "Zposition", 
                                "Xrotation", "Zrotation", "Yrotation" };
                        }
                        else if (value == NodeType.XZY)
                        {
                            channels = new String[] {  
                                "Xrotation", "Zrotation", "Yrotation" };
                        }
                        else if (value == NodeType.XYZ)
                        {
                            channels = new String[] {  
                                "Xrotation", "Yrotation", "Zrotation" };
                        }
                        else if (value == NodeType.YZX)
                        {
                            channels = new String[]{
                                "Yrotation", "Zrotation", "Xrotation" };
                        }
                        else if (value == NodeType.ZYX)
                        {
                            channels = new String[]{
                                "Zrotation", "Yrotation", "Xrotation" };
                        }
                        else
                        {
                            channels = null;
                        }

                        type = value;
                    }
                }

                public BVHBoneNode()
                {                 
                    xmlBoneName = "";
                    Children = new List<BVHBoneNode>();
                    Offset = new Vector3();
                    channels = null;
                    Type = NodeType.Normal;
                    boneName = "";
                }

                public BVHBoneNode(String boneName) : this()
                {
                    BoneName = boneName;
                }
            }
        }

        /// <summary>
        /// The different types of nodes
        /// </summary>
        public enum NodeType
        {
            Null = -1,
            Root = 0,
            Normal = 1,
            EndSite = 2,
            XYZ = 3,
            YZX = 4,
            ZYX = 5,
            XZY = 6
        }

        /// <summary>
        /// Utility class that maps BVH bone structure to Avatar_Skeleton.xml bone structure
        /// </summary>
        public static class BVHToXMLBoneMapping
        {
            /// <summary>
            /// Maps a BVH bone to an XML bone
            /// </summary>
            /// <param name="BoneName">The name of the BVH bone</param>
            /// <returns>The name of the XML bone</returns>
            public static String GetMappingForBVH(String BoneName)
            {
                switch (BoneName)
                {
                    case "hip":
                        return "mPelvis";
                    case "abdomen":
                        return "mTorso";
                    case "chest":
                        return "mChest";
                    case "neck":
                        return "mNeck";
                    case "head":
                        return "mHead";
                    case "lCollar":
                        return "mCollarLeft";
                    case "lShldr":
                        return "mShoulderLeft";
                    case "lForeArm":
                        return "mElbowLeft";
                    case "lHand":
                        return "mWristLeft";
                    case "rCollar":
                        return "mCollarRight";
                    case "rShldr":
                        return "mShoulderRight";
                    case "rForeArm":
                        return "mElbowRight";
                    case "rHand":
                        return "mWristRight";
                    case "lThigh":
                        return "mHipLeft";
                    case "lShin":
                        return "mKneeLeft";
                    case "lFoot":
                        return "mAnkleLeft";
                    case "rThigh":
                        return "mHipRight";
                    case "rShin":
                        return "mKneeRight";
                    case "rFoot":
                        return "mAnkleRight";
                    default:
                        return BoneName;
                }
            }
        }
    }
}
