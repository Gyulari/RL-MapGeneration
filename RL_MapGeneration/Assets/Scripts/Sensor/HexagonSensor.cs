using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Text;
using System;

public class HexagonSensor : ISensor, IDisposable
{
    public event Action UpdateEvent;
    public event Action ResetEvent;

    public IDetector Detector { get; private set; }

    public IEncoder Encoder { get; private set; }

    public bool AutoDetectionEnabled { get; private set; }

    public SensorCompressionType CompressionType
    {
        get { return m_CompressionType; }
        set { m_CompressionType = value; HandleCompressionType(); }
    }
    private SensorCompressionType m_CompressionType;

    private readonly string m_Name;
    private readonly HexagonBuffer m_HexagonBuffer;
    private readonly ObservationSpec m_ObservationSpec;
    private Texture2D m_PerceptionTexture;
    private List<byte> m_CompressedObs;

    public HexagonSensor(
        string name,
        HexagonBuffer buffer,
        SensorCompressionType compressionType,
        ObservationType observationType)
    {
        m_Name = name;

        buffer.GetShape().Validate();
        m_HexagonBuffer = buffer;

        m_CompressionType = compressionType;
        HandleCompressionType();

        m_ObservationSpec = ObservationSpec.Visual(
            m_HexagonBuffer.Rank, m_HexagonBuffer.Rank, m_HexagonBuffer.NumChannels, observationType);
    }

    protected void HandleCompressionType()
    {
        DestroyTexture();

        if (m_CompressionType == SensorCompressionType.PNG) {
            m_PerceptionTexture = new Texture2D(
                m_HexagonBuffer.Rank, m_HexagonBuffer.Rank, TextureFormat.RGB24, false);
            m_CompressedObs = new List<byte>(
                m_HexagonBuffer.Rank * m_HexagonBuffer.Rank * m_HexagonBuffer.NumChannels);
        }
    }

    public string GetName()
    {
        return m_Name;
    }

    public ObservationSpec GetObservationSpec()
    {
        return m_ObservationSpec;
    }

    public CompressionSpec GetCompressionSpec()
    {
        return new CompressionSpec(CompressionType);
    }

    public byte[] GetCompressedObservation()
    {
        m_CompressedObs.Clear();

        var colors = m_HexagonBuffer.GetLayerColors();
        for (int i = 0, n = colors.Length; i < n; i++) {
            m_PerceptionTexture.SetPixels32(colors[i]);
            m_CompressedObs.AddRange(m_PerceptionTexture.EncodeToPNG());
        }

        return m_CompressedObs.ToArray();
    }

    public int Write(ObservationWriter writer)
    {
        int numWritten = 0;
        int w = m_HexagonBuffer.Rank;
        int h = m_HexagonBuffer.Rank;
        int n = m_HexagonBuffer.NumChannels;

        for (int c = 0; c < n; c++){
            for (int x = 0; x < w; x++) {
                for (int y = 0; y < h; y++) {
                    writer[y, x, c] = m_HexagonBuffer.Read(c, x, y);
                    numWritten++;
                }
            }
        }

        return numWritten;
    }

    public virtual void Update()
    {
        if (AutoDetectionEnabled) {
            Detector.OnSensorUpdate();
            Encoder.Encode(Detector.Result);
        }

        UpdateEvent?.Invoke();
    }

    public virtual void Reset()
    {
        Detector?.OnSensorReset();
        ResetEvent?.Invoke();
    }

    public void Dispose()
    {
        DestroyTexture();
    }

    private void DestroyTexture()
    {
        if (m_PerceptionTexture is object) {
            if (Application.isEditor) {
                // Edit Mode tests complain if we use Destroy()
                UnityEngine.Object.DestroyImmediate(m_PerceptionTexture);
            }
            else {
                UnityEngine.Object.Destroy(m_PerceptionTexture);
            }

            m_PerceptionTexture = null;
        }
    }
}
