#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Gyulari.HexSensor
{
    public struct MaterialInfo
    {
        public string name;
        public Color32 color;

        public MaterialInfo(string name, Color32 color)
        {
            this.name = name;
            this.color = color;
        }
    }

    public class TaskSupportUtil
    {
        [MenuItem("HexagonSensor/Generate ChannelLabel List", false, 1)]
        static void GenerateChannelLabels()
        {
            string dirPath = "Assets/Materials/Tiles";
            List<MaterialInfo> mInfos = new List<MaterialInfo>();

            if (Directory.Exists(dirPath)) {
                string[] guids = AssetDatabase.FindAssets("t:Material", new string[] {dirPath});

                if(guids.Length != 0) {
                    foreach(var g in guids) {
                        string assetPath = AssetDatabase.GUIDToAssetPath(g);
                        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                        mInfos.Add(new MaterialInfo(material.name, (Color32)material.color));
                    }
                }
                else {
                    Debug.LogWarning("There is no Material Assets");
                }
            }
            else {
                Debug.LogWarning("You must have correct directory. ");
            }

            foreach(var info in mInfos) {
                Debug.Log(info.name);
                Debug.Log(info.color);
            }
        }
    }
}
#endif