using UnityEngine;
using System.Collections;

public class MissileScript : MonoComponents
{

    public GameObject target;
    public float speed=1.0f;
    public int dmg = 100;

	// Use this for initialization
	void Start () {
	    Init();
        //debug only
	    GetComponent<SpriteRenderer>().color = Color.red;
	}

    void FixedUpdate()
    {
        if (target==null) Destroy(gameObject);
        transform.position=Vector3.MoveTowards(transform.position, target.GetComponent<Transform>().position, speed);
        if (Vector3.Distance(transform.position,target.GetComponent<Transform>().position)<1.0f) Destroy(gameObject);
    }

	void Update () {
	
	}

    void OnDestroy()
    {
        if (target == null) return;
        UnitScript us = target.GetComponent<UnitScript>();
        us.hp -= dmg;
        us.checkLife();
    }
}
