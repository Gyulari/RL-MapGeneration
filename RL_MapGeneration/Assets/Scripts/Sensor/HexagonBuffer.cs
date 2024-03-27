using Unity.MLAgents;
using System;
using UnityEngine;
using Gyulari.HexSensor.Util;

namespace Gyulari.HexSensor
{
    public class HexagonBuffer
    {
        [Serializable]
        public struct Shape
        {
            public int NumChannels;
            public int Rank;

            public Shape(int numChannels, int rank)
            {
                NumChannels = numChannels;
                Rank = rank;
            }

            public void Validate()
            {
                if (NumChannels < 1) {
                    throw new UnityAgentsException("HexagonBuffer has no channels.");
                }
                if (Rank < 1) {
                    throw new UnityAgentsException("HexagonBuffer has invalid rank");
                }
            }

            public override string ToString()
            {
                return $"Hexagon {NumChannels} x {Rank}";
            }
        }

        public Shape GetShape()
        {
            return new Shape(m_NumChannels, m_Rank);
        }

        public int NumChannels
        {
            get { return m_NumChannels; }
            set { m_NumChannels = value; Initialize(); }
        }
        private int m_NumChannels;

        public int Rank
        {
            get { return m_Rank; }
            set { m_Rank = value; Initialize(); }
        }
        private int m_Rank;

        private float[][] m_Values;
        private float[][] m_LinkValues;

        public HexagonBuffer(int numChannels, int rank)
        {
            m_NumChannels = numChannels;
            m_Rank = rank;

            Initialize();
        }

        public HexagonBuffer(Shape shape)
            : this(shape.NumChannels, shape.Rank) { }

        protected virtual void Initialize()
        {
            m_Values = new float[NumChannels][];
            m_LinkValues = new float[NumChannels][];

            for (int i = 0; i < NumChannels; i++) {
                m_Values[i] = new float[CalHexPropertyUtil.GetMaxHexCount(Rank)];
                m_LinkValues[i] = new float[CalHexPropertyUtil.GetMaxHexCount(Rank)];
            }
        }

        public void Clear()
        {
            ClearChannels(0, NumChannels);
        }

        public void ClearChannels(int start, int length)
        {
            for (int i = 0; i < length; i++) {
                ClearChannel(start + i);
            }
        }

        public void ClearChannel(int channel)
        {
            if (channel < NumChannels) {
                Array.Clear(m_Values[channel], 0, m_Values[channel].Length);
            }
        }

        public virtual void Write(int hexIdx, int channel, float value)
        {/*
            m_LinkValues[hexIdx][channel] = link;
            m_Values[hexIdx][channel][link] = value;
            */
            m_Values[channel][hexIdx] = value;
        }

        public virtual bool TryWrite(int hexIdx, int channel, int link, float value)
        {
            bool isContained = Contains(hexIdx);
            if (isContained) {
                Write(channel, link, value);
            }
            return isContained;
        }

        public virtual float Read(int hexIdx, int channel)
        {
            return m_Values[channel][hexIdx];
        }

        public virtual bool TryRead(int hexIdx, int channel, int link, out float value)
        {
            bool isContained = Contains(hexIdx);
            value = isContained ? Read(channel, link) : 0;
            return isContained;
        }

        public virtual bool Contains(int hexIdx)
        {
            return hexIdx < CalHexPropertyUtil.GetMaxHexCount(m_Rank);
        }

        public virtual Color32[][] GetLayerColors()
        {
            ThrowNotSupportedError();
            return null;
        }

        private void ThrowNotSupportedError()
        {
            throw new UnityAgentsException(
                "HexagonBuffer doesn't support PNG compression. " +
                "Use the ColorHexagonBuffer instead.");
        }
    }
}