using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public Byte width;
    public Byte height;
    public GameObject goal;
    public GameObject prefab;

    private Tile[,] tiles;
    private int goalType;

    public struct Cell
    {
        public Cell(Byte pX, Byte pY)
        {
            x = pX;
            y = pY;
        }

        public Vector2 ToVector2()
        {
            return new Vector2(x, y);
        }
        public static bool ToCell(Vector2 pos, out Cell cell)
        {
            cell = new Cell(0, 0);
            if (pos.x < 0 || pos.y < 0)
                return false;

            cell.x = (Byte)pos.x;
            cell.y = (Byte)pos.y;
            return true;
        }

        public Byte x;
        public Byte y;
    }

    //private GameObject[,] DeadPool;
    void Awake()
    {
        
    }

    private void Start()
    {
        goalType = goal.GetComponent<Goals>().type;
        tiles = new Tile[width, height];

        List<int> types = Enumerable.Range(0, ObjectPool.Instance.alive.sprites.Length).Select((index) => index).ToList();
        for (Byte i = 0; i < width; i++)
            for (Byte j = 0; j < height; j++)
            {
                var position = new Vector3(i, j, 0) + transform.position;
                Tile tile = GameObject.Instantiate(prefab, position, Quaternion.identity, transform).GetComponent<Tile>();
                tiles[i, j] = tile;
                InitContent(i, j, types);
            }
    }

    public Coroutine Create(Cell position, Vector2 offset, bool dropped = false)
    {
        SByte index = (SByte)UnityEngine.Random.Range(0, ObjectPool.Instance.alive.sprites.Length);
        return GetTile(position).CreateContent(index, dropped, offset);
    }
    private void InitContent(Byte x, Byte y, List<int> types)
    {
        Cell position = new Cell(x, y);
        List<int> allowedTypes = new List<int>(types);
        if (x > 1)
        {
            SByte back = GetType(new Cell((Byte)(x - 1), y));
            if (back == GetType(new Cell((Byte)(x - 2), y)))
            {
                allowedTypes.Remove(back);
            }
        }
        if (y > 1)
        {
            var down = GetType(new Cell(x, (Byte)(y - 1)));
            if (down == GetType(new Cell(x, (Byte)(y - 2))))
            {
                allowedTypes.Remove(down);
            }
        }
        SByte index = (SByte)allowedTypes[UnityEngine.Random.Range(0, allowedTypes.Count)];
        const bool dropped = false;
        GetTile(position).CreateContent( index, dropped);
    }

    public Tile GetTile(Cell position)
    {
        return GetTile(position.x, position.y);
    }
    public Tile GetTile(Byte x, Byte y)
    {
        return tiles[x, y];
    }
    public SByte GetType(Cell arrayPosition)
    {
        return GetType(arrayPosition.x, arrayPosition.y);
    }

    public SByte GetType(Byte x, Byte y)
    {
        if (!IsValid(x, y))
            return -1;
        return tiles[x, y].tileType;
    }
    public bool IsValid(Cell arrayPosition)
    {
        return IsValid(arrayPosition.x, arrayPosition.y);
    }

    public bool IsValid(Byte x, Byte y)
    {
        bool withInBoundaries = y < height && x < width;
        return withInBoundaries && !tiles[x, y].invalid && tiles[x, y].IsSet();
    }

    internal void SpawnSpecial(int tileType, Vector2 position, Vector2 specialPos)
    {
        ObjectPool.PooledObject dead = ObjectPool.Instance.GetDead(tileType);

        dead.obj.transform.position = position;
        //dead.obj.SetActive(true);

        var toGoal = specialPos - position;
        //dead.anim.SetTrigger("Dead");
        dead.body.gravityScale = 0;
        dead.body.velocity = toGoal * 1f; //, ForceMode2D.Impulse );
    }
    public void SpawnDead(int tileType, Vector2 position)
    {
        ObjectPool.PooledObject dead = ObjectPool.Instance.GetDead(tileType);

        dead.obj.transform.position = position;
        //dead.obj.SetActive(true);
        
        if (tileType != goalType)
        {
            var x = UnityEngine.Random.Range(-1f, 1f);
            var y = UnityEngine.Random.Range(0.1f, 1f);
            dead.body.gravityScale = 1;
            dead.body.velocity = new Vector2(x, y) * 5; //, ForceMode2D.Impulse);
            dead.anim.SetTrigger("Dead");
        }
        else
        {
            var toGoal =  (Vector2)(goal.transform.position) - position;
            dead.anim.SetTrigger("Dead");
            dead.body.gravityScale = 0;
            dead.body.velocity = toGoal * 2.5f; //, ForceMode2D.Impulse );
        }
        
    }
}
