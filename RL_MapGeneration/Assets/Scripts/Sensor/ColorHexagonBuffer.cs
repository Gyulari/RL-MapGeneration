using UnityEngine;
using System.Linq;

namespace Gyulari.HexSensor
{
    public class ColorHexagonBuffer : HexagonBuffer
    {
        public ColorHexagonBuffer(int numChannels, int rank)
            : base(numChannels, rank) { }

        /*
        protected Color32[][] m_Colors;
        protected Color32[] c_Black;

        public ColorHexagonBuffer(int numChannels, int rank)
            : base(numChannels, rank) { }

        public ColorHexagonBuffer(Shape shape)
            : base(shape.NumChannels, shape.Rank) { }

        protected override void Initialize()
        {
            base.Initialize();

            m_Colors = new Color32[NumChannels][];

            for (int i=0; i<NumChannels; i++) {
                m_Colors[i] = new Color32[GetTextureSizeByRank(Rank, 2).width * GetTextureSizeByRank(Rank, 2).height];
            }

            c_Black = Enumerable.Repeat(new Color32(0, 0, 0, 255), GetTextureSizeByRank(Rank, 2).width * GetTextureSizeByRank(Rank, 2).height).ToArray();
            ClearColors();
        }

        public override void Clear()
        {
            base.Clear();
            ClearColors();
        }

        private void ClearColors()
        {
            // for (int i = 0; i < NumChannels; i++) {
                // ClearLayerColors(i);
            // }            
        }

        public override void ClearChannels(int start, int length)
        {
            int channel = start;
            int n = start + length;

            while(channel < n) {
                ClearChannel(channel);
                channel++;
            }
        }

        public override void ClearChannel(int channel)
        {
            base.ClearChannel(channel);
            ClearChannelColors(channel);
        }

        private void ClearChannelColors(int channel)
        {
            int layer = channel / 3;
            int color = channel - layer * 3;

            for (int i = 0, n = m_Colors[layer].Length; i < n; i++) {
                m_Colors[channel][i][color] = 0;
            }
        }

        public override void Write(int hexIdx, int channel, float value)
        {
            base.Write(hexIdx, channel, value);
        }

        public override Color32[][] GetLayerColors()
        {
            return m_Colors;
        }


        private (int width, int height) GetTextureSizeByRank(int rank, int pixelResolution)
        {
            int width = 7 * pixelResolution * (2 * rank - 1);
            int height = 4 * pixelResolution * (3 * rank - 1);

            return (width, height);
        }
        */
    }
}