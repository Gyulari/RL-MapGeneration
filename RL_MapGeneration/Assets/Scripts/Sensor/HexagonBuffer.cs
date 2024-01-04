using UnityEngine;
using Unity.MLAgents;
using System;
using Unity.VisualScripting;

public class HexagonBuffer
{
    public struct Shape
    {
        public int NumChannels;
        public int Width;
        public int Height;
        
        public Vector2Int Size
        {
            get { return new Vector2Int(Width, Height); }
            set { Width = value.x; Height = value.y; }
        }

        public Shape(int numChannels, int width, int height)
        {
            NumChannels = numChannels;
            Width = width;
            Height = height;
        }

        public Shape(int numChannels, Vector2Int size)
            : this(numChannels, size.x, size.y) { }

        public void Validate()
        {
            if (NumChannels < 1) {
                throw new UnityAgentsException("Hexagon buffer has no channels.");
            }

            if (Width < 1) {
                throw new UnityAgentsException("Invalid hexagon buffer width " + Width);
            }

            if (Height < 1) {
                throw new UnityAgentsException("Invalid hexagon buffer height " + Height);
            }
        }

        public override string ToString()
        {
            return $"Hexagon {NumChannels} x {Width} x {Height}";
        }
    }

    public Shape GetShape()
    {
        return new Shape(m_NumChannels, m_Width, m_Height);
    }

    public int NumChannels
    {
        get { return m_NumChannels; }
        set { m_NumChannels = value; Initialize(); }
    }
    private int m_NumChannels;

    public int Width
    {
        get { return m_Width; }
        set { m_Width = value; Initialize(); }
    }
    private int m_Width;

    public int Height
    {
        get { return m_Height; }
        set { m_Height = value; Initialize(); }
    }
    private int m_Height;

    private float[][] m_Values;

    public HexagonBuffer(int numChannels, int width, int height)
    {
        m_NumChannels = numChannels;
        m_Width = width;
        m_Height = height;

        Initialize();
    }

    protected virtual void Initialize()
    {
        m_Values = new float[NumChannels][];

        for(int i=0; i<NumChannels; i++) {
            m_Values[i] = new float[Width * Height];
        }
    }

    public virtual float Read(int channel, int x, int y)
    {
        return m_Values[channel][y * Width + x];
    }

    public virtual float Read(int channel, Vector2Int pos)
    {
        return Read(channel, pos.x, pos.y);
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
