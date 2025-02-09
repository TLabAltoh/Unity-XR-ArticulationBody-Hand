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
        public class SerializableArticulationDrive
        {
            public float stiffness = 1e+5f;
            public float damping = 500;
            public ArticulationDriveType driveType = ArticulationDriveType.Force;

            public ArticulationDrive ToArticulationDrive()
            {
                var drive = new ArticulationDrive();
                drive.stiffness = stiffness;
                drive.damping = damping;
                drive.driveType = driveType;
                return drive;
            }

            public SerializableArticulationDrive(float stiffness, float damping, ArticulationDriveType driveType)
            {
                this.stiffness = stiffness;
                this.damping = damping;
                this.driveType = driveType;
            }

            public SerializableArticulationDrive() { }
        }

        [System.Serializable]
        public class SerializableFingerArticulationDrive
        {
            public SerializableArticulationDrive thumb = new SerializableArticulationDrive();
            public SerializableArticulationDrive others = new SerializableArticulationDrive();
        }

        [SerializeField]
        private SerializableFingerArticulationDrive m_fingerDrive;

        [SerializeField]
        private MasterAndServant m_wristRoot;

        [SerializeField]
        private List<MasterAndServant> m_fingers;

        public Hand hand => (Hand)m_hand;

        public HandVisual handVisual => m_handVisual;

        public MasterAndServant wristRoot => m_wristRoot;

        public SerializableFingerArticulationDrive fingerDrive => m_fingerDrive;

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
