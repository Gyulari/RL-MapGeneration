using UnityEngine;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;

public abstract class HexagonSensorComponentBase : SensorComponent, IDisposable
{
    [SerializeField]
    private string m_ObservationShape;

    public string SensorName
    {
        get { return m_SensorName; }
        set { m_SensorName = value; }
    }
    [SerializeField]
    // [Foldout("Basics")]
    [Tooltip("Name of the generated HexagonSensor")]
    private string m_SensorName = "HexagonSensor";

    public int ObservationStacks
    {
        get { return m_ObservationStacks; }
        set { m_ObservationStacks = value; }
    }
    [SerializeField]
    [Tooltip("The number of stacked observations. Enable stacking (value > 1) "
            + "if agents need to infer movement from observations.")]
    private int m_ObservationStacks = 1;

    public SensorCompressionType CompressionType
    {
        get { return m_CompressionType; }
        set { m_CompressionType = value; OnCompressionTypeChange(); }
    }
    [SerializeField]
    [Tooltip("The compression type used by the sensor.")]
    private SensorCompressionType m_CompressionType = SensorCompressionType.PNG;

    private void OnCompressionTypeChange()
    {
        if (HasSensor) {
            m_HexagonSensor.CompressionType = m_CompressionType;
        }
    }

    public ObservationType ObservationType
    {
        get { return m_ObservationType; }
        set { m_ObservationType = value; }
    }
    [SerializeField]
    [Tooltip("The observation type of the sensor.")]
    private ObservationType m_ObservationType = ObservationType.Default;

    public List<ChannelLabel> ChannelLabels
    {
        get { return m_ChannelLabels; }
        set { m_ChannelLabels = new List<ChannelLabel>(value); }
    }
    [SerializeField, HideInInspector]
    protected List<ChannelLabel> m_ChannelLabels;

    public HexagonBuffer HexagonBuffer
    {
        get { return m_HexagonBuffer; }
        set { m_HexagonBuffer = value; HexagonShape = value.GetShape(); }
    }
    private HexagonBuffer m_HexagonBuffer;

    public HexagonBuffer.Shape HexagonShape
    {
        get { return m_HexagonShape; }
        set { m_HexagonShape = value; UpdateObservationShapeInfo(); }
    }
    [SerializeField, HideInInspector]
    private HexagonBuffer.Shape m_HexagonShape = new HexagonBuffer.Shape(1, 6);

    private void UpdateObservationShapeInfo()
    {
        m_ObservationShape = string.Format("{0} channel{1} x {2} rank",
                    m_HexagonShape.NumChannels, m_HexagonShape.NumChannels == 1 ? "" : "s",
                    m_HexagonShape.Rank);
    }

    public bool HasSensor
    {
        get { return m_HexagonSensor != null; }
    }

    public HexagonSensor HexagonSensor
    {
        get { return m_HexagonSensor; }
    }
    protected HexagonSensor m_HexagonSensor;

    public override ISensor[] CreateSensors()
    {
        m_HexagonSensor = new HexagonSensor(m_SensorName, m_HexagonBuffer, m_CompressionType, m_ObservationType);

        if(ObservationStacks > 1) {
            return new ISensor[]
            {
                new StackingSensor(m_HexagonSensor, m_ObservationStacks)
            };
        }

        return new ISensor[] { m_HexagonSensor };
    }

    private void OnDestroy()
    {
        Dispose();
    }

    public virtual void Dispose() { }
}
