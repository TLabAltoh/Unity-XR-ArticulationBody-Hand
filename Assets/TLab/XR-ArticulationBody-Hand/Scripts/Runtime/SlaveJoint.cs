using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TLab.XR.ArticulationBodyHand
{
    [RequireComponent(typeof(ArticulationBody))]
    public class SlaveJoint : MonoBehaviour
    {
        [SerializeField]
        private Transform m_master;

        [SerializeField]
        private float m_maxLinearVelocity = 5.0f;

        [SerializeField]
        private float m_maxAngularVelocity = 2.0f;

        [HideInInspector, SerializeField]
        private List<SlaveJoint> m_parentJoints = new List<SlaveJoint>();

        [HideInInspector, SerializeField]
        private Transform m_parent;

        [HideInInspector, SerializeField]
        private ArticulationBody m_articulation;

        [HideInInspector, SerializeField]
        private Quaternion m_baseRotation;

        [HideInInspector, SerializeField]
        private Collider m_collider;

        [HideInInspector, SerializeField]
        private List<Collider> m_ignoring = new List<Collider>();

        public void SetMaster(Transform master)
        {
            m_master = master;
        }

        public void IgnoreCollisions(Collider col, bool ignore)
        {
            m_ignoring.ForEach((c) => Physics.IgnoreCollision(c, col, ignore));
        }

        public void IgnoreCollisions()
        {
            if (m_collider != null)
                IgnoreCollisions(m_collider, true);
        }

        private void GetCollisions()
        {
            m_collider = GetComponent<Collider>();
            m_ignoring = GetComponentsInChildren<Collider>().Where(t => t != m_collider).ToList();
        }

        private void GetParentJoints()
        {
            m_parentJoints.Clear();

            m_parent = this.transform.parent;

            var parent = m_parent;

            var fingerJoint = parent.GetComponent<SlaveJoint>();
            while (fingerJoint != null)
            {
                m_parentJoints.Add(fingerJoint);

                parent = parent.parent;
                fingerJoint = parent.GetComponent<SlaveJoint>();
            }
        }

        private void FixedUpdate()
        {
            if (m_articulation.isRoot)
            {
                m_articulation.velocity = (m_master.position - transform.position) / Time.fixedDeltaTime;
                var rotdiff = m_master.rotation * Quaternion.Inverse(transform.rotation);
                rotdiff.ToAngleAxis(out var angle, out var axis);
                m_articulation.angularVelocity = angle * Mathf.Deg2Rad * axis / Time.fixedDeltaTime;
            }
            else
                m_articulation.SetDriveRotation(Quaternion.Inverse(m_baseRotation) * m_master.localRotation, float.MaxValue);

            // articulation.max**Velocity doesn't seem to work when the velocity is set manually. So I limit the velocity manually.
            // This was necessary to avoid breaking the articulation by penetrating the collider.
            m_articulation.velocity = Vector3.ClampMagnitude(m_articulation.velocity, m_maxLinearVelocity);
            m_articulation.angularVelocity = Vector3.ClampMagnitude(m_articulation.angularVelocity, m_maxAngularVelocity);
        }

#if UNITY_EDITOR
        public void Setup()
        {
            GetParentJoints();

            GetCollisions();
        }

        public enum HandJointType
        {
            None,
            FirstFinger,
            OtherFinger,
            WristRoot,
        };

        public void InitArticulation(HandJointType type, ArticulationDrive drive, bool isRightHand)
        {
            var articulation = gameObject.RequireComponent<ArticulationBody>();

            articulation.useGravity = false;

            articulation.anchorPosition = Vector3.zero;
            articulation.anchorRotation = isRightHand ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, 270, 180);

            articulation.linearDamping = 0;
            articulation.jointFriction = 0;
            articulation.angularDamping = 1;

            articulation.matchAnchors = true;

            articulation.automaticCenterOfMass = true;
            articulation.automaticInertiaTensor = true;

            articulation.maxLinearVelocity = m_maxLinearVelocity;
            articulation.maxAngularVelocity = m_maxAngularVelocity;
            articulation.maxDepenetrationVelocity = 3f;

            articulation.collisionDetectionMode = CollisionDetectionMode.Discrete;

            switch (type)
            {
                case HandJointType.None:
                    break;
                case HandJointType.FirstFinger:
                    {
                        // SphericalJoint's limmitation stiffness seems to be less rigid than RevoluteJoint. So I multiply the power here.
                        drive.stiffness *= 5;

                        articulation.jointType = ArticulationJointType.SphericalJoint;

                        articulation.swingYLock = ArticulationDofLock.LimitedMotion;
                        articulation.swingZLock = ArticulationDofLock.LockedMotion;
                        articulation.twistLock = ArticulationDofLock.LimitedMotion;

                        drive.lowerLimit = -10;
                        drive.upperLimit = +90;
                        drive.forceLimit = float.MaxValue;
                        drive.target = 0;
                        drive.targetVelocity = 0;
                        articulation.xDrive = drive;

                        drive.lowerLimit = -5;
                        drive.upperLimit = +5;
                        drive.forceLimit = float.MaxValue;
                        drive.target = 0;
                        drive.targetVelocity = 0;
                        articulation.yDrive = drive;
                    }
                    break;
                case HandJointType.OtherFinger:
                    {
                        articulation.jointType = ArticulationJointType.RevoluteJoint;
                        articulation.twistLock = ArticulationDofLock.LimitedMotion;

                        drive.lowerLimit = -10;
                        drive.upperLimit = +90;
                        drive.forceLimit = float.MaxValue;
                        drive.target = 0;
                        drive.targetVelocity = 0;
                        articulation.xDrive = drive;
                    }
                    break;
                case HandJointType.WristRoot:
                    break;
            }

            m_articulation = articulation;

            m_baseRotation = transform.localRotation;
        }

        void OnDrawGizmos()
        {
            if (m_articulation == null)
                return;
            const float GIZMO_RADIUS = 0.005f;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_articulation.worldCenterOfMass, GIZMO_RADIUS);
        }
#endif
    }
}
