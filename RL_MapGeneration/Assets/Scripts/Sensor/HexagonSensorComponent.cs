using JetBrains.Annotations;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HexagonSensorComponent : HexagonSensorComponentBase
{
    // Hexagon Sensor 생성
    public override ISensor[] CreateSensors()
    {
        // Hexagon Buffer가 없을 시 생성
        if(HexagonBuffer == null) {            
            HexagonShape.Validate();
            HexagonBuffer = new ColorHexagonBuffer(HexagonShape);
        }

        return base.CreateSensors();
    }
}
