using UnityEngine;
using System.Collections;

public class UnitScript : MonoComponents
{
    private PlayerScript playerScript;

    public bool agile = true;
    public bool isStructure = false;
    public bool capital = false;

    public float shootDist = 1.0f;
    public float speed = 1.0f;
    public float rotateSpeed = 1.0f;

    public int owner;
    public MarkerScript targetScript;
    public MarkerScript markerScript; //todo create marker as child for every unit

	// Use this for initialization
	void Start () {
        //todo change owner based on real owner
	    owner = 0;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
	    Init();
	}
	
	// Update is called once per frame
	void Update () {
	    
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
                    gameObject.GetComponent<Transform>().position) < 0.2f)
            {
                if (agile)
                {
                    Move(targetScript.positions[gameObject].generateNewTargetPosition());
                }
                else
                {
                    targetScript.unassign(gameObject);
                }
            }
            else
            {
                Move(targetScript.positions[gameObject].getTargetPosition());
            }
            //todo shooting optimization
        }
    }

    private void Move(Vector3 where)
    {
        Vector3 targetDir = where - transform.position;
        Vector3 forward = transform.forward;
        float angle = Vector3.Angle(targetDir, forward);
        if (angle < 1.0f)
        {
            //good enough
        }
        else if (angle < 5.0f)
        {
            //todo som smaller adjustments
        }
        else
        {
            //todo rotate, use unit circle
        }
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
