using System.Collections.Generic;
using UnityEngine;

public class RandomizerParameterHub
{
    private readonly Dictionary<RandomizerParameterType, IRandomizerParameter> map = new();

    public void Register(IRandomizerParameter param)
    {
        map[param.Id] = param;
    }

    public IReadOnlyDictionary<RandomizerParameterType, IRandomizerParameter> Snapshot()
    {
        return map;
    }

    public EndlessGenerationSettings ToSettings()
    {
        var settings = new EndlessGenerationSettings();
        foreach (var kvp in map)
        {
            settings[kvp.Key] = kvp.Value.Value;
        }
        return settings;
    }
}
public sealed class EndlessGenerationSettings 
    : Dictionary<RandomizerParameterType, object> { }
