
using UnityEngine;
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
    }
}
