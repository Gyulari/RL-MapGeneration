using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DebugChannelData : IDisposable
{
    public static DebugChannelData FromSettings(
        IEncodingSettings settings, bool storePositions = true)
    {
        return new DebugChannelData(settings, storePositions);
    }

    public static DebugChannelData FromLabels(
        IList<ChannelLabel> labels, bool storePositions = false)
    {
        return new DebugChannelData(labels, storePositions);
    }

    public bool HasHexagonPositions { get; private set; }
    private readonly IList<HashSet<Vector2Int>> m_HexagonPositions;

    private readonly bool m_HasObservables;
    private readonly IList<Observable> m_Observables;

    private readonly IList<ChannelLabel> m_Labels;

    private DebugChannelData(IEncodingSettings settings, bool storePositions)
    {
        m_Labels = new List<ChannelLabel>();
        m_Observables = new List<Observable>();
        m_HasObservables = true;

        if(storePositions) {
            HasHexagonPositions = true;
            m_HexagonPositions = new List<HashSet<Vector2Int>>();
        }

        var tags = settings.DetectableTags;

        foreach(string tag in tags) {
            var observables = settings.GetObservables(tag);

            foreach(var obs in observables) {
                m_Observables.Add(obs);

                m_Labels.Add(new ChannelLabel()
                {
                    Name = $"{tag} / {obs.Name}",
                    Color = obs.Color
                });

                if (storePositions) {
                    m_HexagonPositions.Add(new HashSet<Vector2Int>());
                }
            }
        }
    }

    private DebugChannelData(IList<ChannelLabel> labels, bool storePositions)
    {
        m_Labels = new List<ChannelLabel>(labels);
        int n = labels.Count;

        if (storePositions) {
            HasHexagonPositions = true;
            m_HexagonPositions = new List<HashSet<Vector2Int>>(n);

            for (int i = 0; i < n; i++) {
                m_HexagonPositions.Add(new HashSet<Vector2Int>());
            }
        }
    }

    public void ClearHexagonPositions()
    {
        foreach(var positions in m_HexagonPositions) {
            positions.Clear();
        }
    }

    public void AddHexagonPositions(int channel, HashSet<Vector2Int> positions)
    {
        m_HexagonPositions[channel].UnionWith(positions);
    }

    public void AddHexagonPosition(int channel, Vector2Int position)
    {
        m_HexagonPositions[channel].Add(position);
    }

    public HashSet<Vector2Int> GetHexagonPositions(int channel)
    {
        return m_HexagonPositions[channel];
    }

    public string GetChannelName(int channel)
    {
        return m_Labels[channel].Name;
    }

    public Color GetColor(int channel)
    {
        return m_HasObservables
            ? m_Observables[channel].Color
            : m_Labels[channel].Color;
    }

    public void Dispose()
    {
        if (HasHexagonPositions) {
            ClearHexagonPositions();
        }

        if (m_HasObservables) {
            m_Observables.Clear();
        }

        m_Labels.Clear();
    }
}
