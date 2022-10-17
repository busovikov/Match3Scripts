using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelGrid : ScriptableObject
{
    [System.Serializable]
    public struct Tile
    {
        byte main;
        byte blocked;
        byte background;
        
        public TileMap.BasicTileType Main() { return (TileMap.BasicTileType)main; }
        public TileMap.SpetialType Spetial() { return (TileMap.SpetialType)main; }
        public TileMap.BlockedTileType Blocked() { return (TileMap.BlockedTileType)blocked; }
        public TileMap.BackgroundTileType Background() { return (TileMap.BackgroundTileType)background; }

        public void SetMain (System.Enum t) { main = (byte)(TileMap.BasicTileType)t; }
        public void SetSpetial(System.Enum t) { main = (byte)(TileMap.SpetialType)t; }
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
            else if (t.GetType() == typeof(BlockedType))
                tiles[index].SetBlocked(t.GetId());
            else if (t.GetType() == typeof(BackgroundType))
                tiles[index].SetBackground(t.GetId());
        }
    }

    public void SetZero(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            tiles[index].SetMain(TileMap.BasicTileType.None);
            tiles[index].SetBlocked(TileMap.BlockedTileType.Unblocked);
            tiles[index].SetBackground(TileMap.BackgroundTileType.NoBackground);
        }
    }

    public TileMap.BasicTileType GetBasic(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            return tiles[index].Main();
        }
        return TileMap.BasicTileType.None;
    }

    public void SetBasic(int x, int y, TileMap.BasicTileType t)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            tiles[index].SetMain(t);
        }
    }

    public TileMap.BlockedTileType GetBlocked(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            return tiles[index].Blocked();
        }
        return TileMap.BlockedTileType.Unblocked;
    }

    public void SetBlocked(int x, int y, TileMap.BlockedTileType t)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            tiles[index].SetBlocked(t);
        }
    }

    public TileMap.BackgroundTileType GetBackground(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            return tiles[index].Background();
        }
        return TileMap.BackgroundTileType.NoBackground;
    }

    public void SetBackground(int x, int y, TileMap.BackgroundTileType t)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            int index = y * width + x;
            tiles[index].SetBackground(t);
        }
    }

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
        tiles = new Tile[width * height];
    }

}
