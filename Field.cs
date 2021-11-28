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

    private Goals goals;
    private TileMap tileMap;
    private Match match;
    private UIManager uiManager;
    private ScoreManager score;
    private LevelManager levelManager;
    private SoundManager soundManager;
    private Boosters boosters;
    private int comboCount = -1;
    private int processing = 0;
    private bool dirty = false;
    private bool checkAvailableNextFrame = false;
    private bool actionAllowed = true;
    private Boosters.Booster rowDestoy = null;

    public Animator highlighter;

    private struct ToSwap
    {
        public bool swapped;
        public Vector2 first;
        public Vector2 second;
    }

    private ToSwap toSwap;

    // Start is called before the first frame update
    void Awake()
    {
        toSwap = new ToSwap();
        boosters = FindObjectOfType<Boosters>();
        boosters.InitBoosters(ActivateBooster, ReleaseBooster);
        uiManager = FindObjectOfType<UIManager>();
        soundManager = FindObjectOfType<SoundManager>();
        tileMap = GetComponent<TileMap>();
        goals = tileMap.goal.GetComponent<Goals>();
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
        var movesOrTime = goals.GetGoalForGameMode(LevelLoader.Instance.mode);
        if (LevelLoader.Instance.mode == LevelLoader.GameMode.Moves)
        {
            levelManager.StartAsMoves(LevelLoader.Instance.levelMoves > 0 ? LevelLoader.Instance.levelMoves : movesOrTime);
        }
        else
        {
            levelManager.StartAsSeconds(LevelLoader.Instance.levelTime > 0 ? LevelLoader.Instance.levelTime : movesOrTime);
        }
    }

    bool Compare(Vector2 v2, Vector3 v3)
    {
        return v2.Equals((Vector2)v3);
    }
    private void Update()
    {

        if (dirty)
        {
            dirty = false;
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
                if (destroy.destructionList != null)
                {
                    foreach (Vector3 v in destroy.destructionList)
                    {
                        if (Compare(toSwap.first, v) || Compare(toSwap.second, v))
                        {
                            swapback = false;
                            break;
                        }
                    }
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
        else
        {
            if (checkAvailableNextFrame && match.SwapsAvailable() == false)
            {
                checkAvailableNextFrame = false;
                uiManager.ShowNoMatches();
                StartCoroutine(Shuffeling());
            }

            if ((goals.reached || !levelManager.Check()) && processing == 0)
            {
                processing++;
                score.AddScore(levelManager.moves * 10 + levelManager.moves);
                
                if (goals.reached)
                {
                    score.SetTotalScore();
                    levelManager.NextLevel();
                }
                else
                    score.current = 0;
                LevelLoader.EndLevel(goals.reached);
            }
        }
    }

    private IEnumerator Shuffeling(bool once = false)
    {
        if (once)
        {
            yield return Reshuffle();
            dirty = true;
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
        if (!tileMap.IsValid(first) || !tileMap.IsValid(second))
            return;
        toSwap.first = first;
        toSwap.second = second;
        tileMap.GetTile(first).ExchangeWith(tileMap.GetTile(second), () => { toSwap.swapped = true; dirty = true; });
    }

    private void SetPosition(Vector2 position)
    {
        Vector2 offsetPosition = ToField(position - (Vector2)colliderCache.transform.position);
        if (actionAllowed && firstPosition != offsetPosition)
        {
            if (rowDestoy!=null)
            {
                highlighter.SetTrigger("Off");
                actionAllowed = false;
                HideSelection();
                DestroyRow(offsetPosition);
                rowDestoy--;
                rowDestoy = null;
            }
            else if (firstPosition != invalidPosition && IsNeighbours(firstPosition, offsetPosition))
            {
                actionAllowed = false;
                HideSelection();
                Swap(firstPosition, offsetPosition);
                firstPosition = invalidPosition;
            }
            else
            {
                firstPosition = offsetPosition;
                ShowSelection(firstPosition);
            }
        }
    }

    private void DestroyRow(Vector2 position)
    {
        if (!tileMap.IsValid(position))
            return;

        Match.DestructableTiles row = new Match.DestructableTiles(0, (int)position.y, tileMap.width - 1); 
        for (int x = 0; x < tileMap.width; x++)
        {
            Vector2 pos = new Vector2(x, position.y);
            tileMap.GetTile(pos).invalid = true;
            row.destructionList.Add(pos);
        }

        StartCoroutine(Processing(row));
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
    public void OnPointerDown(PointerEventData eventData)
    {
        SetPosition(eventData.pointerCurrentRaycast.worldPosition);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        actionAllowed = true;
    }
    public void OnDrag(PointerEventData eventData)
    {
        SetPosition(eventData.pointerCurrentRaycast.worldPosition);
    }
    private IEnumerator Processing(Match.DestructableTiles destroy)
    {
        if (destroy == null || destroy.destructionList.Count == 0)
            yield break;

        processing++;
        comboCount++;
        foreach (var p in destroy.destructionList)
        {
            Tile item = tileMap.GetTile(p);
            tileMap.SpawnDead(item.tileType, item.transform);
            item.DestroyContent();
        }
        
        if (destroy.destructionList.Count > 0)
        {
            int currentScore = Enumerable.Range(1, destroy.destructionList.Count - 3).Select((index) => index).Sum() + destroy.destructionList.Count;
            score.AddScore(currentScore);
            soundManager.PlayPop();
        }

        yield return new WaitForSeconds(0.2f);

        List<Coroutine> dropAll = new List<Coroutine>();
        for (int x = destroy.minX; x <= destroy.maxX; x++)
        {
            dropAll.Add(StartCoroutine(DropAndReplaceWithNew(x, destroy.minY)));
        }

        foreach (Coroutine dropTask in dropAll)
        {
            yield return dropTask;
        }
        
        processing--;
        dirty = true;
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

        int swap_x = 0;
        int swap_y = 0;
        bool swap = false;

        for (int i = 0; i <  tileMap.width * tileMap.height; i++)
        {
            int index_x = UnityEngine.Random.Range(0, remainingPositions.Count);
            int index_y = UnityEngine.Random.Range(1, remainingPositions[index_x].Count);
            int new_y = remainingPositions[index_x][index_y];
            int new_x = remainingPositions[index_x][0];
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
    private IEnumerator DropAndReplaceWithNew(int x, int fromY)
    {
        List<Coroutine> animations = new List<Coroutine>();
        int destroyedCount = 0;

        for (int y = fromY; y < tileMap.height; y++)
        {
            Tile tile = tileMap.GetTile(x, y);
            if (tile.content == null)
            {
                destroyedCount++;
            }
            else if (destroyedCount > 0)
            {
                Tile bottom = tileMap.GetTile(x, y - destroyedCount);
                animations.Add(tile.DropTo(bottom));
            }
        }

        for (int i = 0; i < destroyedCount; i++)
        {
            var position = new Vector2(x, tileMap.height - destroyedCount + i);
            var offset = new Vector2(0, destroyedCount);
            bool dropped = true;
            animations.Add(tileMap.Create(position, offset, dropped));
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