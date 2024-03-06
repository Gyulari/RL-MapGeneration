using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gyulari.HexSensor;
using Gyulari.HexSensor.Util;
using UnityEngine.Diagnostics;
using UnityEditor;
using System.IO;
using System;

namespace Gyulari.HexMapGeneration
{
    public class HexMap : MonoBehaviour
    {
        struct Node
        {
            public int channel;
            public int link;

            public Node(int channel, int link)
            {
                this.channel = channel;
                this.link = link;
            }
        }

        public const int NumChannels = 8;
        public const int maxRank = 9;

        public HexMapAgent agent;
        public List<GameObject> tiles;
        public List<HexCell_CenterPosInfoByRank> hexCellCenterPosInfo;

        public HexagonBuffer Buffer;


        public void AddTileNode(int hexIdx, int channel, int link)
        {
            Node node = new Node(channel, link);

            // Buffer.Write(channel, hexIdx, link);
            agent.AddNodeToSensorBuffer(hexIdx, node.channel, node.link);

            var rank = CalHexPropertyUtil.GetRankByHexIdx(hexIdx);
            var hexNumInRank = CalHexPropertyUtil.GetHexNumberInRank(rank, hexIdx);

            Instantiate(tiles[channel],
                new Vector3(hexCellCenterPosInfo[rank-1].cell_Info[hexNumInRank].centerPos.x / 10.0f,
                            0.05f,
                            hexCellCenterPosInfo[rank-1].cell_Info[hexNumInRank].centerPos.y / 10.0f),
                Quaternion.Euler(90f, 30f, 0f));
        }

        /*
        public List<ChannelLabel> channelLabels = new List<ChannelLabel>();
        List<MaterialInfo> mInfos = IOUtil.ImportDataByJson<MaterialInfo>("Config/MaterialInfos.json");

        public const int NumChannels = 8;

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
                string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { dirPath });

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
        */
    }
}