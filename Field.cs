using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Linq;
public class Field : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public GameObject selection;

    [HideInInspector]
    public BoxCollider2D colliderCache;
    private static Vector2 invalidPosition = Vector2.left;
    private Vector2 firstPosition = invalidPosition;

    private TileMap tileMap;
    private Match match;
    private UIManager uiManager;
    private ScoreManager score;
    private LevelManager levelManager;
    private Boosters boosters;
    private int comboCount = -1;
    private float lastTimeClick = 0f;
    private bool checkAvailableNextFrame = false;
    private bool actionAllowed = true;
    private Boosters.Booster rowDestoy = null;

    public Animator highlighter;

    private struct DeathTask
    {
        public SByte type;
        public Vector2 tile;
        public Vector2 spetial;
    }
    private struct ToSwap
    {
        public bool swapped;
        public TileMap.Cell first;
        public TileMap.Cell second;
    }
    private ToSwap toSwap;

    // Start is called before the first frame update
    void Awake()
    {
        toSwap = new ToSwap();
        boosters = FindObjectOfType<Boosters>();
        boosters.InitBoosters(ActivateBooster, ReleaseBooster);
        uiManager = FindObjectOfType<UIManager>();
        tileMap = GetComponent<TileMap>();
        match = new Match(tileMap);

        score = FindObjectOfType<ScoreManager>();
        levelManager = FindObjectOfType<LevelManager>();

        colliderCache = GetComponent<BoxCollider2D>();
        colliderCache.size = new Vector2(tileMap.width, tileMap.height);
        colliderCache.offset = colliderCache.size / 2 - Vector2.one / 2;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
    }

    bool Compare(Vector2 v2, Vector3 v3)
    {
        return v2.Equals((Vector2)v3);
    }
    private void Update()
    {

        if (WatchDog.CheckIfDirtyAndReset())
        {
            Debug.Log("dirty");
            Match.DestructableTiles destroy;
            if (match.IsAny(out destroy))
            {
                StartCoroutine(Processing(destroy));
            }
            else
            {
                if (comboCount >= 0)
                {
                    score.AddCombo(comboCount);
                    score.AddScore(10 * comboCount);
                    comboCount = -1;
                }

                checkAvailableNextFrame = true;
            }

            if (toSwap.swapped)
            {
                bool swapback = true;
                bool firstDestroyed = false;
                bool secondDestroyed = false;

                if (destroy.destructionList != null && destroy.destructionList.Count > 0)
                {
                    foreach (Match.Interval i in destroy.destructionList)
                    {
                        firstDestroyed |= i.Belongs(toSwap.first.x, toSwap.first.y);
                        secondDestroyed |= i.Belongs(toSwap.second.x, toSwap.second.y);
                        if (firstDestroyed || secondDestroyed)
                        {
                            swapback = false;
                        }
                    }
                }
                if (!firstDestroyed && ProcessIfCpecial(toSwap.first))
                {
                    swapback = false;
                }
                if (!secondDestroyed && ProcessIfCpecial(toSwap.second))
                {
                    swapback = false;
                }

                if (swapback)
                {
                    tileMap.GetTile(toSwap.first).ExchangeWith(tileMap.GetTile(toSwap.second), null);
                }
                else if (LevelLoader.Instance.mode == LevelLoader.GameMode.Moves)
                {
                    levelManager.SubMoves(1);
                }

                toSwap.swapped = false;
            }
        }
        else if (checkAvailableNextFrame && match != null && match.SwapsAvailable() == false)
        {
            checkAvailableNextFrame = false;
            uiManager.ShowNoMatches();
            StartCoroutine(Shuffeling());
        }
    }

    private IEnumerator Shuffeling(bool once = false)
    {
        if (once)
        {
            yield return Reshuffle();
        }
        else
        {
            Match.DestructableTiles destroy;
            while (match.IsAny(out destroy) || match.SwapsAvailable() == false)
            {
                yield return Reshuffle();
            }
        }
    }

    private void ReleaseBooster(Boosters.Booster booster)
    {
        if (booster.type == Boosters.BoosterType.Erase && rowDestoy != null)
        {
            highlighter.SetTrigger("Off");
            rowDestoy = null;
        }
    }

    public void ActivateBooster(Boosters.Booster booster)
    {
        if (booster.type == Boosters.BoosterType.Mix)
        {
            booster--;
            StartCoroutine(Shuffeling(true)); // true - Once
        }
        else if (booster.type == Boosters.BoosterType.Erase)
        {
            highlighter.enabled = true;
            highlighter.SetTrigger("On");
            rowDestoy = booster;
        }
        else if (booster.type == Boosters.BoosterType.Add)
        {
            booster--;
            if (LevelLoader.Instance.mode == LevelLoader.GameMode.Moves)
            {
                levelManager.AddMoves(2);
            }
            else
            {
                levelManager.AddMoves(5);
            }
        }
    }

    private void Swap(Vector2 first, Vector2 second)
    {
        TileMap.Cell firstCell;
        TileMap.Cell secondCell;
        if (!TileMap.Cell.ToCell(first, out firstCell) ||
            !TileMap.Cell.ToCell(second, out secondCell) ||
            !tileMap.IsValid(firstCell) ||
            !tileMap.IsValid(secondCell))
        {
            return;
        }
        toSwap.first = firstCell;
        toSwap.second = secondCell;
        tileMap.GetTile(firstCell).ExchangeWith(tileMap.GetTile(secondCell), () => { toSwap.swapped = true; });
    }

    private void SetPosition(Vector2 position, bool drag)
    {
        Vector2 offsetPosition = ToField(position - (Vector2)colliderCache.transform.position);
        bool tapped = IsTapped();
        if (actionAllowed && firstPosition != offsetPosition)
        {
            if (rowDestoy != null)
            {
                highlighter.SetTrigger("Off");
                actionAllowed = false;
                HideSelection();
                TileMap.Cell cell;
                if (TileMap.Cell.ToCell(offsetPosition, out cell))
                {
                    DestroyRow(cell);
                    rowDestoy--;
                    rowDestoy = null;
                }
            }
            else if (firstPosition != invalidPosition && IsNeighbours(firstPosition, offsetPosition))
            {
                actionAllowed = false;
                HideSelection();
                Swap(firstPosition,offsetPosition);
                firstPosition = invalidPosition;
            }
            else
            {
                firstPosition = offsetPosition;
                ShowSelection(firstPosition);
            }
        }
        else if (actionAllowed && firstPosition == offsetPosition  && !drag && tapped)
        {
            TileMap.Cell cell;
            if (TileMap.Cell.ToCell(offsetPosition, out cell))
            {
                ProcessIfCpecial(cell);
            }
        }
    }

    private void DestroyRow(TileMap.Cell position)
    {
        if (!tileMap.IsValid(position))
            return;

        Match.DestructableTiles row = new Match.DestructableTiles(0, (Byte)position.y, (Byte)(tileMap.width - 1));
        Match.Interval interval = new Match.Interval(Match.Interval.Orientation.Horizontal);
        interval.cell.x = 0;
        interval.cell.y = (Byte)position.y;
        interval.count = (Byte)tileMap.width;
        row.destructionList.Add(interval);

        StartCoroutine(Processing(row, false));
    }

    private void DestroyColumn(TileMap.Cell position)
    {
        if (!tileMap.IsValid(position))
            return;

        Match.DestructableTiles column = new Match.DestructableTiles((Byte)position.x, 0, (Byte)position.x);
        Match.Interval interval = new Match.Interval(Match.Interval.Orientation.Vertical);
        interval.cell.x = (Byte)position.x;
        interval.cell.y = 0;
        interval.count = (Byte)tileMap.height;
        column.destructionList.Add(interval);

        StartCoroutine(Processing(column, false));
    }

    private bool ProcessIfCpecial(params TileMap.Cell[] cells)
    {
        bool any = false;
        foreach (TileMap.Cell cell in cells)
        {
            any |= ActivateIfSpecial(cell);
        }

        return any;
    }
    private bool ActivateIfSpecial(TileMap.Cell cell)
    {
        var type = tileMap.GetTile(cell).tileType.Main();
        if (type == TileMap.BasicTileType.Rocket_V)
        {
            var item = tileMap.GetTile(cell);
            item.DestroyContent();
        }
        else if (type == TileMap.BasicTileType.Rocket_H)
        {
            var item = tileMap.GetTile(cell);
            item.DestroyContent();
        }
        else if (type == TileMap.BasicTileType.Caudron)
        {
        }
        else if (type == TileMap.BasicTileType.Poison_Green)
        {
        }
        else if (type == TileMap.BasicTileType.Poison_Blue)
        {
        }
        else if (type == TileMap.BasicTileType.Poison_Black)
        {
        }
        else
        {
            return false;
        }
        return true;
    }

    private Vector2 ToField(Vector2 position)
    {
        return new Vector2((float)Math.Round(position.x), (float)Math.Round(position.y));
    }
    private bool IsNeighbours(Vector2 firstPosition, Vector2 secondPosition)
    {
        var xOffset = Math.Abs(firstPosition.x - secondPosition.x);
        var yOffset = Math.Abs(firstPosition.y - secondPosition.y);

        return xOffset == 1 && yOffset == 0 || xOffset == 0 && yOffset == 1;
    }

    bool IsTapped()
    {
        float currentTimeClick = Time.realtimeSinceStartup;
        float last = lastTimeClick;
        lastTimeClick = currentTimeClick;
        float diff = Mathf.Abs(currentTimeClick - last);
        if (diff < 0.5f)
        {
            return true;
        }
        return false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        SetPosition(eventData.pointerCurrentRaycast.worldPosition, false);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        actionAllowed = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        SetPosition(eventData.pointerCurrentRaycast.worldPosition, true);
    }

    bool IsSpecial(Match.Interval interval, out TileMap.Cell specialPos)
    {
        if (interval.crossing)
        {
            specialPos = interval.crossingCell;
            return true;
        }
        
        if (interval.count > 3)
        {
            if (interval.Belongs(toSwap.first.x, toSwap.first.y))
            {
                specialPos = toSwap.first;
            }
            else if (interval.Belongs(toSwap.second.x, toSwap.second.y))
            {
                specialPos = toSwap.second;
            }
            else
            {
                specialPos = interval.cell;
            }
            return true;
        }
        specialPos = new TileMap.Cell();
        return false;
    }

    private IEnumerator Processing(Match.DestructableTiles destroy, bool checkSpecial = true)
    {
        if (destroy == null || destroy.destructionList.Count == 0)
            yield break;

        comboCount++;
        Dictionary<TileMap.Cell, Byte> specialCount = new Dictionary<TileMap.Cell, Byte>();
        Byte destroyed = 0;
        foreach (Match.Interval interval in destroy.destructionList)
        {
            TileMap.Cell specialPos = new TileMap.Cell();
            bool special = checkSpecial && IsSpecial(interval, out specialPos);
            Tile specialTile = tileMap.GetTile(specialPos);
            if(special)
                specialTile.toEngulf = new GameObject[interval.count];

            for (Byte i = 0; i < interval.count; i++)
            {
                Byte x = interval.mOrientation == Match.Interval.Orientation.Horizontal ? (Byte)(interval.cell.x + i) : interval.cell.x;
                Byte y = interval.mOrientation == Match.Interval.Orientation.Horizontal ? interval.cell.y : (Byte)(interval.cell.y + i);
                Tile item = tileMap.GetTile(x, y);

                if (item.tileType.Main() == TileMap.BasicTileType.None)
                {
                    continue;
                }
                if (special)
                {
                    specialTile.ToEngulf(i, item);
                    continue;
                }
                item.DestroyContent();
                destroyed += interval.count;
            }
            if (special)
            {
                if (!specialCount.ContainsKey(specialPos))
                {
                    specialCount.Add(specialPos, 0);
                }
                if (interval.count == 4 && !interval.crossing && interval.mOrientation == Match.Interval.Orientation.Horizontal)
                {
                    specialCount[specialPos] += 3; 
                }
                else
                {
                    specialCount[specialPos] += interval.count;
                }

                specialTile.Engulf();
            }
        }


        List<Coroutine> engulfAll = new List<Coroutine>();
        foreach (var specialPair in specialCount)
        {
            engulfAll.Add( tileMap.CreateSpecial(specialPair.Key, (TileMap.BasicTileType)(specialPair.Value - 3))); // can not go lower than 0
        }
        foreach (Coroutine dropTask in engulfAll)
        {
            yield return engulfAll;
        }

        if (destroyed > 0)
        {
            int currentScore = Enumerable.Range(1, destroyed - 3).Select((index) => index).Sum() + destroyed;
            score.AddScore(currentScore);
        }
        
    }

    
    private IEnumerator Reshuffle()
    {
        int shuffeling = 0;
        var until = new WaitUntil(() => shuffeling == 0);
        List<int> axis_y = Enumerable.Range(0, tileMap.height + 1).Select((index) => index - 1).ToList();
        List<List<int>> remainingPositions = new List<List<int>>();

        for (int i = 0; i < tileMap.width; i++)
        {
            axis_y[0] = i;
            remainingPositions.Add(new List<int>(axis_y));
        }

        Byte swap_x = 0;
        Byte swap_y = 0;
        bool swap = false;

        for (int i = 0; i <  tileMap.width * tileMap.height; i++)
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
                if (!tileMap.GetTile(swap_x, swap_y).ExchangeWith(tileMap.GetTile(new_x, new_y), () => { shuffeling--;  }))
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
    private IEnumerator DropAndReplaceWithNew(Byte x, Byte fromY)
    {
        var pause = new WaitForSeconds(0.03f);
        List<Coroutine> animations = new List<Coroutine>();
        int destroyedCount = 0;

        for (Byte y = fromY; y < tileMap.height; y++)
        {
            Tile tile = tileMap.GetTile(x, y);
            if (tile.content == null)
            {
                destroyedCount++;
            }
            else if (destroyedCount > 0)
            {
                Tile bottom = tileMap.GetTile(x, (Byte)(y - destroyedCount));
                animations.Add(tile.DropTo(bottom));
            }
            yield return pause;
        }

        for (int i = 0; i < destroyedCount; i++)
        {
            var position = new TileMap.Cell(x, (Byte)(tileMap.height - destroyedCount + i));
            var offset = new Vector2(0, destroyedCount);
            bool dropped = true;
            animations.Add(tileMap.Create(position, offset, dropped));
            yield return pause;
        }

        // Wait for all animations are done
        foreach (Coroutine animation in animations)
        {
            yield return animation;
        }
    }

    public void ShowSelection(Vector3 position)
    {
        selection.SetActive(true);
        selection.transform.position = position + transform.position;
    }

    public void HideSelection()
    {
        selection.SetActive(false);
    }
}