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
    public struct HexCell_InfoByRank
    {
        public int rank;
        public List<HexCell_Info> cell_Info;

        public HexCell_InfoByRank(int rank, List<HexCell_Info> cell_Info)
        {
            this.rank = rank;
            this.cell_Info = cell_Info;
        }
    }

    public struct HexCell_Info
    {
        public int hexIdx;
        public Vector2 centerPos;

        public HexCell_Info(int hexIdx, Vector2 centerPos)
        {
            this.hexIdx = hexIdx;
            this.centerPos = centerPos;
        }
    }

    public struct HexagonCellPixels
    {
        public int resolution;
        public bool[] pixels;

        public HexagonCellPixels(int resolution, bool[] pixels)
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
            int maxRank = 9;
            
            List<HexCell_InfoByRank> hexCell_InfosByRank = new List<HexCell_InfoByRank>();

            for(int r=1; r<=maxRank; r++) {
                List<HexCell_Info> hexCell_Infos = new List<HexCell_Info>();
                var cellTuples = GenerateHexCellCentorPosList(r);

                foreach(var t in cellTuples) {
                    hexCell_Infos.Add(new HexCell_Info(t.hexIdx, t.centerPos));
                }

                hexCell_InfosByRank.Add(new HexCell_InfoByRank(r, hexCell_Infos));
            }

            IOUtil.ExportDataByJson(hexCell_InfosByRank, "Config/HexCellCenterPosInfo.json");
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

                for(int j = 1; j<rank; j++) {
                    if (i == 5 && j == rank - 1)
                        break;

                    baseCenterPos = new Vector2(baseCenterPos.x + (xOffset * xMovement), baseCenterPos.y + (yOffset * yMovement));
                    hexCell_ByRank.Add((++baseHexIdx, baseCenterPos));
                }
            }

            return hexCell_ByRank;
        }

        [MenuItem("HexagonSensor/Generate HexagonCell Pixels", false, 3)]
        static void GenerateHexagonCellPixels()
        {
            int maxResolution = 32;

            List<HexagonCellPixels> hexCell_PixelsInfo = new List<HexagonCellPixels>();

            for(int i=1; i<=maxResolution; i++) {
                hexCell_PixelsInfo.Add(new HexagonCellPixels(i, HexagonCellPixelsByResolution(i)));
            }

            IOUtil.ExportDataByJson(hexCell_PixelsInfo, "Config/HexCellPixelsInfo.json");

            /*
            List<HexCell_InfoByRank> hexCell_InfosByRank = new List<HexCell_InfoByRank>();

            for (int r = 1; r <= maxRank; r++) {
                List<HexCell_Info> hexCell_Infos = new List<HexCell_Info>();
                var cellTuples = GenerateHexCellCentorPosList(r);

                foreach (var t in cellTuples) {
                    hexCell_Infos.Add(new HexCell_Info(t.hexIdx, t.centerPos));
                }

                hexCell_InfosByRank.Add(new HexCell_InfoByRank(r, hexCell_Infos));
            }

            IOUtil.ExportDataByJson(hexCell_InfosByRank, "Config/HexCellCenterPosInfo.json");
            */
        }

        private static bool[] HexagonCellPixelsByResolution(int resolution)
        {
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
            int height = resolution * 8;

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