using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelGrid : ScriptableObject
{
    public byte width;
    public byte height;
    public TileMap.AliveType[] tiles;

    public bool Valid()
    {
        return tiles.Length == width * height;
    }

    public void Init(string _name = null)
    {
        if (_name != null)
        {
            name = _name;
        }
        width = 8;
        height = 8;
        tiles = new TileMap.AliveType[width * height];
    }

}
