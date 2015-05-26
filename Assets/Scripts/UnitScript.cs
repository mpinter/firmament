using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UnitScript : MonoComponents
{
    private PlayerScript playerScript;
    private AIScript aiScript;
    private ForcesScript forcesScript;

    public enum UnitType
    {
        transforming, vector, miner, fighter, artillery, capital, satelite, basePlanet, fighterPlanet, artilleryPlanet, capitalPlanet, blackHole, planet, other, sun
    };

    public UnitType unitType = UnitType.other;

    public bool agile = false;
    public bool isStructure = false;
    public bool isMineable = false;
    public bool capital = true;
    public bool canAttack = true;
    public bool miner = false;

    public int hp = 500;
    private int healthMax;
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

    public string qText = "";
    public string wText = "";
    public string eText = "";
    public string rText = "";

    public Vector3 asteroidDrag;

    public int owner=0;
    public List<MarkerScript> targetScriptList;
    public MarkerScript markerScript;
    public GameObject centerMarker;
    public MarkerScript lastMined;

    public ParticleSystem primaryThrust;
    public ParticleSystem secondaryThrust;
    public ParticleSystem transformParticles;
    public ParticleSystem mineParticles;

    public List<ProductionItem> productionQueue;

    public GameObject textStatus;
    public GameObject progress;
    public GameObject health;

    public class ProductionItem
    {
        public string prefabPath;
        public int quantity;
        public int limit = 0;
        public int owner;
        public float remainingTime;
        public float resetTime;
        public float spawnRadius;
        public float cost;
        public bool endless = false;
        public bool active = false;

        public ProductionItem(string _prefabPath,int _quantity,float _resetTime, float _spawnRadius, float _cost, bool _endless, int _owner=0)
        {
            prefabPath = _prefabPath;
            quantity = _quantity;
            resetTime = _resetTime;
            spawnRadius = _spawnRadius;
            cost = _cost;
            endless = _endless;
            remainingTime = resetTime;
            owner = _owner;
        }

        public void Update(MarkerScript ms,Transform t)
        {
            if (active)
            {
                remainingTime -= Time.deltaTime;
                if (remainingTime < 0) spawn(ms,t);
            }
        }

        public void spawn(MarkerScript ms, Transform t)
        {
            int _quan = (endless) ? Mathf.Clamp(quantity, 0, limit) : quantity;
            for (int i = 0; i < _quan; i++)
            {
                GameObject latest = Instantiate(Resources.Load(prefabPath, typeof(GameObject))) as GameObject;
                latest.transform.position = t.position + Vector3.Normalize(Random.insideUnitCircle) * spawnRadius;
                latest.GetComponent<UnitScript>().owner = owner;
                /*if (latest.GetComponent<UnitScript>().unitType == UnitType.vector)
                {
                    Debug.Log("is vector, force ?");
                    latest.GetComponent<UnitScript>().centerMarker = ms.gameObject;
                    //ms.forcePosition(latest.gameObject, latest.gameObject, latest.GetComponent<UnitScript>(), t);
                }*/
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
        setOwner(owner);
	    if (owner >= 0)
	    {
	        //here was once stuff
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
            marker = Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)),transform.position,Quaternion.identity) as GameObject;
            marker.GetComponent<MarkerScript>().assign(gameObject,false);
	    }
        if ((unitType == UnitType.blackHole) && (owner >= 0))
        {
            productionQueue = new List<ProductionItem>();
            var prod = new ProductionItem("Prefabs/vector", 20, 1, 20, 0, true, owner);
            prod.limit = 120 - forcesScript.resSecondary;
            productionQueue.Add(prod);
        }
	    if (unitType == UnitType.vector)
	    {
	        forcesScript.resSecondary++;
	        ParticleSystem[] pa=GetComponentsInChildren<ParticleSystem>();
	        foreach (var sys in pa)
	        {
	            if (sys.CompareTag("MorphParticles"))
	            {
	                transformParticles = sys;
                    sys.Stop();
	            }
	            if (sys.CompareTag("ThrustParticles"))
	            {
	                primaryThrust = sys;
                    sys.Play();
	            }
	            if (sys.CompareTag("MineParticles"))
	            {
	                mineParticles = sys;
	                sys.Stop();
	            }
	        }
	    }
        if (health != null)
        {
            health.GetComponentInChildren<Image>().fillMethod=Image.FillMethod.Radial360;
            health.GetComponentInChildren<Image>().type=Image.Type.Filled;
        }
        if (progress != null)
        {
            progress.GetComponentInChildren<Image>().fillMethod = Image.FillMethod.Radial360;
            progress.GetComponentInChildren<Image>().type = Image.Type.Filled;
        }
	    healthMax = hp;
	    if (owner >= 0)
	    {
	        if (isStructure && unitType!=UnitType.blackHole)
	        {
	            forcesScript.structures.Add(gameObject);
	        }
	        forcesScript.units.Add(gameObject);
	    }
	}

    void Update()
    {
        checkTask();
        if (textStatus != null)
        {
            textStatus.transform.position = transform.position;
        }
        if (health != null)
        {
            //if (unitType == UnitType.blackHole) health.transform.position = transform.TransformPoint(transform.position);
            if (playerScript.selected.Contains(gameObject) || unitType==UnitType.blackHole)
            {
                health.GetComponentInChildren<Image>().enabled = true;
                health.GetComponentInChildren<Image>().fillAmount=(float)hp/(float)healthMax;
            }
            else
            {
                health.GetComponentInChildren<Image>().enabled = false;
            }
        }
        if (progress!=null)
        {
           // if (unitType == UnitType.blackHole) progress.transform.position = transform.TransformPoint(transform.position);
            if (playerScript.selected.Contains(gameObject) && isMineable)
            {
                progress.GetComponentInChildren<Image>().enabled = true;
                progress.GetComponentInChildren<Image>().fillAmount = minerals/5000.0f;
            }
            else if ((playerScript.selected.Contains(gameObject) || unitType == UnitType.blackHole)&&(owner==0))
            {
                progress.GetComponentInChildren<Image>().enabled = true;
                 if (transformTimer > 0)
                {
                    progress.GetComponentInChildren<Image>().fillAmount = (100-transformTimer)/100.0f;
                    if (textStatus != null)
                    {
                        textStatus.transform.FindChild("Status").GetComponent<Text>().text = "Terraforming..";
                    }
                } else if (productionQueue != null && productionQueue.Count > 0)
                {
                    progress.GetComponentInChildren<Image>().fillAmount=productionQueue[0].remainingTime/productionQueue[0].resetTime;
                    //also write status
                    if (textStatus != null)
                    {
                        textStatus.transform.FindChild("Status").GetComponent<Text>().text = "Producing " + productionQueue.Count + " squads..";
                    }
                }
                 else
                 {
                     //write empty
                     progress.GetComponentInChildren<Image>().fillAmount = 0.0f;
                     if (textStatus != null)
                     {
                         textStatus.transform.FindChild("Status").GetComponent<Text>().text = "";
                     }
                 }
            }
            else
            {
                progress.GetComponentInChildren<Image>().fillAmount = 0.0f;
                progress.GetComponentInChildren<Image>().enabled = false;
                if (textStatus != null)
                {
                    textStatus.transform.FindChild("Status").GetComponent<Text>().text = "";
                }
            }
        }
        
        if (productionQueue != null) updateQueue();
        if ((hp<=0)&&(!isStructure)) Destroy(gameObject);
    }

	void FixedUpdate () {
	    if (!isTransforming) checkMove();
	}

    public void Cancel()
    {
        if (productionQueue!=null) productionQueue.Clear();
        if (!targetScriptList[0].positions[gameObject].isCircular) targetScriptList.Clear();
    }

    private void updateQueue()
    {
        if (productionQueue.Count==0) return;
        if (!productionQueue[0].active && productionQueue[0].cost <= forcesScript.resPrimary)
        {
            productionQueue[0].active = true;
            forcesScript.resPrimary -= productionQueue[0].cost;
        }
        if (unitType == UnitType.blackHole) productionQueue[0].limit = 120 - forcesScript.structures.Count*30 - playerScript.resSecondary;
        productionQueue[0].Update(markerScript,transform);
        if (productionQueue[0].remainingTime<0) productionQueue.RemoveAt(0);
    }

    private void checkMove()
    {
        if (unitType==UnitType.blackHole) return;
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
                GameObject marker = Instantiate(Resources.Load("Prefabs/Marker", typeof (GameObject)), transform.position, Quaternion.identity) as GameObject;
                marker.GetComponent<MarkerScript>().assign(gameObject, false);
            }
            else
            {
                checkSpeed();
                transform.position += transform.up * speed;    
            }
            if (mineParticles!=null) mineParticles.Stop();
        }
        else
        {
            if (miner && mineParticles != null) //ugly fixing once again
            {
                if (targetScriptList[0].parentScript == null) mineParticles.Stop();
                if (Vector3.Distance(targetScriptList[0].positions[gameObject].getTargetPosition(), gameObject.GetComponent<Transform>().position) > 3.0f) mineParticles.Stop();
            }
            if ((targetScriptList[0].positions[gameObject].follow ||
                 targetScriptList[0].positions[gameObject].isCircular) &&
                (targetScriptList[0].positions[gameObject].markerPos == null))
            {
                targetScriptList[0].unassign(gameObject);
                checkMove();
                return;
            }
            if (
                Vector3.Distance(targetScriptList[0].positions[gameObject].getTargetPosition(),
                    gameObject.GetComponent<Transform>().position) < 1.0f)
            {
                if (targetScriptList.Count > 1 && !miner) //miners need to rally up and down
                {
                    targetScriptList[0].unassign(gameObject);
                    checkMove();
                    return;
                }
                else
                {
                    if ((agile)||(isStructure))
                    {
                        if (targetScriptList[0].positions[gameObject].isCircular)
                        {
                            Orbit(targetScriptList[0].positions[gameObject].generateNewTargetPosition() /*+ targetScriptList[0].getOffset()*/);
                            if (miner)
                            {
                                checkMine();
                            }
                            if (unitType == UnitType.vector) startTerraforming(targetScriptList[0].parentScript.gameObject);
                        }
                        else
                        {
                            if (mineParticles != null) mineParticles.Stop();
                            Move(targetScriptList[0].positions[gameObject].generateNewTargetPosition(),
                                targetScriptList[0].positions[gameObject].getTurnSpeedModifier(transform.position));
                        }
                    }
                    else
                    {
                        if (mineParticles != null) mineParticles.Stop();
                        if (!targetScriptList[0].positions[gameObject].follow)
                        {
                            targetScriptList[0].unassign(gameObject);
                            checkMove();
                            return;
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
        if ((hp<=0)&&(!isStructure)) Destroy(gameObject);
        if (hp < 0) hp = 0;
    }

    public void select()
    {
        if (playerScript == null)
        {
            Debug.Log("selecting what you should not");
            return;
        }
        playerScript.selected.Add(gameObject);
        //renderer.color = Color.red;
    }

    public void unselect()
    {
        if (playerScript == null)
        {
            Debug.Log("selecting what you should not");
            return;
        }
        unselect_noremove();
        playerScript.selected.Remove(gameObject);
    }

    public void unselect_noremove()
    {
        renderer.color = Color.white;
        //playerScript.selectedDraw.Remove(gameObject);
    }

    public void OnDestroy()
    {
        //do when thinking
        //todo list forces
        if (playerScript!=null) unselect();
        if (unitType == UnitType.vector || unitType == UnitType.miner)
        {
            forcesScript.resSecondary--;
        }
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
        MarkerScript home = (miner.GetComponent<UnitScript>().forcesScript.asteroidMarkers.ContainsKey(gameObject)) ? miner.GetComponent<UnitScript>().forcesScript.asteroidMarkers[gameObject] : null;
        if (home != null)
        {
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
                if (!mineParticles.isPlaying) mineParticles.Play();
            }
            else
            {
                Debug.Log("sending home");
                mineParticles.Stop();
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
        else if ((targetScriptList[0].parentScript.owner == owner) && !(targetScriptList[0].parentScript.isLocked && targetScriptList[0].parentScript.unitType==UnitType.basePlanet))
        {
            if (currentLoad > 0)
            {
                if (!mineParticles.isPlaying) mineParticles.Play();
                currentLoad -= gatherSpeed*Time.deltaTime;
                forcesScript.resPrimary += gatherSpeed*Time.deltaTime;
            }
            else if (!(isLocked||isTransforming))
            {
                mineParticles.Stop();
                currentLoad = 0;
                if (lastMined!=null) lastMined.assignFront(gameObject); //if not workign change to assign(gameObject,false)
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
                if (targetScriptList.Count == 0)
                {
                    Destroy(gameObject);
                    return;
                }
                if (targetScriptList[0] != null)
                {
                    if (targetScriptList[0].parentScript == null)
                    {
                        Destroy(gameObject);
                        return;
                    }
                    transformCoef = (101 - targetScriptList[0].parentScript.transformTimer)/100.0f;
                    if (targetScriptList[0].parentScript.isLocked)
                    {
                        targetScriptList[0].parentScript.transformTimer -= transformCoef*Time.deltaTime;
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
            }
            transformTimer -= transformCoef*Time.deltaTime;
        }
        if (transformTimer <= 0.0f)
        {
            if (isTransforming)
            {
                isTransforming = false;
                //todo so far only transform, later change
                miner = true;
                transformParticles.Stop();
                GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/Placehold/miner", typeof(Sprite)) as Sprite;
            }
            else if (isLocked && unitType!=UnitType.satelite) //satelites are locked for life
            {
                if (isStructure)
                {
                    //todo change sprite - building complete
                    //GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/Placehold/planets_01", typeof(Sprite)) as Sprite;
                    isLocked = false;
                    switch (unitType)
                    {
                        case UnitType.basePlanet:
                            hp = 10000;
                            qText = "";
                            wText = "Fight.Pl.  1000";
                            eText = "Art.Pl.  1000";
                            rText = "Cap.Pl.   10000";
                            break;
                        case UnitType.fighterPlanet:
                            hp = 50000;
                            qText = "Fighter  100";
                            wText = "";
                            eText = "";
                            rText = "";
                            break;
                        case UnitType.artilleryPlanet:
                            hp = 50000;
                            qText = "Artillery  100";
                            wText = "";
                            eText = "";
                            rText = "";
                            break;
                        case UnitType.capitalPlanet:
                            hp = 100000;
                            qText = "CapitalShip  1000";
                            wText = "";
                            eText = "";
                            rText = "";
                            break;
                    }
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
                if (textStatus != null) textStatus.transform.FindChild("Text").GetComponent<Text>().text = "Terraformed Planet";
                transformCoef = 0.0f;
                qText = "";
                wText = "";
                eText = "";
                rText = "";
                break;
            case UnitType.fighterPlanet:
                if (textStatus != null) textStatus.transform.FindChild("Text").GetComponent<Text>().text = "War-Factory";
                transformCoef = 5.0f;
                qText = "";
                wText = "";
                eText = "";
                rText = "";
                break;
            case UnitType.artilleryPlanet:
                if (textStatus != null) textStatus.transform.FindChild("Text").GetComponent<Text>().text = "Heavy-Factory";
                transformCoef = 5.0f;
                qText = "";
                wText = "";
                eText = "";
                rText = "";
                break;
            case UnitType.capitalPlanet:
                if (textStatus != null) textStatus.transform.FindChild("Text").GetComponent<Text>().text = "Capital Planet";
                transformCoef = 5.0f;
                qText = "";
                wText = "";
                eText = "";
                rText = "";
                break;
        }
    }

    public void startTerraforming(GameObject planet)
    {
        UnitScript planetScript = planet.GetComponent<UnitScript>();
        if (planetScript.owner < 0 && planetScript.unitType==UnitType.planet)
        {
            planetScript.setOwner(owner);
            forcesScript.structures.Add(planet);
            planetScript.startBuildingPlanet(UnitType.basePlanet);
        }
        if (planetScript.unitType == UnitType.basePlanet && planetScript.owner == owner && planetScript.isLocked)
        {
            isLocked = true;
            transformTimer = 100;
            transformCoef=(101-planetScript.transformTimer);
            GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/Misc/transparency_hack", typeof(Sprite)) as Sprite;
            primaryThrust.Stop();
            transformParticles.Play();
        }
    }

    //this is ungly but done due to time constraints, i'd prefer inheritance for diferent units/behaviours
    public void actionQ()
    {
        switch (unitType)
        {
            case UnitType.fighterPlanet:
                if (forcesScript.resPrimary > 100)
                {
                    forcesScript.resPrimary -= 100;
                    productionQueue.Add(new ProductionItem("Prefabs/fighter", 4, 5f, planetRadius + 1, 0, false));
                }
                break;
            case UnitType.artilleryPlanet:
                if (forcesScript.resPrimary > 100)
                {
                    forcesScript.resPrimary -= 100;
                    productionQueue.Add(new ProductionItem("Prefabs/artillery", 2, 5f, planetRadius + 1, 0, false));
                }
                break;
            case UnitType.capitalPlanet:
                if (forcesScript.resPrimary > 100)
                {
                    forcesScript.resPrimary -= 100;
                    productionQueue.Add(new ProductionItem("Prefabs/capital", 1, 10f, planetRadius + 1, 0, false));
                }
                break;
            case UnitType.vector:
                isTransforming = true;
                unitType = UnitType.miner;
                canAttack = false;
                miner = true;
                GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/Misc/transparency_hack", typeof(Sprite)) as Sprite;
                primaryThrust.Stop();
                transformParticles.Play();
                primaryThrust.Stop();
                transformTimer = 100;
                transformCoef = 10;
                break;
        }
    }

    public void actionW()
    {
        switch (unitType)
        {
            case UnitType.basePlanet:
                if (forcesScript.resPrimary > 1000)
                {
                    forcesScript.resPrimary -= 1000;
                    startBuildingPlanet(UnitType.fighterPlanet);
                }
                break;
        }
    }

    public void actionE()
    {
        switch (unitType)
        {
            case UnitType.basePlanet:
                if (forcesScript.resPrimary > 1000)
                {
                    forcesScript.resPrimary -= 1000;
                    startBuildingPlanet(UnitType.artilleryPlanet);  
                }
                break;
        }
    }
    
    public void actionR()
    {
        switch (unitType)
        {
            case UnitType.basePlanet:
                if (forcesScript.resPrimary > 1000)
                {
                    forcesScript.resPrimary -= 1000;
                    startBuildingPlanet(UnitType.capitalPlanet);
                }
                break;
        }
    }

    //also sets enemy layer masks
    private void setOwner(int ownerId)
    {
        owner = ownerId;
        if (ownerId == 0)
        {
            forcesScript = playerScript;
            gameObject.layer = ownerId + 16;
            for (int i = 0; i < 8; i++)
            {
                if (i == ownerId) continue;
                enemiesLayerMask = enemiesLayerMask | 1 << (i + 16);
            }
        }
        else if (ownerId > 0)
        {
            if (owner > 0) renderer.color = Color.red;
            gameObject.layer = ownerId + 16;
            for (int i = 0; i < 8; i++)
            {
                if (i == ownerId) continue;
                enemiesLayerMask = enemiesLayerMask | 1 << (i + 16);
            }
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

    void OnGUI()
    {
        
    }
}
