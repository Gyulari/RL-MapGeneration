using Unity.MLAgents.Sensors;

public class HexagonSensorComponent : HexagonSensorComponentBase
{
    // Hexagon Sensor ����
    public override ISensor[] CreateSensors()
    {
        // Hexagon Buffer�� ���� �� ����
        if(HexagonBuffer == null) {            
            HexagonShape.Validate();
            HexagonBuffer = new ColorHexagonBuffer(HexagonShape);
        }

        return base.CreateSensors();
    }
}
