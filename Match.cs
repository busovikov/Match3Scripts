using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match 
{
    private TileMap tiles;
    private Interval setX;
    private Interval[] setY;

    public bool bisy = false;

    public class Interval
    {
        public Interval(Orientation orientation)
        {
            mOrientation = orientation;
            Reset();
        }

        public void Reset()
        {
            cell = new TileMap.Cell();
            crossingCell = new TileMap.Cell();
            crossing = false;
            count = 0;
            type = -1;
        }

        public Interval Clone()
        {
            Interval interval = new Interval(mOrientation);
            interval.cell = cell;
            interval.crossingCell = crossingCell;
            interval.crossing = crossing;
            interval.count = count;
            interval.type = type;
            return interval;
        }
        public enum Orientation
        { 
            Horizontal = 0,
            Vertical = 1
        }

        public static Interval operator++ (Interval i)
        {
            i.count++;
            return i;
        }

        public bool Belongs(Byte pX, Byte pY)
        {
            if (mOrientation == Orientation.Horizontal)
            {
                return cell.y == pY && cell.x <= pX && pX < cell.x + count;
            }
            return cell.x == pX && cell.y <= pY && pY < cell.y + count;
        }
        public bool IsCrossing(Interval interval, out TileMap.Cell crossing)
        {
            crossing = new TileMap.Cell();
            if (mOrientation == interval.mOrientation)
            {
                return false;
            }
            if (mOrientation == Orientation.Horizontal)
            {
                return IsCrossing(this, interval, out crossing);
            }
            return IsCrossing(interval, this, out crossing);
        }

        static bool IsCrossing(Interval horizontal, Interval vertical, out TileMap.Cell crossing)
        {
            crossing = new TileMap.Cell(vertical.cell.x, horizontal.cell.y);
            return horizontal.cell.x <= vertical.cell.x && vertical.cell.x < horizontal.cell.x + horizontal.count &&
                vertical.cell.y <= horizontal.cell.y && horizontal.cell.y < vertical.cell.y + vertical.count;
        }

        public Orientation mOrientation;
        public TileMap.Cell cell;
        public TileMap.Cell crossingCell;
        public bool crossing;
        public Byte count;
        public SByte type;
    }

    public struct Crossing
    {
       public Crossing(Byte pX, Byte pY)
       {
           x = pX;
           y = pY;
           index = 0;
       }
        public Byte x;
        public Byte y;
        public SByte index;
    }
    public class DestructableTiles
    {
        public Byte minX;
        public Byte minY;
        public Byte maxX;
        public List<Interval> destructionList;

        public DestructableTiles(Byte minX, Byte minY, Byte maxX)
        {
            this.minX = minX;
            this.minY = minY;
            this.maxX = maxX;
            this.destructionList = new List<Interval>(10);
        }
    }

    public Match(TileMap tilesMap)
    {
        tiles = tilesMap;
        InitSets();
    }

    public bool SwapsAvailable()
    {
        for (Byte x = 0; x < tiles.width; x++)
            for (Byte y = 0; y < tiles.height; y++)
            {
                Vector2 position = new Vector2(x, y);
                TileMap.Cell tmp;
                TileMap.Cell.ToCell(position, out tmp);
                int type = tiles.GetType(tmp);

                Vector2 next = new Vector2(x, y + 1);
                if (TileMap.Cell.ToCell(next, out tmp))
                {
                    int nextType = tiles.GetType(tmp);
                    if (type == nextType && (CheckPotentialOnEdge(position, Vector2.down, type) || CheckPotentialOnEdge(next, Vector2.up, type)))
                    {
                        return true;
                    }
                }
                next = new Vector2(x, y + 2);
                if (TileMap.Cell.ToCell(next, out tmp))
                {
                    int nextType = tiles.GetType(tmp);
                    if (type == nextType && CheckPotentialBetween(position, Vector2.up, type))
                    {
                        return true;
                    }
                }
                next = new Vector2(x + 1, y);
                if (TileMap.Cell.ToCell(next, out tmp))
                {
                    int nextType = tiles.GetType(tmp);
                    if (type == nextType && (CheckPotentialOnEdge(position, Vector2.left, type) || CheckPotentialOnEdge(next, Vector2.right, type)))
                    {
                        return true;
                    }
                }
                next = new Vector2(x + 2, y);
                if (TileMap.Cell.ToCell(next, out tmp))
                {
                    int nextType = tiles.GetType(tmp);
                    if (type == nextType && CheckPotentialBetween(position, Vector2.right, type))
                    {
                        return true;
                    }
                }
            }
        return false;
    }
    private bool CheckPotentialOnEdge(Vector2 position, Vector2 direction, int type)
    {
        var forward = position + direction;
        var right = new Vector2(direction.y, direction.x);
        var left = -right;

        bool result = false;
        TileMap.Cell tmp;
        if (TileMap.Cell.ToCell(forward + direction, out tmp))
        {
            result |= tiles.GetType(tmp) == type;
        }

        if (TileMap.Cell.ToCell(forward + left, out tmp))
        {
            result |= tiles.GetType(tmp) == type;
        }

        if (TileMap.Cell.ToCell(forward + right, out tmp))
        {
            result |= tiles.GetType(tmp) == type;
        }

        return result;
    }

    private bool CheckPotentialBetween(Vector2 position, Vector2 direction, int type)
    {
        var forward = position + direction;
        var right = new Vector2(direction.y, direction.x);
        var left = -right;

        bool result = false;
        TileMap.Cell tmp;
        if (TileMap.Cell.ToCell(forward + left, out tmp))
        {
            result |= tiles.GetType(tmp) == type;
        }

        if (TileMap.Cell.ToCell(forward + right, out tmp))
        {
            result |= tiles.GetType(tmp) == type;
        }

        return result;
    }
    private void InitSets()
    {
        setX = new Interval(Interval.Orientation.Horizontal);
        setY = new Interval[tiles.width];
        for (int i = 0; i < tiles.width; i++)
        {
            setY[i] = new Interval(Interval.Orientation.Vertical);
        }
    }

    public bool IsAny(out DestructableTiles destructableTiles)
    {
        destructableTiles = new DestructableTiles(Byte.MaxValue, Byte.MaxValue, 0);
        for (Byte y = 0; y < tiles.height; y++)
        {
            for (Byte x = 0; x < tiles.width; x++)
            {
                StackOn(x, y, ref setX, destructableTiles);
                StackOn(x, y, ref setY[x], destructableTiles);
                if (y == tiles.height - 1)
                {
                    Sink(ref setY[x], destructableTiles);
                }
            }
            Sink(ref setX, destructableTiles);
        }
        
         return destructableTiles.destructionList.Count > 0;
    }

    private void StackOn(Byte x, Byte y, ref Interval interval, DestructableTiles destructableTiles)
    {
        SByte type = tiles.GetType(x, y);
        if (interval.count > 0 && interval.type != type)
        {
            Sink(ref interval, destructableTiles);
        }
        if (tiles.IsValid(x, y))
        {
            if (interval.count == 0)
            {
                interval.type = type;
                interval.cell.x = x;
                interval.cell.y = y;
            }
            interval++;
        }
    }

    private void Sink(ref Interval interval, DestructableTiles destructableTiles)
    {
        if (interval.count >= 3)
        {
            Byte maxX = interval.mOrientation == Interval.Orientation.Horizontal ? (Byte)(interval.cell.x + interval.count - 1) : interval.cell.x;
            destructableTiles.minX = Math.Min(destructableTiles.minX, interval.cell.x);
            destructableTiles.maxX = Math.Max(destructableTiles.maxX, maxX);
            destructableTiles.minY = Math.Min(destructableTiles.minY, interval.cell.y);

            foreach (Interval currentInterval in destructableTiles.destructionList)
            {
                TileMap.Cell crossing;
                if (interval.IsCrossing(currentInterval, out crossing))
                {
                    interval.crossing = currentInterval.crossing = true;
                    interval.crossingCell = currentInterval.crossingCell = crossing;
                }
            }
            destructableTiles.destructionList.Add(interval.Clone());
        }
        interval.Reset();
    }

}
