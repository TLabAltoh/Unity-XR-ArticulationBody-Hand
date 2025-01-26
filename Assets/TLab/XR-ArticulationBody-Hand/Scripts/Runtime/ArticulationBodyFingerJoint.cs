using System.Collections.Generic;
using UnityEngine;

namespace TLab.XR.ArticulationBodyHand
{
    [RequireComponent(typeof(ConfigurableJoint))]
    [RequireComponent(typeof(Rigidbody))]
    public class ArticulationBodyFingerJoint : MonoBehaviour
    {
        [SerializeField]
        private Transform m_master;

        [SerializeField]
        private bool m_root = false;

        [SerializeField]
        private float m_maxLinearVelocity = 5.0f;

        [SerializeField]
        private float m_maxAngularVelocity = 2.0f;

        [SerializeField]
        private float m_maxDepenetrationVelocity = 1.0f;

        [Header("Ignore Collider")]

        [SerializeField] private Collider m_collider;

        [SerializeField] private List<Collider> m_colliders = new List<Collider>();

        [SerializeField]
        private List<Collider> m_ignoring = new List<Collider>();

        [Header("Parent Joints")]

        [SerializeField]
        private List<ArticulationBodyFingerJoint> m_parentJoints = new List<ArticulationBodyFingerJoint>();

        [SerializeField]
        private Transform m_parent;

        [SerializeField]
        private Rigidbody m_rigidbody;

        [SerializeField]
        private ConfigurableJoint m_joint;

        private float m_positionError = 0f;

        private float m_rotationError = 0f;

        private Transform m_anchor;

        private Transform m_goal;

        // Property

        public ConfigurableJoint joint => m_joint;

        public Rigidbody rb => m_rigidbody;

        public List<Collider> colliders => m_colliders;

        private void InitJoint()
        {
            m_joint.autoConfigureConnectedAnchor = false;
            m_joint.configuredInWorldSpace = false;

            if (m_parent != null)
                m_joint.connectedBody = m_parent.GetComponent<Rigidbody>();

            m_joint.anchor = Vector3.zero;
            m_joint.connectedAnchor = this.transform.localPosition;
        }

        public void SetMaster(Transform master, bool root)
        {
            m_master = master;

            m_root = root;
        }

        private void InstantiateAnchor()
        {
            if (m_anchor != null)
                return;

            var anchor = new GameObject(this.gameObject.name + ".anchor").transform;

            anchor.position = this.transform.position;
            anchor.rotation = this.transform.rotation;
            anchor.parent = this.transform;

            m_anchor = anchor;
        }

        private void InstantiateGoal()
        {
            if (m_goal != null)
                return;

            var goal = new GameObject(this.gameObject.name + ".goal").transform;

            goal.position = this.transform.position;
            goal.rotation = this.transform.rotation;
            goal.parent = this.transform.parent;

            m_goal = goal;
        }

        private Quaternion GetJointAxisWorldRotation()
        {
            var xAxis = m_joint.axis;
            var zAxis = Vector3.Cross(m_joint.axis, m_joint.secondaryAxis);
            var yAxis = Vector3.Cross(zAxis, xAxis);

            var axisRot = Quaternion.LookRotation(zAxis, yAxis);

            if (m_joint.configuredInWorldSpace)
                return axisRot;
            else
                return m_joint.transform.rotation * axisRot;
        }

        private void IgnoreCollisions(Collider collider, bool ignore)
        {
            if (ignore && !m_ignoring.Contains(collider))
                m_ignoring.Add(collider);
            else if (!ignore && m_ignoring.Contains(collider))
                m_ignoring.Remove(collider);

            m_colliders.ForEach((c) => Physics.IgnoreCollision(c, collider, ignore));
        }

        private void IgnoreCollisions()
        {
            m_ignoring.Clear();

            if (m_collider != null)
                IgnoreCollisions(m_collider, true);
        }

        private void GetCollisions()
        {
            m_collider = m_rigidbody.GetComponent<Collider>();

            m_colliders.Clear();

            var colliders = m_rigidbody.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
                m_colliders.Add(collider);
        }

        private void GetParentJoints()
        {
            m_parentJoints.Clear();

            m_parent = this.transform.parent;

            var parent = m_parent;

            var joint = parent.GetComponent<ArticulationBodyFingerJoint>();
            while (joint != null)
            {
                m_parentJoints.Add(joint);

                if (parent.parent != null)
                {
                    parent = parent.parent;
                    joint = parent.GetComponent<ArticulationBodyFingerJoint>();
                }
            }
        }

        public void BoneLateStart()
        {
            IgnoreCollisions();
        }

        public void BoneStart()
        {
            GetParentJoints();

            GetCollisions();

            InitJoint();
        }

        public void BoneAwake()
        {
            m_rigidbody = Utility.RequireComponent<Rigidbody>(this.gameObject);

            m_joint = Utility.RequireComponent<ConfigurableJoint>(this.gameObject);
        }

        void FixedUpdate()
        {
            m_goal.rotation = m_goal.parent.rotation * m_master.localRotation;

            if (m_root)
                m_joint.connectedAnchor = m_master.position;
            else
                m_joint.targetPosition = -m_master.position;

            var jointFrameRotation = GetJointAxisWorldRotation();

            var targetRotation = Quaternion.Inverse(jointFrameRotation);
            targetRotation *= m_anchor.rotation;
            targetRotation *= Quaternion.Inverse(m_goal.rotation);
            targetRotation *= jointFrameRotation;

            m_joint.targetRotation = targetRotation;

            // Error
            m_positionError = Vector3.Distance(m_joint.transform.position, m_master.position);

            if (m_positionError < 0.00001f)
                m_positionError = 0.0f;

            // Stability
            if (m_positionError > 0.5f)
            {

            }

            m_rigidbody.velocity = Vector3.ClampMagnitude(m_rigidbody.velocity, m_maxLinearVelocity);
            m_rigidbody.angularVelocity = Vector3.ClampMagnitude(m_rigidbody.angularVelocity, m_maxAngularVelocity);
            m_rigidbody.maxDepenetrationVelocity = m_maxDepenetrationVelocity;
        }

        void Start()
        {
            InstantiateGoal();
            InstantiateAnchor();
        }

        void OnDrawGizmos()
        {
            const float GIZMO_RADIUS = 0.005f;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_rigidbody.worldCenterOfMass, GIZMO_RADIUS);
        }
    }
}
