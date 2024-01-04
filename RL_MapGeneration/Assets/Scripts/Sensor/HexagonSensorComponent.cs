using JetBrains.Annotations;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HexagonSensorComponent : HexagonSensorComponentBase
{
    // Hexagon Sensor ����
    public override ISensor[] CreateSensors()
    {
        if(HexagonBuffer == null) {            
            HexagonShape.Validate();
            HexagonBuffer = new ColorHexagonBuffer(HexagonShape);
        }

        return base.CreateSensors();
    }
}
