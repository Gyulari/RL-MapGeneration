using Unity.MLAgents.Sensors;

namespace Gyulari.HexSensor
{
    // Implementation of a Hexagon Sensor Compoennt
    public class HexagonSensorComponent : HexagonSensorComponentBase
    {
        public override ISensor[] CreateSensors()
        {
            // Create HexagonBuffer
            if (HexagonBuffer == null) {
                HexagonShape.Validate();
                HexagonBuffer = new ColorHexagonBuffer(HexagonShape);
            }

            return base.CreateSensors();
        }
    }
}