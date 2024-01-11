using System;
using UnityEngine;

public enum ObservableType
{
    Distance, OneHot, User
};

public class Observable
{
    public const string Distance = "Distance";
    public const string OneHot = "One-Hot";

    [HideInInspector]
    public string Name;

    [HideInInspector]
    public ObservableType Type;

    [HideInInspector]
    public int Index;

    [HideInInspector]
    public Func<float> Getter;

    public bool Enabled = true;

    public Color Color;

    public Observable(
        ObservableType type,
        string name,
        int index = -1,
        Func<float> getter = null)
    {
        Type = type;
        Name = name;
        Index = index;
        Getter = getter;
        Color = UnityEngine.Random.ColorHSV(0, 1, 0.5f, 1, 0.5f, 1, 1, 1);
    }

    public float Value()
    {
        return Mathf.Clamp01(Getter.Invoke());
    }

    public ObservableType Evaluate(IDetectable detectable, out float value)
    {
        value = Type switch
        {
            ObservableType.User
              => detectable.Observables.GetObservable(Index).Value(),
            ObservableType.OneHot
              => 1,
            _ => 0
        };

        return Type;
    }

    public Observable Copy()
    {
        return new Observable(Type, Name, Index);
    }

    public bool Equals(Observable other)
    {
        return other.Type == Type && other.Name == Name;
    }
}
