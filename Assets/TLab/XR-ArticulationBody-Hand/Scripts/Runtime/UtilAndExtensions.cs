using System.Collections.Generic;
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
        public static void SetDriveRotation(this ArticulationBody body, Quaternion targetLocalRotation)
        {
            Vector3 target = body.ToTargetRotationInReducedSpace(targetLocalRotation);

            // assign to the drive targets...
            ArticulationDrive xDrive = body.xDrive;
            xDrive.target = target.x * Mathf.Deg2Rad;
            body.xDrive = xDrive;

            ArticulationDrive yDrive = body.yDrive;
            yDrive.target = target.y * Mathf.Deg2Rad;
            body.yDrive = yDrive;

            ArticulationDrive zDrive = body.zDrive;
            zDrive.target = target.z * Mathf.Deg2Rad;
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


        /// <summary>
        /// Assigns articulation body joint drive target rotations for the entire hierarchy of bodies.
        /// </summary>
        /// <param name="bodies"> array of hierarchy of bodies to apply rotations to </param>
        /// <param name="targetTransforms"> array of transforms these art bodies try to mimic </param>
        /// <param name="startIndexes"> is obtained by calling articulationBody.GetDofStartIndices(startIndecies) </param> 
        /// <param name="driveTargets"> is obtained by calling articulationBody.GetDriveTargets(driveTargets) </param>
        public static void SetDriveRotations(ref ArticulationBody[] bodies, ref Transform[] targetTransforms, ref List<int> startIndexes, ref List<float> driveTargets)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i].isRoot)
                    continue;
                int j = bodies[i].index;
                int index = startIndexes[j];

                bool rotateX = bodies[i].twistLock != ArticulationDofLock.LockedMotion;
                bool rotateY = bodies[i].swingYLock != ArticulationDofLock.LockedMotion;
                bool rotateZ = bodies[i].swingZLock != ArticulationDofLock.LockedMotion;

                Vector3 targets = bodies[i].ToTargetRotationInReducedSpace(targetTransforms[i].localRotation);

                int dofIndex = 0;
                if (rotateX)
                {
                    driveTargets[index] = targets.x * Mathf.Deg2Rad;
                    dofIndex++;
                }
                if (rotateY)
                {
                    driveTargets[index + dofIndex] = targets.y * Mathf.Deg2Rad;
                    dofIndex++;
                }
                if (rotateZ)
                {
                    driveTargets[index + dofIndex] = targets.z * Mathf.Deg2Rad;
                }
            }

            bodies[0].SetDriveTargets(driveTargets);
        }
    }
}
