using UnityEngine;
using System.Collections;

public abstract class MonoComponents : MonoBehaviour {

    public SpriteRenderer renderer;
    public Transform transform;
    public GameObject gameObject;

    protected virtual void Init()
    {
        renderer = GetComponent<SpriteRenderer>();
        transform = GetComponent<Transform>();
        gameObject = GetComponent<GameObject>();
    }
    
}
