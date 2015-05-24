using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using System.Collections;

public class UnitRaycastScript : MonoComponents
{
    public float aggroRange=5.0f; //only when no cooldown and not attacking
    public float avoidRange=5.0f;
    public float attackRange=5.0f;
    public int maxInterval = 6000;
    public int avoidInterval = 10000;
    public int attackInterval = 20;
    public int aggroInterval = 60;
    private int currentInterval = 0;
    private List<KeyValuePair<float, float>> ranges = new List<KeyValuePair<float, float>>();
    private List<float> currentCd = new List<float>();
    private List<string> guns = new List<string>(); 

    public float weaponCd=5.0f;

    private UnitScript unitScript;

	void Start () {
	    Init();
	    unitScript = gameObject.GetComponent<UnitScript>();
	    switch (unitScript.unitType)
	    {
	        case UnitScript.UnitType.vector:
                ranges.Add(new KeyValuePair<float, float>(0.0f,0.0f));
                currentCd.Add(5.0f);
                guns.Add("Prefabs/Laserz");
	            break;
            case UnitScript.UnitType.artillery:
                ranges.Add(new KeyValuePair<float, float>(0.0f, 0.0f));
                currentCd.Add(15.0f);
                guns.Add("Prefabs/Missile");
                break;
            case UnitScript.UnitType.fighter:
                ranges.Add(new KeyValuePair<float, float>(15.0f, 65.0f));
                ranges.Add(new KeyValuePair<float, float>(65.0f, 115.0f));
                ranges.Add(new KeyValuePair<float, float>(115.0f, 165.0f));

                ranges.Add(new KeyValuePair<float, float>(180 + 15.0f, 180 + 65.0f));
                ranges.Add(new KeyValuePair<float, float>(180 + 65.0f, 180 + 115.0f));
                ranges.Add(new KeyValuePair<float, float>(180 + 115.0f, 180 + 165.0f));
	            for (int i = 0; i < 6; i++)
	            {
	                currentCd.Add(weaponCd);
                    guns.Add("Prefabs/HugeMissile");
	            }
                break;
            case UnitScript.UnitType.satelite:
                ranges.Add(new KeyValuePair<float, float>(0.0f,360.0f));
                currentCd.Add(0.0f);
                guns.Add("Prefabs/Laserz");
	            break;
	    }
	}
	
	void Update ()
	{
	    currentInterval++;
	    currentInterval %= maxInterval;
	    if (currentInterval%avoidInterval == 0)
	    {
	        if (unitScript.isTransforming || (unitScript.isLocked && unitScript.unitType != UnitScript.UnitType.satelite))
	            return;
	        RaycastHit2D avoidHit = Physics2D.Raycast(transform.position, transform.up, avoidRange, 1 << 8);
	        Debug.DrawLine(transform.position, transform.position + transform.up*avoidRange, Color.green);
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
	                unitScript.avoidRotate = (Vector3.Cross(forward, targetDir).z < 0f)
	                    ? unitScript.rotateSpeed
	                    : -unitScript.rotateSpeed;
	            }
	        }
	        else
	        {
	            unitScript.avoiding = false;
	        }
	    }
	    if (unitScript.canAttack)
	    {
	        for (int range = 0; range < ranges.Count; range++)
	        {
	            if (currentCd[range] < 0f)
	            {
	                //attack
	                if (currentInterval%attackInterval == 0)
	                {
	                    Debug.DrawLine(transform.position,
	                        transform.position +
	                        (Quaternion.Euler(0, 0, Random.Range(ranges[range].Key, ranges[range].Value))*transform.up*
	                         attackRange), Color.red,1);
	                    RaycastHit2D attackHit = Physics2D.Raycast(transform.position,
	                        (Quaternion.Euler(0, 0, Random.Range(ranges[range].Key, ranges[range].Value))*transform.up),
	                        attackRange,
	                        unitScript.enemiesLayerMask);
                        //Debug.Log("tryattack");
	                    if (attackHit.collider != null &&
	                        !(attackHit.collider.gameObject.GetComponent<UnitScript>().isStructure && unitScript.agile))
	                    {
	                        //Debug.Log("ATTACK!");
	                        //Debug.Log(attackHit.distance);
	                        if (unitScript.targetScriptList.Count > 0 &&
	                            (unitScript.targetScriptList[0].parentScript ==
	                             attackHit.collider.GetComponent<UnitScript>() ||
	                             !unitScript.targetScriptList[0].positions[gameObject].attack))
	                        {
	                            //Debug.Log("already attacking");
	                            shootAt(attackHit.collider.gameObject, range);
	                            //Debug.Log(unitScript.targetScriptList[0].positions[gameObject].attack);

	                        }
	                        else if ((unitScript.targetScriptList.Count == 0) ||
	                                 (unitScript.targetScriptList[0].positions[gameObject].attack &&
	                                  !unitScript.targetScriptList[0].positions[gameObject].follow &&
	                                  !unitScript.targetScriptList[0].positions[gameObject].isCircular))
	                        {
	                            //add marker beginning
	                            Debug.Log("ADD MARKER"); //todo attack move and check this
	                            attackHit.collider.GetComponent<UnitScript>().markerScript.assignFront(gameObject);
	                            unitScript.targetScriptList[0].positions[gameObject].attack = true;
	                            shootAt(attackHit.collider.gameObject, range);
	                        }
	                        else
	                        {
	                            //Debug.Log("Could shoot, but why bother?");
	                            currentCd[range] += Random.Range(0.0f, 0.5f);
	                        }
	                        if (unitScript.targetScriptList.Count > 0 &&
	                            !unitScript.targetScriptList[0].positions[gameObject].attack)
	                        {
	                            //casually shoot at things I encounter
	                            //covered in first if, this just for debug
	                            //Debug.Log("Casual scrub");
	                        }

	                    }
	                }
	                if (currentInterval%aggroInterval == 0)
	                {
	                    if ((unitScript.targetScriptList.Count == 0) ||
	                        (unitScript.targetScriptList[0].positions[gameObject].attack &&
	                         !unitScript.targetScriptList[0].positions[gameObject].follow &&
	                         !unitScript.targetScriptList[0].positions[gameObject].isCircular))
	                    {
	                        //try aggro
	                        Debug.DrawLine(transform.position,
	                            transform.position +
	                            (Quaternion.Euler(0, 0, Random.Range(0f, 360f))*transform.up*attackRange),
	                            Color.yellow);
	                        RaycastHit2D aggroHit = Physics2D.Raycast(transform.position,
	                            Quaternion.Euler(0, 0, Random.Range(0f, 360f))*transform.up, aggroRange,
	                            unitScript.enemiesLayerMask);
	                        if (aggroHit.collider != null)
	                        {
	                            Debug.Log("AGGRO!");
	                            aggroHit.collider.GetComponent<UnitScript>().markerScript.assignFront(gameObject);
	                            unitScript.targetScriptList[0].positions[gameObject].attack = true;
	                        }
	                    }
	                }
	            }
	            else
	            {
	                currentCd[range] -= Time.deltaTime;
	            }
	        }
	    }
	}

    void shootAt(GameObject target,int gunId)
    {
        //instantiate shot
        //give it target
        //shot script - on destroy reduce hp
        //Debug.Log(target);
        GameObject missile = Instantiate(Resources.Load(guns[gunId], typeof (GameObject))) as GameObject;
        missile.transform.position = gameObject.transform.position;
        if (missile.GetComponent<MissileScript>().laser) missile.transform.parent = gameObject.transform;
        missile.GetComponent<MissileScript>().target = target;
        currentCd[gunId] = weaponCd;
    }
}
