using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TLab.XR.ArticulationBodyHand
{
    [RequireComponent(typeof(ArticulationBody))]
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

        [SerializeField] private List<Collider> m_childColliders = new List<Collider>();

        [SerializeField]
        private List<Collider> m_ignoring = new List<Collider>();

        [Header("Parent Joints")]

        [SerializeField]
        private List<ArticulationBodyFingerJoint> m_parentArticulations = new List<ArticulationBodyFingerJoint>();

        [SerializeField]
        private Transform m_parent;

        [SerializeField]
        private ArticulationBody m_articulation;

        private float m_positionError = 0f;

        private float m_rotationError = 0f;

        private Transform m_anchor;

        private Transform m_goal;

        public List<Collider> colliders => m_childColliders;

        private void InitArticulation()
        {
            m_articulation.useGravity = false;
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

            var anchor = new GameObject(name + ".anchor").transform;

            anchor.position = transform.position;
            anchor.rotation = transform.rotation;
            anchor.parent = transform;

            m_anchor = anchor;
        }

        private void InstantiateGoal()
        {
            if (m_goal != null)
                return;

            var goal = new GameObject(name + ".goal").transform;

            goal.position = transform.position;
            goal.rotation = transform.rotation;
            goal.parent = transform.parent;

            m_goal = goal;
        }

        private Quaternion GetArticulationAxisWorldRotation()
        {
            return transform.rotation * m_articulation.anchorRotation;
        }

        private void IgnoreCollisions(Collider col, bool ignore)
        {
            if (ignore && !m_ignoring.Contains(col))
                m_ignoring.Add(col);
            else if (!ignore && m_ignoring.Contains(col))
                m_ignoring.Remove(col);

            m_childColliders.ForEach((c) => Physics.IgnoreCollision(c, col, ignore));
        }

        private void IgnoreCollisions()
        {
            m_ignoring.Clear();

            if (m_collider != null)
                IgnoreCollisions(m_collider, true);
        }

        private void GetCollisions()
        {
            m_collider = GetComponent<Collider>();
            m_childColliders = GetComponentsInChildren<Collider>().ToList();
        }

        private void GetParentJoints()
        {
            m_parentArticulations.Clear();

            m_parent = this.transform.parent;

            var parent = m_parent;

            var fingerJoint = parent.GetComponent<ArticulationBodyFingerJoint>();
            while (fingerJoint != null)
            {
                m_parentArticulations.Add(fingerJoint);

                if (parent.parent != null)
                {
                    parent = parent.parent;
                    fingerJoint = parent.GetComponent<ArticulationBodyFingerJoint>();
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

            InitArticulation();
        }

        public void BoneAwake()
        {
            m_articulation = Utility.RequireComponent<ArticulationBody>(gameObject);
        }

        void FixedUpdate()
        {
            m_goal.rotation = m_goal.parent.rotation * m_master.localRotation;

            //if (m_root)
            //    m_articulation.connectedAnchor = m_master.position;
            //else
            //    m_articulation.targetPosition = -m_master.position;

            var articulationAxisWorldRotation = GetArticulationAxisWorldRotation();

            var targetRotation = Quaternion.Inverse(articulationAxisWorldRotation);
            targetRotation *= m_anchor.rotation;
            targetRotation *= Quaternion.Inverse(m_goal.rotation);
            targetRotation *= articulationAxisWorldRotation;
            var targetEuler = targetRotation.eulerAngles;

            var xDrive = m_articulation.xDrive;
            xDrive.target = targetEuler.x;
            m_articulation.xDrive = xDrive;

            var yDrive = m_articulation.yDrive;
            yDrive.target = targetEuler.y;
            m_articulation.yDrive = yDrive;

            var zDrive = m_articulation.zDrive;
            zDrive.target = targetEuler.z;
            m_articulation.zDrive = zDrive;

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

        void Start()
        {
            InstantiateGoal();
            InstantiateAnchor();
        }

        void OnDrawGizmos()
        {
            if (m_articulation == null)
                return;

            const float GIZMO_RADIUS = 0.005f;
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(m_articulation.worldCenterOfMass, GIZMO_RADIUS);
        }
    }
}
