using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class TestAgent : Agent
{
    private HexagonBuffer m_SensorBuffer;

    public override void Initialize()
    {
        m_SensorBuffer = new ColorHexagonBuffer(1, 10, 10);

        var sensorComp = GetComponent<HexagonSensorComponent>();
        sensorComp.HexagonBuffer = m_SensorBuffer;

        sensorComp.ChannelLabels = new List<ChannelLabel>()
        {
            new ChannelLabel("Wall", new Color32(0, 128, 255, 255)),
            new ChannelLabel("Food", new Color32(64, 255, 64, 255)),
            new ChannelLabel("Visited", new Color32(255, 64, 64, 255)),
        };
    }
}
