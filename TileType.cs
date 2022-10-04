using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TileType : ScriptableObject
{
    public TileMap.AliveType id;

    public Texture2D image;
    public bool dirty;
}
