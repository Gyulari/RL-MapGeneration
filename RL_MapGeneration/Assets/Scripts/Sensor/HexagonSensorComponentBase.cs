using UnityEngine;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.MLAgents;

public abstract class HexagonSensorComponentBase : SensorComponent, IDisposable
{
    [SerializeField]
    private string m_ObservationShape;  // channel 및 rank 표시

    #region Basic Settings

    // 생성된 Sensor의 이름
    public string SensorName
    {
        get { return m_SensorName; }
        set { m_SensorName = value; }
    }
    [SerializeField]
    private string m_SensorName = "HexagonSensor";

    // Observation Stack 수
    public int ObservationStacks
    {
        get { return m_ObservationStacks; }
        set { m_ObservationStacks = value; }
    }
    [SerializeField]
    private int m_ObservationStacks = 1;

    // Sensor가 사용하는 압축 유형
    public SensorCompressionType CompressionType
    {
        get { return m_CompressionType; }
        set { m_CompressionType = value; OnCompressionTypeChange(); }
    }
    [SerializeField]
    private SensorCompressionType m_CompressionType = SensorCompressionType.PNG;

    // Sensor가 사용하는 압축 유형을 새로운 압축 유형으로 변경
    private void OnCompressionTypeChange()
    {
        if (HasSensor) {
            m_HexagonSensor.CompressionType = m_CompressionType;
        }
    }

    // Sensor의 Observation 유형
    public ObservationType ObservationType
    {
        get { return m_ObservationType; }
        set { m_ObservationType = value; }
    }
    [SerializeField]
    private ObservationType m_ObservationType = ObservationType.Default;

    #endregion

    // Channel Label 리스트
    public List<ChannelLabel> ChannelLabels
    {
        get { return m_ChannelLabels; }
        set { m_ChannelLabels = new List<ChannelLabel>(value); }
    }
    [SerializeField, HideInInspector]
    protected List<ChannelLabel> m_ChannelLabels;

    // Non-Editor floag for subcomponents.
    protected bool m_Debug_IsEnabled;

#if (UNITY_EDITOR)

    #region Debug Settings

    [SerializeField]
    [Tooltip("Whether this component should create its sensor on Awake(). Select " +
            "option to test a stand-alone sensor component not attached to an agent.")]
    private bool m_Debug_CreateSensorOnAwake;

    [SerializeField]
    [Tooltip("Whether to recreate the sensor everytime inspector settings change. " +
        "Select option to immediately see the effects of settings updates " +
        "that would otherwise not be changeable at runtime. \u2794 Only available " +
        "for auto-created sensor. Note that scene GUI edits or tag changes will " +
        "NOT trigger re-initialization and can result in errors.")]
    private bool m_Debug_CreateSensorOnValidate = true;

    [SerializeField]
    [Tooltip("FixedUpdate interval for auto-created sensor.")]
    [Range(1, 20)]
    private int m_Debug_FrameInterval = 1;
    private int m_Debug_FrameCount;

    [SerializeField]
    [Tooltip("Whether to draw the grid buffer contents (runtime only). " +
        "Disable and re-enable the toggle if visualization freezes.")]
    private bool m_Debug_DrawHexagonBuffer;
    // Flag for tracking toggle state.
    private bool m_Debug_DrawHexagonBufferEnabled;
    // Inspector flag for NaughtyAttributes.
    private bool IsNotPlaying => !Application.isPlaying;

    [SerializeField]
    private HexagonBufferDrawer m_Debug_HexagonBufferDrawer;
    private DebugChannelData m_Debug_ChannelData;

    #endregion

#endif

    #region Buffer and Shape

    // HexagonBuffer 
    public HexagonBuffer HexagonBuffer
    {
        get { return m_HexagonBuffer; }
        set { m_HexagonBuffer = value; HexagonShape = value.GetShape(); }
    }
    private HexagonBuffer m_HexagonBuffer;

    // HexagonBuffer Shape
    public HexagonBuffer.Shape HexagonShape
    {
        get { return m_HexagonShape; }
        set { m_HexagonShape = value; UpdateObservationShapeInfo(); }
    }
    [SerializeField, HideInInspector]
    private HexagonBuffer.Shape m_HexagonShape = new HexagonBuffer.Shape(1, 6);

    protected void UpdateHexagonChannelCount(int numChannels)
    {
        var shape = HexagonShape;
        shape.NumChannels = numChannels;
        HexagonShape = shape;
    }

    protected void UpdateHexagonSize(int rank)
    {
        var shape = HexagonShape;
        shape.Rank = rank;
        HexagonShape = shape;
    }

    // Observation Shape 정보 갱신
    private void UpdateObservationShapeInfo()
    {
        m_ObservationShape = string.Format("{0} channel{1} x {2} rank",
                    m_HexagonShape.NumChannels,
                    m_HexagonShape.NumChannels == 1 ? "" : "s",    // Channel이 다수인 경우에는 복수형 s 추가
                    m_HexagonShape.Rank);
    }

    #endregion 

    // Sensor 생성 여부
    public bool HasSensor
    {
        get { return m_HexagonSensor != null; }
    }

    // HexagonSensor
    public HexagonSensor HexagonSensor
    {
        get { return m_HexagonSensor; }
    }
    protected HexagonSensor m_HexagonSensor;

    // Sensor 생성
    public override ISensor[] CreateSensors()
    {
#if (UNITY_EDITOR)
        if (Application.isPlaying) {
            EditorUtil.HideBehaviorParametersEditor();

            CoroutineUtil.Stop(this, m_Debug_OnSensorCreated);
            m_Debug_OnSensorCreated = new InvokeAfterFrames(
                this, Debug_ToggleDrawGridBuffer).Coroutine;
        }
#endif

        // Hexagon Sensor 생성
        m_HexagonSensor = new HexagonSensor(m_SensorName, m_HexagonBuffer, m_CompressionType, m_ObservationType);

        // 단일 Observation Stack이 아닌 경우
        if(ObservationStacks > 1) {
            return new ISensor[]
            {
                new StackingSensor(m_HexagonSensor, m_ObservationStacks)
            };
        }

        // HexagonSensor 인스턴스를 가지는 ISensor 배열 반환
        return new ISensor[] { m_HexagonSensor };
    }
#if (UNITY_EDITOR)

    private IEnumerator m_Debug_OnSensorCreated;

    #region Debug Methods

    // Invoked on sensor creation and on m_Debug_DrawGridBuffer toggle change.
    private void Debug_ToggleDrawGridBuffer()
    {
        if (Debug_HasRuntimeSensor()) {
            if (m_Debug_DrawHexagonBuffer != m_Debug_DrawHexagonBufferEnabled) {
                Debug_SetDrawGridBufferEnabled(m_Debug_DrawHexagonBuffer);
            }
        }
    }

    private void Debug_SetDrawGridBufferEnabled(bool enabled, bool standby = false)
    {
        if (enabled) {
            m_Debug_ChannelData?.Dispose();
            m_Debug_ChannelData = Debug_CreateChannelData();
            m_HexagonSensor.UpdateEvent += m_Debug_HexagonBufferDrawer.OnSensorUpdate;
            ((IDebugable)m_HexagonSensor.Encoder)?.SetDebugEnabled(true, m_Debug_ChannelData);
            m_Debug_HexagonBufferDrawer.Enable(this, m_Debug_ChannelData, m_HexagonBuffer);
        }
        else {
            m_Debug_ChannelData?.Dispose();

            if (Debug_HasRuntimeSensor()) {
                m_HexagonSensor.UpdateEvent -= m_Debug_HexagonBufferDrawer.OnSensorUpdate;
                ((IDebugable)m_HexagonSensor.Encoder)?.SetDebugEnabled(false);
            }

            if (standby) {
                m_Debug_HexagonBufferDrawer.Standby();
            }
            else {
                m_Debug_HexagonBufferDrawer.Disable();
            }
        }

        m_Debug_IsEnabled = enabled;
        m_Debug_DrawHexagonBufferEnabled = enabled;
    }

    private DebugChannelData Debug_CreateChannelData()
    {
        // Create from settings.
        if (HasSensor && m_HexagonSensor.AutoDetectionEnabled) {
            // TODO Assuming IEncoder writes grid positions to DebugChannelData.
            // Might want to parameterize this at some point.
            return DebugChannelData.FromSettings(m_HexagonSensor.Encoder.Settings);
        }


        // Create from labels provided via ChannelLabels property.
        if (m_ChannelLabels != null && m_ChannelLabels.Count > 0) {
            return DebugChannelData.FromLabels(m_ChannelLabels);
        }


        // Create fallback labels.
        int n = m_HexagonShape.NumChannels;
        var labels = new List<ChannelLabel>(n);

        for (int i = 0; i < n; i++) {
            labels.Add(new ChannelLabel(
                "Observation " + i,
                Color.HSVToRGB(i / (float)n, 1, 1)));
        }

        return DebugChannelData.FromLabels(labels, false);
    }


    // Standalone sensor component.

    private void Debug_CreateSensorOnAwake()
    {
        var agent = GetComponentInParent<Agent>();

        if (m_Debug_CreateSensorOnAwake) {
            if (agent != null) {
                Debug.LogWarning("'Auto-Create Sensor' was selected, but this component is " +
                    $"attached to agent '{agent.name}'. The option is being disabled.");

                m_Debug_CreateSensorOnAwake = false;
            }
            else {
                if (!m_Debug_CreateSensorOnValidate) {
                    Debug.LogWarning("Sensor might not react properly or or throw errors " +
                        "when inspector values change. You can select 'Debug > Re-Initialize " +
                        "On Change' to always refresh the sensor.");
                }

                CreateSensors();
            }
        }
        else if (agent == null) {
            Debug.LogWarning("No agent was found on this or a parent gameobject. " +
                "You can select 'Debug > Auto-Create Sensor' to create a standalone sensor.");
        }
    }

    private void Debug_CreateSensorOnValidate()
    {
        if (Debug_HasRuntimeSensor() && m_Debug_CreateSensorOnAwake && m_Debug_CreateSensorOnValidate) {
            // Debug grid drawer standby during sensor refresh.
            Debug_SetDrawGridBufferEnabled(false, true);

            CreateSensors();
            m_Debug_FrameCount = 0;
        }
    }

    private void Debug_UpdateSensor()
    {
        if (m_Debug_CreateSensorOnAwake) {
            if (m_Debug_FrameCount % m_Debug_FrameInterval == 0) {
                // We ignore the StackingSensor for debug options.
                m_HexagonSensor.Update();
            }
            m_Debug_FrameCount++;
        }
    }

    private bool Debug_HasRuntimeSensor()
    {
        return Application.isPlaying && HasSensor;
    }


    private void Awake()
    {
        m_Debug_HexagonBufferDrawer.Disable();
        Debug_CreateSensorOnAwake();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) {
            Debug_CreateSensorOnValidate();
        }
        else {
            HandleValidate();
        }
    }

    protected virtual void HandleValidate() { }

    private void FixedUpdate()
    {
        Debug_UpdateSensor();
    }

    private void OnApplicationQuit()
    {
        Debug_SetDrawGridBufferEnabled(false);
    }

    #endregion

#endif

    private void Reset()
    {
        HandleReset();
    }

    protected virtual void HandleReset() { }

    private void OnDestroy()
    {
        Dispose();
    }

    public virtual void Dispose() { }
}
