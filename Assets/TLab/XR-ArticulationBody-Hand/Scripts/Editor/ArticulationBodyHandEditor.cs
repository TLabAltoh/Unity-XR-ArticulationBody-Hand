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

            InitializeSkeleton(m_instance);

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

            if (GUILayout.Button("Auto Fit Collider", width))
            {
                AutoFitCollider(m_instance);

                EditorUtility.SetDirty(m_instance);
                EditorSceneManager.MarkSceneDirty(m_instance.gameObject.scene);
            }

            if (GUILayout.Button("Set Up In Editor", width))
            {
                m_instance.SetUp();

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

        private void InitializeSkeleton(ArticulationBodyHand hand)
        {
            if (hand.jointPairs.Count == 0)
            {
                for (var i = (int)HandJointId.HandThumb0; i <= (int)HandJointId.HandPinky3; ++i)
                    hand.jointPairs.Add(null);
            }
        }

        private void AutoFitCollider(ArticulationBodyHand hand)
        {
            var start = (int)HandJointId.HandThumb0;
            var end = (int)HandJointId.HandPinky3;
            for (var i = start; i <= end; ++i)
            {
                if (hand.jointPairs[i - start] != null && hand.jointPairs[i - start].slave != null)
                {
                    var slave = hand.jointPairs[i - start].slave;

                    var child = slave.transform.GetChild(0);

                    if (child != null)
                    {
                        var dist = Vector3.Distance(slave.transform.position, child.transform.position);

                        Utility.RequireComponent<CapsuleCollider>(slave.transform);

                        if (slave.TryGetComponent(out CapsuleCollider col))
                        {
                            col.height = dist;
                            col.direction = 0;  // X-Axis
                            col.radius = 0.01f;

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
                    }
                }
            }
        }

        private void AutoMapJoints(ArticulationBodyHand hand)
        {
            if (Hand == null)
            {
                InitializeSkeleton(hand);
                return;
            }

            var rootTransform = hand.transform;

            var fbxBoneName = FbxBoneNameFromHandJointId(HandJointId.HandWristRoot);

            var root = rootTransform.FindChildRecursive(fbxBoneName);

            if (root == null)
            {
                Debug.LogError($"GameObject is NULL: {fbxBoneName}");
                return;
            }

            var rootSlave = root.GetComponent<ArticulationBodyFingerJoint>();
            var rootMaster = hand.handVisual.transform.FindChildRecursive(fbxBoneName);

            m_instance.SetRoot(rootSlave, rootMaster);

            rootSlave.SetMaster(rootMaster, true);

            var start = (int)HandJointId.HandThumb0;
            var end = (int)HandJointId.HandPinky3;
            for (var i = start; i <= end; ++i)
            {
                fbxBoneName = FbxBoneNameFromHandJointId((HandJointId)i);
                hand.jointPairs[i - start].slave = rootTransform.FindChildRecursive(fbxBoneName).GetComponent<ArticulationBodyFingerJoint>();
                hand.jointPairs[i - start].master = hand.handVisual.transform.FindChildRecursive(fbxBoneName);

                hand.jointPairs[i - start].slave.SetMaster(hand.jointPairs[i - start].master, false);
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
