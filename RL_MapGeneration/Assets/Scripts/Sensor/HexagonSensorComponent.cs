using Unity.MLAgents.Sensors;

namespace Gyulari.HexSensor
{
    // Implementation of a HexagonSensorCompoennt
    public class HexagonSensorComponent : HexagonSensorComponentBase
    {
        public override ISensor[] CreateSensors()
        {
            // Create HexagonBuffer
            if (HexagonBuffer == null) {
                HexagonShape.Validate();
                // HexagonBuffer = new ColorHexagonBuffer(HexagonShape);
                HexagonBuffer = new HexagonBuffer(HexagonShape);
            }

            return base.CreateSensors();
        }
    }
}