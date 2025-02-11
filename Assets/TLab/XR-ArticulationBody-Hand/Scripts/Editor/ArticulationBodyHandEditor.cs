using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Oculus.Interaction.Input;

namespace TLab.XR.ArticulationBodyHand.Editor
{
    [CustomEditor(typeof(ArticulationBodyHand))]
    public class ArticulationBodyHandEditor : UnityEditor.Editor
    {
        private SerializedProperty m_handProperty;

        private ArticulationBodyHand m_instance;

        private IHand Hand => m_handProperty.objectReferenceValue as IHand;

        private void OnEnable()
        {
            m_handProperty = serializedObject.FindProperty("m_hand");

            m_instance = (ArticulationBodyHand)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject);

            serializedObject.ApplyModifiedProperties();

            if (Hand == null)
            {
                EditorGUILayout.HelpBox("To automatically map joints, assign ihand to this instance.", MessageType.Warning, true);

                return;
            }

            EditorGUILayout.BeginHorizontal();

            var width = GUILayout.Width(Screen.width / 4);

            if (GUILayout.Button("Auto Map Joints", width))
            {
                AutoMapJoints(m_instance);

                EditorUtility.SetDirty(m_instance);
                EditorSceneManager.MarkSceneDirty(m_instance.gameObject.scene);
            }

            if (GUILayout.Button("Setup", width))
            {
                Setup(m_instance);

                EditorUtility.SetDirty(m_instance);
                EditorSceneManager.MarkSceneDirty(m_instance.gameObject.scene);
            }

            EditorGUILayout.EndHorizontal();
        }

        private static readonly string[] fbxHandSidePrefix = { "l_", "r_" };
        private static readonly string fbxHandBonePrefix = "b_";

        private static readonly string[] fbxHandBoneNames =
        {
            "wrist",
            "forearm_stub",
            "thumb0",
            "thumb1",
            "thumb2",
            "thumb3",
            "index1",
            "index2",
            "index3",
            "middle1",
            "middle2",
            "middle3",
            "ring1",
            "ring2",
            "ring3",
            "pinky0",
            "pinky1",
            "pinky2",
            "pinky3"
        };

        private static readonly string[] fbxHandFingerNames =
        {
            "thumb",
            "index",
            "middle",
            "ring",
            "pinky"
        };

        private static readonly HandJointId[] fbxSphericalHandJointIds =
        {
            HandJointId.HandThumb1,
            HandJointId.HandIndex1,
            HandJointId.HandRing1,
            HandJointId.HandPinky1,
            HandJointId.HandMiddle1,
        };

        private void Setup(ArticulationBodyHand hand, HandJointId id, SlaveJoint slave, SlaveJoint.HandJointType type, ArticulationDrive drive)
        {
            var child = slave.transform.GetChild(0);

            if (child != null)
            {
                var dist = Vector3.Distance(slave.transform.position, child.transform.position);

                if (type == SlaveJoint.HandJointType.WristRoot)
                {
                }
                else
                    slave.gameObject.RequireComponent<CapsuleCollider>();

                slave.InitArticulation(id, type, drive, (hand.hand).Handedness == Handedness.Right);

                if (slave.TryGetComponent(out CapsuleCollider col))
                {
                    col.height = dist * 1.5f;
                    col.direction = 0;  // X-Axis
                    col.radius = 0.008f;

                    switch (Hand.Handedness)
                    {
                        case Handedness.Left:
                            col.center = new Vector3(-dist / 2, 0, 0);
                            break;
                        case Handedness.Right:
                            col.center = new Vector3(dist / 2, 0, 0);
                            break;
                    }
                }

                EditorUtility.SetDirty(slave.gameObject);
            }
        }

        private SlaveJoint.HandJointType GetHandJointTypeById(HandJointId id)
        {
            if (id == HandJointId.HandThumb0 || id == HandJointId.HandPinky0)
                return SlaveJoint.HandJointType.FixedJoint;

            return fbxSphericalHandJointIds.Any(t => t == id) ? SlaveJoint.HandJointType.SphericalJoint : SlaveJoint.HandJointType.RevoluteJoint;
        }

        private void Setup(ArticulationBodyHand hand, HandJointId id)
        {
            if (hand.GetFingerByIndex((int)id - (int)HandJointId.HandThumb0) != null && hand.GetFingerByIndex((int)id - (int)HandJointId.HandThumb0).slave != null)
                Setup(hand, id, hand.GetFingerByIndex((int)id - (int)HandJointId.HandThumb0).slave, GetHandJointTypeById(id), (id >= HandJointId.HandThumb0 && id <= HandJointId.HandThumb3) ? m_instance.fingerDrive.thumb.ToArticulationDrive() : m_instance.fingerDrive.others.ToArticulationDrive());
        }

        private void Setup(ArticulationBodyHand hand)
        {
            for (var i = (int)HandJointId.HandThumb0; i <= (int)HandJointId.HandPinky3; ++i)
                Setup(hand, (HandJointId)i);

            Setup(hand, HandJointId.HandWristRoot, m_instance.wristRoot.slave, SlaveJoint.HandJointType.WristRoot, new ArticulationDrive());

            m_instance.wristRoot.slave.Setup();
            m_instance.GetFingers().ForEach((t) => t.slave.Setup());
        }

        private void AutoMapJoints(ArticulationBodyHand hand)
        {
            if (Hand == null)
                return;

            hand.InitFingerByLength((int)HandJointId.HandPinky3 - (int)HandJointId.HandThumb0 + 1);

            var fbxBoneName = FbxBoneNameFromHandJointId(HandJointId.HandWristRoot);

            var root = hand.transform.FindChildRecursive(fbxBoneName);
            if (root == null)
            {
                Debug.LogError($"GameObject is NULL: {fbxBoneName}");
                return;
            }

            SlaveJoint slave;
            Transform master;
            slave = root.GetComponent<SlaveJoint>();
            master = hand.handVisual.transform.FindChildRecursive(fbxBoneName);

            m_instance.SetWristRootRelationOfMaster2Slave(master, slave);
            slave.SetMaster(master);

            for (var i = (int)HandJointId.HandThumb0; i <= (int)HandJointId.HandPinky3; ++i)
            {
                fbxBoneName = FbxBoneNameFromHandJointId((HandJointId)i);
                hand.GetFingerByIndex(i - (int)HandJointId.HandThumb0).SetMaster(hand.handVisual.transform.FindChildRecursive(fbxBoneName));
                hand.GetFingerByIndex(i - (int)HandJointId.HandThumb0).SetSlave(hand.transform.FindChildRecursive(fbxBoneName).GetComponent<SlaveJoint>());

                hand.GetFingerByIndex(i - (int)HandJointId.HandThumb0).slave.SetMaster(hand.GetFingerByIndex(i - (int)HandJointId.HandThumb0).master);
            }
        }

        private string FbxBoneNameFromHandJointId(HandJointId handJointId)
        {
            if (handJointId >= HandJointId.HandThumbTip && handJointId <= HandJointId.HandPinkyTip)
                return fbxHandSidePrefix[(int)Hand.Handedness] + fbxHandFingerNames[(int)handJointId - (int)HandJointId.HandThumbTip] + "_finger_tip_marker";
            else
                return fbxHandBonePrefix + fbxHandSidePrefix[(int)Hand.Handedness] + fbxHandBoneNames[(int)handJointId];
        }
    }
}
