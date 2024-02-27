using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gyulari.HexSensor;
using Gyulari.HexSensor.Util;
using UnityEngine.Diagnostics;
using UnityEditor;
using System.IO;

namespace Gyulari.HexMapGeneration
{
    public class HexMap : MonoBehaviour
    {
        List<ChannelLabel> channelLabels = new List<ChannelLabel>();
        List<MaterialInfo> mInfos = IOUtil.ImportDataByJson<MaterialInfo>("Config/MaterialInfos.json");

        public int NumChannels;

        public HexagonBuffer Buffer;

        public List<GameObject> tiles;

        private void Awake()
        {
            InitChannelLabels();
            InitHexagonTiles();
        }

        private void InitChannelLabels()
        {
            foreach (var mInfo in mInfos) {
                channelLabels.Add(new ChannelLabel(mInfo.name, mInfo.color));
            }

            NumChannels = channelLabels.Count;
        }

        private void InitHexagonTiles()
        {
            string dirPath = "Assets/Prefabs/Tiles";

            if (Directory.Exists(dirPath)) {
                string[] guids = AssetDatabase.FindAssets("t:Prefab Asset", new string[] { dirPath });

                if(guids.Length != 0) {
                    foreach(var g in guids) {
                        string assetPath = AssetDatabase.GUIDToAssetPath(g);
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        tiles.Add(obj);
                    }
                }
                else {
                    Debug.LogWarning("Prefab asset does not exist in \"Asset/Prefabs/Tiles\". Please add a valid hexaogn tile prefab to the directory.");
                }
            }
            else {
                Debug.LogWarning("Directory for generating hexagon tile list does not exist. Please create the \"Asset/Prefabs/Tiles\" directory.");
            }
        }
    }
}