using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using Gyulari.HexSensor;
using Gyulari.HexSensor.Util;

public class TestAgent : Agent
{
    private HexagonBuffer m_SensorBuffer;

    public override void Initialize()
    {
        Debug.Log(IOUtil.ImportDataByJson<MaterialInfo>());

        m_SensorBuffer = new ColorHexagonBuffer(1, 6);

        var sensorComp = GetComponent<HexagonSensorComponent>();
        sensorComp.HexagonBuffer = m_SensorBuffer;

        sensorComp.ChannelLabels = new List<ChannelLabel>()
        {
            // new ChannelLabel("Start Tile", new Color32(0, 
            //new ChannelLabel("Start Tile", new Color());
            new ChannelLabel("Wall", new Color32(0, 128, 255, 255)),
            new ChannelLabel("Food", new Color32(64, 255, 64, 255)),
            new ChannelLabel("Visited", new Color32(255, 64, 64, 255)),
        };
    }
}
