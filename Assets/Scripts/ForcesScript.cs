using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ForcesScript : MonoBehaviour {

    //all the super important structures
    public HashSet<GameObject> units = new HashSet<GameObject>();
    public HashSet<GameObject> structures = new HashSet<GameObject>();

    public Dictionary<GameObject, MarkerScript> asteroidMarkers = new Dictionary<GameObject, MarkerScript>();

    public float resPrimary;
    public int resSecondary;

    public int id;
}
