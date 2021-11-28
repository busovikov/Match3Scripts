using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match 
{
    private TileMap tiles;
    private List<Vector3> setX;
    private List<Vector3>[] setY;

    public bool bisy = false;

    public class DestructableTiles
    {
        public int minX;
        public int minY;
        public int maxX;
        public HashSet<Vector3> destructionList;

        public DestructableTiles(int minX, int minY, int maxX)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.destructionList = new HashSet<Vector3>();
        }
    }

    public Match(TileMap tilesMap)
    {
        tiles = tilesMap;
        InitSets();
    }

    public bool SwapsAvailable()
    {
        for (int x = 0; x < tiles.width; x++)
            for (int y = 0; y < tiles.height; y++)
            {
                var position = new Vector2(x, y);
                var next = new Vector2(x, y + 1);
                if (tiles.GetType(position) == tiles.GetType(next) && (ForTwo(position, Vector2.down) || ForTwo(next, Vector2.up)))
                {
                    return true;
                }
                next = new Vector2(x, y + 2);
                if (tiles.GetType(position) == tiles.GetType(next) && ForHole(position, Vector2.up))
                {
                    return true;
                }
                next = new Vector2(x + 1, y);
                if (tiles.GetType(position) == tiles.GetType(next) && (ForTwo(position, Vector2.left) || ForTwo(next, Vector2.right)))
                {
                    return true;
                }
                next = new Vector2(x + 2, y);
                if (tiles.GetType(position) == tiles.GetType(next) && ForHole(position, Vector2.right))
                {
                    return true;
                }
            }
        return false;
    }
    private bool ForTwo(Vector2 position, Vector2 direction)
    {
        var type = tiles.GetType(position);

        var forward = position + direction;
        var right = new Vector2(direction.y, direction.x);
        var left = -right;

        return tiles.GetType(forward + direction) == type ||
            tiles.GetType(forward + left) == type ||
            tiles.GetType(forward + right) == type;
    }

    private bool ForHole(Vector2 position, Vector2 direction)
    {
        var type = tiles.GetType(position);

        var forward = position + direction;
        var right = new Vector2(direction.y, direction.x);
        var left = -right;

        return tiles.GetType(forward + left) == type || tiles.GetType(forward + right) == type;
    }
    private void InitSets()
    {
        setX = new List<Vector3>(tiles.width);
        setY = new List<Vector3>[tiles.width];
        for (int i = 0; i < tiles.width; i++)
        {
            setY[i] = new List<Vector3>(tiles.height);
        }
    }

    public bool IsAny(out DestructableTiles destructableTiles)
    {
        destructableTiles = new DestructableTiles(int.MaxValue, int.MaxValue, 0);
        for (int y = 0; y < tiles.height; y++)
        {
            for (int x = 0; x < tiles.width; x++)
            {
                StackOn(x, y, setX, destructableTiles);
                StackOn(x, y, setY[x], destructableTiles);
                if (y == tiles.height - 1)
                {
                    Sink(setY[x], destructableTiles);
                }
            }
            Sink(setX, destructableTiles);
        }
        
         return destructableTiles.destructionList.Count > 0;
    }
    private void StackOn(int x, int y, List<Vector3> set, DestructableTiles destructableTiles)
    {
        int type = tiles.GetType(x, y);
        if (set.Count > 0 && set[0].z != type) // Store type in z to avoid checking for validity
        {
            Sink(set, destructableTiles);
        }
        if (tiles.IsValid(x, y))
        {
            set.Add(new Vector3(x, y, type));
        }
    }

    private void Sink(List<Vector3> set, DestructableTiles destructableTiles)
    {
        if (set.Count >= 3)
        {
            foreach (Vector3 v in set) 
            { 
                tiles.GetTile(v).invalid = true;
                if (v.x > destructableTiles.maxX)
                {
                    destructableTiles.maxX = (int)v.x;
                }
                if (v.x < destructableTiles.minX)
                {
                    destructableTiles.minX = (int)v.x;
                }
                if (v.y < destructableTiles.minY)
                {
                    destructableTiles.minY = (int)v.y;
                }
            }
            destructableTiles.destructionList.UnionWith(set);
        }
        set.Clear();
    }
}
