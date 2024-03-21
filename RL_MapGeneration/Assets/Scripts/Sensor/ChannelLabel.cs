using System;
using UnityEngine;

[Serializable]
public struct ChannelLabel
{
    public string Name;
    public Color Color;

    public ChannelLabel(string name, Color color)
    {
        Name = name;
        Color = color;
    }

    public static ChannelLabel Default
        => new ChannelLabel()
        {
            Name = "Observable",
            Color = Color.cyan
        };
}