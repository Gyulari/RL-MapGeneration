using System.Collections.Generic;

public interface IEncodingSettings
{
    IList<string> DetectableTags { get; }

    PointModifierType GetPointModifierType(string tag);

    IEnumerable<Observable> GetObservables(string tag);
}
