using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Oculus.Interaction.Input;

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

        private Hand m_mastersHand;

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

            if (m_collider != null)
            {
                var material = new PhysicMaterial("PhysicsHand");
                material.frictionCombine = PhysicMaterialCombine.Average;
                material.bounceCombine = PhysicMaterialCombine.Minimum;
                material.dynamicFriction = 1f;
                material.staticFriction = 1;

                m_collider.material = material;
                m_collider.sharedMaterial = material;

                m_ignoring = GetComponentsInChildren<Collider>().Where(t => t != m_collider).ToList();
            }
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

        private void OnEnable()
        {
            if (m_articulation.isRoot)
            {
                m_articulation.TeleportRoot(m_master.position, m_master.rotation);
                m_articulation.velocity = Vector3.zero;
                m_articulation.angularVelocity = Vector3.zero;
                m_articulation.WakeUp();
            }
        }

        private void Start()
        {
            if (m_articulation.isRoot)
                m_mastersHand = m_master.GetComponentInParent<Hand>();
        }

        private void FixedUpdate()
        {
            if (m_articulation.isRoot)
            {
                if (Vector3.Distance(m_master.position, m_mastersHand.transform.parent.position) > 0.1f)
                {
                    m_articulation.velocity = (m_master.position - transform.position) / Time.fixedDeltaTime;

                    var rotdiff = m_master.rotation * Quaternion.Inverse(transform.rotation);
                    rotdiff.ToAngleAxis(out var angle, out var axis);
                    m_articulation.angularVelocity = angle * Mathf.Deg2Rad * axis / Time.fixedDeltaTime;
                }
            }
            else
                m_articulation.SetDriveRotation(m_master.localRotation * Quaternion.Inverse(m_baseRotation), float.MaxValue);

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
            WristRoot,
            FixedJoint,
            RevoluteJoint,
            SphericalJoint,
        };

        public void InitArticulation(HandJointId id, HandJointType type, ArticulationDrive drive, bool isRightHand)
        {
            var articulation = gameObject.RequireComponent<ArticulationBody>();

            articulation.mass = 0.1f;
            articulation.useGravity = false;

            articulation.anchorPosition = Vector3.zero;
            articulation.anchorRotation = isRightHand ? Quaternion.Euler(0, 90, 0) : Quaternion.Euler(0, 270, 180);

            articulation.linearDamping = 0;
            articulation.jointFriction = 0;
            articulation.angularDamping = 1;

            articulation.solverIterations = 30;
            articulation.solverVelocityIterations = 20;

            articulation.matchAnchors = true;

            articulation.maxLinearVelocity = m_maxLinearVelocity;
            articulation.maxAngularVelocity = m_maxAngularVelocity;
            articulation.maxDepenetrationVelocity = 0.001f;

            articulation.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            switch (type)
            {
                case HandJointType.None:
                    break;
                case HandJointType.WristRoot:
                    articulation.mass *= 3;
                    if (articulation.isRoot)
                        articulation.immovable = false;
                    break;
                case HandJointType.FixedJoint:
                    articulation.jointType = ArticulationJointType.FixedJoint;
                    break;
                case HandJointType.RevoluteJoint:
                    {
                        articulation.jointType = ArticulationJointType.RevoluteJoint;
                        articulation.twistLock = ArticulationDofLock.LimitedMotion;

                        drive.lowerLimit = -10;
                        drive.upperLimit = +90;
                        drive.forceLimit = 0;
                        drive.target = 0;
                        drive.targetVelocity = 0;
                        articulation.xDrive = drive;
                    }
                    break;
                case HandJointType.SphericalJoint:
                    {
                        drive.stiffness *= 5;

                        articulation.jointType = ArticulationJointType.SphericalJoint;

                        articulation.swingYLock = ArticulationDofLock.LimitedMotion;
                        articulation.swingZLock = ArticulationDofLock.LockedMotion;
                        articulation.twistLock = ArticulationDofLock.LimitedMotion;

                        drive.lowerLimit = -10;
                        drive.upperLimit = +90;
                        drive.forceLimit = 0;
                        drive.target = 0;
                        drive.targetVelocity = 0;
                        articulation.xDrive = drive;

                        drive.lowerLimit = -30;
                        drive.upperLimit = +30;
                        drive.forceLimit = 0;
                        drive.target = 0;
                        drive.targetVelocity = 0;
                        articulation.yDrive = drive;
                    }
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
