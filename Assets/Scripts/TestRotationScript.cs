using UnityEngine;
using System.Collections;

public class TestRotationScript : MonoBehaviour
{

    private Vector3 orig;

	// Use this for initialization
    void Awake()
    {
        orig = transform.eulerAngles;
    }

	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate ()
	{
	    Vector3 pos = transform.parent.position;
	    transform.eulerAngles = orig;
	    transform.position = pos;
	}
}
