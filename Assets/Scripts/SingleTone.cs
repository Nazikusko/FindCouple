using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTone<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T use;
    public static T Use
    {
        get
        {
            if (use == null) use = FindObjectOfType<T>();
            if (use == null) use = new GameObject("SINGLETON" + typeof(T)).AddComponent<T>();

            return use;
        }
    }
}
