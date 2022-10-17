using System;
using UnityEngine;

[CreateAssetMenu]
public class SpetialType : TileType
{
    public TileMap.SpetialType spetialID;
    public override Enum GetId()
    {
        return spetialID;
    }

    public override bool SetId(Enum e)
    {
        var value = (TileMap.SpetialType)e;
        if (spetialID != value)
        {
            spetialID = value;
            return true;
        }
        return false;
    }
}