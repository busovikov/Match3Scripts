using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    #region
    [HideInInspector]
    public static ObjectPool Instance;

    public TileSet tileSet;
    private Dictionary<object, Sprite> textures;
    private Dictionary<object, Sprite> texturesDestroyed;
    public struct PooledObject
    {
        public GameObject obj;
        public SpriteRenderer spriteRenderer;
        public Rigidbody2D body;
        public Animator anim;
    }

    public interface IPooled
    {
        IList<PooledObject> Pool { get; }
        GameObject Prefab { get; }
        int Index { set; get; }
        int Amount { get; }
        Sprite GetSprite(int i);

        void Init();
    }

    [System.Serializable]
    public class Pooled : IPooled
    {
        private List<PooledObject> pool;
        public int amount = 10;

        [SerializeField]
        public int Amount { get => amount; }
        public Sprite[] sprites;
        public GameObject prefab;
        public int Index { set; get; }

        public IList<PooledObject> Pool => pool;

        public GameObject Prefab => prefab;

        public Sprite GetSprite(int i)
        {
            if (i < 0 || i >= sprites.Length)
                return null;
            return sprites[i];
        }

        public void Init()
        {
            pool = new List<PooledObject>(amount);
        }
    }
    #endregion

    public Pooled dead;
    public Pooled alive;
    public Pooled blocked;
    public Pooled special;
    public Pooled specialActivated;

    void Awake()
    {
        Instance = FindObjectOfType<ObjectPool>();
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitPool(dead);
        loadTextures();
    }

    private void loadTextures()
    {
        textures = new Dictionary<object, Sprite>();
        texturesDestroyed = new Dictionary<object, Sprite>();

        Assembly asm = typeof(TileMap).Assembly;
        

        foreach (var t in tileSet.tiles)
        {
            var tex1 = t.type.image;
            var tex2 = t.type.destructionImage;
            Type type = asm.GetType(t.category);
            object o;
            if (Enum.TryParse(type, t.value, out o))
            {
                if (tex1 != null)
                {
                    textures[o] = Sprite.Create(tex1, new Rect(0, 0, tex1.width, tex1.height), new Vector2(.5f, .5f), 512f);
                }
                if (tex2 != null)
                    texturesDestroyed[o] = Sprite.Create(tex2, new Rect(0, 0, tex2.width, tex2.height), new Vector2(.5f, .5f));
            }
        }
    }

    PooledObject MakePooled(GameObject prefab)
    {
        PooledObject pooledObject;
        pooledObject.obj = Instantiate(prefab);
        pooledObject.body = pooledObject.obj.GetComponent<Rigidbody2D>();
        pooledObject.anim = pooledObject.obj.GetComponent<Animator>();
        pooledObject.spriteRenderer = pooledObject.obj.GetComponent<SpriteRenderer>();
        pooledObject.obj.SetActive(false);
        return pooledObject;
    }
    void InitPool<T>(T pooled) where T : IPooled
    {
        pooled.Init();
        for (int i = 0; i < pooled.Amount; i++)
        {
            pooled.Pool.Add(MakePooled(pooled.Prefab));
        }
    }

    int AddToPool<T>(int amount, T pooled) where T : IPooled
    {
        for (int i = 0; i < amount; i++)
        {
            pooled.Pool.Add(MakePooled(pooled.Prefab));
        }
        return pooled.Pool.Count - amount;
    }
    
    PooledObject Get<T>(T pooled) where T : IPooled
    {
        if (pooled.Pool == null)
        {
            InitPool(pooled);
        }
        int amount = pooled.Pool.Count;
        int offset = pooled.Index++;
        for (int i = 0; i < amount; i++)
        {
            int index = (i + offset) % amount;
            if (pooled.Pool[index].obj.activeInHierarchy == false)
            {
                return pooled.Pool[index];
            }
        }
        pooled.Index = AddToPool(3,pooled);
        return pooled.Pool[pooled.Index];
    }

    public PooledObject GetAlive(System.Enum type)
    {
        PooledObject o = Get(alive);

        Sprite s = null;
        textures.TryGetValue(type, out s);
        o.spriteRenderer.sprite = s;

        o.obj.SetActive(true);
        o.obj.transform.localScale = Vector3.one * .9f;
        return o;
    }

    public PooledObject GetDead(System.Enum type)
    {
        PooledObject o = Get(dead);

        Sprite s = null;
        texturesDestroyed.TryGetValue(type, out s);
        o.spriteRenderer.sprite = s;

        o.obj.transform.rotation = Quaternion.identity;
        o.obj.SetActive(true);
        return o;
    }
    /*
    public PooledObject GetDead(int type)
    {
        PooledObject o = Get(dead);
        o.spriteRenderer.sprite = dead.GetSprite(type);
        o.spriteRenderer.color = Color.white;
        o.obj.transform.rotation = Quaternion.identity;
        o.obj.SetActive(true);
        return o;
    }

    public PooledObject GetAlive(int type)
    {
        PooledObject o = Get(alive);
        o.spriteRenderer.sprite = alive.GetSprite(type);
        o.obj.SetActive(true);
        o.obj.transform.localScale = Vector3.one * .9f;
        return o;
    }

    public PooledObject GetSpecial(int type)
    {
        PooledObject o = Get(special);
        o.spriteRenderer.sprite = special.GetSprite(type);
        o.obj.SetActive(true);
        return o;
    }

    public PooledObject GetSpecialActivated(int type)
    {
        PooledObject o = Get(specialActivated);
        o.spriteRenderer.sprite = specialActivated.GetSprite(type);
        o.obj.SetActive(true);
        return o;
    }

    internal PooledObject GetBlocked(int type)
    {
        PooledObject o = Get(blocked);
        o.spriteRenderer.sprite = blocked.GetSprite(type);
        o.obj.SetActive(true);
        return o;
    }*/
}
