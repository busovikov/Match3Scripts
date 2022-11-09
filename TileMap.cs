using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Mathematics;

public class TileMap : MonoBehaviour
{
    [HideInInspector]
    public BoxCollider2D colliderCache;

#if UNITY_EDITOR
    public TileType currentTile;
    public LevelGrid currentLevel;
    public Dictionary<System.Enum, TileType> gizmos;
#endif
    public Vector2 size;
    public Vector2 offset;
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
    [System.Serializable]
    public enum BlockedTileType
    {
        Gravestone,
        CartonBox,
        WoodenBox,
        Chain,
        Web,
        Ice,
        Snowdrift,


        Transparent = 254,

        Unblocked = 255
    }
    [System.Serializable]
    public enum BackgroundTileType
    {
        Carpet,

        NoBackground = 255
    }
    [System.Serializable]
    public enum BasicTileType
    {
        Bat,
        Cat,
        Ghost,
        Pumpkin,
        Zombie,
        TypeSize,

        Random = 254,

        None = 255
    }

    [System.Serializable]
    public enum SpetialType
    {
        Rocket_V,
        Rocket_H,
        Caudlron,
        Poison_Green,
        Poison_Blue,
        Poison_Black,

        Rocket_UP,
        Rocket_DN,
        Rocket_LT,
        Rocket_RT,

        None = 255
    }

    void Awake()
    {
        
    }

    public void UpdateSize()
    {
        if (currentLevel == null)
            return;
        size = new Vector2(currentLevel.width, currentLevel.height);
        offset = size / 2 - Vector2.one / 2;

        colliderCache = GetComponent<BoxCollider2D>();
        colliderCache.size = size;
    }
    public void StartLevel()
    {
        if (levelGrid == null)
        {
            Debug.Log("Level Grid not set");
            return;
        }
        SoundManager soundManager = FindObjectOfType<SoundManager>();

        UpdateSize();

        tiles = new Tile[levelGrid.width, levelGrid.height];

        for (Byte j = 0; j < levelGrid.height; j++)
            for (Byte i = 0; i < levelGrid.width; i++)
            {
                var tileType = levelGrid.tiles[i + j * levelGrid.width];
                var position = new Vector2(i, j) + (Vector2)transform.position - offset;
                Tile tile = GameObject.Instantiate(prefab, position, Quaternion.identity, transform).GetComponent<Tile>();
                tile.gameObject.name = new Vector2(i, j).ToString();

                tiles[i, j] = tile;

                InitContent(i, j, tileType);

                if (SkipTile(tile))
                    continue;

                Bind(tile, i, j);
                tile.upIsEmpty += OnUpIsEmpty;
                tile.backgroundInteracted += OnBackgroundInteract;
                tile.blockedInteracted += OnBlockedInteract;
                tile.spetialInteracted += OnSpetialActivated;
                tile.contentDeleted += OnTileDeleted;
                tile.contentDeleted += soundManager.OnTileDeleted;
            }
    }

    private void Update()
    {
        //for (Byte j = 0; j < levelGrid.height; j++)
            //for (Byte i = 0; i < levelGrid.width; i++)
                //GetTile(i, j).Drop();
    }
    private void Start()
    {
        
        

        
    }

    bool SkipTile(Tile t)
    {
        return t.tileType.Main() == BasicTileType.None && t.tileType.Spetial() == SpetialType.None && t.tileType.Blocked() == BlockedTileType.Unblocked; 
    }
    void Bind(Tile tile, Byte x, Byte y)
    {
        int x2 = x - 1;

        if (x2 >= 0 && tiles[x2, y] != null)
        {
            tile.left = tiles[x2, y];
            tiles[x2, y].right = tile;
        }

        for (int y2 = y - 1; y2 >= 0; y2--)
        {
            if (tiles[x, y2] == null || SkipTile(tiles[x, y2])) continue;

            tile.down = tiles[x, y2];
            tiles[x, y2].up = tile;
            break;
        }
    }

    void OnSpetialActivated(Tile sender, SpetialType type)
    {
        const float rocketSpeed = 20f;
        var position = sender.container.transform.position;
        if (type == SpetialType.Rocket_V)
        {
            ObjectPool.PooledObject up = ObjectPool.Instance.GetSpecialActivated(SpetialType.Rocket_UP);
            ObjectPool.PooledObject down = ObjectPool.Instance.GetSpecialActivated(SpetialType.Rocket_DN);

            up.obj.transform.position = position;
            up.body.gravityScale = 0;
            up.body.velocity = Vector2.up * rocketSpeed;
            down.obj.transform.position = position;
            down.body.gravityScale = 0;
            down.body.velocity = Vector2.down * rocketSpeed;
        }
        else if (type == SpetialType.Rocket_H)
        {
            ObjectPool.PooledObject left = ObjectPool.Instance.GetSpecialActivated(SpetialType.Rocket_LT);
            ObjectPool.PooledObject right = ObjectPool.Instance.GetSpecialActivated(SpetialType.Rocket_RT);
            left.obj.transform.position = position;
            left.body.gravityScale = 0;
            left.body.velocity = Vector2.left * rocketSpeed;
            right.obj.transform.position = position;
            right.body.gravityScale = 0;
            right.body.velocity = Vector2.right * rocketSpeed;
        }
    }

    void OnTileDeleted(Tile sender, BasicTileType type)
    {
        ObjectPool.PooledObject dead = ObjectPool.Instance.GetDead(type);
        dead.obj.transform.position = sender.container.transform.position;
        var x = UnityEngine.Random.Range(-1f, 1f);
        var y = UnityEngine.Random.Range(0.1f, 1f);
        dead.body.gravityScale = 1;
        dead.body.velocity = new Vector2(x, y) * 5; //, ForceMode2D.Impulse);
        dead.anim.SetTrigger("Dead");
    }

    void OnBlockedInteract(Tile sender, BlockedTileType type)
    {
        Debug.Log("Blocked: " + type.ToString());
    }

    void OnBackgroundInteract(Tile sender, BackgroundTileType type)
    {
        Debug.Log("BackGround: " + type.ToString());
    }

    void OnUpIsEmpty(Tile sender, int offset)
    {
        bool dropped = true;
        Create(sender, Vector2.up * offset, dropped);
    }
    public Coroutine Create(Tile tile, Vector2 offset, bool dropped = false)
    {
        var index = (BasicTileType)UnityEngine.Random.Range(0, (int)BasicTileType.TypeSize);
        return tile.CreateContent(index, dropped, offset);
    }
    public Coroutine Create(Cell position, Vector2 offset, bool dropped = false)
    {
        var index = (BasicTileType)UnityEngine.Random.Range(0, (int)BasicTileType.TypeSize);
        return GetTile(position).CreateContent(index, dropped, offset);
    }

    public Coroutine CreateSpecial(Cell position, SpetialType index)
    {
        return GetTile(position).CreateContentSpecial(index);

    }
    private void InitContent(Byte x, Byte y, LevelGrid.Tile t)
    {
        var tile = GetTile(new Cell(x, y));
        var main = t.Main();
        var spetial = t.Spetial();
        var blocked = t.Blocked();
        if (main == BasicTileType.Random)
        {
            List<BasicTileType> allowedTypes = Enumerable.Range(0, (int)BasicTileType.TypeSize).Select((index) => (BasicTileType)index).ToList<BasicTileType>();
            if (x > 1)
            {
                var back = GetType(new Cell((Byte)(x - 1), y));
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
            main = allowedTypes[UnityEngine.Random.Range(0, allowedTypes.Count)];
        }
        
        const bool dropped = false;

        tile.CreateBlockedContent(t.Blocked(), 3, false);
        tile.CreateContent(main, dropped);
        tile.CreateContentSpecial(spetial);
        tile.placeHolder.gameObject.SetActive(main != BasicTileType.None || spetial != SpetialType.None || blocked != BlockedTileType.Unblocked && blocked != BlockedTileType.Transparent);
    }

    public IEnumerator Reshuffle()
    {
        int shuffeling = 0;
        var until = new WaitUntil(() => shuffeling == 0);
        List<int> axis_y = Enumerable.Range(0, levelGrid.height + 1).Select((index) => index - 1).ToList();
        List<List<int>> remainingPositions = new List<List<int>>();

        for (int i = 0; i < levelGrid.width; i++)
        {
            axis_y[0] = i;
            remainingPositions.Add(new List<int>(axis_y));
        }

        Byte swap_x = 0;
        Byte swap_y = 0;
        bool swap = false;

        for (int i = 0; i < levelGrid.width * levelGrid.height; i++)
        {
            int index_x = UnityEngine.Random.Range(0, remainingPositions.Count);
            int index_y = UnityEngine.Random.Range(1, remainingPositions[index_x].Count);
            Byte new_y = (Byte)remainingPositions[index_x][index_y];
            Byte new_x = (Byte)remainingPositions[index_x][0];
            remainingPositions[index_x].RemoveAt(index_y);
            if (remainingPositions[index_x].Count == 1)
            {
                remainingPositions.RemoveAt(index_x);
            }

            if (swap)
            {
                shuffeling++;
                if (!GetTile(swap_x, swap_y).ExchangeWith(GetTile(new_x, new_y), () => { shuffeling--; }))
                {
                    continue;
                }
            }
            else
            {
                swap_x = new_x;
                swap_y = new_y;
            }

            swap = !swap;
        }
        yield return until;
    }

    public Match.DestructableTiles GetIntervalRow(TileMap.Cell position)
    {
        if (!IsValid(position))
            return null;

        Match.DestructableTiles row = new Match.DestructableTiles(0, (Byte)position.y, (Byte)(levelGrid.width - 1));
        Match.Interval interval = new Match.Interval(Match.Interval.Orientation.Horizontal);
        interval.cell.x = 0;
        interval.cell.y = (Byte)position.y;
        interval.count = (Byte)levelGrid.width;
        row.destructionList.Add(interval);

        return row;
    }

    public Match.DestructableTiles GetIntervalColumn(TileMap.Cell position)
    {
        if (!IsValid(position))
            return null;

        Match.DestructableTiles column = new Match.DestructableTiles((Byte)position.x, 0, (Byte)position.x);
        Match.Interval interval = new Match.Interval(Match.Interval.Orientation.Vertical);
        interval.cell.x = (Byte)position.x;
        interval.cell.y = 0;
        interval.count = (Byte)levelGrid.height;
        column.destructionList.Add(interval);

        return column;
    }

    public Tile GetTile(Cell position)
    {
        return GetTile(position.x, position.y);
    }
    public Tile GetTile(Byte x, Byte y)
    {
        return tiles[x, y];
    }
    public BasicTileType GetType(Cell arrayPosition)
    {
        return GetType(arrayPosition.x, arrayPosition.y);
    }

    public BasicTileType GetType(Byte x, Byte y)
    {
        if (!IsValid(x, y))
            return BasicTileType.None;
        return tiles[x, y].tileType.Main();
    }
    public bool IsValid(Cell arrayPosition)
    {
        return IsValid(arrayPosition.x, arrayPosition.y);
    }

    public bool IsValid(Byte x, Byte y)
    {
        bool withInBoundaries = y < levelGrid.height && x < levelGrid.width;
        return withInBoundaries && tiles != null && tiles[x, y] != null && !tiles[x, y].Invalid && tiles[x, y].IsSet();
    }


#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        if (currentLevel == null || currentLevel.tiles == null || gizmos == null)
            return;

        var _size = new Vector2(currentLevel.width, currentLevel.height);
        Vector3 _offset = _size / 2 - Vector2.one / 2;

        var position = transform.position - _offset;
        Rect rect = new Rect(position.x, position.y, currentLevel.width, currentLevel.height);

        for (byte i = 0; i < currentLevel.width; ++i)
            for (byte j = 0; j < currentLevel.height; ++j)
            {
                int index = i + currentLevel.width * j;
                Vector3 pos = new Vector3(i, j) + position;
                int x = (int)math.round(pos.x);
                int y = (int)math.round(pos.y);

                Gizmos.color = new Color(0.1176471f, (i + j) % 2 == 0 ? 0.1019608f : 0.1419608f, .1960784f, .7843137f);
                Gizmos.DrawCube(pos, Vector3.one);

                foreach (var e in currentLevel.tiles[index].GetfTypes())
                {
                    TileType tile;
                    if (gizmos.TryGetValue(e, out tile))
                    {
                        Gizmos.DrawIcon(pos, tile.name);
                    }
                }
            }
    }

#endif
}
