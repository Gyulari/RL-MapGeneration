using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using Gyulari.HexSensor;
using Gyulari.HexSensor.Util;
using Unity.MLAgents.Sensors;
using System.Runtime.CompilerServices;
using System;
using Unity.MLAgents.Actuators;

namespace Gyulari.HexSensor
{
    public class TestAgent : Agent
    {
        public event Action EpisodeBeginEvent;

        private HexagonBuffer m_SensorBuffer;

        // private bool m_IsTraining;
        // private List<int> m_ValidActions;

        HexagonSensorComponent sensorComp;

        public override void Initialize()
        {
            // m_IsTraining = Academy.Instance.IsCommunicatorOn;
            // m_ValidActions = new List<int>(5);

            sensorComp = GetComponent<HexagonSensorComponent>();

            sensorComp.ChannelLabels = new List<ChannelLabel>();
            List<MaterialInfo> mInfos = IOUtil.ImportDataByJson<MaterialInfo>("Config/MaterialInfos.json");

            foreach (var mInfo in mInfos)
                sensorComp.ChannelLabels.Add(new ChannelLabel(mInfo.name, mInfo.color));

            m_SensorBuffer = new ColorHexagonBuffer(sensorComp.ChannelLabels.Count, 6);
            sensorComp.HexagonBuffer = m_SensorBuffer;
        }

        public void WriteNode(int channel, int hexIdx, int value)
        {
            sensorComp.HexagonBuffer.Write(channel, hexIdx, value);
        }

        /*
        public override void OnEpisodeBegin()
        {
            EpisodeBeginEvent.Invoke();
        }

        public void StartEpisode(HexagonBuffer buffer)
        {

        }

        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            
        }
        */

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            var action = actionBuffers.DiscreteActions[0];
        }
    }
}


