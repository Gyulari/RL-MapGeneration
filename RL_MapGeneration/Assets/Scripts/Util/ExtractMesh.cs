using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


public class ExtractMesh : MonoBehaviour
{
    Mesh mesh;

#if UNITY_EDITOR
    [ContextMenu("ExtractMesh")]
    void ExtractMeshFrom3DModel()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        string path = "Assets/Meshes/ExtractedMesh.asset";
        AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(path));
        AssetDatabase.SaveAssets();
    }
#endif
}
