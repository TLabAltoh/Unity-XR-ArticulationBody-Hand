using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

namespace TLab.XR.ArticulationBodyHand
{
    [System.Serializable]
    public class JointPair
    {
        public Transform master;
        public ArticulationBodyFingerJoint slave;
    }

    public class ArticulationBodyHand : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IHand))]
        private Object m_hand;

        [SerializeField]
        private HandVisual m_handVisual;

        [SerializeField]
        private JointPair m_jointRoot;

        [SerializeField]
        private List<JointPair> m_jointPairs;

        public Object hand => m_hand;

        public HandVisual handVisual => m_handVisual;

        public List<JointPair> jointPairs => m_jointPairs;

        private bool m_started = false;

        #region REGISTRY
        public static List<ArticulationBodyFingerJoint> registry = new List<ArticulationBodyFingerJoint>();

        public static void Register(ArticulationBodyFingerJoint joint)
        {
            if (!registry.Contains(joint))
                registry.Add(joint);
        }

        public static void Unregister(ArticulationBodyFingerJoint joint)
        {
            if (registry.Contains(joint))
                registry.Remove(joint);
        }
        #endregion REGISTRY

#if UNITY_EDITOR
        public void SetRoot(ArticulationBodyFingerJoint slave, Transform master)
        {
            m_jointRoot.slave = slave;
            m_jointRoot.master = master;
        }

        public void SetUp()
        {
            m_jointRoot.slave.BoneAwake();
            m_jointPairs.ForEach((j) => j.slave.BoneAwake());

            m_jointRoot.slave.BoneStart();
            m_jointPairs.ForEach((j) => j.slave.BoneStart());

            m_jointRoot.slave.BoneLateStart();
            m_jointPairs.ForEach((j) => j.slave.BoneLateStart());
        }
#endif

        private void Start()
        {
            this.BeginStart(ref m_started);

            this.EndStart(ref m_started);
        }
    }
}
