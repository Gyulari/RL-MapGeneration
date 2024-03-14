using UnityEngine;
using System.Linq;

public class ColorHexagonBuffer : HexagonBuffer
{
    protected int m_NumLayers;
    protected Color32[][] m_Colors;
    protected Color32[] c_Black;

    public ColorHexagonBuffer(int numChannels, int rank)
        : base(numChannels, rank) { }

    public ColorHexagonBuffer(Shape shape)
        : base(shape.NumChannels, shape.Rank) { }

    protected override void Initialize()
    {
        base.Initialize();

        m_NumLayers = Mathf.CeilToInt(NumChannels / 3f);
        m_Colors = new Color32[m_NumLayers][];

        for (int i = 0; i < m_NumLayers; i++) {
            m_Colors[i] = new Color32[GetTextureSizeByRank().width * GetTextureSizeByRank().height];
        }

        c_Black = Enumerable.Repeat(new Color32(0, 0, 0, 255), GetTextureSizeByRank().width * GetTextureSizeByRank().height).ToArray();
        ClearColors();
    }

    public override void Clear()
    {
        base.Clear();
        ClearColors();
    }

    public void ClearLayer(int layer)
    {
        int channel = layer * 3;
        base.ClearChannel(channel);
        base.ClearChannel(channel + 1);
        base.ClearChannel(channel + 2);
        ClearLayerColors(layer);
    }

    public override void ClearChannels(int start, int length)
    {
        int channel = start;
        int n = start + length;

        while (channel < n) {
            if (channel % 3 == 0 && channel < n - 1) {
                ClearLayerColors(channel / 3);

                base.ClearChannel(channel);
                base.ClearChannel(channel + 1);
                base.ClearChannel(channel + 2);
                channel += 3;
            }
            else {
                ClearChannel(channel);
                channel++;
            }
        }
    }

    public override void ClearChannel(int channel)
    {
        base.ClearChannel(channel);
        ClearChannelColors(channel);
    }

    public override void Write(int channel, int hexIdx, float value)
    {
        base.Write(channel, hexIdx, value);

        int layer = channel / 3;
        int color = channel - layer * 3;
        // m_Colors[layer][(Height - y - 1) * Width + x][color] = (byte)(value * 255);      // What is this?
    }

    public override Color32[][] GetLayerColors()
    {
        return m_Colors;
    }

    private void ClearColors()
    {
        for (int i = 0; i < m_NumLayers; i++) {
            ClearLayerColors(i);
        }
    }

    private void ClearLayerColors(int layer)
    {
        System.Array.Copy(c_Black, m_Colors[layer], m_Colors[layer].Length);
    }

    private void ClearChannelColors(int channel)
    {
        int layer = channel / 3;
        int color = channel - layer * 3;

        for (int i = 0, n = m_Colors[layer].Length; i < n; i++) {
            m_Colors[layer][i][color] = 0;
        }
    }

    private (int width, int height) GetTextureSizeByRank()
    {
        int width = 7 * 2 * (2 * Rank - 1);
        int height = 4 * 2 * (3 * Rank - 1);

        return (width, height);
    }
}
