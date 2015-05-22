using System.Collections.Generic;
using System.Runtime.Remoting;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerScript : ForcesScript {

    public HashSet<GameObject> selected=new HashSet<GameObject>();
    public int id;
    private bool selectHit = false; //check if unit was succesfully clicked this frame

    //todo null && switch actions
    public bool performingAction = false;

	void Start ()
	{
	    id = 0;
	}
	
	void Update () {
	
	}

    void LateUpdate()
    {
        //todo pretify
        if (Input.GetMouseButtonUp(0) && (CameraScript.downTime < 0.1f) && !selectHit && !performingAction)
        {
            //todo
            foreach (var unit in selected)
            {
                unit.GetComponent<UnitScript>().unselect_noremove();
            }
            selected.Clear();
        }
        if (Input.GetMouseButtonUp(1) && (CameraScript.downTime < 0.1f) && !selectHit && selected.Count>0 && !performingAction)
        {
            //todo markers display only if their linked units are selected - check this on slection change - gethashcode
            GameObject marker = (GameObject)Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)));
            marker.GetComponent<Transform>().position = Camera.main.ScreenToWorldPoint(CameraScript.lastClickPos);
            setTarget(marker.GetComponent<MarkerScript>());
        }
        selectHit = false;
    }

    public void processLeftClick(GameObject obj)
    {
        if (obj == null) return;
        //todo for now only selecting
        if (obj.tag == "Selectable")
        {
            foreach (var unit in selected)
            {
                unit.GetComponent<UnitScript>().unselect_noremove();
            }
            selected.Clear();
            obj.GetComponent<UnitScript>().select();
            selectHit = true;
        }
    }

    public void processRightClick(GameObject obj)
    {
        if (obj == null) return;
        //todo - for now only moving, add attack, mining and stuff
         if (obj.tag == "Selectable")
        {
            setTarget(obj.GetComponent<UnitScript>().markerScript);
            selectHit = true;
        }
    }

    private void setTarget(MarkerScript target)
    {
        foreach (var unit in selected)
        {
            if (unit.GetComponent<UnitScript>().owner == id)
            {
                //todo late at night , check this with working mind (cuz right now rhymes are nigh)
                target.assign(unit.gameObject);
                //unit.GetComponent<UnitScript>().targetScript = target.GetComponent<UnitScript>().markerScript;
            }
        }
    }
}
