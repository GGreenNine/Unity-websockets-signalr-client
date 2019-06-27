using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    public static T Instance
    {
        get { return instance; }
    }

    protected virtual void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            //singleton already exists
            Destroy(gameObject);
        }
    }
}