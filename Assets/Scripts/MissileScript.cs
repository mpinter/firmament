using UnityEngine;
using System.Collections;

public class MissileScript : MonoComponents
{

    public GameObject target;
    public float speed=1f;
    public int dmg = 100;

    private LineRenderer lineRenderer;

    //laser stuff
    public bool phaser = false;
    public bool laser = false;
    public float laserTTL = 3.0f;
    private Vector3 originPostion;
    private Vector3 startPos;
    private Vector3 endPos;

    //todo missile stuff (smoke particle effect)
    public bool missile = true;

	// Use this for initialization
	void Start () {
	    Init();
        //debug only
	    //GetComponent<SpriteRenderer>().color = Color.red;
	    if (!missile)
	    {
	        lineRenderer = gameObject.GetComponent<LineRenderer>();
	        if (phaser)
	        {
	            lineRenderer.SetWidth(0.2f, 0.2f);
	            lineRenderer.useWorldSpace = false;
	        }
	        else if (laser)
	        {
                lineRenderer.SetWidth(0.1f, 0.2f);
                lineRenderer.useWorldSpace = true;
	        }
	    }
	    else
	    {
	        //todo particles
	    }
	}

    void FixedUpdate()
    {
        
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        if (!laser)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.GetComponent<Transform>().position,
                speed);
            if (Vector3.Distance(transform.position, target.GetComponent<Transform>().position) < 1.0f)
                Destroy(gameObject);
        }
        else
        {
            UnitScript us = target.GetComponent<UnitScript>();
            us.hp -= dmg;
            laserTTL -= Time.deltaTime;
            if (laserTTL<0) Destroy(gameObject);
        }
    }

	void Update ()
	{
	    if (target == null)
	    {
	        Destroy(gameObject);
	        return;
	    }
	    if (phaser)
	    {
            Vector3 direction = 0.2f*Vector3.Normalize((target.GetComponent<Transform>().position - transform.position));
            lineRenderer.SetPosition(0,-direction);
            lineRenderer.SetPosition(1, direction);
        }
        else if (laser)
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, target.GetComponent<Transform>().position);
        }
        else
        {
            
        }
	}

    void OnDestroy()
    {
        if (!laser)
        {
            if (target == null) return;
            UnitScript us = target.GetComponent<UnitScript>();
            us.hp -= dmg;
            us.checkLife();
        }
    }
}
