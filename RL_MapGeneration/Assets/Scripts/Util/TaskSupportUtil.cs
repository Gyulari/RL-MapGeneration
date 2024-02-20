#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System;

namespace Gyulari.HexSensor.Util
{
    [System.Serializable]
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

    [System.Serializable]
    public struct HexCell_CenterPosInfoByRank
    {
        public int rank;
        public List<HexCell_CenterPosInfo> cell_Info;

        public HexCell_CenterPosInfoByRank(int rank, List<HexCell_CenterPosInfo> cell_Info)
        {
            this.rank = rank;
            this.cell_Info = cell_Info;
        }
    }

    public struct HexCell_CenterPosInfo
    {
        public int hexIdx;
        public Vector2 centerPos;

        public HexCell_CenterPosInfo(int hexIdx, Vector2 centerPos)
        {
            this.hexIdx = hexIdx;
            this.centerPos = centerPos;
        }
    }

    public struct HexCell_Pixels
    {
        public int resolution;
        public bool[] pixels;

        public HexCell_Pixels(int resolution, bool[] pixels)
        {
            this.resolution = resolution;
            this.pixels = pixels;
        }
    }

    public class TaskSupportUtil
    {
        TaskSupportUtil instance = new TaskSupportUtil();

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
                    IOUtil.ExportDataByJson(mInfos, "Config/MaterialInfos.json");
                }
                else {
                    Debug.LogWarning("Material asset does not exist in \"Asset/Materials/Tiles\". Please add a valid material to the directory.");
                }
            }
            else {
                Debug.LogWarning("Directory for generating ChannelLabel List does not exist. Please create the \"Asset/Materials/Tiles\" directory.");
            }
        }

        [MenuItem("HexagonSensor/Generate HexagonCell CenterPos List", false, 2)]
        static void GenerateHexCellCenterPosInfo()
        {
            const int maxRank = 9;
            
            List<HexCell_CenterPosInfoByRank> hexCell_CenterPosInfosByRank = new List<HexCell_CenterPosInfoByRank>();

            for(int r=1; r<=maxRank; r++) {
                List<HexCell_CenterPosInfo> hexCell_CenterPosInfos = new List<HexCell_CenterPosInfo>();
                var cellTuples = GenerateHexCellCentorPosList(r);

                foreach(var t in cellTuples) {
                    hexCell_CenterPosInfos.Add(new HexCell_CenterPosInfo(t.hexIdx, t.centerPos));
                }

                hexCell_CenterPosInfosByRank.Add(new HexCell_CenterPosInfoByRank(r, hexCell_CenterPosInfos));
            }

            IOUtil.ExportDataByJson(hexCell_CenterPosInfosByRank, "Config/HexCellCenterPosInfo.json");
        }

        private static List<(int hexIdx, Vector2 centerPos)> GenerateHexCellCentorPosList(int rank)
        {
            if(rank == 1) {
                return new List<(int, Vector2)>() { (0, Vector2.zero) };
            }

            int baseHexIdx = 1;
            Vector2 baseCenterPos = new Vector2((rank - 1) * 7, 0);

            for (int r=3; r<=rank; r++) {
                baseHexIdx += (r - 2) * 6;
            }            
            
            List<(int, Vector2)> hexCell_ByRank = new List<(int, Vector2)>();

            hexCell_ByRank.Add((baseHexIdx, baseCenterPos));

            for(int i = 0; i < 6; i++) {
                int xMovement = (i < 3) ? -1 : 1;
                int yMovement = (i == 0 || i == 5) ? -1 : 1;
                float xOffset = (i == 1 || i == 4) ? 7.0f : 3.5f;
                float yOffset = (i == 1 || i == 4) ? 0.0f : 6.0f;

                for(int j = 1; j < rank; j++) {
                    if (i == 5 && j == rank - 1)
                        break;

                    baseCenterPos = new Vector2(baseCenterPos.x + (xOffset * xMovement), baseCenterPos.y + (yOffset * yMovement));
                    hexCell_ByRank.Add((++baseHexIdx, baseCenterPos));
                }
            }

            return hexCell_ByRank;
        }

        [MenuItem("HexagonSensor/Generate HexagonCell Pixels", false, 3)]
        static void GenerateHexCellPixelsInfo()
        {
            const int maxResolution = 32;

            List<HexCell_Pixels> hexCell_PixelsInfo = new List<HexCell_Pixels>();

            for(int i=1; i<=maxResolution; i++) {
                hexCell_PixelsInfo.Add(new HexCell_Pixels(i, GenerateHexCellPixelsByResolution(i)));
            }

            IOUtil.ExportDataByJson(hexCell_PixelsInfo, "Config/HexCellPixelsInfo.json");
        }

        private static bool[] GenerateHexCellPixelsByResolution(int resolution)
        {
            // (width x resolution) x (height x resolution)
            int numPixels = 56 * (int)Mathf.Pow(resolution, 2);

            bool[] pixels = new bool[numPixels];

            for(int i=0; i < numPixels; i++) {
                pixels[i] = InHexagon(i, resolution);
            }

            return pixels;
        }

        private static bool InHexagon(int cell_Idx, int resolution)
        {
            int width = resolution * 7;

            // CenterPos of cell
            float x = cell_Idx % width + 0.5f;
            float y = (cell_Idx / width) + 0.5f;

            bool func1 = y < 4.0f / 7.0f * x + 6 * resolution;
            bool func2 = y < -4.0f / 7.0f * x + 10 * resolution;
            bool func3 = y > -4.0f / 7.0f * x + 2 * resolution;
            bool func4 = y > 4.0f / 7.0f * x - 2 * resolution;

            return func1 && func2 && func3 && func4;
        }
    }
}
#endif