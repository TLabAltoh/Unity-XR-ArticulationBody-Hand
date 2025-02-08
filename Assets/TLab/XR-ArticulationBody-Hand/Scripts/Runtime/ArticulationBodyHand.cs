using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.Input;

namespace TLab.XR.ArticulationBodyHand
{
    [System.Serializable]
    public class MasterAndServant
    {
        public Transform master => m_master;
        public SlaveJoint slave => m_slave;

        public Transform m_master;
        public SlaveJoint m_slave;

        public void SetMaster(Transform master) => m_master = master;
        public void SetSlave(SlaveJoint slave) => m_slave = slave;
    }

    public class ArticulationBodyHand : MonoBehaviour
    {
        [SerializeField, Interface(typeof(IHand))]
        private Object m_hand;

        [SerializeField]
        private HandVisual m_handVisual;

        [System.Serializable]
        public class SerializableDrive
        {
            public float stiffness = 1e+5f;
            public float damping = 100;
            public float forceLimit = 100;
            public ArticulationDriveType driveType = ArticulationDriveType.Force;

            public ArticulationDrive ToArticulationDrive()
            {
                var drive = new ArticulationDrive();
                drive.stiffness = stiffness;
                drive.damping = damping;
                drive.forceLimit = forceLimit;
                drive.driveType = driveType;
                return drive;
            }
        }

        [System.Serializable]
        public class FingerDrive
        {
            public SerializableDrive thumb = new SerializableDrive();
            public SerializableDrive others = new SerializableDrive();
        }

        [SerializeField]
        private FingerDrive m_fingerDrive;

        [SerializeField]
        private MasterAndServant m_wristRoot;

        [SerializeField]
        private List<MasterAndServant> m_fingers;

        public Object hand => m_hand;

        public FingerDrive fingerDrive => m_fingerDrive;

        public HandVisual handVisual => m_handVisual;

        public MasterAndServant wristRoot => m_wristRoot;

        private bool m_started = false;

        public MasterAndServant GetFingerByIndex(int i) => m_fingers[i];

        public int GetFingerLength() => m_fingers.Count;

#if UNITY_EDITOR
        public void InitFingerByLength(int length)
        {
            m_fingers = new List<MasterAndServant>();
            for (int i = 0; i < length; i++)
                m_fingers.Add(new MasterAndServant());
        }

        public void SetWristRootRelationOfMaster2Slave(Transform master, SlaveJoint slave)
        {
            m_wristRoot.SetMaster(master);
            m_wristRoot.SetSlave(slave);
        }

        public List<MasterAndServant> GetFingers() => m_fingers;
#endif

        private void Start()
        {
            this.BeginStart(ref m_started);

            m_fingers.ForEach(t => t.slave.IgnoreCollisions());

            this.EndStart(ref m_started);
        }
    }
}
