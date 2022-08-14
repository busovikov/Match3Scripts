using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelGrid : ScriptableObject
{
    public byte width;
    public byte height;
    public int[] tiles;
}
