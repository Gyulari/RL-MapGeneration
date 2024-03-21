using Gyulari.HexSensor;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using System;
using Gyulari.HexSensor.Util;
using UnityEngine;

namespace Gyulari.HexMapGeneration
{
    public class HexMapAgent : Agent
    {
        public event Action EpisodeBeginEvent;
        public event Action EpisodeEndEvent;
        public event Action<int, int, int> AddTileEvent;

        private HexagonBuffer m_SensorBuffer;
        private HexagonBuffer m_HexMapBuffer;

        private int curHexIdx;

        public override void Initialize()
        {
            m_SensorBuffer = new ColorHexagonBuffer(HexMap.NumChannels, Controller.m_MapRank);

            var sensorComp = GetComponent<HexagonSensorComponent>();
            sensorComp.HexagonBuffer = m_SensorBuffer;
            sensorComp.ChannelLabels = InitChannelLabels();
        }

        public override void OnEpisodeBegin()
        {
            EpisodeBeginEvent.Invoke();
        }

        public void StartEpisode(HexagonBuffer buffer)
        {
            m_HexMapBuffer ??= buffer;
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var channel = actionBuffers.DiscreteActions[0];
            var link = actionBuffers.DiscreteActions[1];

            m_SensorBuffer.Write(curHexIdx, channel, link);
            AddTileEvent.Invoke(curHexIdx, channel, link);
            curHexIdx++;

            if (curHexIdx == CalHexPropertyUtil.GetMaxHexCount(Controller.m_MapRank)) {
                curHexIdx = 0;
                EpisodeEndEvent.Invoke();
                EndEpisode();
            }
        }

        private List<ChannelLabel> InitChannelLabels()
        {
            List<ChannelLabel> channelLabels = new List<ChannelLabel>();
            List<MaterialInfo> mInfos = IOUtil.ImportDataByJson<MaterialInfo>("Config/MaterialInfos.json");

            foreach (var mInfo in mInfos) {
                channelLabels.Add(new ChannelLabel(mInfo.name, mInfo.color));
            }

            return channelLabels;
        }
    }
}