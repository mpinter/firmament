using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class UnitRaycastScript : MonoComponents
{
    private float aggroRange=5.0f; //only when no cooldown and not attacking
    private float avoidRange=5.0f;
    private float attackRange=5.0f;
    private List<KeyValuePair<float, float>> ranges = new List<KeyValuePair<float, float>>();

    public float weaponCd=5.0f;
    private float currentCd=0.0f;

    private UnitScript unitScript;

	void Start () {
	    Init();
	    unitScript = gameObject.GetComponent<UnitScript>();
        //single range for test - right in front
        ranges.Add(new KeyValuePair<float, float>(0.0f,0.0f));
	}
	
	void Update ()
	{
        RaycastHit2D avoidHit = Physics2D.Raycast(transform.position, transform.up, avoidRange, 1 << 8);
        Debug.DrawLine(transform.position, transform.position+transform.up * avoidRange, Color.green);
	    if (avoidHit.collider != null)
	    {
	        Transform planetPos = avoidHit.collider.GetComponent<Transform>();
            UnitScript planetScript = avoidHit.collider.GetComponent<UnitScript>();
	        if (!(unitScript.targetScriptList.Count > 0 && unitScript.targetScriptList[0] == planetScript.markerScript))
	        {
                unitScript.avoiding = true;
                //get center, calculate angle
                Vector3 targetDir = Vector3.Normalize(planetPos.position - transform.position);
                Vector3 forward = transform.up;
                float angle = Vector3.Angle(targetDir, forward);
                unitScript.avoidRotate = (Vector3.Cross(forward, targetDir).z < 0f) ? unitScript.rotateSpeed : -unitScript.rotateSpeed;    
	        }
	    }
	    else
	    {
	        unitScript.avoiding = false;
	    }
	    if (currentCd < 0f)
	    {
	        //attack
	        var range = ranges[Random.Range(0, ranges.Count - 1)];
            Debug.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, Random.Range(range.Key, range.Value)) * transform.up * attackRange), Color.red);
            RaycastHit2D attackHit = Physics2D.Raycast(transform.position, (Quaternion.Euler(0, 0, Random.Range(range.Key, range.Value)) * transform.up), attackRange,unitScript.enemiesLayerMask);
	        if (attackHit.collider != null)
	        {
	            Debug.Log("ATTACK!");
                Debug.Log(attackHit.distance);
	            if (unitScript.targetScriptList.Count > 0 &&
	                (unitScript.targetScriptList[0].parentScript == attackHit.collider.GetComponent<UnitScript>() || !unitScript.targetScriptList[0].positions[gameObject].attack))
	            {
	                Debug.Log("already attacking");
                    shootAt(attackHit.collider.gameObject);
                    Debug.Log(unitScript.targetScriptList[0].positions[gameObject].attack);

	            }
                else if ((unitScript.targetScriptList.Count == 0) || (unitScript.targetScriptList[0].positions[gameObject].attack && !unitScript.targetScriptList[0].positions[gameObject].follow && !unitScript.targetScriptList[0].positions[gameObject].isCircular))
	            {
                    //add marker beginning
                    Debug.Log("ADD MARKER"); //todo attack move and check this
                    attackHit.collider.GetComponent<MarkerScript>().assignFront(gameObject);
	                unitScript.targetScriptList[0].positions[gameObject].attack = true;
                    shootAt(attackHit.collider.gameObject);
                }
                else
                {
                    Debug.Log("Could shoot, but why bother?");
                }
                if (unitScript.targetScriptList.Count > 0 && !unitScript.targetScriptList[0].positions[gameObject].attack)
                {
                    //casually shoot at things I encounter
                    //covered in first if, this just for debug
                    Debug.Log("Casual scrub");
                }
                
	        }
            else if ((unitScript.targetScriptList.Count == 0) || (unitScript.targetScriptList[0].positions[gameObject].attack && !unitScript.targetScriptList[0].positions[gameObject].follow && !unitScript.targetScriptList[0].positions[gameObject].isCircular))
	        {
	            //try aggro
                Debug.DrawLine(transform.position, transform.position + (Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * transform.up * attackRange), Color.yellow);
                RaycastHit2D aggroHit = Physics2D.Raycast(transform.position, Quaternion.Euler(0, 0, Random.Range(0f, 360f)) * transform.up, aggroRange, unitScript.enemiesLayerMask);
                if (aggroHit.collider != null)
                {
                    Debug.Log("AGGRO!");
                    aggroHit.collider.GetComponent<MarkerScript>().assignFront(gameObject);
                    unitScript.targetScriptList[0].positions[gameObject].attack = true;
                }
	        }
	    }
	    else
	    {
	        currentCd -= Time.deltaTime;
	    }
	}

    void shootAt(GameObject target)
    {
        //instantiate shot
        //give it target and layermask
        //shot script - on destroy reduce hp
        currentCd = weaponCd;
    }
}
