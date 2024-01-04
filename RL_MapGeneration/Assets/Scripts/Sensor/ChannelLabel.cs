using System;
using UnityEngine;

[Serializable]
public struct ChannelLabel
{
    [Tooltip("Name used for debugging / visualization.")]
    public string Name;

    [Tooltip("Color used for debugging / visualization.")]
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