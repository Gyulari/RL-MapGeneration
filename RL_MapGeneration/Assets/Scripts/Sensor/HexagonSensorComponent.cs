using Unity.MLAgents.Sensors;

public class HexagonSensorComponent : HexagonSensorComponentBase
{
    // Hexagon Sensor 积己
    public override ISensor[] CreateSensors()
    {
        // Hexagon Buffer啊 绝阑 矫 积己
        if(HexagonBuffer == null) {            
            HexagonShape.Validate();
            HexagonBuffer = new ColorHexagonBuffer(HexagonShape);
        }

        return base.CreateSensors();
    }
}
