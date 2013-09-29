using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;

namespace SkeletalTracking
{
    public class BVHAnimationEncoder
    {
        public struct AnimationMetaData{
            private string expressionName;

            /// <summary>
            /// Animation Priority
            /// </summary>
            public int Priority;

            /// <summary>
            /// The animation length in seconds.
            /// </summary>
            public Single Length;

            /// <summary>
            /// Expression set in the client.  Null if [None] is selected
            /// </summary>
            public string ExpressionName{
                get{
                    return expressionName;
                }
                set{
                    if(value.Trim().Length == 0 || value.Contains("None"))
                        value = null;
                    else
                        expressionName = value;
                }
            }

            /// <summary>
            /// The time in seconds to start the animation
            /// </summary>
            public Single InPoint;

            /// <summary>
            /// The time in seconds to end the animation
            /// </summary>
            public Single OutPoint;

            /// <summary>
            /// Loop the animation
            /// </summary>
            public bool Loop;

            /// <summary>
            /// Meta data. Ease in Seconds.
            /// </summary>
            public Single EaseInTime;

            /// <summary>
            /// Meta data. Ease out seconds.
            /// </summary>
            public Single EaseOutTime;

            /// <summary>
            /// Meta Data for the Hand Pose
            /// </summary>
            public uint HandPose;

            /// <summary>
            /// Number of joints defined in the animation
            /// </summary>
            public uint BoneCount;

            /// <summary>
            /// Contains an array of joints
            /// </summary>
            public binBVHJoint[] Bones;
        }

        /// <summary>
        /// The byte array for the encodedData
        /// </summary>
        public byte[] EncodedData;

        /// <summary>
        /// Searialize a bvh animation file into an animation asset
        /// </summary>
        /// <param name="animation">The meta data associated with the bvh</param>
        public BVHAnimationEncoder(AnimationMetaData animation)
        {
            byte[] p_encodedData = new byte[4194304]; //TODO: make this dynamic, not just ~4mB
            int i=0;     //The current byte index of p_encodedData

            Utils.UInt16ToBytes((UInt16)1, p_encodedData, i); i+=2;//First 2 bytes always equal to 1
            Utils.UInt16ToBytes((UInt16)0, p_encodedData, i); i+=2;//Second 2 bytes always equal to 0
            Utils.IntToBytes(animation.Priority, p_encodedData, i);  i+=4;
            Utils.FloatToBytes(animation.Length, p_encodedData, i); i+=4; 

            //get the byte representation of ExpressionName
            byte[] b_expression = Utils.StringToBytes(animation.ExpressionName);
            if (b_expression.Length == 0)
            {
                p_encodedData[i] = (byte)0;
                i++;
            }
            else
            {
                for (int j = 0; j < b_expression.Length; j++)
                {
                    p_encodedData[i] = b_expression[j]; //Stick the found bytes into p_encodedData
                    i++;                                //Update current index
                }
            }

            Utils.FloatToBytes(animation.InPoint, p_encodedData, i); i+=4;
            Utils.FloatToBytes(animation.OutPoint, p_encodedData, i); i+=4;
            Utils.IntToBytes(animation.Loop ? (int)1 : (int)0, p_encodedData, i); i+=4;
            Utils.FloatToBytes(animation.EaseInTime, p_encodedData, i); i+=4;
            Utils.FloatToBytes(animation.EaseOutTime, p_encodedData, i); i+=4;
            Utils.UIntToBytes(animation.HandPose, p_encodedData, i); i+=4;

            //Serialize the number of Bones
            Utils.UIntToBytes(animation.BoneCount, p_encodedData, i); i+=4; 

            // Serialize the Bones in the animation file.
            // As taken from OpenMetaverse.UtilsConversions, 
            //      "Joints are variable length blocks of binary data consisting of joint data and keyframes"
            for (int iter = 0; iter < animation.BoneCount; iter++)
            {
                WriteJointToBin(animation.Bones[iter], p_encodedData ,ref i, animation); 
            }

            //Mark end of file with 4 bytes of 0's
            Utils.UIntToBytes(UInt32.MinValue, p_encodedData, i); i += 4;

            //Trim byte array
            EncodedData = new byte[i];
            for (int j = 0; j < EncodedData.Length; j++)
            {
                EncodedData[j] = p_encodedData[j];
            }
        }

        ~BVHAnimationEncoder()
        {
            System.Console.WriteLine("inside destructor");
        }

        /// <summary>
        /// Writes the provided joints to the byte array.
        /// </summary>
        /// <param name="bone">The bone to be added to the byte array</param>
        /// <param name="encodedData">The location of the encoded bone data to go</param>
        /// <param name="i">The offset of the byte array</param>
        /// <returns></returns>
        public void WriteJointToBin(binBVHJoint bone, byte[] encodedData, ref int i, AnimationMetaData animation)
        {
            /*
                109
                84
                111
                114
                114
                111
                0 <--- Null terminator
            */

            byte[] b_jointName = Utils.StringToBytes(bone.Name);
            for(int j=0; j<b_jointName.Length; j++){
                encodedData[i] = b_jointName[j]; //Stick the found bytes into b_jointName
                i++;                             //Update current index
            }

            /* 
                 2 <- Priority Revisited
                 0
                 0
                 0
            */

            /* 
                5 <-- 5 keyframes
                0
                0
                0
                ... 5 Keyframe data blocks
            */

            /* 
                2 <-- 2 keyframes
                0
                0
                0
                ..  2 Keyframe data blocks
            */
            Utils.IntToBytes(bone.Priority, encodedData, i); i+=4; //Every Joint should have 
                                                                   //same priority as Priority property
            //rotation keys
            if (bone.rotationkeys == null)
            {
                Utils.IntToBytes(0, encodedData, i); i += 4; //The number of rotation keyframes
            }
            else
            {
                Utils.IntToBytes(bone.rotationkeys.Length, encodedData, i); i += 4; //The number of rotation keyframes
            }
            WriteKeysToBin(bone.rotationkeys, encodedData, ref i, -180f, 180f, animation);

            //position keys
            if (bone.positionkeys == null)
            {
                Utils.IntToBytes(0, encodedData, i); i += 4; //4 bytes saying the num of position keyframes
            }
            else
            {
                Utils.IntToBytes(bone.positionkeys.Length, encodedData, i); i += 4; //4 bytes saying the num of position keyframes
            }
            WritePosKeysToBin(bone.positionkeys, encodedData, ref i, animation);
        }

        /// <summary>
        /// Writes the position keys to the byte array.
        /// </summary>
        /// <param name="keys">The array of Position Keys</param>
        /// <param name="encodedData">The byte array to write to</param>
        /// <param name="i">The reference to the index to write to</param>
        /// <param name="animation">The animation meta data used by the encoder</param>
        public void WritePosKeysToBin(binBVHJointKey[] keys, byte[] encodedData, ref int i, AnimationMetaData animation)
        {
            if (keys == null) { return; }

            foreach (binBVHJointKey j_key in keys)
            {
                //Note that all these are 2 bytes, not 4
                Utils.UInt16ToBytes(Utils.FloatToUInt16(j_key.time, animation.InPoint, animation.OutPoint), encodedData, i); i += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16((float)j_key.key_element.X, -80f, 80f), encodedData, i); i += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16((float)j_key.key_element.Y, 0f, 80f), encodedData, i); i += 2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16((float)j_key.key_element.Z, -80f, 80f), encodedData, i); i += 2;
            }
        }

        /// <summary>
        /// Write the Key frames for the joint to the byte array
        /// </summary>
        /// <param name="binBVHJointKey">The array of keys for the joint</param>
        /// <param name="encodedData">The bye array to write to</param>
        /// <param name="i">Offset in the Byte Array.  Will be advanced</param>
        /// <param name="keycount">Number of Keyframes</param>
        /// <param name="min">Scaling Min to pass to the FloatToUint16 method</param>
        /// <param name="max">Scaling Max to pass to the FloatToUint16 method</param>
        /// <returns></returns>
        public void WriteKeysToBin(binBVHJointKey[] keys, byte[] encodedData, ref int i, float min, float max, AnimationMetaData animation)
        {
            if (keys == null) { return; }
            /*
                17          255  <-- Time Code
                17          255  <-- Time Code
                255         255  <-- X
                127         127  <-- X
                255         255  <-- Y
                127         127  <-- Y
                213         213  <-- Z
                142         142  <---Z
            */
            foreach(binBVHJointKey j_key in keys)
            {
                //Note that all these are 2 bytes, not 4
                Utils.UInt16ToBytes(Utils.FloatToUInt16(j_key.time, animation.InPoint, animation.OutPoint), encodedData, i); i+=2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16((float)j_key.key_element.X, min, max), encodedData, i); i+=2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16((float)j_key.key_element.Y, min, max), encodedData, i); i+=2;
                Utils.UInt16ToBytes(Utils.FloatToUInt16((float)j_key.key_element.Z, min, max), encodedData, i); i+=2;
            }
        }
    }
}
