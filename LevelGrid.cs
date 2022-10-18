using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelGrid : ScriptableObject
{
    [System.Serializable]
    public struct Tile
    {
        [SerializeField] bool spetial;
        [SerializeField] byte main;
        [SerializeField] byte blocked;
        [SerializeField] byte background;

        public Enum[] GetfTypes()
        {
            Enum[] enums = new System.Enum[3];

            enums[0] = Background();
            enums[1] = spetial ? Spetial() : Main();
            enums[2] = Blocked();

            return enums;
        }
        public TileMap.BasicTileType Main() { return !spetial ? (TileMap.BasicTileType)main : TileMap.BasicTileType.None; }
        public TileMap.SpetialType Spetial() { return spetial ? (TileMap.SpetialType)main : TileMap.SpetialType.None; }
        public TileMap.BlockedTileType Blocked() { return (TileMap.BlockedTileType)blocked; }
        public TileMap.BackgroundTileType Background() { return (TileMap.BackgroundTileType)background; }

        public void SetMain (System.Enum t) { main = (byte)(TileMap.BasicTileType)t; }
        public void SetSpetial(System.Enum t) { main = (byte)(TileMap.SpetialType)t; spetial = true; }
        public void SetBlocked (System.Enum t) { blocked = (byte)(TileMap.BlockedTileType)t; }
        public void SetBackground (System.Enum t) { background = (byte)(TileMap.BackgroundTileType)t; }

    };

    
    public byte width;
    public byte height;
    public Tile[] tiles;

    public void Set(int x, int y, TileType t)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            if (t.GetType() == typeof(MainType))
                tiles[index].SetMain(t.GetId());
            else if (t.GetType() == typeof(SpetialType))
                tiles[index].SetSpetial(t.GetId());
            else if (t.GetType() == typeof(BlockedType))
                tiles[index].SetBlocked(t.GetId());
            else if (t.GetType() == typeof(BackgroundType))
                tiles[index].SetBackground(t.GetId());
        }
    }

    internal void RebuildGrid(byte oldWidth, byte oldHeight)
    {
        if (tiles == null)
        {
            tiles = new LevelGrid.Tile[width * height];
            for (int i = 0; i < width * height; i++)
            {
                tiles[i].SetMain(TileMap.BasicTileType.Random);
            }
            return;
        }

        var newOne = new LevelGrid.Tile[width * height];
        for (byte x = 0; x < width; ++x)
            for (byte y = 0; y < height; ++y)
            {
                int index = x + oldWidth * y;
                int current = x + width * y;
                if (x < oldWidth && y < oldHeight && index < tiles.Length)
                {
                    newOne[current] = tiles[index];
                }
                else
                {
                    newOne[current].SetMain(TileMap.BasicTileType.Random);
                }
            }
        tiles = newOne;
    }
    public bool SetWidth(byte w)
    {
        if (width != w)
        {
            var old_width = width;
            width = w;
            RebuildGrid(old_width, height);
            return true;
        }
        return false;
    }

    public bool SetHeight(byte h)
    {
        if (height != h)
        {
            var old_height = height;
            height = h;
            RebuildGrid(width, old_height);
            return true;
        }
        return false;
    }

    public bool SetName(string n)
    {
        if (name != n)
        {
            name = n;
            return true;
        }
        return false;
    }

    public bool Valid()
    {
        return tiles.Length == width * height;
    }

    public void SetZero(int x, int y, TileType t)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            if (t.GetType() == typeof(MainType))
                tiles[index].SetMain(TileMap.BasicTileType.None);
            else if (t.GetType() == typeof(BlockedType))
                tiles[index].SetBlocked(TileMap.BlockedTileType.Unblocked);
            else if (t.GetType() == typeof(BackgroundType))
                tiles[index].SetBackground(TileMap.BackgroundTileType.NoBackground);
        }
    }

    public void Init(string _name = null)
    {
        if (_name != null)
        {
            name = _name;
        }
        RebuildGrid(0, 0);
    }

    public void Validate()
    {
        if (tiles == null || tiles.Length != width * height)
        {
            RebuildGrid(0, 0);
        }
    }
}
