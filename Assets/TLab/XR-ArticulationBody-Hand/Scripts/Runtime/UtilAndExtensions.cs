using UnityEngine;

namespace TLab.XR.ArticulationBodyHand
{
    public static class UtilAndExtensions
    {
        public static T RequireComponent<T>(this GameObject self) where T : Component
        {
            var result = self.GetComponent<T>();

            if (result == null)
                result = self.AddComponent<T>();

            return result;
        }

        /// <summary>
        /// Sets an articulation body drive target rotation to match a given targetLocalRotation.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="targetLocalRotation"></param>
        public static void SetDriveRotation(this ArticulationBody body, Quaternion targetLocalRotation, float forceLimit)
        {
            Vector3 target = body.ToTargetRotationInReducedSpace(targetLocalRotation);

            // assign to the drive targets...
            ArticulationDrive xDrive = body.xDrive;
            xDrive.forceLimit = forceLimit;
            xDrive.target = target.x;
            body.xDrive = xDrive;

            ArticulationDrive yDrive = body.yDrive;
            yDrive.forceLimit = forceLimit;
            yDrive.target = target.y;
            body.yDrive = yDrive;

            ArticulationDrive zDrive = body.zDrive;
            zDrive.forceLimit = forceLimit;
            zDrive.target = target.z;
            body.zDrive = zDrive;
        }

        /// <summary>
        /// Converts targetLocalRotation into reduced space of this articulation body.
        /// </summary>
        /// <param name="body"> ArticulationBody to apply rotation to </param>
        /// <param name="targetLocalRotation"> target's local rotation this articulation body is trying to mimic </param>
        /// <returns></returns>

        public static Vector3 ToTargetRotationInReducedSpace(this ArticulationBody body, Quaternion targetLocalRotation)
        {
            if (body.isRoot)
                return Vector3.zero;
            Vector3 axis;
            float angle;

            //Convert rotation to angle-axis representation (angles in degrees)
            targetLocalRotation.ToAngleAxis(out angle, out axis);

            // Converts into reduced coordinates and combines rotations (anchor rotation and target rotation)
            Vector3 rotInReducedSpace = Quaternion.Inverse(body.anchorRotation) * axis * angle;

            return rotInReducedSpace;
        }
    }
}
