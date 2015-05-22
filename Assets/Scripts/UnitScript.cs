using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitScript : MonoComponents
{
    private PlayerScript playerScript;

    public bool agile = false;
    public bool isStructure = false;
    public bool capital = true;

    public int hp = 500;
    public float shootDist = 10.0f;
    public float followDist = 10.0f;
    public float speed = 0.05f;
    public float speedMax = 0.05f;
    public float speedMin = 0.02f;
    public float acceleration = 0.001f;
    public float deceleration = 0.001f;
    public float rotateSpeed = 1.0f;
    private float overshootMin = 0.0f;

    public float planetRadius = 5.0f;

    public bool avoiding = false;
    public bool lockOn = true;
    public float avoidRotate;
    public int enemiesLayerMask; //all - 16711680

    public int owner=0;
    public List<MarkerScript> targetScriptList;
    public MarkerScript markerScript; //todo create marker as child for every unit

	// Use this for initialization
	void Start () {
        Init();
	    //enemiesLayerMask = 16711680 - (1 << (owner+16));
	    enemiesLayerMask = 0;
	    for (int i = 0; i < 8; i++)
	    {
	        if (i == owner) continue;
	        enemiesLayerMask += 1 << (i + 16);
	    }
        //todo this can be null and add AI
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        GameObject marker = Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
        marker.transform.parent = transform;
	    marker.transform.localPosition = Vector3.zero;
	    markerScript = marker.GetComponent<MarkerScript>();
	    markerScript.parentScript = this;
        //todo this is for test only - if agile create local marker
	    if (agile)
	    {
            marker = Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)),Vector3.zero,Quaternion.identity) as GameObject;
            marker.GetComponent<MarkerScript>().assign(gameObject,false);
	    }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
	    checkMove();
	}

    private void checkMove()
    {
        if (targetScriptList.Count==0)
        {
            if (agile) Debug.Log("Agile no Marker!!??");
            checkSpeed();
            transform.position += transform.up * speed;
        }
        else
        {
            if (
                Vector3.Distance(targetScriptList[0].positions[gameObject].getTargetPosition(),
                    gameObject.GetComponent<Transform>().position) < 1.0f)
            {
                if (targetScriptList.Count > 1)
                {
                    targetScriptList[0].unassign(gameObject);
                    checkMove();
                }
                else
                {
                    if (agile)
                    {
                        if (targetScriptList[0].positions[gameObject].isCircular)
                        {
                            Orbit(targetScriptList[0].positions[gameObject].generateNewTargetPosition());
                        }
                        else
                        {
                            Move(targetScriptList[0].positions[gameObject].generateNewTargetPosition(),
                                targetScriptList[0].positions[gameObject].getTurnSpeedModifier(transform.position));
                        }
                    }
                    else
                    {
                        if (!targetScriptList[0].positions[gameObject].follow)
                        {
                            targetScriptList[0].unassign(gameObject);
                            checkMove();
                        }
                    }
                }
            }
            else if (avoiding)
            {
                Avoid();
            }
            else
            {
                Debug.DrawLine(transform.position, targetScriptList[0].positions[gameObject].getTargetPosition());
                Move(targetScriptList[0].positions[gameObject].getTargetPosition(), targetScriptList[0].positions[gameObject].getTurnSpeedModifier(transform.position));
            }
            
        }
    }

    private void checkSpeed()
    {
        if (targetScriptList.Count==0)
        {
            speedDown();
        }
        else if (targetScriptList[0].positions[gameObject].follow)
        {
            float keepDist = (targetScriptList[0].parentScript.owner == owner) ? followDist : shootDist;
            Debug.Log(keepDist);
            if (Vector3.Distance(targetScriptList[0].positions[gameObject].getTargetPosition(), transform.position) <
                keepDist)
            {
                speedDown();
            }
            else
            {
                speedUp();
            }
        }
        else
        {
            speedUp();
        }
    }

    private void speedUp()
    {
        speed += acceleration;
        if (speed > speedMax) speed = speedMax;
        deceleration = acceleration;
    }

    private void speedDown()
    {
        deceleration += acceleration;
        speed -= deceleration;
        if (speed < 0f) speed = 0f;
        if (targetScriptList.Count > 0 && agile && speed < speedMin)
        {
            speed = speedMin;
            deceleration = acceleration;
        }
    }

    private void Orbit(Vector3 next)
    {
        Vector3 targetDir = Vector3.Normalize(next - transform.position);
        Vector3 forward = transform.up;
        float angle = Vector3.Angle(targetDir, forward);
        if (Vector3.Cross(forward, targetDir).z < 0f) angle = -angle;
        transform.rotation = transform.rotation * Quaternion.Euler(0, 0, angle);
        transform.position = next;
    }

    private void Avoid()
    {
        checkSpeed();
        float angle = avoidRotate;
        transform.rotation = transform.rotation * Quaternion.Euler(0, 0, angle);
        transform.position += transform.up * speed;
    }

    private void Move(Vector3 where, float turnModifier)
    {
        checkSpeed();
        Vector3 targetDir = Vector3.Normalize(where - transform.position);
        Vector3 forward = transform.up;
        float angle = Vector3.Angle(targetDir, forward);
        if (angle < 2.0f)
        {
            //overshooting
            /*if (Random.Range(0.0f, 1.0f) > 0.9f) overshootMin = Random.Range(0.0f, 3.0f);
            angle = Vector3.Angle(targetDir, forward);
            if (angle > 0.3f)
            {
                angle = rotateSpeed;
                if (Vector3.Cross(forward, targetDir).z < 0f) angle = -angle;
                transform.rotation = transform.rotation * Quaternion.Euler(0, 0, angle);
            }*/
        }
        else if (false)
        {
            //smaller adjustments
            if (Vector3.Cross(forward, targetDir).z < 0f) angle = -angle;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle),0.5f);
        }
        else
        {
            //rotate fixed speed
            angle = rotateSpeed/turnModifier;
            if (Vector3.Cross(forward, targetDir).z < 0f) angle = -angle;
            transform.rotation = transform.rotation * Quaternion.Euler(0, 0, angle);
        }
        transform.position += transform.up*speed;
    }

    public void checkLife()
    {
        //todo add fancy explosion, ondestroy event
        if (hp<0) Destroy(gameObject);
    }

    public void select()
    {
        if (playerScript == null)
        {
            Debug.Log("slecting what you should not");
            return;
        }
        playerScript.selected.Add(gameObject);
        renderer.color = Color.red;
    }

    public void unselect()
    {
        if (playerScript == null)
        {
            Debug.Log("slecting what you should not");
            return;
        }
        unselect_noremove();
        playerScript.selected.Remove(gameObject);
    }

    public void unselect_noremove()
    {
        renderer.color = Color.white;
    }
}
