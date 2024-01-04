using UnityEngine;
using System.Linq;

public class ColorHexagonBuffer : HexagonBuffer
{
    public ColorHexagonBuffer(int numChannels, int widht, int height)
        : base(numChannels, widht, height) { }

    public ColorHexagonBuffer(int numChannels, Vector2Int size)
        : base(numChannels, size.x, size.y) { }

    public ColorHexagonBuffer(Shape shape)
        : base(shape.NumChannels, shape.Width, shape.Height) { }
}
