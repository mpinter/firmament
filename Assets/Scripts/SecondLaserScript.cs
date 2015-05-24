using UnityEngine;
using System.Collections;

public class SecondLaserScript : MonoBehaviour
{

    public bool phaser = false;
    private Vector3 originPostion;
    private Vector3 startPos;
    private Vector3 endPos;

	// Use this for initialization
	void Start ()
	{
	    originPostion = gameObject.transform.position;

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
