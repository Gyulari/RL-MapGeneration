using UnityEngine;
using Gyulari.HexSensor;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using Gyulari.HexSensor.Util;

namespace Gyulari.HexMapGeneration
{
    public class Controller : MonoBehaviour
    {
        [SerializeField]
        private HexMapAgent m_Agent;

        [SerializeField]
        private HexMap m_HexMap;

        [SerializeField]
        [Tooltip("Max rank of the HexMap")]
        private int m_MapRank = 9;

        private void Awake()
        {
            m_HexMap.Buffer = new HexagonBuffer(HexMap.NumChannels, m_MapRank);
            m_HexMap.tiles = InitHexagonTiles();
            m_HexMap.hexCellCenterPosInfo = IOUtil.ImportDataByJson<HexCell_CenterPosInfoByRank>("Config/HexCellCenterPosInfo.json");
            m_Agent.EpisodeBeginEvent += OnEpisodeBegin;
        }

        private void OnEpisodeBegin()
        {
            Debug.Log("Controller : EpisodeBeginEvent");
        }

        private void OnApplicationQuit()
        {
            m_Agent.EpisodeBeginEvent -= OnEpisodeBegin;
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