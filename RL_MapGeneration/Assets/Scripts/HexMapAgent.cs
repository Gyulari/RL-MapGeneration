using Gyulari.HexMapGeneration;
using Gyulari.HexSensor;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;
using System;
using Gyulari.HexSensor.Util;

namespace Gyulari.HexMapGeneration
{
    public class HexMapAgent : Agent
    {
        public event Action EpisodeBeginEvent;

        [SerializeField]
        private HexMap m_HexMap;

        private HexagonBuffer m_SensorBuffer;
        private HexagonBuffer m_HexMapBuffer;

        private int curHexIdx = 0;

        public override void Initialize()
        {
            m_SensorBuffer = new ColorHexagonBuffer(HexMap.NumChannels, 9);

            var sensorComp = GetComponent<HexagonSensorComponent>();
            sensorComp.HexagonBuffer = m_SensorBuffer;
            sensorComp.ChannelLabels = InitChannelLabels();
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

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var channelAction = actionBuffers.DiscreteActions[0];
            var link = actionBuffers.DiscreteActions[1];

            if(curHexIdx == CalHexPropertyUtil.GetMaxHexCount(HexMap.maxRank)) {
                return;
            }

            m_HexMap.AddTileNode(curHexIdx, channelAction, link);
            curHexIdx++;
        }

        /*
        public event Action EpisodeBeginEvent;

        [SerializeField] private HexMap m_HexMap;
        private HexagonBuffer m_SensorBuffer;
        private HexagonBuffer m_HexMapBuffer;

        private List<int> m_ValidActions;

        private bool m_IsTraining;
        private bool m_IsActive;

        public override void Initialize()
        {
            m_IsTraining = Academy.Instance.IsCommunicatorOn;
            m_ValidActions = new List<int>(5);

            // m_SensorBuffer = new ColorHexagonBuffer(m_HexMap.NumChannels, m_HexMapBuffer.Rank);
            m_SensorBuffer = new ColorHexagonBuffer(8, 9);

            var sensorComp = GetComponent<HexagonSensorComponent>();
            sensorComp.HexagonBuffer = m_SensorBuffer;
            sensorComp.ChannelLabels = m_HexMap.channelLabels;
        }

        public override void OnEpisodeBegin()
        {
            EpisodeBeginEvent.Invoke();
        }

        public void StartEpisode(HexagonBuffer buffer)
        {
            m_HexMapBuffer ??= buffer;
        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            m_ValidActions.Clear();
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var action = actionBuffers.DiscreteActions[0];
        }

        private void FixedUpdate()
        {
            // UpdateSensorBuffer();
            RequestDecision();
        }

        private void UpdateSensorBuffer()
        {
            m_SensorBuffer.Clear();

            m_SensorBuffer.Write(0, 0, m_HexMapBuffer.Read(1, 0));
        }
        */
    }
}