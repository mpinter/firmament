using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MasterScript : MonoBehaviour
{
    public List<ForcesScript> forces=new List<ForcesScript>();

    public GameObject friendlyHole;
    public GameObject hostileHole;

	void Start ()
	{
	    forces.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>());
        foreach (var force in GameObject.FindGameObjectsWithTag("AI"))
	    {
            forces.Add(force.GetComponent<AIScript>());
	    }
	}
	
	void Update () {
        if (hostileHole.GetComponent<UnitScript>().hp<0.0f) Camera.main.gameObject.GetComponent<CameraZoomScript>().Accomplished();
	    if (Input.GetKeyDown(KeyCode.X))
	    {
	        Application.Quit();
	    }
	}

    void LateUpdate()
    {

    }
}
