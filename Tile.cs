using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Tile : MonoBehaviour
{
    [SerializeField]
    public GameObject container;
    public GameObject content;
    public int tileType;
    public bool invalid = false;

    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
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
        Coroutine first =  StartCoroutine(SwapAnimation());
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
        
        while(elapsed < duration)
        {
            content.transform.position = Vector2.Lerp(InitialOffset, container.transform.position, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        content.transform.position = container.transform.position;

        if (dropped)
        {
            invalid = false;
            animator.enabled = true;
            content.transform.localPosition = Vector3.zero;
            content.transform.localScale = Vector3.one * 0.9f;
            animator.SetTrigger("Dropped");
        }
        
    }

    public void OnDropAnimationEnded()
    {
        animator.enabled = false;
    }

    public void DestroyContent()
    {
        //StopCoroutine(SwapAnimation());
        tileType = 0;
        content.SetActive(false);
        content.transform.SetParent(null);
        content = null;
    }

    public Coroutine DropTo(Tile tileToDrop)
    {
        if (content != null)
        {
            //StopCoroutine(SwapAnimation());
            tileToDrop.StopCoroutine(SwapAnimation());
            tileToDrop.invalid = true;
            tileToDrop.content = content;
            tileToDrop.tileType = tileType;
            tileToDrop.content.transform.SetParent(tileToDrop.container.transform);

            content = null;
            tileType = 0;
            return StartCoroutine(tileToDrop.SwapAnimation(true));
        }
        return null;
    }
    public Coroutine CreateContent(int type, bool dropped, Vector3 offset = default)
    {
        //content = Instantiate(prefab, container.transform.position + offset, Quaternion.identity, container.transform);
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


}
