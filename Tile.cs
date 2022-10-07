using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile : MonoBehaviour
{
    [SerializeField]
    public GameObject container;
    public GameObject content;
    public GameObject[] toEngulf;
    public SByte tileType;
    private bool invalid = false;

    public Tile up, down, left, right;
    public delegate void DeleteTileHandler(Tile sender, SByte type);
    public event DeleteTileHandler contentDeleted;
    public delegate void UpIsEmptyHandler(Tile sender);
    public event UpIsEmptyHandler upIsEmpty;
    public delegate void Engulfed();
    public event Engulfed engulfed;

    private Animator animator;

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
        if (Invalid == false && content == null)
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

    public void OnEngulfed()
    {
        Invalid = false;
    }
    public void ToEngulf(Byte index, Tile tile)
    {
        engulfed += tile.OnEngulfed;
        toEngulf[index] = tile.Detach();
        tile.Invalid = true;
    }
    public Coroutine Engulf()
    {
        return StartCoroutine(SyncContents());
    }

    public IEnumerator SyncContents()
    {
        float elapsed = 0f;
        float duration = 0.35f;
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
        engulfed?.Invoke();
        engulfed = null;
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
        float duration = 0.2f;
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
        tileType = -1;
        content.transform.SetParent(null);
        content = null;
        return tmp;
    }
    public void DestroyContent()
    {
        SByte type = tileType;
        GameObject obj = Detach();
        if (obj != null)
        {
            if (contentDeleted != null)
            {
                contentDeleted(this, type);
            }
            obj.SetActive(false);
        }
    }

    public Coroutine DropTo(Tile tileToDrop)
    {
        if (content != null)
        {
            StopCoroutine(SyncContent());
            tileToDrop.StopCoroutine(SyncContent());
            tileToDrop.Invalid = true;
            tileToDrop.content = content;
            tileToDrop.tileType = tileType;
            tileToDrop.content.transform.SetParent(tileToDrop.container.transform);

            content = null;
            tileType = -1;
            return StartCoroutine(tileToDrop.SyncContent(true));
        }
        return null;
    }
    public Coroutine CreateContent(SByte type, bool dropped, Vector3 offset = default)
    {
        ObjectPool.PooledObject pooledObj = ObjectPool.Instance.GetAlive(type);
        pooledObj.obj.transform.position = container.transform.position + offset;
        content = pooledObj.obj;
        content.transform.SetParent(container.transform);
        tileType = type;

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

    public Coroutine CreateContentSpecial(SByte type)
    {
        ObjectPool.PooledObject pooledObj = ObjectPool.Instance.GetSpecial(type);
        pooledObj.obj.transform.position = container.transform.position;
        content = pooledObj.obj;
        content.GetComponent<SpriteRenderer>().sortingOrder++;
        content.transform.SetParent(container.transform);
        tileType = (SByte)(type + 10);
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
