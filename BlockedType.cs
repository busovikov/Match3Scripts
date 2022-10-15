using System;
using UnityEngine;

[CreateAssetMenu]
public class BlockedType : TileType
{
    public TileMap.BlockedTileType blockedID;
    public override Enum GetId()
    {
        return blockedID;
    }

    public override bool SetId(Enum e)
    {
        var value = (TileMap.BlockedTileType)e;
        if (blockedID != value)
        {
            blockedID = value;
            return true;
        }
        return false;
    }
}
