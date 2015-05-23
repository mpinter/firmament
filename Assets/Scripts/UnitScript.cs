﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitScript : MonoComponents
{
    private PlayerScript playerScript;
    private AIScript aiScript;
    private ForcesScript forcesScript;

    public enum UnitType
    {
        transforming, vector, miner, fighter, artillery, capital, satelite, basePlanet, fighterPlanet, artilleryPlanet, capitalPlanet, blackHole, other
    };

    public UnitType unitType = UnitType.other;

    public bool agile = false;
    public bool isStructure = false;
    public bool isMineable = false;
    public bool capital = true;
    public bool canAttack = true;
    public bool miner = false;

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
    public float currentLoad = 0;
    public float maxLoad = 100;

    public float planetRadius = 5.0f;
    public float gatherSpeed = 1.0f;
    public float minerals = 5000.0f;
    public MarkerScript rally = null;

    public bool avoiding = false;
    public bool lockOn = true;
    public float avoidRotate;
    public int enemiesLayerMask; //all - 16711680

    public bool isTransforming = false; //no movement
    public bool isLocked = false; //no commands
    public float transformTimer = 0.0f;
    public float transformCoef = 0.0f;

    public Vector3 asteroidDrag;

    public int owner=0;
    public List<MarkerScript> targetScriptList;
    public MarkerScript markerScript;
    public GameObject centerMarker;
    public MarkerScript lastMined;

    public List<ProductionItem> productionQueue;

    public class ProductionItem
    {
        public string prefabPath;
        public int quantity;
        public int limit = 0;
        public float remainingTime;
        public float resetTime;
        public float spawnRadius;
        public float cost;
        public bool endless = false;
        public bool active = false;

        public ProductionItem(string _prefabPath,int _quantity,float _resetTime, float _spawnRadius, float _cost, bool _endless)
        {
            prefabPath = _prefabPath;
            quantity = _quantity;
            resetTime = _resetTime;
            spawnRadius = _spawnRadius;
            cost = _cost;
            endless = _endless;
            remainingTime = resetTime;
        }

        public void Update(Transform t)
        {
            if (active)
            {
                remainingTime -= Time.deltaTime;
                if (remainingTime < 0) spawn(t);
            }
        }

        public void spawn(Transform t)
        {
            if (limit > 0) Mathf.Clamp(quantity, 1, limit);
            for (int i = 0; i < quantity; i++)
            {
                GameObject latest = Instantiate(Resources.Load(prefabPath, typeof(GameObject))) as GameObject;
                latest.transform.position = t.position + Vector3.Normalize(Random.insideUnitCircle) * spawnRadius;
            }
            if (endless)
            {
                remainingTime = resetTime;
            }
        }

    }

	void Start () {
        Init();
	    //enemiesLayerMask = 16711680 - (1 << (owner+16));
	    enemiesLayerMask = 0;
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
	    if (owner >= 0)
	    {
	        setOwner(owner);
	    }
	    else
	    {
	        if (isMineable)
	        {
	            asteroidDrag = Random.insideUnitCircle;
	        }
	    }
        if (centerMarker != null)
        {
            //well f- me sideways and call me daisy, if this works I shall be known as an Oracle of class design
            //also, it's late again so I'm getting overly poetic
            centerMarker.GetComponent<MarkerScript>().forcePosition(gameObject, gameObject, this, centerMarker.transform);
            transform.position = targetScriptList[0].positions[gameObject].generateNewTargetPosition();
        }
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

    void Update()
    {
        checkTask();
        if (productionQueue != null) updateQueue();
    }

	void FixedUpdate () {
	    if (!isTransforming) checkMove();
	}

    private void updateQueue()
    {
        Debug.Log(forcesScript);
        if (productionQueue.Count==0) return;
        if (!productionQueue[0].active && productionQueue[0].cost <= forcesScript.resPrimary)
        {
            productionQueue[0].active = true;
            forcesScript.resPrimary -= productionQueue[0].cost;
        }
        productionQueue[0].Update(transform);
        if (productionQueue[0].remainingTime<0) productionQueue.RemoveAt(0);
    }

    private void checkMove()
    {
        if (isMineable)
        {
            //todo finetune
            transform.position = Vector3.MoveTowards(transform.position, transform.position+asteroidDrag,Time.deltaTime*0.2f);
            return;
        }
        if (targetScriptList.Count==0)
        {
            if (agile)
            {
                Debug.Log("Agile no Marker!!??");
                GameObject marker = Instantiate(Resources.Load("Prefabs/Marker", typeof (GameObject)), Vector3.zero, Quaternion.identity) as GameObject;
                marker.GetComponent<MarkerScript>().assign(gameObject, false);
            }
            else
            {
                checkSpeed();
                transform.position += transform.up * speed;    
            }
            
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
                    if ((agile)||(isStructure))
                    {
                        if (targetScriptList[0].positions[gameObject].isCircular)
                        {
                            Orbit(targetScriptList[0].positions[gameObject].generateNewTargetPosition() + targetScriptList[0].getOffset());
                            if (miner) checkMine();
                            if (unitType == UnitType.vector) startTerraforming(targetScriptList[0].parentScript.gameObject);
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

    public void OnDestroy()
    {
        //do when thinking
        //todo list forces
        if (playerScript!=null) unselect();
        foreach (var target in targetScriptList)
        {
            if (target!=null) target.unassign_noremove(gameObject);
        }
        if (isMineable) markerScript.Remove();
    }

    public float Gather()
    {
        if (!isMineable) Debug.Log("What are you trying to mine really ?!?");
        minerals -= gatherSpeed*Time.deltaTime;
        if (minerals < 0)
        {
            sendMinersHome();
            Destroy(gameObject);
        }
        return gatherSpeed*Time.deltaTime;
    }

    //todo VERY prone to bugs, check here
    public void sendMinersHome()
    {
        foreach (var record in markerScript.positions)
        {
            sendHome(record.Key);
        }
    }

    //this is ugly (crosscall), todo rewrite
    public void sendHome(GameObject miner)
    {
        Debug.Log("home?");
        MarkerScript home = (miner.GetComponent<UnitScript>().forcesScript.asteroidMarkers.ContainsKey(gameObject)) ? miner.GetComponent<UnitScript>().forcesScript.asteroidMarkers[gameObject] : null;
        if (home != null)
        {
            Debug.Log("send home");
            home.assign(miner, false);
        }
        else
        {
            markerScript.unassign(gameObject);
        }
    }

    public void checkMine()
    {
        //should be called only when orbiting
        if (targetScriptList[0].parentScript.isMineable)
        {
            if (currentLoad < maxLoad)
            {
                currentLoad+=targetScriptList[0].parentScript.Gather();
                //todo particle effect
            }
            else
            {
                currentLoad = maxLoad;
                lastMined = targetScriptList[0];
                targetScriptList[0].parentScript.sendHome(gameObject);
                //todo check! prone to bugs - asteroids should allow only friendly planet targets and set to null when planets are destroyed
                /*if (targetScriptList[0].parentScript.targetScriptList.Count > 0)
                {
                    MarkerScript currentTarget = targetScriptList[0];
                    targetScriptList[0].parentScript.targetScriptList[0].assign(gameObject,false);
                    currentTarget.assign(gameObject,true);
                }*/
            }
            
        }
        else if (targetScriptList[0].parentScript.owner == owner)
        {
            if (currentLoad > 0)
            {
                //dropoff todo particle effect
                currentLoad -= gatherSpeed*Time.deltaTime;
                forcesScript.resPrimary += gatherSpeed*Time.deltaTime;
            }
            else
            {
                currentLoad = 0;
                if (lastMined!=null) lastMined.assign(gameObject,false);
            }
        }
        
    }

    public void checkTask()
    {
        if (transformTimer > 0.0f)
        {
            if (unitType == UnitType.vector && isLocked)
            {
                //a bit unsafe, but if it crashes here at least I should know what is wrong
                //todo finetune parameters
                transformCoef = (101 - targetScriptList[0].parentScript.transformTimer) / 100.0f;
                targetScriptList[0].parentScript.transformTimer -= transformCoef*Time.deltaTime;
            }
            transformTimer -= transformCoef*Time.deltaTime;
        }
        if (transformTimer <= 0.0f)
        {
            if (isTransforming)
            {
                isTransforming = false;
                //todo so far only transform, later change
                GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/Placehold/miner", typeof(Sprite)) as Sprite;
            }
            else if (isLocked && unitType!=UnitType.satelite) //satelites are locked for life
            {
                if (isStructure)
                {
                    //todo change sprite - building complete
                    GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/Placehold/planets_01", typeof(Sprite)) as Sprite;
                    isLocked = false;
                }
                else
                {
                    //builder finished
                    Destroy(gameObject);   
                }
            }
        }
    }

    //
    public void startBuildingPlanet(UnitType buildingType)
    {
        productionQueue = new List<ProductionItem>();
        unitType = buildingType;
        isLocked = true;
        transformTimer = 100;
        //todo add hp progressively
        switch (buildingType)
        {
            case UnitType.basePlanet:
                transformCoef = 0.0f;
                hp = 1000;
                break;
            case UnitType.fighterPlanet:
                transformCoef = 5.0f;
                hp = 10000;
                break;
            case UnitType.artilleryPlanet:
                transformCoef = 5.0f;
                hp = 10000;
                break;
            case UnitType.capitalPlanet:
                transformCoef = 5.0f;
                hp = 100000;
                break;
        }
    }

    public void startTerraforming(GameObject planet)
    {
        UnitScript planetScript = planet.GetComponent<UnitScript>();
        if (planetScript.owner < 0)
        {
            planetScript.setOwner(owner);
            planetScript.startBuildingPlanet(UnitType.basePlanet);
        }
        if (planetScript.unitType == UnitType.basePlanet && planetScript.owner == owner && planetScript.isLocked)
        {
            isLocked = true;
            transformTimer = 100;
            transformCoef=(101-planetScript.transformTimer);
            GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/Placehold/transform", typeof(Sprite)) as Sprite;
            //todo particles
        }
    }

    private void produce(UnitType productionType)
    {
        
    }

    //this is ungly but done due to time constraints, i'd prefer inheritance for diferent units/behaviours
    public void actionQ()
    {
        switch (unitType)
        {
            case UnitType.basePlanet:
            case UnitType.fighterPlanet:
            case UnitType.artilleryPlanet:
            case UnitType.capitalPlanet:
                productionQueue.Add(new ProductionItem("Prefabs/wut",3,10f,planetRadius+1,0,false));
                break;
            case UnitType.vector:
                isTransforming = true;
                unitType = UnitType.miner;
                //todo change sprite
                GetComponent<SpriteRenderer>().sprite =Resources.Load("Sprites/Placehold/transform", typeof (Sprite)) as Sprite;
                transformTimer = 100;
                transformCoef = 10;
                break;
        }
    }

    //also sets enemy layer masks
    private void setOwner(int ownerId)
    {
        owner = ownerId;
        for (int i = 0; i < 8; i++)
        {
            if (i == ownerId) continue;
            enemiesLayerMask += 1 << (i + 16);
        }
        if (ownerId == 0)
        {
            forcesScript = playerScript;
        }
        else if (ownerId > 0)
        {
            foreach (var ai in GameObject.FindGameObjectsWithTag("AI"))
            {
                if (ai.GetComponent<AIScript>().id == ownerId)
                {
                    aiScript = ai.GetComponent<AIScript>();
                    forcesScript = aiScript;
                }
            }
        }
        else forcesScript = null;
    }
}
