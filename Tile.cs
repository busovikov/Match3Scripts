using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile : MonoBehaviour
{
    [SerializeField]
    public GameObject container;
    public GameObject content;
    public SByte tileType;
    public bool invalid = false;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public Coroutine Engulf(GameObject[] toEngulf)
    {
        return StartCoroutine(SwapAnimation(toEngulf));
    }

    public IEnumerator SwapAnimation(GameObject[] toEngulf)
    {
        float elapsed = 0f;
        float duration = 0.15f;
        GameObject[] contents = (GameObject[])toEngulf.Clone();
        Vector3[] InitialOffset = new Vector3[contents.Length];
        for (int i = 0; i < contents.Length; i++)
        {
            if (contents[i] != null)
            {
                InitialOffset[i] = contents[i].transform.position;
            }
        }

        while (elapsed < duration)
        {
            for (int i = 0; i < contents.Length; i++)
            {
                if (contents[i] != null)
                {
                    contents[i].transform.position = Vector2.Lerp(InitialOffset[i], container.transform.position, elapsed / duration);
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
    }

    public bool ExchangeWith(Tile other, Action onExchanged)
    {
        if (content == null || other.content == null)
            return false;

        other.invalid = true;
        invalid = true;

        var tmp = other.content;
        other.content = this.content;
        content = tmp;

        content.transform.SetParent(container.transform);
        other.content.transform.SetParent(other.container.transform);

        var type = other.tileType;
        other.tileType = tileType;
        tileType = type;

        StartCoroutine(SyncContent(other, onExchanged));
        return true;
    }
    public IEnumerator SyncContent(Tile other, Action onExchanged)
    {
        Coroutine first = StartCoroutine(SwapAnimation());
        Coroutine second = StartCoroutine(other.SwapAnimation());

        yield return first;
        yield return second;

        if (onExchanged != null)
        {
            onExchanged();
        }

        invalid = false;
        other.invalid = false;
    }
    public bool IsSet()
    {
        return content != null && container.transform.position == content.transform.position;
    }
    public IEnumerator SwapAnimation(bool dropped = false)
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
            yield break;
        }
        content.transform.position = container.transform.position;

        if (dropped)
        {
            invalid = false;
            animator.enabled = true;
            animator.SetTrigger("Dropped");
        }

    }

    public void OnDropAnimationEnded()
    {
        animator.enabled = false;
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
        StopCoroutine(SwapAnimation());
        var tmp = content;
        tileType = -1;
        content.transform.SetParent(null);
        content = null;
        return tmp;
    }
    public void DestroyContent()
    {
        Detach().SetActive(false);
    }

    public Coroutine DropTo(Tile tileToDrop)
    {
        if (content != null)
        {
            StopCoroutine(SwapAnimation());
            tileToDrop.StopCoroutine(SwapAnimation());
            tileToDrop.invalid = true;
            tileToDrop.content = content;
            tileToDrop.tileType = tileType;
            tileToDrop.content.transform.SetParent(tileToDrop.container.transform);

            content = null;
            tileType = -1;
            return StartCoroutine(tileToDrop.SwapAnimation(true));
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
            invalid = true;
            return StartCoroutine(SwapAnimation(dropped));
        }
        return null;
    }

    public Coroutine CreateContentSpecial(SByte type)
    {
        ObjectPool.PooledObject pooledObj = ObjectPool.Instance.GetSpecial(type);
        pooledObj.obj.transform.position = container.transform.position;
        content = pooledObj.obj;
        content.transform.SetParent(container.transform);
        tileType = (SByte)(type + 10);
        animator.enabled = true;
        animator.SetTrigger("Special");
        return StartCoroutine(WaitForAnimationDone());
    }

    public IEnumerator WaitForAnimationDone()
    {
        yield return new WaitUntil(() => { return animator.enabled == false; });
    }

}
