using UnityEngine;
using System.Linq;

public class ColorHexagonBuffer : HexagonBuffer
{
    public ColorHexagonBuffer(int numChannels, int rank)
        : base(numChannels, rank) { }

    public ColorHexagonBuffer(Shape shape)
        : base(shape.NumChannels, shape.Rank) { }
}
