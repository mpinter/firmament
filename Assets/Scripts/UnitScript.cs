using UnityEngine;
using System.Collections;

public class UnitScript : MonoComponents
{
    private PlayerScript playerScript;

    public bool agile = true;
    public bool isStructure = false;
    public bool capital = false;

    public float shootDist = 1.0f;
    public float speed = 0.05f;
    public float rotateSpeed = 1.0f;
    private float overshootMin = 0.0f;

    public int owner;
    public MarkerScript targetScript;
    public MarkerScript markerScript; //todo create marker as child for every unit

	// Use this for initialization
	void Start () {
        Init();
        //todo change owner based on real owner
	    owner = 0;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        GameObject marker = Instantiate(Resources.Load("Prefabs/Marker",typeof(GameObject))) as GameObject;
        marker.transform.parent = transform;
	    markerScript = marker.GetComponent<MarkerScript>();
	    markerScript.parentScript = this;
        //todo this is for test only - if agile create local marker
	    if (agile)
	    {
            marker = Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject))) as GameObject;
            marker.GetComponent<MarkerScript>().assign(gameObject);
	    }
	}
	
	// Update is called once per frame
	void Update () {
	    checkMove();
	}

    private void checkMove()
    {
        if (targetScript == null)
        {
            if (agile) Debug.Log("Agile no Marker!!??");
        }
        else
        {
            if (
                Vector3.Distance(targetScript.positions[gameObject].getTargetPosition(),
                    gameObject.GetComponent<Transform>().position) < 1.0f)
            {
                if (agile)
                {
                    Move(targetScript.positions[gameObject].generateNewTargetPosition(), targetScript.positions[gameObject].getTurnSpeedModifier(transform.position));
                }
                else
                {
                    targetScript.unassign(gameObject);
                }
            }
            else
            {
                Debug.DrawLine(transform.position,targetScript.positions[gameObject].getTargetPosition());
                Move(targetScript.positions[gameObject].getTargetPosition(), targetScript.positions[gameObject].getTurnSpeedModifier(transform.position));
            }
            //todo shooting optimization
        }
    }

    private void Move(Vector3 where, float turnModifier)
    {
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

    public void select()
    {
        playerScript.selected.Add(gameObject);
        renderer.color = Color.red;
    }

    public void unselect()
    {
        unselect_noremove();
        playerScript.selected.Remove(gameObject);
    }

    public void unselect_noremove()
    {
        renderer.color = Color.white;
    }
}
