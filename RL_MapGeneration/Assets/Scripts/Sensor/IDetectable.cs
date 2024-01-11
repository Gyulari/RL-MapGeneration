using System.Collections.ObjectModel;

public interface IDetectable
{
    string Tag { get; }

    string Name { get; }

    ObservableCollection Observables { get; }

    ObservableCollection InitObservables();

    void AddObservables();
}