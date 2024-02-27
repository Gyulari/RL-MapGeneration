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

        m_SensorBuffer = new ColorHexagonBuffer(sensorComp.ChannelLabels.Count, 3);
        sensorComp.HexagonBuffer = m_SensorBuffer;

        sensorComp.HexagonBuffer.Write(0, 0, 1);

        int r = 0;

        // Rank 2
        for (r = 1; r <= 6; r++) {
            sensorComp.HexagonBuffer.Write(0, r, 2);
        }
        
        // Rank 3
        for (r = 7; r <= 18; r++) {
            sensorComp.HexagonBuffer.Write(0, r, 3);
        }
        /*
        // Rank 4
        for (r = 19; r <= 36; r++) {
            sensorComp.HexagonBuffer.Write(3, r, 4);
        }

        // Rank 5
        for (r = 37; r <= 60; r++) {
            sensorComp.HexagonBuffer.Write(4, r, 5);
        }

        // Rank 6
        for (r = 61; r <= 90; r++) {
            sensorComp.HexagonBuffer.Write(5, r, 6);
        }
        
        // Rank 7
        for (r = 91; r <= 126; r++) {
            sensorComp.HexagonBuffer.Write(6, r, 7);
        }

        // Rank 8
        for (r = 127; r <= 168; r++) {
            sensorComp.HexagonBuffer.Write(7, r, 8);
        }

        // Rank 9
        for (r = 169; r <= 216; r++) {
            sensorComp.HexagonBuffer.Write(1, r, 9);
        }
        */
    }
}
