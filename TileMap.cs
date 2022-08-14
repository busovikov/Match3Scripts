using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

public class TileMap : MonoBehaviour
{
    public Byte width;
    public Byte height;
    public GameObject prefab;
    [SerializeField] Transform goal;

    public LevelGrid levelGrid;
    private Tile[,] tiles;

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

   public enum AliveType
    {
        Bat = 0,
        Cat = 1,
        Ghost = 2,
        Pumpkin = 3,
        Zombie = 4
    }
    public enum SpecialType
    {
        Rocket_V = 10,
        Rocket_H = 11,
        Caudron = 12,
        Poison_Green = 13,
        Poison_Blue = 14,
        Poison_Black = 15
    }

    public enum SpecialActivatedType
    {
        Rocket_UP = 0,
        Rocket_DN = 1,
        Rocket_LT = 2,
        Rocket_RT = 3,
    }

    void Awake()
    {
        
    }

    private void Start()
    {
        SoundManager soundManager = FindObjectOfType<SoundManager>();
        if (levelGrid != null)
        {
            width = levelGrid.width;
            height = levelGrid.height;
        }
        tiles = new Tile[width, height];

        List<int> types = Enumerable.Range(0, ObjectPool.Instance.alive.sprites.Length).Select((index) => index).ToList();
        for (Byte i = 0; i < width; i++)
            for (Byte j = 0; j < height; j++)
            {
                if (levelGrid != null && levelGrid.tiles[i + j * width] == -1)
                    continue;

                var position = new Vector3(i, j, 0) + transform.position;
                Tile tile = GameObject.Instantiate(prefab, position, Quaternion.identity, transform).GetComponent<Tile>();
                tile.gameObject.name = position.ToString();
                Bind(tile, i, j);
                tile.contentDeleted += OnTileDeleted;
                tile.contentDeleted += soundManager.OnTileDeleted;
                tile.upIsEmpty += OnUpIsEmpty;
                InitContent(i, j, types);
            }
    }

    void Bind(Tile tile, Byte x, Byte y)
    {
        tiles[x, y] = tile;

        int x2 = x - 1;
        
        if (x2 >= 0 && tiles[x2, y] != null)
        {
            tile.left = tiles[x2, y];
            tiles[x2, y].right = tile;
        }

        for (int y2 = y - 1; y2 >= 0; y2--)
        {
            if (tiles[x, y2] != null)
            {
                tile.down = tiles[x, y2];
                tiles[x, y2].up = tile;
                break;
            }
        }
    }
    void OnTileDeleted(Tile sender, SByte type)
    {
        SpawnDead(type, sender.transform.position);
    }

    void OnUpIsEmpty(Tile sender)
    {
        bool dropped = true;
        Create(sender, Vector2.up, dropped);
    }
    public Coroutine Create(Tile tile, Vector2 offset, bool dropped = false)
    {
        SByte index = (SByte)UnityEngine.Random.Range(0, ObjectPool.Instance.alive.sprites.Length);
        return tile.CreateContent(index, dropped, offset);
    }
    public Coroutine Create(Cell position, Vector2 offset, bool dropped = false)
    {
        SByte index = (SByte)UnityEngine.Random.Range(0, ObjectPool.Instance.alive.sprites.Length);
        return GetTile(position).CreateContent(index, dropped, offset);
    }

    public Coroutine CreateSpecial(Cell position, SByte index)
    {
        return GetTile(position).CreateContentSpecial(index);

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
        GetTile(position).CreateContent(index, dropped);
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
        return withInBoundaries && tiles != null && tiles[x, y] != null && !tiles[x, y].Invalid && tiles[x, y].IsSet();
    }

    public void SpawnDead(int tileType, Vector2 position)
    {
        const float rocketSpeed = 15f;
        if (tileType == Goals.type)
        {
            ObjectPool.PooledObject dead = ObjectPool.Instance.GetDead(tileType);
            dead.obj.transform.position = position;
            var toGoal = (Vector2)(goal.position) - position;
            dead.anim.SetTrigger("Dead");
            dead.body.gravityScale = 0;
            dead.body.velocity = toGoal * 2.5f; //, ForceMode2D.Impulse );
        }
        else if (tileType < (SByte)TileMap.SpecialType.Rocket_V)
        {
            ObjectPool.PooledObject dead = ObjectPool.Instance.GetDead(tileType);
            dead.obj.transform.position = position;
            var x = UnityEngine.Random.Range(-1f, 1f);
            var y = UnityEngine.Random.Range(0.1f, 1f);
            dead.body.gravityScale = 1;
            dead.body.velocity = new Vector2(x, y) * 5; //, ForceMode2D.Impulse);
            dead.anim.SetTrigger("Dead");
        }
        else if (tileType == (SByte)TileMap.SpecialType.Rocket_V)
        {
            ObjectPool.PooledObject up = ObjectPool.Instance.GetSpecialActivated((SByte)TileMap.SpecialActivatedType.Rocket_UP);
            ObjectPool.PooledObject down = ObjectPool.Instance.GetSpecialActivated((SByte)TileMap.SpecialActivatedType.Rocket_DN);

            up.obj.transform.position = position;
            up.body.gravityScale = 0;
            up.body.velocity = Vector2.up * rocketSpeed;
            down.obj.transform.position = position;
            down.body.gravityScale = 0;
            down.body.velocity = Vector2.down * rocketSpeed;
        }
        else if (tileType == (SByte)TileMap.SpecialType.Rocket_H)
        {
            ObjectPool.PooledObject left = ObjectPool.Instance.GetSpecialActivated((SByte)TileMap.SpecialActivatedType.Rocket_LT);
            ObjectPool.PooledObject right = ObjectPool.Instance.GetSpecialActivated((SByte)TileMap.SpecialActivatedType.Rocket_RT);
            left.obj.transform.position = position;
            left.body.gravityScale = 0;
            left.body.velocity = Vector2.left * rocketSpeed;
            right.obj.transform.position = position;
            right.body.gravityScale = 0;
            right.body.velocity = Vector2.right * rocketSpeed;
        }

    }
}
