﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerScript : ForcesScript {

    public HashSet<GameObject> selected=new HashSet<GameObject>();
    private bool selectHit = false; //check if unit was succesfully clicked this frame
    public Dictionary<GameObject,GameObject> selectedDraw = new Dictionary<GameObject, GameObject>();
    public GameObject minimapCamera;
    //public Dictionary<GameObject, GameObject> shownMarkers = new Dictionary<GameObject, GameObject>();

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
	    }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            qAction();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            wAction();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            eAction();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            rAction();
        }
        if (Input.GetKey(KeyCode.Tab))
        {
            resPrimary += 47;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            foreach (var unit in selected)
            {
                if (unit == null) continue; //wat?
                if (unit.GetComponent<UnitScript>().unitType == UnitScript.UnitType.blackHole) continue;
                if (!(unit.GetComponent<UnitScript>().isTransforming || unit.GetComponent<UnitScript>().isLocked))
                    unit.GetComponent<UnitScript>().Cancel();
            }
        }
        DrawLines();
	}

    void DrawLines()
    {
        //on a scale of 0 to retarded, this code snippet would be your autistic cousin (just as retarded as unity's handling of destroyed units)
        //for future reference - never use GameObject as dictionary key, the hash value gets fucked up when the unit is destroyed
        foreach (KeyValuePair<GameObject,GameObject> wat in selectedDraw)
        {
            if (wat.Key==null) Destroy(wat.Value);
        }
        //since this is relatively cheap enough and requires less thinking than removing only the required ones
        foreach (var unit in selectedDraw.Keys.ToArray())
        {
            if (unit == null)
            {
                //Destroy(selectedDraw[unit]);
                selectedDraw.Remove(unit);
                selected.Remove(unit);
                asteroidMarkers.Remove(unit);
                continue;
            }
            if ((!selected.Contains(unit)) || (unit.GetComponent<UnitScript>().targetScriptList.Count == 0 && !unit.GetComponent<UnitScript>().isMineable))
            {
                Destroy(selectedDraw[unit]);
                selectedDraw.Remove(unit);
                selected.Remove(unit);
                if (asteroidMarkers.ContainsKey(unit) && asteroidMarkers[unit] == null)
                {
                    asteroidMarkers.Remove(unit);
                }
                continue;
            } 
            if (unit.GetComponent<UnitScript>().targetScriptList.Count > 0 && (unit.GetComponent<UnitScript>().targetScriptList[0] == null || (unit.GetComponent<UnitScript>().targetScriptList[0].parentScript!=null && unit.GetComponent<UnitScript>().targetScriptList[0].parentScript.gameObject == null)))
            {
                Destroy(selectedDraw[unit]);
                selectedDraw.Remove(unit);
                selected.Remove(unit);
                if (asteroidMarkers.ContainsKey(unit) && asteroidMarkers[unit] == null)
                {
                    asteroidMarkers.Remove(unit);
                }
                continue;
            }
            if (asteroidMarkers.ContainsKey(unit) && asteroidMarkers[unit] == null)
            {
                asteroidMarkers.Remove(unit);
                continue;
            }
        }
        foreach (var unit in selected)
        {
            if (unit.GetComponent<UnitScript>().isMineable)
            {
                //draw all miners
                foreach (var asteroidMarker in asteroidMarkers)
                {
                    if (!selectedDraw.ContainsKey(asteroidMarker.Key))
                        selectedDraw[asteroidMarker.Key]=(Instantiate(Resources.Load("Prefabs/Line", typeof (GameObject)) as GameObject));
                    selectedDraw[asteroidMarker.Key].GetComponent<LineRenderer>().SetPosition(0, asteroidMarker.Key.transform.position);
                    selectedDraw[asteroidMarker.Key].GetComponent<LineRenderer>().SetPosition(1, asteroidMarker.Value.transform.position);
                    selectedDraw[asteroidMarker.Key].GetComponent<LineRenderer>().SetColors(Color.yellow, Color.yellow);
                }
            }
            /*if (unit.GetComponent<UnitScript>().isStructure && unit.GetComponent<UnitScript>().rally != null)
            {
                GameObject target = unit.GetComponent<UnitScript>().rally.gameObject;
                if (!selectedDraw.ContainsKey(unit))
                    selectedDraw[unit] = (Instantiate(Resources.Load("Prefabs/Line", typeof(GameObject)) as GameObject));
                selectedDraw[unit].GetComponent<LineRenderer>().SetPosition(0, target.transform.position);
                selectedDraw[unit].GetComponent<LineRenderer>().SetPosition(1, unit.transform.position);
                selectedDraw[unit].GetComponent<LineRenderer>().SetColors(Color.yellow, Color.yellow);

            } else */if (!unit.GetComponent<UnitScript>().isStructure)
            {
                if (unit.GetComponent<UnitScript>().targetScriptList.Count == 0) continue;
                
                MarkerScript target = unit.GetComponent<UnitScript>().targetScriptList[0];
                if (target == null) continue;
                if (!target.positions.ContainsKey(unit)) continue; //in case errors should happen, silently ignore
                Color color;
                if (target.positions[unit].attack)
                {
                    color = Color.red;
                }
                else
                {
                    color = Color.white;
                }
                if (!selectedDraw.ContainsKey(unit))
                    selectedDraw[unit] = (Instantiate(Resources.Load("Prefabs/Line", typeof(GameObject)) as GameObject));
                selectedDraw[unit].GetComponent<LineRenderer>().SetPosition(0, target.transform.position);
                selectedDraw[unit].GetComponent<LineRenderer>().SetPosition(1, unit.transform.position);
                selectedDraw[unit].GetComponent<LineRenderer>().SetColors(color,color);
            }
        }
        
    }

    void OnGUI()
    {
        GameObject.FindGameObjectWithTag("ResourceText").GetComponent<Text>().text = "Resources: " + resPrimary +
                                                                                          " Population: " + resSecondary +
                                                                                          "/" + (120-structures.Count*30);
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
        if (Input.GetMouseButtonUp(0) && (CameraScript.downTime < 0.15f) && !selectHit)
        {
            float x = Input.mousePosition.x - (Screen.width - 400);
            float y = Input.mousePosition.y - (Screen.height - 258);
            if (performingAction)
            {
                GameObject marker = (GameObject)Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)));
                if (x > 0 && y > 0 && Camera.main.gameObject.GetComponent<CameraZoomScript>().minimapActive)
                {
                    marker.GetComponent<Transform>().position = minimapCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(x, y));
                }
                else
                {
                    marker.GetComponent<Transform>().position = Camera.main.ScreenToWorldPoint(CameraScript.lastClickPos);
                }     
                bool additive = Input.GetKey(KeyCode.LeftShift);
                setTarget(marker.GetComponent<MarkerScript>(), additive);
                foreach (var unit in selected)
                {
                    if (!marker.GetComponent<MarkerScript>().positions.ContainsKey(unit)) continue; //some may have been skipped (locked/transforming/not mine)
                    marker.GetComponent<MarkerScript>().positions[unit].attack = true;
                }
                performingAction = false;
            }
            else if (!((Input.mousePosition.x < 300 && Input.mousePosition.y < 55) || (x > 0 && y > 0 && Camera.main.gameObject.GetComponent<CameraZoomScript>().minimapActive)))
            {
                foreach (var unit in selected)
                {
                    unit.GetComponent<UnitScript>().unselect_noremove();
                }
                selected.Clear();    
            }
        }
        if (Input.GetMouseButtonUp(1) && (CameraScript.downTime < 0.15f) && !selectHit && selected.Count>0 && !performingAction)
        {
            
            //todo markers display only if their linked units are selected - check this on slection change - gethashcode
            GameObject marker = (GameObject)Instantiate(Resources.Load("Prefabs/Marker", typeof(GameObject)));
            float x = Input.mousePosition.x - (Screen.width - 400);
            float y = Input.mousePosition.y - (Screen.height - 258);
            if (x > 0 && y > 0 && Camera.main.gameObject.GetComponent<CameraZoomScript>().minimapActive)
            {
                marker.GetComponent<Transform>().position = minimapCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(x, y));
            }
            else
            {
                marker.GetComponent<Transform>().position = Camera.main.ScreenToWorldPoint(CameraScript.lastClickPos);    
            }
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
            if (!performingAction)
            {
                unselectAll();
                obj.GetComponent<UnitScript>().select();
                selectHit = true;
            }
            else
            {
                processRightClick(obj);
            }
        }
    }

    public void processRightClick(GameObject obj)
    {
        if (obj == null) return;
         if (obj.tag == "Selectable")
         {
            bool additive = Input.GetKey(KeyCode.LeftShift);
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
            else if (unit.GetComponent<UnitScript>().isMineable && target.parentScript!=null && target.parentScript.owner == id &&
                     target.parentScript.isStructure)
            {
                //todo remove dead markers
                asteroidMarkers[unit] = target;
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
