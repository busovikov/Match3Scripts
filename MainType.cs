using System;
using UnityEngine;

[CreateAssetMenu]
public class MainType : TileType
{
    public TileMap.BasicTileType mainID;

    public override Enum GetId()
    {
        return mainID;
    }

    public override bool SetId(Enum e)
    {
        var value = (TileMap.BasicTileType)e;
        if (mainID != value)
        {
            mainID = value;
            return true;
        }
        return false;
    }
}
