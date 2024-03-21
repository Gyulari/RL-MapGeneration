using UnityEngine;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;
using System.Collections;
using NaughtyAttributes;
using Gyulari.HexSensor.Util;

namespace Gyulari.HexSensor
{
    // Base class for HexagonSensorComponent
    public abstract class HexagonSensorComponentBase : SensorComponent, IDisposable
    {
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

        // ObservationType of the sensor.
        public ObservationType ObservationType
        {
            get { return m_ObservationType; }
            set { m_ObservationType = value; }
        }
        [Foldout("Basics")]
        private ObservationType m_ObservationType = ObservationType.Default;

        #endregion


        #region Channel Label

        // ChannelLabel list.
        // Utilized for debugging only.
        public List<ChannelLabel> ChannelLabels
        {
            get { return m_ChannelLabels; }
            set { m_ChannelLabels = new List<ChannelLabel>(value); }
        }
        [SerializeField, HideInInspector]
        protected List<ChannelLabel> m_ChannelLabels;

        #endregion


#if (UNITY_EDITOR)

        #region Debug Settings

        [SerializeField]
        [OnValueChanged("Debug_ToggleDrawHexagonBuffer")]
        [Foldout("Debug")]
        [Label("Draw Hexagon Buffer")]
        [Tooltip("Option used to draw the hexagon buffer contents (runtime only). " +
            "Disable and re-enable the toggle if visualization freezes.")]
        private bool m_Debug_DrawHexagonBuffer;
        private bool m_Debug_DrawHexagonBufferEnabled;    // Flag for tracking 'Draw Hexagon Buffer' toggle state.

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

        // Update information about channel and rank to inspector.
        private void UpdateObservationShapeInfo()
        {
            m_ObservationShape = string.Format("{0} channel{1} x {2} rank",
                        m_HexagonShape.NumChannels,
                        m_HexagonShape.NumChannels == 1 ? "" : "s",
                        m_HexagonShape.Rank);
        }        

        #endregion


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

            // Maximum number of observable objects : Maximum number of tiles in HexMap with its maximum rank
            // Size of observations for each hexagon tile : 2 (Channel, Link)
            m_HexagonSensor = new HexagonSensor(
                m_SensorName,
                m_HexagonBuffer, 
                CalHexPropertyUtil.GetMaxHexCount(m_HexagonBuffer.Rank), 
                2, 
                m_ObservationType);

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
                if(m_Debug_DrawHexagonBuffer != m_Debug_DrawHexagonBufferEnabled) {
                    Debug_SetDrawHexagonBufferEnabled(m_Debug_DrawHexagonBuffer);
                }
            }
        }

        // Enable or disable DrawHexagonBuffer
        private void Debug_SetDrawHexagonBufferEnabled(bool enabled, bool standby = false)
        {
            if (enabled) {
                m_Debug_ChannelData?.Dispose();
                m_Debug_ChannelData = Debug_CreateChannelData();
                m_Debug_HexagonBufferDrawer.Enable(this, m_Debug_ChannelData, m_HexagonBuffer);
            }
            else {
                m_Debug_ChannelData?.Dispose();


                if (standby) {
                    m_Debug_HexagonBufferDrawer.Standby();
                }
                else {
                    m_Debug_HexagonBufferDrawer.Disable();
                }
            }

            m_Debug_DrawHexagonBufferEnabled = enabled;
        }

        // Create channel data to output in debug window
        // If ChannelLabels is not provided, fallback channel labels are created
        private DebugChannelData Debug_CreateChannelData()
        {
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

        // Whether the HexagonSensor was created
        public bool HasSensor
        {
            get { return m_HexagonSensor != null; }
        }

        private bool Debug_HasRuntimeSensor()
        {
            return Application.isPlaying && HasSensor;
        }

        private void Awake()
        {
            m_Debug_HexagonBufferDrawer.Disable();
        }

        private void FixedUpdate()
        {
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