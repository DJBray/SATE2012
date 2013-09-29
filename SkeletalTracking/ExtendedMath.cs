using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace SkeletalTracking
{
    public static class ExtendedMath
    {
        /*public void setBoneRadii(Skeleton skel)
        {
            JointCollection joints = skel.Joints;

            //boneRadii[(int)BoneTypes.abdomen] = new BoneRadius(BoneTypes.abdomen, getHypotenuse(joints[JointType.HipCenter], joints[JointType.Spine]));
            //boneRadii[(int)BoneTypes.chest] = new BoneRadius(BoneTypes.chest, getHypotenuse(joints[JointType.Spine], joints[JointType.ShoulderCenter]));
            //boneRadii[(int)BoneTypes.head] = new BoneRadius(BoneTypes.head, getHypotenuse(joints[JointType.Head], joints[JointType.ShoulderCenter]));  //tricky
            //boneRadii[(int)BoneTypes.hip] = new BoneRadius(BoneTypes.hip, getHypotenuse(joints[JointType.HipLeft], joints[JointType.HipCenter]));  //tricky
            //boneRadii[(int)BoneTypes.lCollar] = new BoneRadius(BoneTypes.lCollar, getHypotenuse(joints[JointType.ShoulderLeft], joints[JointType.ShoulderCenter]));
            //boneRadii[(int)BoneTypes.lFoot] = new BoneRadius(BoneTypes.lFoot, getHypotenuse(joints[JointType.FootLeft], joints[JointType.AnkleLeft]));
            //boneRadii[(int)BoneTypes.lForeArm] = new BoneRadius(BoneTypes.lForeArm, getHypotenuse(joints[JointType.WristLeft], joints[JointType.ElbowLeft]));
            //boneRadii[(int)BoneTypes.lHand] = new BoneRadius(BoneTypes.lHand, getHypotenuse(joints[JointType.HandLeft], joints[JointType.WristLeft]));
            //boneRadii[(int)BoneTypes.lShin] = new BoneRadius(BoneTypes.lShin, getHypotenuse(joints[JointType.AnkleLeft], joints[JointType.KneeLeft]));
            //boneRadii[(int)BoneTypes.lShldr] = new BoneRadius(BoneTypes.lShldr, getHypotenuse(joints[JointType.ElbowLeft], joints[JointType.ShoulderLeft]));
            //boneRadii[(int)BoneTypes.lThigh] = new BoneRadius(BoneTypes.lThigh, getHypotenuse(joints[JointType.KneeLeft], joints[JointType.HipLeft]));
            //boneRadii[(int)BoneTypes.neck] = new BoneRadius(BoneTypes.neck, getHypotenuse(joints[JointType.ShoulderCenter], joints[JointType.Head]));
            //boneRadii[(int)BoneTypes.rCollar] = new BoneRadius(BoneTypes.rCollar, getHypotenuse(joints[JointType.ShoulderRight], joints[JointType.ShoulderCenter]));
            //boneRadii[(int)BoneTypes.rFoot] = new BoneRadius(BoneTypes.rFoot, getHypotenuse(joints[JointType.FootRight], joints[JointType.AnkleRight]));
            //boneRadii[(int)BoneTypes.rForeArm] = new BoneRadius(BoneTypes.rForeArm, getHypotenuse(joints[JointType.WristRight], joints[JointType.ElbowRight]));
            //boneRadii[(int)BoneTypes.rHand] = new BoneRadius(BoneTypes.rHand, getHypotenuse(joints[JointType.HandRight], joints[JointType.WristRight]));
            //boneRadii[(int)BoneTypes.rShin] = new BoneRadius(BoneTypes.rShin, getHypotenuse(joints[JointType.AnkleRight], joints[JointType.KneeRight]));
            //boneRadii[(int)BoneTypes.rShldr] = new BoneRadius(BoneTypes.rShldr, getHypotenuse(joints[JointType.ElbowRight], joints[JointType.ShoulderRight]));
            //boneRadii[(int)BoneTypes.rThigh] = new BoneRadius(BoneTypes.rThigh, getHypotenuse(joints[JointType.KneeRight], joints[JointType.HipRight]));

            hasRadiiData = true;
        }*/

        private static double getHypotenuse(Joint start, Joint finish)
        {
            double dx = finish.Position.X - start.Position.X; //should I take absolute value?
            double dy = finish.Position.Y - start.Position.Y;
            double dz = -1 * (finish.Position.Z - start.Position.Z);

            //compute hypotenuse length
            return Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        /*public void coordinateToEuler(Skeleton skel)
        {
            BoneRotationAngles[] n_boneRotAngles = new BoneRotationAngles[BVHWriter.NUMBONES];

            this.calculateThighs(skel);
            this.calculateAbdomen(skel);

            double[] eulerAngles = { this.radiansToDegrees(xRotation), 
                                     0.0, 
                                     this.radiansToDegrees(zRotation) };

            return eulerAngles;
        }*/

        /*private Vector4 findLimbRotation(Joint origin, Joint endingJoint, BoneTypes type)
        {
            double h = boneRadii[(int)type].BoneLength;

            double dx = endingJoint.Position.X - origin.Position.X; //should I take absolute value?
            double dy = endingJoint.Position.Y - origin.Position.Y;
            double dz = -1 * (endingJoint.Position.Z - origin.Position.Z);

            double xRotation = Math.Asin(dy / h);
            //assures us that the angle measured is only in reference to the POSITIVE y-axis 
            //and whose absolute value is < 180
            if (dx < 0 && dy > 0)
                xRotation = Math.PI - xRotation;
            else if (dx < 0 && dy < 0)
                xRotation = -1 * (Math.PI + xRotation);
            xRotation *= -1; //Necessary to get it into the virtual world format

            double zRotation = Math.Asin(dz / h);
            if (dz > 0 && dx < 0)
                zRotation = Math.PI - zRotation;
            else if (dz < 0 && dx < 0)
                zRotation = -1 * (Math.PI + zRotation);
            zRotation *= -1;

            Vector4 eulerAngles = new Vector4();
            eulerAngles.X = ExtendedMath.radiansToDegrees(xRotation);
            eulerAngles.Y = 0.0f;
            eulerAngles.Z = ExtendedMath.radiansToDegrees(zRotation);
            return eulerAngles;
        }

        private Vector4 findLimbRotation(Joint origin, Joint endingJoint, BoneTypes type, Joint witness, BoneTypes witnessType)
        {
            double h = boneRadii[(int)type].BoneLength;

            double dx = endingJoint.Position.X - origin.Position.X; //should I take absolute value?
            double dy = endingJoint.Position.Y - origin.Position.Y;
            double dz = -1 * (endingJoint.Position.Z - origin.Position.Z);

            double xRotation = Math.Asin(dy / h);
            //assures us that the angle measured is only in reference to the POSITIVE y-axis 
            //and whose absolute value is < 180
            if (dx < 0 && dy > 0)
                xRotation = Math.PI - xRotation;
            else if (dx < 0 && dy < 0)
                xRotation = -1 * (Math.PI + xRotation);
            xRotation *= -1; //Necessary to get it into the virtual world format

            double r_z = witness.Position.Z - origin.Position.Z
            double yRotation = Math.Asin(witness.Position.Z;

            double zRotation = Math.Asin(dz / h);
            if (dz > 0 && dx < 0)
                zRotation = Math.PI - zRotation;
            else if (dz < 0 && dx < 0)
                zRotation = -1 * (Math.PI + zRotation);
            zRotation *= -1;

            Vector4 eulerAngles = new Vector4();
            eulerAngles.X = ExtendedMath.radiansToDegrees(xRotation);
            eulerAngles.Y = 0.0f; 
            if(type == BoneTypes.lShin || type == BoneTypes.rShin || type == BoneTypes.lForeArm || type == BoneTypes.rForeArm)
                eulerAngles.Z = 0.0f;
            else
                eulerAngles.Z = this.radiansToDegrees(zRotation);
            return eulerAngles;
        }

        private LinkedList<Vector4> calculateThighs(Skeleton skel)
        {
            LinkedList<Vector4> shinRotations = this.calculateShins(skel);

            //calculate left leg
            this.findLimbRotation(skel.Joints[JointType.HipLeft], skel.Joints[JointType.KneeLeft], BoneTypes.lThigh, skel.Joints[JointType.AnkleLeft], BoneTypes.lShin);

            //calculate right leg
            this.findLimbRotation(skel.Joints[JointType.HipRight], skel.Joints[JointType.KneeRight], BoneTypes.rThigh, skel.Joints[JointType.AnkleRight], BoneTypes.rShin);

            return shinRotations;
        }

        private LinkedList<Vector4> calculateShins(Skeleton skel)
        {
            LinkedList<Vector4> feetRotations = this.calculateFeet(skel);

            //calculate left shin
            feetRotations.AddFirst(this.findLimbRotation(skel.Joints[JointType.KneeLeft], skel.Joints[JointType.AnkleLeft], BoneTypes.lShin));

            //calculate right shin
            feetRotations.AddFirst(this.findLimbRotation(skel.Joints[JointType.KneeRight], skel.Joints[JointType.AnkleRight], BoneTypes.rShin));

            return feetRotations;
        }

        private LinkedList<Vector4> calculateFeet(Skeleton skel)
        {
            Vector4 feetTriple = new Vector4();
            feetTriple.X = 0.0f;
            feetTriple.Y = 0.0f;
            feetTriple.Z = 0.0f;

            LinkedList<Vector4> r = new LinkedList<Vector4>();
            r.AddFirst(feetTriple);
            r.AddFirst(feetTriple);
            return r;
        }

        private void calculateAbdomen()
        {

        }*/

        public static double[] quatToEuler(BoneOrientation bone)
        {
            //double[] eulerAngles = {bone.Position.X, bone.Position.Y, bone.Position.Z};

            BoneRotation rotation = bone.HierarchicalRotation;

            float q0 = rotation.Quaternion.W;
            float q1 = rotation.Quaternion.X;
            float q2 = rotation.Quaternion.Y;
            float q3 = rotation.Quaternion.Z;

            double xRotation = Math.Atan2(2 * (q0 * q1 + q2 * q3), 1 - (2 * (Math.Pow(q1, 2.0) + Math.Pow(q2, 2.0))));
            double yRotation = Math.Asin(2 * (q0 * q2 - q3 * q1));
            double zRotation = Math.Atan2(2 * (q0 * q3 + q1 * q2), 1 - (2 * (Math.Pow(q2, 2.0) + Math.Pow(q3, 2.0))));

            xRotation = ExtendedMath.radiansToDegrees(xRotation);
            yRotation = ExtendedMath.radiansToDegrees(yRotation);
            zRotation = ExtendedMath.radiansToDegrees(zRotation);

            double[] eulerAngles = new double[3];
            eulerAngles[0] = xRotation;
            eulerAngles[1] = yRotation;
            eulerAngles[2] = zRotation;

            return eulerAngles;
        }

        public static float radiansToDegrees(double angleInRadians)
        {
            return (float)(angleInRadians * 180 / Math.PI);
        }

        public static float degreesToRadians(double angleInDegrees)
        {
            return (float)(angleInDegrees * Math.PI / 180);
        }
    }
}
