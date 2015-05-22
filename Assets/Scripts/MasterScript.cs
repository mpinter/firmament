using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class MasterScript : MonoBehaviour
{
    public List<ForcesScript> forces=new List<ForcesScript>();

	void Start ()
	{
	    forces.Add(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>());
        foreach (var force in GameObject.FindGameObjectsWithTag("AI"))
	    {
            forces.Add(force.GetComponent<AIScript>());
	    }
	}
	
	void Update () {
	
	}

    void LateUpdate()
    {

    }
}
