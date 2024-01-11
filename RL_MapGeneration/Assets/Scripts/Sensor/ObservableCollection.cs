using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ObservableCollection
{
    public int Count => m_Observables.Count;

    [SerializeField, HideInInspector]
    private List<Observable> m_Observables;

    public ObservableCollection()
    {
        m_Observables = new List<Observable>();
    }

    public ObservableCollection Copy()
    {
        var copy = new ObservableCollection();
        foreach(var obs in m_Observables) {
            copy.Add(obs.Copy());
        }
        return copy;
    }

    public void CopyTo(List<Observable> target)
    {
        target.Clear();
        target.AddRange(m_Observables);
    }

    public Observable GetObservable(int index)
    {
        return m_Observables[index];
    }

    public bool ContainsOtherThan(IList<Observable> list, out IList<Observable> result)
    {
        result = new List<Observable>();

        foreach(var obs in m_Observables) {
            if (!list.Any(o => o.Name == obs.Name)) {
                result.Add(obs);
            }
        }

        return result.Count > 0;
    }

    public bool Equals(ObservableCollection other)
    {
        if (other == null || other.Count != Count) {
            return false;
        }

        for (int i = 0, n = Count; i < n; i++) {
            if (!other.GetObservable(i).Equals(m_Observables[i])) {
                return false;
            }
        }

        return true;
    }

    public int Add(string name, Func<float> getter)
    {
        if (name == Observable.Distance || name == Observable.OneHot) {
            Debug.LogError($"'{name}' is a dedicated observable name.");
        }
        if (m_Observables.Any(o => o.Name == name)) {
            Debug.LogError($"Observable name '{name}' already added.");
        }
        else if (m_Observables.Any(o => o.Getter == getter)) {
            Debug.LogError($"Observable getter {getter} already added.");
        }
        else {
            m_Observables.Add(new Observable(ObservableType.User, name, m_Observables.Count, getter));
        }

        return m_Observables.Count;
    }

    public void Add(Observable observable)
    {
        m_Observables.Add(observable);
    }
}
