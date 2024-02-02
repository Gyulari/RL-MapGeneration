using UnityEngine;
using Unity.MLAgents;
using System;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class HexagonBuffer
{
    public struct Shape
    {
        public int NumChannels;
        public int Rank;
        // public int Index;
        
        /*
        public int Size
        {
            get { return Rank; }
            set { Rank = value; }
        }
        */

        public Shape(int numChannels, int rank)
        {
            NumChannels = numChannels;
            Rank = rank;
        }

        // Channel, Rank가 유효한지 검사
        public void Validate()
        {
            if (NumChannels < 1) {
                throw new UnityAgentsException("Hexagon buffer has no channels.");
            }

            if (Rank < 1) {
                throw new UnityAgentsException("Invalid hexagon buffer Rank " + Rank);
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

    /*
    public int Height
    {
        get { return m_Height; }
        set { m_Height = value; Initialize(); }
    }
    private int m_Height;
    */

    private float[][] m_Values;

    public HexagonBuffer(int numChannels, int rank)
    {
        m_NumChannels = numChannels;
        m_Rank = rank;

        Initialize();
    }

    protected virtual void Initialize()
    {
        m_Values = new float[NumChannels][];

        int NumHex = (Rank == 1) ? 1 : 6 * (Rank - 1);

        for(int i=0; i<NumChannels; i++) {
            m_Values[i] = new float [NumHex];
        }
    }

    public virtual float Read(int channel, int rank, int hexNum)
    {
        return m_Values[channel][GetHexIndex(rank, hexNum)];
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

    // 얘 뭔 코드를 짜놓은겨 도대체
    private int GetHexIndex(int rank, int hexNum)
    {
        if (rank == 1) return 0;

        int hexIdx = 0;
        for(int i=2; i <= rank; i++) {
            hexIdx += 6 * (i - 1);
        }

        return hexIdx;
    }
}
