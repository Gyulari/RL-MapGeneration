public interface IDetector
{
    public DetectionResult Result { get; }

    public void OnSensorUpdate();

    public void OnSensorReset();
}