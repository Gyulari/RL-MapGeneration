using UnityEngine;
using Gyulari.HexSensor.Util;

namespace Gyulari.HexMapGeneration
{
    public class Controller : MonoBehaviour
    {
        [SerializeField]
        private HexMapAgent m_Agent;

        [SerializeField]
        private HexMap m_HexMap;

        public static int m_MapRank = 9;

        private void Awake()
        {
            m_HexMap.Buffer = new HexagonBuffer(HexMap.NumChannels, m_MapRank);
            m_HexMap.m_CellPosList = IOUtil.ImportDataByJson<HexCell_CenterPosInfoByRank>("Config/HexCellCenterPosInfo.json");
            m_Agent.EpisodeBeginEvent += OnEpisodeBegin;
            m_Agent.EpisodeEndEvent += m_HexMap.ClearTiles;
            m_Agent.AddTileEvent += m_HexMap.AddTile;
        }

        private void OnEpisodeBegin()
        {
            m_Agent.StartEpisode(m_HexMap.Buffer);
        }

        private void OnApplicationQuit()
        {
            m_Agent.EpisodeBeginEvent -= OnEpisodeBegin;
            m_Agent.EpisodeEndEvent -= m_HexMap.ClearTiles;
            m_Agent.AddTileEvent -= m_HexMap.AddTile;
        }
    }
}