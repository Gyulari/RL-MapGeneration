using UnityEngine;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;
using System.Collections;
using Unity.MLAgents;
using NaughtyAttributes;

namespace Gyulari.HexSensor
{
    // Abstract class for hexagon sensor component.
    public abstract class HexagonSensorComponentBase : SensorComponent, IDisposable
    {
        // Information about channel and rank.
        [SerializeField, ReadOnly]
        private string m_ObservationShape;

        #region Sensor Settings

        public string SensorName
        {
            get { return m_SensorName; }
            set { m_SensorName = value; }
        }
        [SerializeField]
        [Foldout("Basics")]
        private string m_SensorName = "HexagonSensor";

        // The number of stacked observations.
        // Enable to infer the temporal pattern of observations.
        // Note. Changing ObservationStacks at runtime does not affect sorting sensors.
        public int ObservationStacks
        {
            get { return m_ObservationStacks; }
            set { m_ObservationStacks = value; }
        }
        [SerializeField, Min(1)]
        [Foldout("Basics")]
        private int m_ObservationStacks = 1;

        // SensorCompressionType used by the sensor.
        public SensorCompressionType CompressionType
        {
            get { return m_CompressionType; }
            set { m_CompressionType = value; OnCompressionTypeChange(); }
        }
        [SerializeField]
        [Foldout("Basics")]
        private SensorCompressionType m_CompressionType = SensorCompressionType.PNG;

        // CompressionType sync between sensor and sensor component.
        private void OnCompressionTypeChange()
        {
            if (HasSensor) {
                m_HexagonSensor.CompressionType = m_CompressionType;
            }
        }

        // ObservationType of the sensor.
        // Note. Changing ObservationType after sensor is created has no effect.
        public ObservationType ObservationType
        {
            get { return m_ObservationType; }
            set { m_ObservationType = value; }
        }
        [SerializeField]
        [Foldout("Basics")]
        private ObservationType m_ObservationType = ObservationType.Default;

        #endregion

        // ChannelLabel list.
        // Utilized for debugging only.
        public List<ChannelLabel> ChannelLabels
        {
            get { return m_ChannelLabels; }
            set { m_ChannelLabels = new List<ChannelLabel>(value); }
        }
        [SerializeField, HideInInspector]
        protected List<ChannelLabel> m_ChannelLabels;

#if (UNITY_EDITOR)

        #region Debug Settings

        [SerializeField]
        [EnableIf("IsNotPlaying")]
        [Foldout("Debug")]
        [Label("Auto-Create Sensor")]
        [Tooltip("Option used to test stand-alone sensor component not attached to agent. " +
            "When the option is selected, the component creates sensor in Awake().")]
        private bool m_Debug_CreateSensorOnAwake;

        [SerializeField]
        [EnableIf(EConditionOperator.And, "IsNotPlaying", "m_Debug_CreateSensorOnAwake")]
        [Foldout("Debug")]
        [Label("Re-Initialize On Change")]
        [Tooltip("Option used to immediately see the effect of settings updates that would otherwise not be changeable at runtime. " +
            "When the option is selected, the component recreates sensor whenever the inspector settings change. " +
            "Only available on auto-create sensor ." +
            "Note. Scene GUI edits or tag changes will not trigger re-initialization and can result in erros.")]
        private bool m_Debug_CreateSensorOnValidate = true;

        [SerializeField]
        [EnableIf("m_Debug_CreateSensorOnAwake")]
        [Foldout("Debug")]
        [Label("Update Interval")]
        [Tooltip("FixedUpdate interval for auto-create sensor. ")]
        [Range(1, 20)]
        private int m_Debug_FrameInterval = 1;
        private int m_Debug_FrameCount;

        [SerializeField]
        [OnValueChanged("Debug_ToggleDrawHexagonBuffer")]
        [Foldout("Debug")]
        [Label("Draw Hexagon Buffer")]
        [Tooltip("Option used to draw the hexagon buffer contents (runtime only). " +
            "Disable and re-enable the toggle if visualization freezes.")]
        private bool m_Debug_DrawHexagonBuffer;
        private bool m_Debug_DrawHexagonBufferEnabled;    // Flag for tracking 'Draw Hexagon Buffer' toggle state.

        private bool IsNotPlaying => !Application.isPlaying;    // Inspector flag for NaughtyAttributes.

        [SerializeField]
        [ShowIf("m_Debug_DrawHexagonBuffer")]
        [Foldout("Debug")]
        private HexagonBufferDrawer m_Debug_HexagonBufferDrawer;
        private DebugChannelData m_Debug_ChannelData;

        #endregion

#endif

        #region Buffer and Shape

        // HexagonBuffer used for the sensor.
        // Note. Changing this after the sensor is created has no effect.
        public HexagonBuffer HexagonBuffer
        {
            get { return m_HexagonBuffer; }
            set { m_HexagonBuffer = value; HexagonShape = value.GetShape(); }
        }
        private HexagonBuffer m_HexagonBuffer;

        // Shape of the sensor's HexagonBuffer.
        // In HexagonSensorComponentBase, the shape is only used for
        // displaying the observation shape information in the inspector.
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

        // Update information about channel and rank to inspector.
        private void UpdateObservationShapeInfo()
        {
            m_ObservationShape = string.Format("{0} channel{1} x {2} rank",
                        m_HexagonShape.NumChannels,
                        m_HexagonShape.NumChannels == 1 ? "" : "s",
                        m_HexagonShape.Rank);
        }

        #endregion

        // Whether the HexagonSensor was created.
        public bool HasSensor
        {
            get { return m_HexagonSensor != null; }
        }

        // HexagonSensor instance.
        public HexagonSensor HexagonSensor
        {
            get { return m_HexagonSensor; }
        }
        protected HexagonSensor m_HexagonSensor;

        public override ISensor[] CreateSensors()
        {
#if (UNITY_EDITOR)
            if (Application.isPlaying) {
                EditorUtil.HideBehaviorParametersEditor();

                CoroutineUtil.Stop(this, m_Debug_OnSensorCreated);
                m_Debug_OnSensorCreated = new InvokeAfterFrames(
                    this, Debug_ToggleDrawHexagonBuffer).Coroutine;
            }
#endif

            m_HexagonSensor = new HexagonSensor(m_SensorName, m_HexagonBuffer, m_CompressionType, m_ObservationType);

            if (ObservationStacks > 1) {
                return new ISensor[]
                {
                new StackingSensor(m_HexagonSensor, m_ObservationStacks)
                };
            }

            return new ISensor[] { m_HexagonSensor };
        }
#if (UNITY_EDITOR)

        private IEnumerator m_Debug_OnSensorCreated;

        #region Debug Methods

        // Invoked on sensor craetion and on 'm_Debug_DrawHexagonBuffer' toggle change.
        private void Debug_ToggleDrawHexagonBuffer()
        {
            if (Debug_HasRuntimeSensor()) {
                if (m_Debug_DrawHexagonBuffer != m_Debug_DrawHexagonBufferEnabled) {
                    Debug_SetDrawHexagonBufferEnabled(m_Debug_DrawHexagonBuffer);
                }
            }
        }

        private void Debug_SetDrawHexagonBufferEnabled(bool enabled, bool standby = false)
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

            m_Debug_DrawHexagonBufferEnabled = enabled;
        }

        private DebugChannelData Debug_CreateChannelData()
        {
            // Create from settings.
            if (HasSensor && m_HexagonSensor.AutoDetectionEnabled) {
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
                // Debug hexagon drawer standby during sensor refresh.
                Debug_SetDrawHexagonBufferEnabled(false, true);

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
            Debug_SetDrawHexagonBufferEnabled(false);
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
}