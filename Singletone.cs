
using UnityEngine;
using UnityEngine.SceneManagement;

public class Singletone<T> : MonoBehaviour where T : Component
{
    private static T instance;
    public static T Instance
    {
        get 
        {
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    GameObject go = new GameObject(typeof(T).ToString());
                    DontDestroyOnLoad(go);
                    instance = go.AddComponent<T>();
                }
            }
            return instance;
        }
    }

    protected void Awake()
    {
        instance = this as T;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Init();
    }

    virtual public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    { 
    
    }
    virtual public void Init()
    { 
    
    }

   
}
