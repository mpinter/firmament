using System.Collections.Generic;
using System.Runtime.Remoting;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerScript : ForcesScript {

    public HashSet<GameObject> selected=new HashSet<GameObject>();
    private bool selectHit = false; //check if unit was succesfully clicked this frame

    //todo null && switch actions
    public bool performingAction = false;

	void Start ()
	{
	    id = 0;
	}
	
	void Update () {
	    if (Input.GetKey(KeyCode.A))
	    {
	        performingAction = true;
            Debug.Log("action");
	    }
	}

    void LateUpdate()
    {
        //todo pretify
        if (Input.GetMouseButtonUp(0) && (CameraScript.downTime < 0.1f) && !selectHit)
        {
            //todo
            if (performingAction)
            {
                GameObject marker = (GameObject)Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)));
                marker.GetComponent<Transform>().position = Camera.main.ScreenToWorldPoint(CameraScript.lastClickPos);
                bool additive = (Input.GetKey(KeyCode.LeftShift)) ? true : false;
                setTarget(marker.GetComponent<MarkerScript>(), additive);
                foreach (var unit in selected)
                {
                    marker.GetComponent<MarkerScript>().positions[unit].attack = true;
                }
                performingAction = false;
            }
            else
            {
                foreach (var unit in selected)
                {
                    unit.GetComponent<UnitScript>().unselect_noremove();
                }
                selected.Clear();    
            }
        }
        if (Input.GetMouseButtonUp(1) && (CameraScript.downTime < 0.1f) && !selectHit && selected.Count>0 && !performingAction)
        {
            
            //todo markers display only if their linked units are selected - check this on slection change - gethashcode
            GameObject marker = (GameObject)Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)));
            marker.GetComponent<Transform>().position = Camera.main.ScreenToWorldPoint(CameraScript.lastClickPos);
            bool additive = (Input.GetKey(KeyCode.LeftShift)) ? true : false;
            setTarget(marker.GetComponent<MarkerScript>(),additive);
        }
        selectHit = false;
    }

    public void processLeftClick(GameObject obj)
    {
        if (obj == null) return;
        //todo for now only selecting
        if (obj.tag == "Selectable")
        {
           unselectAll();
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
            Debug.Log("Right click");
            bool additive = (Input.GetKey(KeyCode.LeftShift)) ? true : false;
            setTarget(obj.GetComponent<UnitScript>().markerScript,additive);
            selectHit = true;
        }
    }

    private void setTarget(MarkerScript target,bool additive)
    {
        foreach (var unit in selected)
        {
            //Debug.Log("asfasfasfas");
            //Debug.Log(unit);
            //Debug.Log(target);
            if (unit.GetComponent<UnitScript>().owner == id && unit.GetComponent<UnitScript>().markerScript!=target)
            {
                target.assign(unit.gameObject,additive);
                if (target.parentScript!=null && target.parentScript.owner != id) target.positions[unit.gameObject].attack = true;
            }
            else if (unit.GetComponent<UnitScript>().isMineable && target.parentScript.owner == id &&
                     target.parentScript.isStructure)
            {
                /*
                Debug.Log("lcick planet");
                Debug.Log(unit.GetComponent<UnitScript>().targetScriptList.Count);
                target.assign(unit.gameObject, additive);
                Debug.Log(unit.GetComponent<UnitScript>().targetScriptList.Count);*/
                //todo remove dead markers
                asteroidMarkers[unit] = target;//unit.GetComponent<UnitScript>().targetScriptList[unit.GetComponent<UnitScript>().targetScriptList.Count - 1];
                //Debug.Log(unit);
                //Debug.Log(target.gameObject);
                //unit.GetComponent<UnitScript>().targetScriptList.Clear(); //asteroid doesn't need this, contains targets from all players
            }
        }
    }

    public void unselectAll()
    {
        foreach (var unit in selected)
        {
            unit.GetComponent<UnitScript>().unselect_noremove();
        }
        selected.Clear();
    }
}
