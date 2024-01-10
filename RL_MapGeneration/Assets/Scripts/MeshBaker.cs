using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class MeshBaker : MonoBehaviour
{
    Mesh mesh;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

#if UNITY_EDITOR
    [ContextMenu("SaveMesh")]
    void saveMesh()
    {
        Debug.Log(mesh);
        string path = "Assets/MyMesh.asset";
        AssetDatabase.CreateAsset(mesh, AssetDatabase.GenerateUniqueAssetPath(path));
        AssetDatabase.SaveAssets();
    }
#endif
}
