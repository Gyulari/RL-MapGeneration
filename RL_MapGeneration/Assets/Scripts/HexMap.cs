using System.Collections.Generic;
using UnityEngine;
using Gyulari.HexSensor.Util;
using Gyulari.HexSensor;
using UnityEditor;
using System.IO;

namespace Gyulari.HexMapGeneration
{
    public class HexMap : MonoBehaviour
    {
        public const int NumChannels = 9;

        public List<GameObject> m_Tiles;
        public List<HexCell_CenterPosInfoByRank> m_CellPosList;

        public HexagonBuffer Buffer;

        private void Awake()
        {
            m_Tiles = InitHexagonTiles();
        }

        public void AddTile(int hexIdx, int channel, int link)
        {
            Buffer.Write(hexIdx, channel, link);
            
            var cellRank = CalHexPropertyUtil.GetRankByHexIdx(hexIdx);
            var cellNumber = CalHexPropertyUtil.GetHexNumberInRank(cellRank, hexIdx);

            GameObject tile = Instantiate(m_Tiles[channel],
                new Vector3(m_CellPosList[cellRank - 1].cell_Info[cellNumber].centerPos.x / 10.0f,
                            0.05f,
                            m_CellPosList[cellRank - 1].cell_Info[cellNumber].centerPos.y / 10.0f),
                Quaternion.Euler(90f, 30f, 0f));

            tile.transform.SetParent(this.transform);
        }

        public void ClearTiles()
        {
            var tiles = gameObject.transform.GetComponentsInChildren<Transform>(true);

            foreach (var tile in tiles) {
                if (tile == this.transform) continue;
                tile.parent = null;
                Destroy(tile.gameObject);
            }
        }

        private List<GameObject> InitHexagonTiles()
        {
            List<GameObject> tiles = new List<GameObject>();

            string dirPath = "Assets/Prefabs/Tiles";

            if (Directory.Exists(dirPath)) {
                string[] guids = AssetDatabase.FindAssets("t:GameObject", new string[] { dirPath });

                if (guids.Length != 0) {
                    foreach (var g in guids) {
                        string assetPath = AssetDatabase.GUIDToAssetPath(g);
                        GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        tiles.Add(obj);
                    }
                    return tiles;
                }
                else {
                    Debug.LogWarning("Prefab asset does not exist in \"Asset/Prefabs/Tiles\". Please add a valid hexaogn tile prefab to the directory.");
                }
            }
            else {
                Debug.LogWarning("Directory for generating hexagon tile list does not exist. Please create the \"Asset/Prefabs/Tiles\" directory.");
            }

            return null;
        }
    }
}