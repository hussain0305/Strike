using System;
using System.Collections.Generic;
using UnityEngine;

public class WeightedRandomPicker<T>
{
    private readonly Dictionary<T, float> _weights = new Dictionary<T, float>();
    private readonly List<(T Item, float Cumulative)> _entries = new List<(T, float)>();
    private float _totalWeight;
    private bool _dirty = true;
    
    public void AddOrUpdateChoice(T item, float weight)
    {
        if (weight > 0f)
            _weights[item] = weight;
        else
            _weights.Remove(item);
        
        _dirty = true;
    }
    
    public void AddChoice(T item, float weight) => AddOrUpdateChoice(item, weight);
    
    public T Pick()
    {
        if (_weights.Count == 0)
            throw new InvalidOperationException("No items to pick from");
        
        if (_dirty)
            Rebuild();
        
        float r = UnityEngine.Random.value * _totalWeight;
        foreach (var (item, cum) in _entries)
            if (r < cum)
                return item;
        
        return _entries[_entries.Count - 1].Item;
    }
    
    private void Rebuild()
    {
        _entries.Clear();
        _totalWeight = 0f;
        
        foreach (var kvp in _weights)
        {
            _totalWeight += kvp.Value;
            _entries.Add((kvp.Key, _totalWeight));
        }
        
        _dirty = false;
    }
}