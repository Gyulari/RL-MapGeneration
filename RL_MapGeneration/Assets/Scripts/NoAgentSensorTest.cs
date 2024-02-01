using Gyulari.HexSensor.Util;
using Gyulari.HexSensor;
using System.Collections.Generic;
using UnityEngine;

public class NoAgentSensorTest : MonoBehaviour
{
    private HexagonBuffer m_SensorBuffer;

    private void Start()
    {
        var sensorComp = GetComponent<HexagonSensorComponent>();

        sensorComp.ChannelLabels = new List<ChannelLabel>();
        List<MaterialInfo> mInfos = IOUtil.ImportDataByJson<MaterialInfo>("Config/MaterialInfos.json");

        foreach (var mInfo in mInfos)
            sensorComp.ChannelLabels.Add(new ChannelLabel(mInfo.name, mInfo.color));

        m_SensorBuffer = new ColorHexagonBuffer(sensorComp.ChannelLabels.Count, 6);
        sensorComp.HexagonBuffer = m_SensorBuffer;

        for(int i=0; i<64; i++) {
            sensorComp.HexagonBuffer.Write(0, i, 1);
        }

        /*
        sensorComp.HexagonBuffer.Write(0, 0, 1);
        sensorComp.HexagonBuffer.Write(1, 2, 2);
        sensorComp.HexagonBuffer.Write(2, 4, 3);
        sensorComp.HexagonBuffer.Write(3, 6, 4);       
        sensorComp.HexagonBuffer.Write(4, 8, 5);
        sensorComp.HexagonBuffer.Write(5, 10, 6);
        sensorComp.HexagonBuffer.Write(6, 12, 7);
        sensorComp.HexagonBuffer.Write(7, 14, 8);
        */
    }
}
