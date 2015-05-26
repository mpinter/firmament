using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class AIScript : ForcesScript
{

    public GameObject player;
    public float actionCooldown=0.0f;
    public float buildCooldown = 0.0f;
    public bool active=true;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (!active) return;
	    if (actionCooldown > 0.0f) actionCooldown -= Time.deltaTime;
	    else
	    {
	        if (player.GetComponent<PlayerScript>().structures.Count > 0)
	        {
                GameObject marker = (GameObject)Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)));
	            marker.GetComponent<Transform>().position = player.GetComponent<PlayerScript>().structures.ElementAt(Random.Range(0,player.GetComponent<PlayerScript>().structures.Count)).transform.position;
	            foreach (var unit in units)
	            {
	                if (unit.GetComponent<UnitScript>().isStructure) continue; 
	                if (Random.Range(0.0f, 1.0f) > 0.8f)
	                {
	                    marker.GetComponent<MarkerScript>().assign(unit,false);
	                    marker.GetComponent<MarkerScript>().positions[unit].attack = true;
	                }
	            }
	        }
	        actionCooldown = 40.0f + Random.Range(0.0f, 80.0f);
	    }
	    if (buildCooldown > 0.0f) buildCooldown -= Time.deltaTime;
        else
	    {
	        foreach (var structure in structures)
	        {
                if (structure.GetComponent<UnitScript>().productionQueue==null) structure.GetComponent<UnitScript>().productionQueue=new List<UnitScript.ProductionItem>();
                switch (structure.GetComponent<UnitScript>().unitType)
                {
                    case UnitScript.UnitType.fighterPlanet:
                        structure.GetComponent<UnitScript>().productionQueue.Add(new UnitScript.ProductionItem("Prefabs/fighter", 4, 5f, structure.GetComponent<UnitScript>().planetRadius + 1, 0, false,id));
                        break;
                    case UnitScript.UnitType.artilleryPlanet:
                        structure.GetComponent<UnitScript>().productionQueue.Add(new UnitScript.ProductionItem("Prefabs/artillery", 2, 5f, structure.GetComponent<UnitScript>().planetRadius + 1, 0, false,id));
                        break;
                    case UnitScript.UnitType.capitalPlanet:
                        structure.GetComponent<UnitScript>().productionQueue.Add(new UnitScript.ProductionItem("Prefabs/capital", 1, 10f, structure.GetComponent<UnitScript>().planetRadius + 1, 0, false,id));
                        break;
                }
	        }
            buildCooldown = 40.0f + Random.Range(0.0f, 80.0f);
	    }
	}
}
