using System.Collections.Generic;
using UnityEngine;

public static class SpawnWeights
{
    public static WeightedRandomPicker<HexStackShape> HexShapePicker(int numSectors)
    {
        var picker = new WeightedRandomPicker<HexStackShape>();
        
        picker.AddChoice(HexStackShape.Pyramid, 1);
        picker.AddChoice(HexStackShape.Uniform, 1);
        picker.AddChoice(HexStackShape.PeripheryWithInner, 1);

        if (numSectors > 4)
        {
            picker.AddOrUpdateChoice(HexStackShape.Pyramid, 3);
            picker.AddOrUpdateChoice(HexStackShape.PeripheryWithInner, 6);
        }
        
        else if (numSectors > 1)
        {
            picker.AddOrUpdateChoice(HexStackShape.Pyramid, 3);
            picker.AddOrUpdateChoice(HexStackShape.PeripheryWithInner, 3); //3;
        }
        
        return picker;
    }
    
    public static WeightedRandomPicker<PointTokenType> HexTokenPicker(HexStackShape shape)
    {
        var picker = new WeightedRandomPicker<PointTokenType>();
        
        picker.AddChoice(PointTokenType.Pin_1x, 1);
        picker.AddChoice(PointTokenType.Pin_2x, 1);
        picker.AddChoice(PointTokenType.Pin_4x, 1);
        
        if(shape == HexStackShape.Pyramid)
            picker.AddOrUpdateChoice(PointTokenType.Pin_1x, 3);
        else if (shape == HexStackShape.Uniform)
            picker.AddOrUpdateChoice(PointTokenType.Pin_2x, 3);
        else if (shape == HexStackShape.PeripheryWithInner)
            picker.AddOrUpdateChoice(PointTokenType.Pin_2x, 3);

        return picker;
    }
    
    public static WeightedRandomPicker<(int, int)> HexStackDimensionsPicker(float xLength, float zLength, PointTokenType selectedToken)
    {
        Vector3 dimensions = CollectiblePrefabMapping.Instance.GetPointTokenDimension(selectedToken);
        int maxRings = Mathf.FloorToInt(Mathf.Min(xLength / (2 * dimensions.x), zLength / (2 * dimensions.z)));
        
        float midRows = (maxRings + 1) / 2f;
        
        var picker = new WeightedRandomPicker<(int, int)>();
        for (int numRows = 1; numRows <= maxRings; numRows++)
        {
            float rowWeight = (maxRings + 1) - Mathf.Abs(numRows - midRows);
            float midLevels = (numRows + 1) / 2f;
            
            for (int numLevels = 1; numLevels <= numRows; numLevels++)
            {
                float levelWeight = (numRows + 1) - Mathf.Abs(numLevels - midLevels);
                float weight = rowWeight * levelWeight;
                
                picker.AddChoice((numRows, numLevels), weight);
            }
        }

        return picker;
    }

}