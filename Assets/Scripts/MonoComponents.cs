using UnityEngine;
using System.Collections;

public abstract class MonoComponents : MonoBehaviour {

    new public SpriteRenderer renderer;
    new public Transform transform;
    new public Rigidbody rigidbody;

    protected virtual void Init()
    {
        renderer = GetComponent<SpriteRenderer>();
        transform = GetComponent<Transform>();
        rigidbody = GetComponent<Rigidbody>();
    }
    
}
