public static class SpawnWeights
{
    public static WeightedRandomPicker<HexStackShape> HexShapePicker(int numSectors)
    {
        var picker = new WeightedRandomPicker<HexStackShape>();
        
        picker.AddChoice(HexStackShape.Pyramid, 1);
        picker.AddChoice(HexStackShape.Uniform, 1);
        picker.AddChoice(HexStackShape.PeripheryWithInner, 1);

        if (numSectors > 4)
            picker.AddOrUpdateChoice(HexStackShape.PeripheryWithInner, 5);
        
        else if (numSectors > 1)
            picker.AddOrUpdateChoice(HexStackShape.Pyramid, 3);
        
        return picker;
    }
    
    // public static WeightedRandomPicker<PointTokenType> TokenPicker(SpawnContext ctx)
    // {
    //     var p = new WeightedRandomPicker<PointTokenType>();
    //     // e.g. if shape known:
    //     if (ctx.Shape == HexStackShape.Pyramid)
    //     {
    //         p.AddChoice(PointTokenType.Hex1x, 3f);
    //         p.AddChoice(PointTokenType.Hex4x, 1f);
    //     }
    //     // etc...
    //     return p;
    // }
}