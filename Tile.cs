using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile : MonoBehaviour
{
    [SerializeField]
    public GameObject placeHolder;
    public GameObject container;
    public GameObject content;
    public GameObject blockedContent;
    public GameObject[] toEngulf;
    public LevelGrid.Tile tileType;
    private bool invalid = false;

    #region Drop counter
    public class ScopedCounter : IDisposable
    {
        private Tile t;

        public ScopedCounter(Tile tile)
        {
            t = tile;
            t.dropCount++;
        }

        public void Dispose()
        {
            t.dropCount--;
        }
    }
    private int dropCount = 0;
    #endregion

    public Tile up, down, left, right;

    public delegate void InteractBackgroundHandler(Tile sender, TileMap.BackgroundTileType type);
    public delegate void InteractBlockedHandler(Tile sender, TileMap.BlockedTileType type);
    public delegate void DeleteTileHandler(Tile sender, TileMap.BasicTileType type);
    public delegate void InteractSpetialHandler(Tile sender, TileMap.SpetialType type);

    public event InteractBackgroundHandler backgroundInteracted;
    public event InteractBlockedHandler blockedInteracted;
    public event InteractSpetialHandler spetialInteracted;
    public event DeleteTileHandler contentDeleted;

    public delegate void UpIsEmptyHandler(Tile sender);
    public event UpIsEmptyHandler upIsEmpty;
    public delegate void Processed();
    public event Processed processed;

    private Animator animator;
    private int blockedLevel;

    public bool Invalid 
    { 
        get => invalid; 
        set
        { 
            invalid = value;
            if (invalid)
                WatchDog.Inc();
            else
                WatchDog.Dec();
        } 
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DestroyContent();
    }

    private void Update()
    {
        if (tileType.Blocked() == TileMap.BlockedTileType.Unblocked && Invalid == false && content == null)
        {
            if (up != null)
            {
                up.DropTo(this);
            }
            else 
            {
                upIsEmpty?.Invoke(this);
            }
        }
    }

    public void OnProcessed()
    {
        Invalid = false;
    }
    public void ToEngulf(Byte index, Tile tile)
    {
        processed += tile.OnProcessed;
        toEngulf[index] = tile.Detach();
        tile.tileType.SetMain(TileMap.BasicTileType.None);
        tile.Invalid = true;
    }
    public Coroutine Engulf()
    {
        return StartCoroutine(SyncContents());
    }

    public IEnumerator SyncContents()
    {
        float elapsed = 0f;
        float duration = 0.2f;
        GameObject[] contents = (GameObject[])toEngulf.Clone();
        toEngulf = null;
        Vector3[] InitialOffset = new Vector3[contents.Length];
        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] != null)
            {
                //contents[i].transform.SetParent(container.transform);
                InitialOffset[i] = contents[i].transform.position;
            }
        }

        //var renderer = content.GetComponent<SpriteRenderer>();
        //renderer.sortingOrder++;

        while (elapsed < duration)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] != null)
                {
                    contents[i].transform.position = Vector2.Lerp(InitialOffset[i], container.transform.position, elapsed / duration);
                    if (container.transform.position == contents[i].transform.position)
                    {
                        contents[i].SetActive(false);
                        contents[i] = null;
                    }
                }
                
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] != null)
            {
                contents[i].SetActive(false);
            }
        }
        processed?.Invoke();
        processed = null;
        //renderer.sortingOrder--;
    }

    public bool ExchangeWith(Tile other, Action onExchanged)
    {
        if (content == null || other.content == null)
            return false;

        other.Invalid = true;
        Invalid = true;

        var tmp = other.content;
        other.content = this.content;
        content = tmp;

        content.transform.SetParent(container.transform);
        other.content.transform.SetParent(other.container.transform);

        var type = other.tileType;
        other.tileType = tileType;
        tileType = type;

        StartCoroutine(Swap(other, onExchanged));
        return true;
    }
    public IEnumerator Swap(Tile other, Action onExchanged)
    {
        Coroutine first = StartCoroutine(SyncContent());
        Coroutine second = StartCoroutine(other.SyncContent());

        yield return first;
        yield return second;

        if (onExchanged != null)
        {
            onExchanged();
        }

        Invalid = false;
        other.Invalid = false;
    }
    public bool IsSet()
    {
        return content != null && container.transform.position == content.transform.position;
    }

    

    public IEnumerator SyncContent(bool dropped = false)
    {
        float elapsed = 0f;
        float duration = 0.15f;
        var InitialOffset = content.transform.position;

        while (elapsed < duration && content != null)
        {
            content.transform.position = Vector2.Lerp(InitialOffset, container.transform.position, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (content == null)
        {
            Invalid = false;
            yield break;
        }
        content.transform.position = container.transform.position;

        if (dropped)
        {
            Invalid = false;
            //animator.enabled = true;
            animator.Play("Tile_Droped", -1, 0);
        }

    }

    public void OnDropAnimationEnded()
    {
        //animator.enabled = false;
        if (content != null)
        {
            container.transform.rotation = Quaternion.identity;
            content.transform.rotation = Quaternion.identity;
            content.transform.localScale = Vector3.one * 0.9f;
            content.transform.localPosition = Vector3.zero;
        }
    } 

    public GameObject Detach()
    {
        if (content == null)
        {
            return null;
        }
        StopCoroutine(SyncContent());
        var tmp = content;
        content.transform.SetParent(null);
        content = null;
        tileType.SetMain(TileMap.BasicTileType.None);
        return tmp;
    }

    IEnumerator DelayDrop(float t)
    {
        Invalid = true;
        yield return new WaitForSeconds(t);
        OnProcessed();
    }
    public void DestroyContent()
    {
        var blocked = tileType.Blocked();
        var main = tileType.Main();
        var spetial = tileType.Spetial();
        if (blocked == TileMap.BlockedTileType.Unblocked)
        {
            GameObject obj = Detach();
            if (obj != null)
            {
                if (main != TileMap.BasicTileType.None)
                {
                    if (main == Goals.type)
                    {
                        StartCoroutine(Goals.goals.ToGoal(obj));
                    }
                    else
                    {
                        obj.SetActive(false);
                        if (contentDeleted != null)
                        {
                            contentDeleted(this, main);
                        }
                    }
                }
                else if (spetial != TileMap.SpetialType.None)
                {
                    obj.SetActive(false);
                    if (spetialInteracted != null)
                    {
                        spetialInteracted(this, spetial);
                    }
                }
                StartCoroutine(DelayDrop(.2f));
            }
            if (backgroundInteracted != null)
            {
                backgroundInteracted(this, tileType.Background());
            }
        }
        else if (blocked != TileMap.BlockedTileType.Transparent)
        {
            if (blockedInteracted != null)
            {
                blockedInteracted(this, blocked);
            }
            if (blockedLevel-- == 0)
            {
                tileType.SetBlocked(TileMap.BlockedTileType.Unblocked);
            }
        }
    }

    public bool IsFallable()
    {
        return tileType.Blocked() == TileMap.BlockedTileType.Unblocked;
    }
    public Coroutine DropTo(Tile tileToDrop)
    {
        using (var counter = new ScopedCounter(tileToDrop))
        {
            if (tileToDrop.dropCount > 2)
                return null;

            if (content != null && !Invalid )
            {
                StopCoroutine(SyncContent());
                tileToDrop.StopCoroutine(SyncContent());
                tileToDrop.Invalid = true;
                tileToDrop.content = content;
                tileToDrop.tileType = tileType;
                tileToDrop.content.transform.SetParent(tileToDrop.container.transform);

                content = null;
                tileType.SetMain(TileMap.BasicTileType.None);
                return StartCoroutine(tileToDrop.SyncContent(true));
            }

            if(IsFallable() == false || up != null && up.IsFallable() == false)
            {
                Coroutine res = null;
                if (left != null)
                {
                    res = left.DropTo(tileToDrop);
                }
                if (res == null && right != null)
                {
                    res = right.DropTo(tileToDrop);
                }
                return res;
            }
        }
        return null;
    }


    public Coroutine CreateBlockedContent(TileMap.BlockedTileType type, int level, bool permanent)
    {
        tileType.SetBlocked(type);

        if (type != TileMap.BlockedTileType.Unblocked)
        {
            return null;
        }

        ObjectPool.PooledObject pooledObj = ObjectPool.Instance.GetAlive(type);
        pooledObj.obj.transform.position = container.transform.position;
        blockedContent = pooledObj.obj;
        blockedContent.transform.SetParent(container.transform);

        blockedLevel = permanent ? -1 : level;

        return null;
    }
    public Coroutine CreateContent(TileMap.BasicTileType type, bool dropped, Vector3 offset = default)
    {
        tileType.SetMain(type);

        if (type == TileMap.BasicTileType.None)
        {
            Invalid = false;
            return null;
        }

        ObjectPool.PooledObject pooledObj = ObjectPool.Instance.GetAlive(type);
        pooledObj.obj.transform.position = container.transform.position + offset;
        content = pooledObj.obj;
        content.transform.SetParent(container.transform);

        if (dropped)
        {
            Invalid = true;
            return StartCoroutine(SyncContent(dropped));
        }
        else 
        {
            Invalid = false;
        }
        return null;
    }

    public Coroutine CreateContentSpecial(TileMap.SpetialType type)
    {
        if (type == TileMap.SpetialType.None) 
        {
            return null;
        }
        tileType.SetSpetial(type);

        ObjectPool.PooledObject pooledObj = ObjectPool.Instance.GetAlive(type);
        pooledObj.obj.transform.position = container.transform.position;
        content = pooledObj.obj;
        content.GetComponent<SpriteRenderer>().sortingOrder++;
        content.transform.SetParent(container.transform);
        
        animator.Play("Tile_Special", -1, 0);
        return StartCoroutine(WaitForAnimationDone());
    }

    public IEnumerator WaitForAnimationDone()
    {
        while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
        {
           yield return null;
        }
        content.GetComponent<SpriteRenderer>().sortingOrder--;
    }

}
