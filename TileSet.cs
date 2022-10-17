using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TileSet: ScriptableObject
{
    [System.Serializable]
    public struct TypeToTile
    {
        public TypeToTile(System.Enum e, TileType t)
        {
            category = e.GetType().ToString();
            value = e.ToString();
            type = t;
        }
        public string category;
        public string value;
        public TileType type;
    }

    public bool SetName(string value)
    {
        if (name != value)
        {
            name = value;
            return true;
        }
        return false;
    }
    public TypeToTile[] tiles;
}
