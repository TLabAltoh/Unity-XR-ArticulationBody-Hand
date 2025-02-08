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

        [SerializeField]
        private float m_maxDepenetrationVelocity = 1.0f;

        [SerializeField]
        private List<SlaveJoint> m_parentJoints = new List<SlaveJoint>();

        [SerializeField]
        private Transform m_parent;

        [SerializeField]
        private Transform m_root;

        [SerializeField]
        private ArticulationBody m_articulation;

        [SerializeField]
        private Collider m_collider;

        [SerializeField]
        private List<Collider> m_ignoring = new List<Collider>();

        private float m_positionError = 0f;

        private float m_rotationError = 0f;

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

                if (fingerJoint.m_articulation.isRoot)
                    m_root = fingerJoint.m_master;

                parent = parent.parent;
                fingerJoint = parent.GetComponent<SlaveJoint>();
            }
        }

        void FixedUpdate()
        {
            if (m_articulation.isRoot)
            {
                m_articulation.velocity = (m_master.position - transform.position) / Time.fixedDeltaTime;
                var rotdiff = m_master.rotation * Quaternion.Inverse(transform.rotation);
                rotdiff.ToAngleAxis(out var angle, out var axis);
                m_articulation.angularVelocity = angle * Mathf.Deg2Rad * axis / Time.fixedDeltaTime;
            }
            else
                m_articulation.SetDriveRotation(m_master.localRotation);

            m_positionError = Vector3.Distance(m_articulation.transform.position, m_master.position);
            if (m_positionError < 0.00001f)
                m_positionError = 0.0f;
            if (m_positionError > 0.5f)
            {

            }

            m_articulation.velocity = Vector3.ClampMagnitude(m_articulation.velocity, m_maxLinearVelocity);
            m_articulation.angularVelocity = Vector3.ClampMagnitude(m_articulation.angularVelocity, m_maxAngularVelocity);
            m_articulation.maxDepenetrationVelocity = m_maxDepenetrationVelocity;
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
            Finger,
            WristRoot,
        };

        public void InitArticulation(HandJointType type, ArticulationDrive drive)
        {
            var articulation = gameObject.RequireComponent<ArticulationBody>();

            articulation.useGravity = false;

            articulation.SnapAnchorToClosestContact();

            articulation.linearDamping = 0;
            articulation.jointFriction = 0;
            articulation.angularDamping = 0;

            articulation.matchAnchors = true;

            articulation.automaticCenterOfMass = true;
            articulation.automaticInertiaTensor = true;

            switch (type)
            {
                case HandJointType.None:
                    break;
                case HandJointType.Finger:
                    {
                        articulation.jointType = ArticulationJointType.SphericalJoint;

                        drive.lowerLimit = articulation.xDrive.lowerLimit;
                        drive.upperLimit = articulation.xDrive.upperLimit;
                        drive.target = articulation.xDrive.target;
                        drive.targetVelocity = articulation.xDrive.targetVelocity;
                        articulation.xDrive = drive;

                        drive.lowerLimit = articulation.yDrive.lowerLimit;
                        drive.upperLimit = articulation.yDrive.upperLimit;
                        drive.target = articulation.yDrive.target;
                        drive.targetVelocity = articulation.yDrive.targetVelocity;
                        articulation.yDrive = drive;

                        drive.lowerLimit = articulation.zDrive.lowerLimit;
                        drive.upperLimit = articulation.zDrive.upperLimit;
                        drive.target = articulation.zDrive.target;
                        drive.targetVelocity = articulation.zDrive.targetVelocity;
                        articulation.zDrive = drive;
                    }
                    break;
                case HandJointType.WristRoot:
                    break;
            }

            m_articulation = articulation;
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
