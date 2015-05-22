using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ForcesScript : MonoBehaviour {

    //all the super important structures
    public HashSet<GameObject> units=new HashSet<GameObject>();
    public HashSet<GameObject> structures = new HashSet<GameObject>();

    public int resPrimary;
    public int resSecondary;
}
