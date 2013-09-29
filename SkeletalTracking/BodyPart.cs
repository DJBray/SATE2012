using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkeletalTracking
{
    using MatrixLibrary;
    using Microsoft.Kinect;
    using System.IO;

    public enum RelativeAxis{
        X, Y, Z
    };

    #region Body Part class
    public class BodyPart
    {
        #region Variable Declarations
        /// <summary>
        /// Relative x, y, and z axes for the body part.
        /// </summary>
        protected Vector3 x_p;
        protected Vector3 y_p;
        protected Vector3 z_p;

        /// <summary>
        /// Rotation angles around the x, y, and z relative axes
        /// </summary>
        protected double x_rot;
        protected double y_rot;
        protected double z_rot;

        /// <summary>
        /// The new Relative axes formed after performing the rotations on the
        /// relative axes.
        /// </summary>
        protected Vector3 x_p2;
        protected Vector3 y_p2;
        protected Vector3 z_p2;

        /// <summary>
        /// The vector that defines where the body part is
        /// </summary>
        public Vector3 bodyPartVector;
        #endregion

        #region Constructors
        /// <summary>
        /// Construct a new BodyPart using the x, y, and z axes as respective relative axes. (is normalized)
        /// </summary>
        public BodyPart()
        {
            x_p = new Vector3(1, 0, 0);
            y_p = new Vector3(0, 1, 0);
            z_p = new Vector3(0, 0, 1);

            x_rot = 0;
            y_rot = 0;
            z_rot = 0;

            x_p2 = new Vector3();
            y_p2 = new Vector3();
            z_p2 = new Vector3();

            bodyPartVector = new Vector3();
        }

        /// <summary>
        /// Constructs a new BodyPart with the specified body part vector
        /// </summary>
        /// <param name="bodyPartVector">The vector that defines the direction of the body part and it's location on screen</param>
        public BodyPart(Vector3 bodyPartVector)
        {
            this.bodyPartVector = bodyPartVector;

            x_p = new Vector3(1, 0, 0);
            y_p = new Vector3(0, 1, 0);
            z_p = new Vector3(0, 0, 1);

            x_rot = 0;
            y_rot = 0;
            z_rot = 0;

            x_p2 = new Vector3();
            y_p2 = new Vector3();
            z_p2 = new Vector3();

            bodyPartVector = new Vector3();
        }

        /// <summary>
        /// Constructs a new BodyPart using the given axes as relative axes. If axes
        /// are not normalized then they will be normalized in the constructor.
        /// </summary>
        /// <param name="x_p">Relative x-axis</param>
        /// <param name="y_p">Relative y-axis</param>
        /// <param name="z_p">Relative z-axis</param>
        public BodyPart(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            this.x_p = x_p;
            this.y_p = y_p;
            this.z_p = z_p;

            //Normalize relative axis vectors
            if(!x_p.IsUnitVector())
                x_p.Normalize();
            if (!y_p.IsUnitVector())
                y_p.Normalize();
            if (!z_p.IsUnitVector())
                z_p.Normalize();

            x_rot = 0;
            y_rot = 0;
            z_rot = 0;

            x_p2 = new Vector3();
            y_p2 = new Vector3();
            z_p2 = new Vector3();

            bodyPartVector = new Vector3();
        }

        /// <summary>
        /// Constructs a new Body Part vector with the relative axes defined with the specified vectors and
        /// initializes the body part with a body part vector.
        /// </summary>
        /// <param name="x_p">The starting relative x-axis</param>
        /// <param name="y_p">The starting relative y-axis</param>
        /// <param name="z_p">The starting relative z-axis</param>
        /// <param name="bodyPartVector">The vector that defines the direction of the body part and its location on screen</param>
        public BodyPart(Vector3 x_p, Vector3 y_p, Vector3 z_p, Vector3 bodyPartVector)
        {
            this.x_p = x_p;
            this.y_p = y_p;
            this.z_p = z_p;
            this.bodyPartVector = bodyPartVector;

            //Normalize relative axis vectors
            if (!x_p.IsUnitVector())
                x_p.Normalize();
            if (!y_p.IsUnitVector())
                y_p.Normalize();
            if (!z_p.IsUnitVector())
                z_p.Normalize();

            x_rot = 0;
            y_rot = 0;
            z_rot = 0;

            x_p2 = new Vector3();
            y_p2 = new Vector3();
            z_p2 = new Vector3();

            bodyPartVector = new Vector3();
        }
        #endregion

        #region Rotate about an arbitrary axis
        /// <summary>
        /// Forces the Body Part to calculate it's rotation values.
        /// </summary>
        /// <param name="x_p">The parent's relative x_p2 to be our x_p</param>
        /// <param name="y_p">The parent's relative y_p2 to be our y_p</param>
        /// <param name="z_p">The parent's relative z_p2 to be our z_p</param>
        public virtual void CalculateRotations(Vector3 x_p, Vector3 y_p, Vector3 z_p)
        {
            this.x_p = x_p;
            this.y_p = y_p;
            this.z_p = z_p;

            this.x_p2 = x_p;
            this.y_p2 = y_p;
            this.z_p2 = z_p;

            //To be implemented in child classes depending on its behavior
        }

        /// <summary>
        /// Applies the calculated rotations to the provided relative axis and assigns it to
        /// the new relative axis. The rotations are along the relative axes, not standard.
        /// </summary>
        /// <param name="axis">The relative axis to be rotated around</param>
        public void ApplyRotations(RelativeAxis axis, double angle)
        {
            if (axis == RelativeAxis.X)
            {
                y_p2 = rotateVector(y_p2, angle, x_p2);
                z_p2 = rotateVector(z_p2, angle, x_p2);
            }
            else if (axis == RelativeAxis.Y)
            {
                x_p2 = rotateVector(x_p2, angle, y_p2);
                z_p2 = rotateVector(z_p2, angle, y_p2);
            }
            else
            {
                x_p2 = rotateVector(x_p2, angle, z_p2);
                y_p2 = rotateVector(y_p2, angle, z_p2);
            }
        }

        private Vector3 rotateVector(Vector3 vector, double RotAngle, Vector3 axis) //Watch carefully...
        {
            Matrix R = new Matrix(3, 3);
            
            double dx = axis.X;
            double dy = axis.Y;
            double dz = axis.Z;

            double angle = ExtendedMath.degreesToRadians(RotAngle);

            Matrix V = new Matrix(3, 1);
            V[0, 0] = vector.X;
            V[1, 0] = vector.Y;
            V[2, 0] = vector.Z;

            //Rotation matrix about relative x-axis
            R[0, 0] = ((1 - Math.Cos(angle)) * dx * dx) + (Math.Cos(angle));
            R[0, 1] = ((1 - Math.Cos(angle)) * dx * dy) - (Math.Sin(angle) * dz);
            R[0, 2] = ((1 - Math.Cos(angle)) * dx * dz) + (Math.Sin(angle) * dy);
            R[1, 0] = ((1 - Math.Cos(angle)) * dx * dy) + (Math.Sin(angle) * dz);
            R[1, 1] = ((1 - Math.Cos(angle)) * dy * dy) + (Math.Cos(angle));
            R[1, 2] = ((1 - Math.Cos(angle)) * dy * dz) - (Math.Sin(angle) * dx);
            R[2, 0] = ((1 - Math.Cos(angle)) * dx * dz) - (Math.Sin(angle) * dy);
            R[2, 1] = ((1 - Math.Cos(angle)) * dy * dz) + (Math.Sin(angle) * dx);
            R[2, 2] = ((1 - Math.Cos(angle)) * dz * dz) + (Math.Cos(angle));

            //Rotate
            V = Matrix.Multiply(R, V);

         /* Update values
            dx = V[0, 0];
            dy = V[1, 0];
            dz = V[2, 0];

            //Rotation matrix about relative y-axis
            R[0, 0] = ((1 - Math.Cos(y_rot)) * dx * dx) + (Math.Cos(y_rot));
            R[0, 1] = ((1 - Math.Cos(y_rot)) * dx * dy) - (Math.Sin(y_rot) * dz);
            R[0, 2] = ((1 - Math.Cos(y_rot)) * dx * dz) + (Math.Sin(y_rot) * dy);
            R[1, 0] = ((1 - Math.Cos(y_rot)) * dx * dy) + (Math.Sin(y_rot) * dz);
            R[1, 1] = ((1 - Math.Cos(y_rot)) * dy * dy) + (Math.Cos(y_rot));
            R[1, 2] = ((1 - Math.Cos(y_rot)) * dy * dz) - (Math.Sin(y_rot) * dx);
            R[2, 0] = ((1 - Math.Cos(y_rot)) * dx * dz) - (Math.Sin(y_rot) * dy);
            R[2, 1] = ((1 - Math.Cos(y_rot)) * dy * dz) + (Math.Sin(y_rot) * dx);
            R[2, 2] = ((1 - Math.Cos(y_rot)) * dz * dz) + (Math.Cos(y_rot));

            //Rotate
            V = Matrix.Multiply(R, V);

            //Update values
            dx = V[0, 0];
            dy = V[1, 0];
            dz = V[2, 0];

            //Rotation matrix about relative z-axis
            R[0, 0] = ((1 - Math.Cos(z_rot)) * dx * dx) + (Math.Cos(z_rot));
            R[0, 1] = ((1 - Math.Cos(z_rot)) * dx * dy) - (Math.Sin(z_rot) * dz);
            R[0, 2] = ((1 - Math.Cos(z_rot)) * dx * dz) + (Math.Sin(z_rot) * dy);
            R[1, 0] = ((1 - Math.Cos(z_rot)) * dx * dy) + (Math.Sin(z_rot) * dz);
            R[1, 1] = ((1 - Math.Cos(z_rot)) * dy * dy) + (Math.Cos(z_rot));
            R[1, 2] = ((1 - Math.Cos(z_rot)) * dy * dz) - (Math.Sin(z_rot) * dx);
            R[2, 0] = ((1 - Math.Cos(z_rot)) * dx * dz) - (Math.Sin(z_rot) * dy);
            R[2, 1] = ((1 - Math.Cos(z_rot)) * dy * dz) + (Math.Sin(z_rot) * dx);
            R[2, 2] = ((1 - Math.Cos(z_rot)) * dz * dz) + (Math.Cos(z_rot));

            //Rotate
            V = Matrix.Multiply(R, V);

            //Return value as a vector*/
           return new Vector3(V[0, 0], V[1, 0], V[2, 0]);
        }
        #endregion

        #region Update the Body Part Vector
        /// <summary>
        /// The body part vector is updated to the current vector found from the skeleton.
        /// </summary>
        /// <param name="skel">The skeleton frame to be passed in.</param>
        public virtual void UpdateBodyPartVector(Skeleton skel, JointType start, JointType finish)
        {
            if ((skel.Joints[start].TrackingState == JointTrackingState.Tracked) &&
                (skel.Joints[finish].TrackingState == JointTrackingState.Tracked))
            {
                bodyPartVector.X = skel.Joints[finish].Position.X - skel.Joints[start].Position.X;
                bodyPartVector.Y = skel.Joints[finish].Position.Y - skel.Joints[start].Position.Y;
                bodyPartVector.Z = skel.Joints[finish].Position.Z - skel.Joints[start].Position.Z;
            }
            else
            {
                bodyPartVector.X = 0;
                bodyPartVector.Y = 0;
                bodyPartVector.Z = 0;

                x_rot = 0;
                y_rot = 0;
                z_rot = 0;
            }
        }
        #endregion

        #region Write to BVH
        /// <summary>
        /// Puts the body part in string form by outputting the x, z, and y rotations
        /// </summary>
        /// <returns>The Rotations of the body part</returns>
        public override string ToString()
        {
            if (x_rot.Equals(Double.NaN))
                x_rot = 0.0;
            if (y_rot.Equals(Double.NaN))
                y_rot = 0.0;
            if (z_rot.Equals(Double.NaN))
                z_rot = 0.0;
            return x_rot + " " + z_rot + " " + y_rot + " ";
        }

        /// <summary>
        /// Gets the current frame of rotations for a bvh file.
        /// </summary>
        /// <param name="apend">The appended String to store the frame in.</param>
        public virtual String GetCurrFrame(String apend)
        {
            String rotsInString = this.ToString();
            apend += rotsInString;
            return apend;
        }
        #endregion

        #region Getters
        public double getXRot()
        {
            return x_rot;
        }

        public double getYRot()
        {
            return y_rot;
        }

        public double getZRot()
        {
            return z_rot;
        }
        #endregion
    }
    #endregion

    #region Fully Rotational Joints Interface
    /// <summary>
    /// An interface to specify that a joint can rotate about all relative axes.
    /// </summary>
    public interface FullyRotational
    {
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        double calcX_rot();
        /// <summary>
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        double calcY_rot();
        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        double calcZ_rot();
    }
    #endregion

    #region Joints can rotate about all but the Y-Axis Interface
    /// <summary>
    /// An interface to specify that a joint can rotate about all but the relative Y-Axis
    /// </summary>
    public interface MoveButNoTwist 
    {
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        double calcTwist_rot();
        /// <summary>
        /// Calculates the rotation angle about the relative Z-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        double calcZ_rot();
    }
    #endregion

    #region Joints can rotate about all by the Z-Axis Interface
    /// <summary>
    /// An interface to specify that a joint can rotate about all but the Z-axis
    /// </summary>
    public interface MoveButNoZRot
    {
        /// <summary>
        /// Calculates the rotation angle about the relative X-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        double calcX_rot();
        /// <summary>
        /// Calculates the rotation angle about the relative Y-Axis given a vector
        /// </summary>
        /// <returns>The angle calculated</returns>
        double calcY_rot();
    }
    #endregion
}
