using System;
using UnityEngine;

[CreateAssetMenu]
public class BackgroundType : TileType
{
    public TileMap.BackgroundTileType backgroundID;
    public override Enum GetId()
    {
        return backgroundID;
    }

    public override bool SetId(Enum e)
    {
        var value = (TileMap.BackgroundTileType)e;
        if (backgroundID != value)
        {
            backgroundID = value;
            return true;
        }
        return false;
    }
}