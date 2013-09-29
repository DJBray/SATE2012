using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletalTracking
{
    using Microsoft.Kinect;

    #region Bone Classes

    #region Hip
    public class Hip : BodyPart, FullyRotational
    {
        #region Field Variables
        /// <summary>
        /// The offset to be applied to the x_rot to tweak the animation
        /// </summary>
        public const double HIPXOFFSET = 7;
        /// <summary>
        /// The left thigh of the Hip
        /// </summary>
        public Thigh leftThigh;
        /// <summary>
        /// The right thigh of the Hip
        /// </summary>
        public Thigh rightThigh;
        /// <summary>
        /// The abdomen belonging to the Hip
        /// </summary>
        public Abdomen abdomen;
        /// <summary>
        /// The body part vector setting the y_p2 for the body
        /// </summary>
        public Vector3 orthBodyPartVector;
        #endregion

        #region Base Class Overrides
        /// <summary>
        /// Creates a new hip initializing it with a left thigh, right thigh, and an abdomen.
        /// </summary>
        public Hip() : base()
        {
            leftThigh = new Thigh(true);
            rightThigh = new Thigh(false);
            abdomen = new Abdomen();
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero && orthBodyPartVector != zero)
            {
                this.calcY_rot();
                this.calcZ_rot();
                this.calcX_rot();
            }

            this.abdomen.CalculateRotations(x_p2, y_p2, z_p2);
            this.leftThigh.CalculateRotations(x_p2, y_p2, z_p2);
            this.rightThigh.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the body part vector using the skeletal frame and normalizes it.
        /// </summary>
        /// <param name="skel">The skeleton frame containing joint positions</param>
        /// <param name="start">The joint where the hip starts</param>
        /// <param name="finish">The joint where the hip stops</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);

            //hip center - hip right, but needs to be hip right - hip left
            SkeletonPoint p = skel.Joints[JointType.HipCenter].Position;
            SkeletonPoint q = skel.Joints[JointType.HipRight].Position;

            this.orthBodyPartVector.X = p.X - q.X;
            this.orthBodyPartVector.Y = p.Y - q.Y;
            this.orthBodyPartVector.Z = p.Z - q.Z;

            leftThigh.UpdateBodyPartVector(skel, JointType.KneeLeft, JointType.HipLeft);
            rightThigh.UpdateBodyPartVector(skel, JointType.KneeRight, JointType.HipRight);
            abdomen.UpdateBodyPartVector(skel, JointType.HipCenter, JointType.Spine);
        }

        /// <summary>
        /// Appends a frame of data into the file
        /// </summary>
        /// <param name="stream">The file stream to put the data into the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = abdomen.GetCurrFrame(append);
            append = leftThigh.GetCurrFrame(append);
            append = rightThigh.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// 
        /// PRECONDITION: The Y and Z rotations are already performed
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcX_rot()
        {
            //Find y_p component length
            double y_pComp = orthBodyPartVector.DotProduct(y_p2);

            //Find z_p component length
            double z_pComp = orthBodyPartVector.DotProduct(z_p2);

            //Finds the projection of the vector onto the y-z prime plane
            double yz_plane = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the z_p component. This is the x_p rotation.
            this.x_rot = 0.0;// ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / yz_plane)) + HIPXOFFSET;

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcY_rot()
        {
            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);

            //Finds the projection of the vector onto the y-x prime plane
            double yx_plane = Math.Sqrt(Math.Pow(x_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the z_component and the x_p component. This is the y_p rotation.
            this.y_rot = ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / yx_plane));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Y, y_rot);

            return y_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);

            //Finds the projection of the vector onto the y-z prime plane
            double xy_plane = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = (-1)*ExtendedMath.radiansToDegrees(Math.Asin(y_pComp / xy_plane));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Thigh
    public class Thigh : BodyPart, FullyRotational
    {
        #region Field Variables
        /// <summary>
        /// The shin connected to this thigh
        /// </summary>
        public Shin child;
        /// <summary>
        /// True if it's a left thigh, otherwise it is a right thigh
        /// </summary>
        public bool isLeft;
        #endregion

        #region Base Class overrides
        /// <summary>
        /// Creates a new thigh and initializes it with a shin.
        /// </summary>
        /// <param name="isLeft">Specifies if it is a left thigh or a right thigh</param>
        public Thigh(bool isLeft) : base()
        {
            this.isLeft = isLeft;
            this.child = new Shin(isLeft);
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcX_rot();
                this.calcY_rot();
            }

            this.child.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the body part vector using the given skeleton coordinates
        /// </summary>
        /// <param name="skel">The skeleton coordinates taken from the Kinect</param>
        /// <param name="start">The starting location of the limb</param>
        /// <param name="finish">The ending location of the limb</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);
            if(isLeft)
                this.child.UpdateBodyPartVector(skel, JointType.AnkleLeft, JointType.KneeLeft);
            else
                this.child.UpdateBodyPartVector(skel, JointType.AnkleRight, JointType.KneeRight);
        }

        /// <summary>
        /// Appends a frame of data onto the bvh file
        /// </summary>
        /// <param name="stream">The file stream outputting to the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = this.child.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcX_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double yzPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.x_rot = ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / yzPlaneComp));
            //Arcsin will flip the rotation into positive if it goes beyond 90, this 
            //keeps the angle growing in negative direction since the thigh can rotate ~270 degrees
            if (this.x_rot > 0 && z_pComp < 0)
                this.x_rot = (-1)*(this.x_rot + 180);

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// 
        /// PRECONDITION: The x and the z rotations must be calculated first. 
        /// Also the child must have its bodyPartVector.
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcY_rot()
        {
            Vector3 vect = child.bodyPartVector + this.bodyPartVector;
            double z_pComp = vect.DotProduct(z_p2);
            double x_pComp = vect.DotProduct(x_p2);
            //Find z_p component length using the CHILD
            //double z_pComp = child.bodyPartVector.DotProduct(z_p2);

            //Find x_p component length using the CHILD
            //double x_pComp = child.bodyPartVector.DotProduct(x_p2);

            //Finds the projection of the components of the child vector on the xz prime plane
            double xz_plane = Math.Sqrt(Math.Pow(x_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the z_component and the z_p-x_p projection. This is the y_p rotation.
            this.y_rot = ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / xz_plane));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Y, y_rot);

            return y_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);
            //Compute the length of the projection of the vector on the y_p-x_p plane
            double yxPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / yxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Shin
    public class Shin : BodyPart, MoveButNoZRot
    {
        #region Field Variables
        public Foot child;
        public bool isLeft;
        #endregion

        #region Base Class overrides
        /// <summary>
        /// Creates a new thigh and initializes it with a shin.
        /// </summary>
        /// <param name="isLeft">Specifies if it is a left thigh or a right thigh</param>
        public Shin(bool isLeft) : base()
        {
            this.isLeft = isLeft;
            this.child = new Foot(isLeft);
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcX_rot();
                this.calcY_rot();
            }

            this.child.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the body part vector using the given skeleton coordinates
        /// </summary>
        /// <param name="skel">The skeleton coordinates taken from the Kinect</param>
        /// <param name="start">The starting location of the limb</param>
        /// <param name="finish">The ending location of the limb</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);
            if(isLeft)
                this.child.UpdateBodyPartVector(skel, JointType.FootLeft, JointType.AnkleLeft);
            else
                this.child.UpdateBodyPartVector(skel, JointType.FootRight, JointType.AnkleRight);
        }

        /// <summary>
        /// Appends a frame of data onto the bvh file
        /// </summary>
        /// <param name="stream">The file stream outputting to the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = this.child.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcX_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double yzPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.x_rot = ExtendedMath.radiansToDegrees(Math.Acos(y_pComp / yzPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// WARNING: Not been fully evaluated logically.
        /// 
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// 
        /// PRECONDITION: The x and the z rotations must be calculated first. 
        /// Also the child must have its bodyPartVector.
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcY_rot()
        {
            //Find z_p component length using the CHILD
            double z_pComp = child.bodyPartVector.DotProduct(z_p2);

            //Find x_p component length using the CHILD
            double x_pComp = child.bodyPartVector.DotProduct(x_p2);
            //Compute the length of the projection of the vector on the z_p-x_p plane
            double zxPlaneComp = Math.Sqrt(Math.Pow(z_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the z_component and the z_p-x_p projection. This is the y_p rotation.
            this.y_rot = ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / zxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Y, y_rot);

            return y_rot;
        }
        #endregion
    }
    #endregion

    #region Foot
    public class Foot : BodyPart, MoveButNoTwist
    {
        #region Field Variables
        /// <summary>
        /// Specifies if the foot is a left foot.
        /// </summary>
        public bool isLeft;
        #endregion

        #region Base Class overrides
        /// <summary>
        /// Creates a new foot specifying whether or not it is a left foot.
        /// </summary>
        /// <param name="isLeft">True if it is a left foot, otherwise it is a right foot</param>
        public Foot(bool isLeft) : base()
        {
            this.isLeft = isLeft;
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcTwist_rot();
            }
        }

        /// <summary>
        /// Returns the Foot as a string in x, y, z rotation format
        /// </summary>
        /// <returns>The x, y, z rotations for the foot</returns>
        public override string ToString()
        {
            if (x_rot.Equals(Double.NaN))
                x_rot = 0.0;
            if (y_rot.Equals(Double.NaN))
                y_rot = 0.0;
            if (z_rot.Equals(Double.NaN))
                z_rot = 0.0;
            return x_rot + " " + y_rot + " " + z_rot + " ";
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcTwist_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double yzPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.x_rot = 0.0;//ExtendedMath.radiansToDegrees(Math.Asin(y_pComp / yzPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);
            //Compute the length of the projection of the vector on the y_p-x_p plane
            double yxPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = 0.0;//ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / yxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Abdomen
    public class Abdomen : BodyPart, MoveButNoTwist
    {
        #region Field Variables
        /// <summary>
        /// Chest child of the abdomen
        /// </summary>
        public Chest child;
        #endregion

        #region Base Class Overrides
        /// <summary>
        /// Creates a new abdomen initializing the child
        /// </summary>
        public Abdomen() : base()
        {
            child = new Chest();
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcTwist_rot();
            }

            this.child.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the body part vector with the propper joints
        /// </summary>
        /// <param name="skel">The skeleton frame to use to get the vector</param>
        /// <param name="start">The starting joint for the measurement</param>
        /// <param name="finish">The ending joint for the measurement</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);
            this.child.UpdateBodyPartVector(skel, JointType.Spine, JointType.ShoulderCenter);
        }

        /// <summary>
        /// Appends a frame of data onto the bvh file
        /// </summary>
        /// <param name="stream">The file stream outputting to the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = this.child.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcTwist_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double yzPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.x_rot = 0.0;//ExtendedMath.radiansToDegrees(Math.Acos(y_pComp / yzPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);
            //Compute the length of the projection of the vector on the y_p-x_p plane
            double yxPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / yxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Chest
    public class Chest : BodyPart, FullyRotational
    {
        #region Field Variables
        /// <summary>
        /// Chest's left collar
        /// </summary>
        public Collar lcollar;
        /// <summary>
        /// Chest's right collar
        /// </summary>
        public Collar rcollar;
        /// <summary>
        /// The neck child connected to the chest
        /// </summary>
        public Neck neck;
        #endregion

        #region Base Class Overrides
        /// <summary>
        /// Constructs a new chest and initializes it with a left and right collar and a neck.
        /// </summary>
        public Chest() : base()
        {
            lcollar = new Collar(true);
            rcollar = new Collar(false);
            neck = new Neck();
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcX_rot();
                this.calcY_rot();
            }

            this.neck.CalculateRotations(x_p2, y_p2, z_p2);
            this.rcollar.CalculateRotations(x_p2, y_p2, z_p2);
            this.lcollar.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the body part vector for the chest
        /// </summary>
        /// <param name="skel">The skeleton frame containing the joint positions</param>
        /// <param name="start">The starting joint for the chest</param>
        /// <param name="finish">The ending joint for the chest</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);
            lcollar.UpdateBodyPartVector(skel, JointType.ShoulderCenter, JointType.ShoulderLeft);
            rcollar.UpdateBodyPartVector(skel, JointType.ShoulderRight, JointType.ShoulderCenter);
            neck.UpdateBodyPartVector(skel, JointType.ShoulderCenter, JointType.Head);
        }

        /// <summary>
        /// Appends a frame of data onto the bvh file
        /// </summary>
        /// <param name="stream">The file stream outputting to the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = this.neck.GetCurrFrame(append);
            append = this.lcollar.GetCurrFrame(append);
            append = this.rcollar.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcX_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double yzPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.x_rot = ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / yzPlaneComp));
            //TODO: Insert if statement here

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// 
        /// PRECONDITION: The x and the z rotations must be calculated first. 
        /// Also the child must have its bodyPartVector.
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcY_rot()
        {
            Vector3 childVector = rcollar.bodyPartVector + lcollar.bodyPartVector;

            //Find z_p component length using the CHILD
            double z_pComp = childVector.DotProduct(z_p2);

            //Find x_p component length using the CHILD
            double x_pComp = childVector.DotProduct(x_p2);
            //Compute the length of the projection of the vector on the z_p-x_p plane
            double zxPlaneComp = Math.Sqrt(Math.Pow(z_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the z_component and the z_p-x_p projection. This is the y_p rotation.
            this.y_rot = (-1)*ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / zxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Y, y_rot);

            return y_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);
            //Compute the length of the projection of the vector on the y_p-x_p plane
            double yxPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = (-1)*ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / yxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Neck
    public class Neck : BodyPart, MoveButNoTwist
    {
        #region Field Variables
        /// <summary>
        /// The head child of the Neck
        /// </summary>
        public Head child;
        #endregion

        #region Base Class Overrides
        /// <summary>
        /// Constructs a new neck and instanciates it with a head
        /// </summary>
        public Neck() : base()
        {
            child = new Head();
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcTwist_rot();
            }

            this.child.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the body part vector coordinates using the skeleton
        /// </summary>
        /// <param name="skel">The frame containing the coordinates to update the body part vector.</param>
        /// <param name="start">The starting joint of the neck</param>
        /// <param name="finish">The ending joint of the neck</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);
            child.UpdateBodyPartVector(skel, JointType.Head, JointType.Head);
        }

        /// <summary>
        /// Appends a frame of data onto the bvh file
        /// </summary>
        /// <param name="stream">The file stream outputting to the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = this.child.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcTwist_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double yzPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.x_rot = ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / yzPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);
            //Compute the length of the projection of the vector on the y_p-x_p plane
            double yxPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / yxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Head
    public class Head : BodyPart, FullyRotational
    {
        public Head() : base(){}

        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcX_rot()
        {
            return 0.0;
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcX_rot();
                this.calcY_rot();
            }
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// 
        /// PRECONDITION: The x and the z rotations must be calculated first. 
        /// Also the child must have its bodyPartVector.
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcY_rot()
        {
            return 0.0;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            return 0.0;
        }
    }
    #endregion

    #region Collar
    public class Collar : BodyPart, MoveButNoTwist
    {
        #region Field Variables
        /// <summary>
        /// The Collar's shoulder
        /// </summary>
        public Shoulder child;
        /// <summary>
        /// Specifies whether or not the Collar is a left collar or a right collar
        /// </summary>
        public bool isLeft;
        #endregion

        #region Base Class Overrides
        /// <summary>
        /// Constructs a new collar and instanciates it with a shoulder
        /// </summary>
        /// <param name="isLeft">Specifies if the collar is a left collar or a right collar</param>
        public Collar(bool isLeft) : base()
        {
            this.isLeft = isLeft;
            child = new Shoulder(isLeft);
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcTwist_rot();
            }

            this.child.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the Body Part Vector for the Collar
        /// </summary>
        /// <param name="skel">The skeleton containing the position data</param>
        /// <param name="start">The starting joint for the collar</param>
        /// <param name="finish">The ending joint for the collar</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);
            if (isLeft)
                child.UpdateBodyPartVector(skel, JointType.ShoulderLeft, JointType.ElbowLeft);
            else
                child.UpdateBodyPartVector(skel, JointType.ShoulderRight, JointType.ElbowRight);
        }

        /// <summary>
        /// Returns the y, z, x rotations as a string
        /// </summary>
        /// <returns>The y, z, x rotations as a string</returns>
        public override string ToString()
        {
            if (x_rot.Equals(Double.NaN))
                x_rot = 0.0;
            if (y_rot.Equals(Double.NaN))
                y_rot = 0.0;
            if (z_rot.Equals(Double.NaN))
                z_rot = 0.0;
            return y_rot + " " + z_rot + " " + x_rot + " ";
        }

        /// <summary>
        /// Appends a frame of data onto the bvh file
        /// </summary>
        /// <param name="stream">The file stream outputting to the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = this.child.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcTwist_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double yzPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.x_rot = 0.0;//ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / yzPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);
            //Compute the length of the projection of the vector on the y_p-x_p plane
            double yxPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = 0.0;// ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / yxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Shoulder
    public class Shoulder : BodyPart, FullyRotational
    {
        #region Field Variables
        /// <summary>
        /// The Forearm child of the shoulder
        /// </summary>
        public Forearm child;
        /// <summary>
        /// Specifies whether or not the shoulder is a left or right shoulder
        /// </summary>
        public bool isLeft;
        #endregion

        #region Base Class Overrides
        /// <summary>
        /// Constructs a new shoulder with a forearm child.
        /// </summary>
        /// <param name="isLeft">Specifies whether it is a left shoulder or a right shoulder</param>
        public Shoulder(bool isLeft) : base()
        {
            this.isLeft = isLeft;
            this.child = new Forearm(isLeft);
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcX_rot();
                this.calcY_rot();
            }

            this.child.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the body part vector using the given skeleton.
        /// </summary>
        /// <param name="skel">The skeleton containg the points for the shoulder</param>
        /// <param name="start">The starting joint for the shoulder</param>
        /// <param name="finish">The ending joint for the shoulder</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);
            if (isLeft)
                this.child.UpdateBodyPartVector(skel, JointType.WristLeft, JointType.ElbowLeft);
            else
                this.child.UpdateBodyPartVector(skel, JointType.ElbowRight, JointType.WristRight);
        }

        /// <summary>
        /// Returns the z, y, x rotations for the shoulder
        /// </summary>
        /// <returns>The z, y, x rotations for the shoulder</returns>
        public override string ToString()
        {
            if (x_rot.Equals(Double.NaN))
                x_rot = 0.0;
            if (y_rot.Equals(Double.NaN))
                y_rot = 0.0;
            if (z_rot.Equals(Double.NaN))
                z_rot = 0.0;
            return z_rot + " " + y_rot + " " + x_rot + " ";
        }

        /// <summary>
        /// Appends a frame of data onto the bvh file
        /// </summary>
        /// <param name="stream">The file stream outputting to the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = this.child.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations

        /// <summary>
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcY_rot()
        {
            //Find y_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double xzPlaneComp = Math.Sqrt(Math.Pow(x_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.y_rot = ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / xzPlaneComp));
            if (isLeft)
                this.y_rot *= -1;
            //TODO: Insert if statement here

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Y, y_rot);

            return y_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// 
        /// PRECONDITION: The x and the z rotations must be calculated first. 
        /// Also the child must have its bodyPartVector.
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcX_rot()
        {
            //Find z_p component length using the CHILD
            double z_pComp = child.bodyPartVector.DotProduct(z_p2);

            //Find x_p component length using the CHILD
            double y_pComp = child.bodyPartVector.DotProduct(y_p2);
            //Compute the length of the projection of the vector on the z_p-x_p plane
            double yzPlaneComp = Math.Sqrt(Math.Pow(z_pComp, 2) + Math.Pow(y_pComp, 2));

            //Find the angle between the z_component and the z_p-x_p projection. This is the y_p rotation.
            this.x_rot = ExtendedMath.radiansToDegrees(Math.Asin(y_pComp / yzPlaneComp));
            if (!isLeft)
                this.x_rot *= -1;

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);
            //Compute the length of the projection of the vector on the y_p-x_p plane
            double yxPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = ExtendedMath.radiansToDegrees(Math.Asin(y_pComp / yxPlaneComp));
            if (!isLeft)
                this.z_rot *= -1;

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Forearm
    public class Forearm : BodyPart, MoveButNoZRot
    {
        #region Field Variables
        /// <summary>
        /// The hand child for the forearm
        /// </summary>
        public Hand child;
        /// <summary>
        /// Specifies whether or not the forearm is a left forearm or right forearm
        /// </summary>
        public bool isLeft;
        #endregion

        #region Base Class Overrides
        /// <summary>
        /// Constructs a new forearm instanciating it with a hand.
        /// </summary>
        /// <param name="isLeft">Specifies whether the forearm is a left forearm or right</param>
        public Forearm(bool isLeft) : base()
        {
            this.child = new Hand(isLeft);
            this.isLeft = isLeft;
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcX_rot();
                this.calcY_rot();
            }

            this.child.CalculateRotations(x_p2, y_p2, z_p2);
        }

        /// <summary>
        /// Updates the body part vector for the forearm.
        /// </summary>
        /// <param name="skel">The skeleton frame used to get position data</param>
        /// <param name="start">The starting joint for the forearm</param>
        /// <param name="finish">The ending joint for the forearm</param>
        public override void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            base.UpdateBodyPartVector(skel, start, finish);
            if (isLeft)
                this.child.UpdateBodyPartVector(skel, JointType.HandLeft, JointType.WristLeft);
            else
                this.child.UpdateBodyPartVector(skel, JointType.WristRight, JointType.HandRight);
        }
         /// <summary>
         /// Returns the y, z, x rotations for the forearm
         /// </summary>
         /// <returns></returns>
        public override string ToString()
        {
            if (x_rot.Equals(Double.NaN))
                x_rot = 0.0;
            if (y_rot.Equals(Double.NaN))
                y_rot = 0.0;
            if (z_rot.Equals(Double.NaN))
                z_rot = 0.0;
            return y_rot + " " + z_rot + " " + x_rot + " ";
        }

        /// <summary>
        /// Appends a frame of data onto the bvh file
        /// </summary>
        /// <param name="stream">The file stream outputting to the bvh file</param>
        public override String GetCurrFrame(String append)
        {
            append = base.GetCurrFrame(append);
            append = this.child.GetCurrFrame(append);
            return append;
        }
        #endregion

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcY_rot()
        {
            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double xzPlaneComp = Math.Sqrt(Math.Pow(x_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            if(isLeft)
                this.y_rot = (-1)*ExtendedMath.radiansToDegrees(Math.Acos(x_pComp / xzPlaneComp));
            else
                this.y_rot = ExtendedMath.radiansToDegrees(Math.Acos(x_pComp / xzPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Y, y_rot);

            return y_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// 
        /// PRECONDITION: The x rotations must be calculated first. 
        /// Also the child must have its bodyPartVector.
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcX_rot()
        {
            //Find z_p component length using the CHILD
            double z_pComp = child.bodyPartVector.DotProduct(z_p2);

            //Find x_p component length using the CHILD
            double y_pComp = child.bodyPartVector.DotProduct(y_p2);
            //Compute the length of the projection of the vector on the z_p-x_p plane
            double zyPlaneComp = Math.Sqrt(Math.Pow(z_pComp, 2) + Math.Pow(y_pComp, 2));

            //Find the angle between the z_component and the z_p-x_p projection. This is the y_p rotation.
            this.x_rot = 0.0; //ExtendedMath.radiansToDegrees(Math.Asin(x_pComp / zxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.X, x_rot);

            return x_rot;
        }
        #endregion
    }
    #endregion

    #region Hand
    public class Hand : BodyPart, MoveButNoTwist
    {
        /// <summary>
        /// Specifies if the hand is a left hand or a right hand.
        /// </summary>
        public bool isLeft;

        /// <summary>
        /// Constructs a new hand
        /// </summary>
        /// <param name="isLeft">Specifies whether or not it is a left hand (right hand if false)</param>
        public Hand(bool isLeft) : base()
        {
            this.isLeft = isLeft;
        }

        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public override void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            base.CalculateRotations(x_p, y_p, z_p);

            Vector3 zero = new Vector3(0, 0, 0);
            if (bodyPartVector != zero)
            {
                this.calcZ_rot();
                this.calcTwist_rot();
            }
        }

        /// <summary>
        /// Returns the hand z, y, x rotations
        /// </summary>
        /// <returns>The z, y, x rotations of the hand</returns>
        public override string ToString()
        {
            if (x_rot.Equals(Double.NaN))
                x_rot = 0.0;
            if (y_rot.Equals(Double.NaN))
                y_rot = 0.0;
            if (z_rot.Equals(Double.NaN))
                z_rot = 0.0;
            return z_rot + " " + y_rot + " " + x_rot + " ";
        } 

        #region Behavior Implementations
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <param name="dx">x-component</param>
        /// <param name="dy">y-component</param>
        /// <param name="dz">z-component</param>
        /// <returns>The angle calculated</returns>
        public double calcTwist_rot()
        {
            //Find y_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);

            //Find z_p component length
            double z_pComp = bodyPartVector.DotProduct(z_p);
            //Compute the length of the projection of the vector on the y_p-z_p plane
            double xzPlaneComp = Math.Sqrt(Math.Pow(x_pComp, 2) + Math.Pow(z_pComp, 2));

            //Find the angle between the y_component and the y_p-z_p projection. This is the x_p rotation.
            this.y_rot = 0.0;//ExtendedMath.radiansToDegrees(Math.Asin(z_pComp / xzPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Y, y_rot);

            return y_rot;
        }

        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        public double calcZ_rot()
        {
            //Find y_p component length
            double y_pComp = bodyPartVector.DotProduct(y_p);

            //Find x_p component length
            double x_pComp = bodyPartVector.DotProduct(x_p);
            //Compute the length of the projection of the vector on the y_p-x_p plane
            double yxPlaneComp = Math.Sqrt(Math.Pow(y_pComp, 2) + Math.Pow(x_pComp, 2));

            //Find the angle between the y_component and the y_p-x_p projection. This is the z_p rotation.
            this.z_rot = 0.0;// (-1)*ExtendedMath.radiansToDegrees(Math.Asin(y_pComp / yxPlaneComp));

            //Apply rotations to the new relative axes.
            this.ApplyRotations(RelativeAxis.Z, z_rot);

            return z_rot;
        }
        #endregion
    }
    #endregion

    #endregion

    /// <summary>
    /// The BoneTree class is used to monitor rotation data about skeletal joints when using the Kinect. This
    /// class provides the ability to write to disk a bvh file and stream to the virtual world a bvh animation.
    /// This class calculates rotation angles in BVH standard format using a predefined tree-based structure and
    /// has the ability to find rotation angle information about major bones.
    /// </summary>
    public class BoneTree
    {
        /// <summary>
        /// The file name for the .bvh animation file generated
        /// </summary>
        public const String BVHFILENAME = "StreamMotionLive";
        /// <summary>
        /// The Hip represents the root of the Bone Tree
        /// </summary>
        public Hip root;
        /// <summary>
        /// The BVHWriter to be used for writing to disk in BVH Format
        /// </summary>
        private BVHWriter bvhWriter;
        /// <summary>
        /// The BVHStreamer to be used for streaming to the virtual world BVHFiles
        /// </summary>
        public BVHStreamer bvhStreamer;

        #region Constructors
        /// <summary>
        /// Constructs a new Bone Tree without a starting frame
        /// </summary>
        public BoneTree()
        {
            this.root = new Hip();
            this.bvhWriter = new BVHWriter(BVHFILENAME);
            this.bvhStreamer = new BVHStreamer();
        }

        /// <summary>
        /// Constructs a new Bone Tree with a starting frame
        /// </summary>
        /// <param name="skel">The starting skeletal frame</param>
        public BoneTree(Skeleton skel)
        {
            this.root = new Hip();
            this.SendUpdatedLocations(skel);
            this.bvhWriter = new BVHWriter(BVHFILENAME);
        }
        #endregion

        #region Get the current frame
        /// <summary>
        /// Gets the current frame in a BVH file format using rotation angles to calculate each joint's position.
        /// </summary>
        /// <returns>The current frame in BVH Motion file format</returns>
        public String GetCurrFrame()
        {
            String frame = "0 40 0 ";
            frame = root.GetCurrFrame(frame);
            frame += "\n";

            return frame;
        }
        #endregion

        #region Update the Body Parts' location data
        /// <summary>
        /// Updates the body parts' locations using the Skeleton and then calculates rotation angles
        /// according to BVH File format. If writing to file, this writes that frame to file. If streaming,
        /// this streams the current frame to the server.
        /// </summary>
        /// <param name="skel">The Skeleton to be used to calculate the body parts' locations</param>
        public void SendUpdatedLocations(Skeleton skel)
        {
            this.root.UpdateBodyPartVector(skel, JointType.HipLeft, JointType.HipRight);
            this.root.CalculateRotations(new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, -1));
            if (bvhWriter.IsOpen())
                this.AppendToBVHWriter();
            if (bvhStreamer.IsStreaming())
                this.bvhStreamer.SendFrame(this.GetCurrFrame());
        }
        #endregion

        #region BVH Write to File
        /// <summary>
        /// Sees if the motion data is being recorded to disk in a BVH File
        /// </summary>
        /// <returns>True if writing to BVH, false otherwise</returns>
        public bool isWritingToBVHFile()
        {
            return bvhWriter.IsOpen();
        }

        /// <summary>
        /// Starts writing the BVH file to disk if it isn't already, otherwise it does nothing.
        /// </summary>
        public void StartWritingToBVH()
        {
            if (!bvhWriter.IsOpen())
                bvhWriter.Open();
        }

        /// <summary>
        /// Finishes writing to BVHFile and closes the BVHWriter. 
        /// This is where the file is actually written to disk.
        /// </summary>
        public void FinishWritingToBVH()
        {
            if(bvhWriter.IsOpen())
                bvhWriter.CloseAndWrite();
        }

        /// <summary>
        /// Appends the current frame of motion data to the BVHFile
        /// </summary>
        private void AppendToBVHWriter()
        {
            String frame = this.GetCurrFrame();
            bvhWriter.AppendFrame(frame);
        }
        #endregion

        /*#region BVH Streaming to Server
        /// <summary>
        /// Sees if the BVHFile is streaming to the server
        /// </summary>
        /// <returns>True if streaming, false otherwise</returns>
        public bool isStreaming()
        {
            return bvhStreamer.IsStreaming();
        }

        /// <summary>
        /// Starts streaming to the server if not already, otherwise does nothing.
        /// </summary>
        public void startStreaming()
        {
            if(!bvhStreamer.IsStreaming())
                bvhStreamer.StartStreaming();
        }

        /// <summary>
        /// Stops streaming to the server if streaming, otherwise does nothing.
        /// </summary>
        public void stopStreaming()
        {
            if(bvhStreamer.IsStreaming())
                bvhStreamer.StopStreaming();
        }
        #endregion*/

        #region Find Functions
        /// <summary>
        /// Finds the X_Rotation of the specified bodyPart
        /// </summary>
        /// <param name="bodyPart">The name of the body part</param>
        /// <returns>The X_Rotation of the body part specified</returns>
        public double FindXRot(String bodyPart)
        {
            if(bodyPart.Equals("hip"))
            {
                return root.getXRot();
            }
            else if (bodyPart.Equals("abdomen"))
            {
                return root.abdomen.getXRot();
            }
            else if (bodyPart.Equals("chest"))
            {
                return root.abdomen.child.getXRot();
            }
            else if (bodyPart.Equals("neck"))
            {
                return root.abdomen.child.neck.getXRot();
            }
            else if (bodyPart.Equals("head"))
            {
                return root.abdomen.child.neck.child.getXRot();
            }
            else if (bodyPart.Equals("lCollar"))
            {
                return root.abdomen.child.lcollar.getXRot();
            }
            else if (bodyPart.Equals("lShldr"))
            {
                return root.abdomen.child.lcollar.child.getXRot();
            }
            else if (bodyPart.Equals("lForeArm"))
            {
                return root.abdomen.child.lcollar.child.child.getXRot();
            }
            else if (bodyPart.Equals("lHand"))
            {
                return root.abdomen.child.lcollar.child.child.child.getXRot();
            }
            else if (bodyPart.Equals("rCollar"))
            {
                return root.abdomen.child.rcollar.getXRot();
            }
            else if (bodyPart.Equals("rShldr"))
            {
                return root.abdomen.child.rcollar.child.getXRot();
            }
            else if (bodyPart.Equals("rForeArm"))
            {
                return root.abdomen.child.rcollar.child.child.getXRot();
            }
            else if (bodyPart.Equals("rHand"))
            {
                return root.abdomen.child.rcollar.child.child.child.getXRot();
            }
            else if (bodyPart.Equals("lThigh"))
            {
                return root.leftThigh.getXRot();
            }
            else if (bodyPart.Equals("lShin"))
            {
                return root.leftThigh.child.getXRot();
            }
            else if (bodyPart.Equals("lFoot"))
            {
                return root.leftThigh.child.child.getXRot();
            }
            else if (bodyPart.Equals("rThigh"))
            {
                return root.rightThigh.getXRot();
            }
            else if (bodyPart.Equals("rShin"))
            {
                return root.rightThigh.child.getXRot();
            }
            else if (bodyPart.Equals("rFoot"))
            {
                return root.rightThigh.child.child.getXRot();
            }
            return 0.0;
        }

        /// <summary>
        /// Finds the Y_Rotation of the specified bodyPart
        /// </summary>
        /// <param name="bodyPart">The name of the body part</param>
        /// <returns>The Y_Rotation of the body part specified</returns>
        public double FindYRot(String bodyPart)
        {
            if (bodyPart.Equals("hip"))
            {
                return root.getYRot();
            }
            else if (bodyPart.Equals("abdomen"))
            {
                return root.abdomen.getYRot();
            }
            else if (bodyPart.Equals("chest"))
            {
                return root.abdomen.child.getYRot();
            }
            else if (bodyPart.Equals("neck"))
            {
                return root.abdomen.child.neck.getYRot();
            }
            else if (bodyPart.Equals("head"))
            {
                return root.abdomen.child.neck.child.getYRot();
            }
            else if (bodyPart.Equals("lCollar"))
            {
                return root.abdomen.child.lcollar.getYRot();
            }
            else if (bodyPart.Equals("lShldr"))
            {
                return root.abdomen.child.lcollar.child.getYRot();
            }
            else if (bodyPart.Equals("lForeArm"))
            {
                return root.abdomen.child.lcollar.child.child.getYRot();
            }
            else if (bodyPart.Equals("lHand"))
            {
                return root.abdomen.child.lcollar.child.child.child.getYRot();
            }
            else if (bodyPart.Equals("rCollar"))
            {
                return root.abdomen.child.rcollar.getYRot();
            }
            else if (bodyPart.Equals("rShldr"))
            {
                return root.abdomen.child.rcollar.child.getYRot();
            }
            else if (bodyPart.Equals("rForeArm"))
            {
                return root.abdomen.child.rcollar.child.child.getYRot();
            }
            else if (bodyPart.Equals("rHand"))
            {
                return root.abdomen.child.rcollar.child.child.child.getYRot();
            }
            else if (bodyPart.Equals("lThigh"))
            {
                return root.leftThigh.getYRot();
            }
            else if (bodyPart.Equals("lShin"))
            {
                return root.leftThigh.child.getYRot();
            }
            else if (bodyPart.Equals("lFoot"))
            {
                return root.leftThigh.child.child.getYRot();
            }
            else if (bodyPart.Equals("rThigh"))
            {
                return root.rightThigh.getYRot();
            }
            else if (bodyPart.Equals("rShin"))
            {
                return root.rightThigh.child.getYRot();
            }
            else if (bodyPart.Equals("rFoot"))
            {
                return root.rightThigh.child.child.getYRot();
            }
            return 0.0;
        }

        /// <summary>
        /// Finds the Z_Rotation of the specified bodyPart
        /// </summary>
        /// <param name="bodyPart">The name of the body part</param>
        /// <returns>The Z_Rotation of the body part specified</returns>
        public double FindZRot(String bodyPart)
        {
            if (bodyPart.Equals("hip"))
            {
                return root.getZRot();
            }
            else if (bodyPart.Equals("abdomen"))
            {
                return root.abdomen.getZRot();
            }
            else if (bodyPart.Equals("chest"))
            {
                return root.abdomen.child.getZRot();
            }
            else if (bodyPart.Equals("neck"))
            {
                return root.abdomen.child.neck.getZRot();
            }
            else if (bodyPart.Equals("head"))
            {
                return root.abdomen.child.neck.child.getZRot();
            }
            else if (bodyPart.Equals("lCollar"))
            {
                return root.abdomen.child.lcollar.getZRot();
            }
            else if (bodyPart.Equals("lShldr"))
            {
                return root.abdomen.child.lcollar.child.getZRot();
            }
            else if (bodyPart.Equals("lForeArm"))
            {
                return root.abdomen.child.lcollar.child.child.getZRot();
            }
            else if (bodyPart.Equals("lHand"))
            {
                return root.abdomen.child.lcollar.child.child.child.getZRot();
            }
            else if (bodyPart.Equals("rCollar"))
            {
                return root.abdomen.child.rcollar.getZRot();
            }
            else if (bodyPart.Equals("rShldr"))
            {
                return root.abdomen.child.rcollar.child.getZRot();
            }
            else if (bodyPart.Equals("rForeArm"))
            {
                return root.abdomen.child.rcollar.child.child.getZRot();
            }
            else if (bodyPart.Equals("rHand"))
            {
                return root.abdomen.child.rcollar.child.child.child.getZRot();
            }
            else if (bodyPart.Equals("lThigh"))
            {
                return root.leftThigh.getZRot();
            }
            else if (bodyPart.Equals("lShin"))
            {
                return root.leftThigh.child.getZRot();
            }
            else if (bodyPart.Equals("lFoot"))
            {
                return root.leftThigh.child.child.getZRot();
            }
            else if (bodyPart.Equals("rThigh"))
            {
                return root.rightThigh.getZRot();
            }
            else if (bodyPart.Equals("rShin"))
            {
                return root.rightThigh.child.getZRot();
            }
            else if (bodyPart.Equals("rFoot"))
            {
                return root.rightThigh.child.child.getZRot();
            }
            return 0.0;
        }
        #endregion
    }
}
