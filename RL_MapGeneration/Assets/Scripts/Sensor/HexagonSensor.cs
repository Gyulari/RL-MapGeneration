using UnityEngine;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.AI;
using Unity.MLAgents;
using Grpc.Core;
using System.Linq.Expressions;

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

        m_ObservationSpec = new ObservationSpec(
            new InplaceArray<int>(GetTextureSizeByRank().height,
                                  GetTextureSizeByRank().width,
                                  m_HexagonBuffer.NumChannels),
            new InplaceArray<DimensionProperty>(
                DimensionProperty.TranslationalEquivariance,
                DimensionProperty.TranslationalEquivariance,
                DimensionProperty.None),
            observationType
        );
    }

    public void EnableAutoDetection(IDetector detector, IEncoder encoder)
    {
        Detector = detector;
        Encoder = encoder;
        AutoDetectionEnabled = true;
    }

    protected void HandleCompressionType()
    {
        DestroyTexture();

        // 텍스처 크기가 지금 217로 되어있는데 여기서는 217x217으로 접근하기 때문으로 보임, 텍스처 크기를 grid format 변환할때처럼 기반으로 해서 수정해야할듯
        if(m_CompressionType == SensorCompressionType.PNG) {
            m_PerceptionTexture = new Texture2D(
                GetTextureSizeByRank().width, GetTextureSizeByRank().height, TextureFormat.RGB24, false);

            m_CompressedObs = new List<byte>(
                GetTextureSizeByRank().width * GetTextureSizeByRank().height * m_HexagonBuffer.NumChannels);
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
        int rank = m_HexagonBuffer.Rank;
        int numChannels = m_HexagonBuffer.NumChannels;
        int maxHexNum = m_HexagonBuffer.GetMaxHexCount(rank);
        
        for(int c = 0; c < numChannels; c++) {
            for(int h = 0; h < maxHexNum; h++) {
                writer[h, h, c] = m_HexagonBuffer.Read(c, h);
                numWritten++;
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
        if(m_PerceptionTexture is object) {
            if (Application.isEditor) {
                UnityEngine.Object.DestroyImmediate(m_PerceptionTexture);
            }
            else {
                UnityEngine.Object.Destroy(m_PerceptionTexture);
            }

            m_PerceptionTexture = null;
        }
    }

    private (int width, int height) GetTextureSizeByRank()
    {
        int width = 7 * 16 * (2 * m_HexagonBuffer.Rank - 1);
        int height = 4 * 16 * (3 * m_HexagonBuffer.Rank - 1);

        return (width, height);
    }

    /*
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
    */
}
