using System;
using System.Collections.Generic;
using UnityEngine;

public class WeightBasedPicker<T>
{
    private readonly Dictionary<T, float> weights = new Dictionary<T, float>();
    private readonly List<(T Item, float Cumulative)> entries = new List<(T, float)>();
    private float totalWeight;
    private bool dirty = true;
    
    public void AddOrUpdateChoice(T item, float weight)
    {
        if (weight > 0f)
            weights[item] = weight;
        else
            weights.Remove(item);

        dirty = true;
    }
    
    public void AddChoice(T item, float weight)
    {
        AddOrUpdateChoice(item, weight);
    }
    
    public T Pick()
    {
        if (dirty)
            Rebuild();
        
        float rand = UnityEngine.Random.value * totalWeight;
        foreach (var (item, cumulativeWeight) in entries)
            if (rand < cumulativeWeight)
                return item;
        
        return entries[entries.Count - 1].Item;
    }
    
    private void Rebuild()
    {
        entries.Clear();
        totalWeight = 0f;
        
        foreach (var kvp in weights)
        {
            totalWeight += kvp.Value;
            entries.Add((kvp.Key, totalWeight));
        }
        
        dirty = false;
    }
}