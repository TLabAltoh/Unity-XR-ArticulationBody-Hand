using UnityEngine;
using UnityEditor;

namespace TLab.Project.Editor
{
    public class OutlineProcessor : SerializeableEditorWindow
    {
        [SerializeField, HideInInspector] private GameObject[] m_targets;

        [SerializeField, HideInInspector] private string m_meshSavePath;
        [SerializeField, HideInInspector] private string m_materialSavePath;

        [MenuItem("TLab/Project/Outline Processor")]
        private static void Init()
        {
            var window = (OutlineProcessor)GetWindow(typeof(OutlineProcessor));
            window.Show();
        }

        private const float ERROR = 1e-8f;

        private void DrawProperty(in SerializedObject @object, string name, string label)
        {
            var prop = @object.FindProperty(name);
            if (prop != null)
                EditorGUILayout.PropertyField(prop, new GUIContent(label), true);
        }

        private void OnGUI()
        {
            var @object = new SerializedObject(this);
            DrawProperty(@object, "m_targets", "Targets");
            @object.ApplyModifiedProperties();

            if (GUILayout.Button("Process Mesh") && PathUtil.SelectPath(ref m_meshSavePath, "Save Path"))
                ProcessMesh();
        }

        public void SaveMesh(Mesh mesh, MeshFilter meshFilter)
        {
            var path = m_meshSavePath + "/" + mesh.name + ".asset";
            var copyMesh = Instantiate(mesh);
            var copyMeshName = copyMesh.name.ToString();
            copyMesh.name = copyMeshName.Substring(0, copyMeshName.Length - "(Clone)".Length);
            var asset = AssetDatabase.LoadAssetAtPath<Mesh>(path);

            if (asset != null)
            {
                EditorUtility.CopySerialized(copyMesh, asset);
                meshFilter.sharedMesh = asset;
            }
            else
            {
                AssetDatabase.CreateAsset(copyMesh, path);
                meshFilter.sharedMesh = copyMesh;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Saved Process Mesh: " + path);
        }

        public void ProcessMesh(GameObject obj)
        {
            var meshFilters = obj.GetComponents<MeshFilter>();

            foreach (var meshFilter in meshFilters)
            {
                var mesh = Instantiate(meshFilter.sharedMesh);
                mesh.name = meshFilter.sharedMesh.name;

                var normals = mesh.normals;
                var vertices = mesh.vertices;
                var vertexCount = mesh.vertexCount;

                var softEdges = new Color[normals.Length];

                for (int i = 0; i < vertexCount; i++)
                {
                    var softEdge = Vector3.zero;

                    for (int j = 0; j < vertexCount; j++)
                    {
                        var v = vertices[i] - vertices[j];

                        if (v.sqrMagnitude < ERROR)
                            softEdge += normals[j];
                    }

                    softEdge.Normalize();
                    softEdges[i] = new Color(softEdge.x, softEdge.y, softEdge.z, 0);
                }

                mesh.name = mesh.name + "#Outline";
                mesh.colors = softEdges;
                meshFilter.sharedMesh = mesh;
                EditorUtility.SetDirty(meshFilter);

                SaveMesh(mesh, meshFilter);
            }

            EditorUtility.SetDirty(obj);
        }

        public void ProcessMesh()
        {
            foreach (var target in m_targets)
                ProcessMesh(target);
        }
    }
}