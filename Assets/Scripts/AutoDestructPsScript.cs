using UnityEngine;
using System.Collections;

public class AutoDestructPsScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	    if (!gameObject.GetComponent<ParticleSystem>().IsAlive())
	    {
	        Destroy(gameObject);
	    }
	}
}
