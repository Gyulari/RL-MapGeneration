public interface IEncoder
{
    IEncodingSettings Settings { get; set; }

    // HexagonBuffer HexagonBuffer { set; }

    void Encode(DetectionResult result);
}