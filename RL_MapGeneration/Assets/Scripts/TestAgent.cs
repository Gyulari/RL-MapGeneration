using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;
using Gyulari.HexSensor;
using Gyulari.HexSensor.Util;
using Mono.Cecil;

public class TestAgent : Agent
{
    private HexagonBuffer m_SensorBuffer;

    public override void Initialize()
    {
        var sensorComp = GetComponent<HexagonSensorComponent>();
        
        sensorComp.ChannelLabels = new List<ChannelLabel>();
        List<MaterialInfo> mInfos = IOUtil.ImportDataByJson<MaterialInfo>("Config/MaterialInfos.json");

        foreach(var mInfo in mInfos)
            sensorComp.ChannelLabels.Add(new ChannelLabel(mInfo.name, mInfo.color));

        m_SensorBuffer = new ColorHexagonBuffer(sensorComp.ChannelLabels.Count, 6);
        sensorComp.HexagonBuffer = m_SensorBuffer;
    }
}
