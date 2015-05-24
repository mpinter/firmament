using System.Collections.Generic;
using System.Linq;
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
        if (Input.GetKeyDown(KeyCode.Q))
        {
            qAction();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (var unit in selected)
            {
                if (!(unit.GetComponent<UnitScript>().isTransforming || unit.GetComponent<UnitScript>().isLocked))
                    unit.GetComponent<UnitScript>().Cancel();
            }
        }
	}

    void OnGUI()
    {
        if (selected.Count > 0)
        {
            GameObject.FindGameObjectWithTag("QButton").GetComponentInChildren<Text>().text = selected.First().GetComponent<UnitScript>().qText;
            GameObject.FindGameObjectWithTag("WButton").GetComponentInChildren<Text>().text = selected.First().GetComponent<UnitScript>().wText;
            GameObject.FindGameObjectWithTag("EButton").GetComponentInChildren<Text>().text = selected.First().GetComponent<UnitScript>().eText;
            GameObject.FindGameObjectWithTag("RButton").GetComponentInChildren<Text>().text = selected.First().GetComponent<UnitScript>().rText;
        }
        else
        {
            GameObject.FindGameObjectWithTag("QButton").GetComponentInChildren<Text>().text = "Q";
            GameObject.FindGameObjectWithTag("WButton").GetComponentInChildren<Text>().text = "W";
            GameObject.FindGameObjectWithTag("EButton").GetComponentInChildren<Text>().text = "E";
            GameObject.FindGameObjectWithTag("RButton").GetComponentInChildren<Text>().text = "R";
        }
    }

    public void qAction()
    {
        foreach (var unit in selected)
        {
            if (!(unit.GetComponent<UnitScript>().isTransforming || unit.GetComponent<UnitScript>().isLocked))
                unit.GetComponent<UnitScript>().actionQ();
        }
    }

    public void wAction()
    {
        foreach (var unit in selected)
        {
            if (!(unit.GetComponent<UnitScript>().isTransforming || unit.GetComponent<UnitScript>().isLocked))
                unit.GetComponent<UnitScript>().actionW();
        }
    }

    public void eAction()
    {
        foreach (var unit in selected)
        {
            if (!(unit.GetComponent<UnitScript>().isTransforming || unit.GetComponent<UnitScript>().isLocked))
                unit.GetComponent<UnitScript>().actionE();
        }
    }

    public void rAction()
    {
        foreach (var unit in selected)
        {
            if (!(unit.GetComponent<UnitScript>().isTransforming || unit.GetComponent<UnitScript>().isLocked))
                unit.GetComponent<UnitScript>().actionR();
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
            else if (Input.mousePosition.x > 300 && Input.mousePosition.y > 55)
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
            Debug.Log("djobhdfu");
           unselectAll();
            obj.GetComponent<UnitScript>().select();
            selectHit = true;
        }
    }

    public void processRightClick(GameObject obj)
    {
        if (obj == null) return;
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
            if (unit.GetComponent<UnitScript>().isLocked) continue;
            if (unit.GetComponent<UnitScript>().owner == id && unit.GetComponent<UnitScript>().markerScript!=target)
            {
                if (unit.GetComponent<UnitScript>().isStructure)
                {
                    unit.GetComponent<UnitScript>().rally = target;
                }
                else
                {
                    target.assign(unit.gameObject, additive);
                    if (target.parentScript != null && target.parentScript.owner != id) target.positions[unit.gameObject].attack = true;    
                }
            }
            else if (unit.GetComponent<UnitScript>().isMineable && target.parentScript.owner == id &&
                     target.parentScript.isStructure)
            {
                //todo remove dead markers
                asteroidMarkers[unit] = target;

            }
        }
    }

    public void unselectAll()
    {
        Debug.Log("unselect all");
        foreach (var unit in selected)
        {
            unit.GetComponent<UnitScript>().unselect_noremove();
        }
        selected.Clear();
    }
}
