using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;

public class AIScript : ForcesScript
{

    public GameObject player;
    public GameObject playerHole;
    private GameObject holeMarker;
    public float actionCooldown=0.0f;
    public float buildCooldown = 0.0f;
    public bool active=true;

	// Use this for initialization
	void Start () {
        holeMarker = (GameObject)Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)));
	    holeMarker.GetComponent<Transform>().position = playerHole.GetComponent<Transform>().position;
	    holeMarker.GetComponent<MarkerScript>().persistent = true;
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
	            foreach (var unit in units.ToArray())
	            {
	                if (Random.Range(0.0f, 1.0f) > 0.5f) continue;
	                if (unit == null)
	                {
	                    units.Remove(unit);
	                    continue;
	                }
	                if (unit.GetComponent<UnitScript>().isStructure) continue; //fixed, no longer needed.. but afraid to remove now :)
                    if (unit.GetComponent<UnitScript>().targetScriptList.Count > 0 && unit.GetComponent<UnitScript>().targetScriptList[0].positions.ContainsKey(unit) && unit.GetComponent<UnitScript>().targetScriptList[0].positions[unit].attack) continue;
	                if (unit.GetComponent<UnitScript>().unitType == UnitScript.UnitType.vector)
	                {
	                    holeMarker.GetComponent<MarkerScript>().assign(unit,false);
                        holeMarker.GetComponent<MarkerScript>().positions[unit].attack = true;
	                } 
	                else
	                {
	                    if (Random.Range(0.0f, 1.0f) > 0.5f)
	                    {
	                        marker.GetComponent<MarkerScript>().assign(unit, false);
	                        marker.GetComponent<MarkerScript>().positions[unit].attack = true;
	                    }
	                    else
	                    {
                            holeMarker.GetComponent<MarkerScript>().assign(unit, false);
                            holeMarker.GetComponent<MarkerScript>().positions[unit].attack = true;
	                    }
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
