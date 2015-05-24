using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class MarkerScript : MonoComponents {

    public Dictionary<GameObject,Position> positions=new Dictionary<GameObject, Position>();
    private int capitalCount;
    public UnitScript parentScript=null;
    public bool hasParent = false;
    public float orbit=10.0f;

    private Vector3 previousPos;

    public class Position
    {
        public bool follow;
        public bool attack;
        public bool isset;

        public Transform markerPos;
        private Vector3 pos;
        public bool isCircular;
        private float angle = 0.0f;
        private float speed = 0.001f;
        private float radius = 2.0f; //todo set in contructor (for planets and stuff, later add boolean if supposed to scale)
        private bool isAgile;

        public Position(GameObject which, UnitScript whereScript,Transform mt,float radiusScale=-1.0f,bool forceCircular=false)
        {
            if (radiusScale < 0f)
            {
                speed = which.GetComponent<UnitScript>().speedMax*10;
            }
            else
            {
                speed = (0.01f + Random.Range(0f, 0.07f))/radius;
            }
            markerPos = mt;
            UnitScript whichScript = which.GetComponent<UnitScript>();
            if (whereScript == null)
            {
                isAgile = whichScript.agile;
            }
            else
            {
                isCircular = whereScript.isStructure;
                if (forceCircular) isCircular = true;
                follow = !isCircular;
                if (isCircular)
                {
                    //get closest point
                    radius = (radiusScale<0) ? whereScript.planetRadius : radiusScale;
                    angle = (Vector3.Angle(mt.up, which.transform.position - mt.position) + 90);
                    if (Vector3.Cross(mt.up, which.transform.position - mt.position).z < 0f) angle += 90;
                    if ((which.transform.position.x > mt.position.x) && (which.transform.position.y > mt.position.y)) angle += 180;
                    angle*= Mathf.Deg2Rad;
                    angle += Random.Range(-0.8f, 0.8f);
                }
            }
            isset = false;
        }

        public Vector3 getTargetPosition()
        {
            if (follow) return markerPos.position;
            if (isCircular)
            {
                if (angle > 2 * Math.PI) angle -= (float)(2 * Math.PI);
                pos = new Vector3(radius * (float)Math.Cos(angle) + markerPos.position.x, radius * (float)Math.Sin(angle) + markerPos.position.y, 0f);
                return pos;
            }
            return pos+markerPos.position;
        }

        public Vector3 generateNewTargetPosition()
        {
            isset = true;
            if (isCircular)
            {
                angle += speed*Time.deltaTime;
                //Debug.Log(angle);
                //Debug.Log(speed);
                return getTargetPosition();
            }
            else if (isAgile)
            {
                pos = (follow) ? Vector2.zero : Random.insideUnitCircle*radius;
                return pos + markerPos.position;
            }
            else if (follow)
            {
                return markerPos.position;
            }
            else
            {
                Debug.Log("Shouldn't really get here with current units, maybe with spores only?");
                pos = (follow) ? Vector2.zero : Random.insideUnitCircle;
                pos += markerPos.position;
                return pos;
            }
        }

        public void setPosition(Vector3 position)
        {
            isset = true;
            pos = position;
        }


        public float getTurnSpeedModifier(Vector3 unitPos)
        {
            if (isAgile && !follow && !isCircular)
            {
                if (Vector3.Distance(unitPos, markerPos.position) < radius)
                {
                    return radius/2; //todo finetune
                }
            }
            return 1;
        }
    }

    //pyramid - yay , tesim sa na debug (; ;)
    public void generateCapitals(Vector3 normalized_direction)
    {
        int capitalOffset = (int)(Math.Log(capitalCount)/2.0);
        int currentOffset = 0;
        float leftOffsetBase = 0;
        float currentLeftOffset = 0;
        int currentRow = 0;
        int rowLimit = 1;
        Vector3 cross = Vector3.Cross(normalized_direction, Vector3.forward);
        foreach (var record in positions)
        {
            if (record.Key.GetComponent<UnitScript>().capital)
            {
                Vector3 newpos = normalized_direction*(capitalOffset-currentOffset);
                newpos += cross*currentLeftOffset;
                record.Value.setPosition(newpos);
                currentRow++;
                currentLeftOffset -= 3;
                if (currentRow == rowLimit)
                {
                    currentOffset++;
                    leftOffsetBase += 1.5f;
                    currentLeftOffset = leftOffsetBase;
                    currentRow = 0;
                    rowLimit *= 2;
                }
            }
        }
    }

    public Vector3 getOffset()
    {
        return transform.position - previousPos;
    }

    void Update()
    {
        if (parentScript != null) hasParent = true;
        if (hasParent && parentScript==null) Remove();
        if (parentScript==null && positions.Count==0) Destroy(gameObject);
    }

    void LateUpdate()
    {
        previousPos = transform.position;
    }

    void Awake()
    {
        Init();
        capitalCount = 0;
        previousPos = transform.position;
    }

    public void assignFront(GameObject obj)
    {
        //forced - rewrites if record exists
        obj.GetComponent<UnitScript>().targetScriptList.Insert(0,this);
        positions[obj] = new Position(obj, parentScript, gameObject.GetComponent<Transform>());
        if (obj.GetComponent<UnitScript>().capital)
        {
            capitalCount++;
            generateCapitals(Vector3.Normalize(gameObject.GetComponent<Transform>().position - obj.GetComponent<Transform>().position));
        }
        else
        {
            positions[obj].generateNewTargetPosition();
        }
    }

    public void assign(GameObject obj,bool additive)
    {
        if (positions.ContainsKey(obj)) return;
        if (additive)
        {
            obj.GetComponent<UnitScript>().targetScriptList.Add(this);  
        }
        else
        {
            if (obj.GetComponent<UnitScript>().targetScriptList.Count > 0)
            {
                foreach (var targetScript in obj.GetComponent<UnitScript>().targetScriptList)
                {
                    targetScript.unassign_noremove(obj);    
                }
                obj.GetComponent<UnitScript>().targetScriptList.Clear();
            }
            obj.GetComponent<UnitScript>().targetScriptList.Add(this);    
        }
        positions.Add(obj,new Position(obj,parentScript,gameObject.GetComponent<Transform>()));
        if (obj.GetComponent<UnitScript>().capital)
        {
            capitalCount++;
            generateCapitals(Vector3.Normalize(gameObject.GetComponent<Transform>().position - obj.GetComponent<Transform>().position));
        }
        else
        {
            positions[obj].generateNewTargetPosition();
        }
    }

    //use with caution, currently used for orbiting planets and satellites
    public void forcePosition(GameObject obj, GameObject which, UnitScript whereScript, Transform mt)
    {
        obj.GetComponent<UnitScript>().targetScriptList.Add(this);
        positions.Add(obj, new Position(which,whereScript,mt,orbit,true));
    }

    public void unassign(GameObject obj)
    {
        positions.Remove(obj);
        if (obj.GetComponent<UnitScript>().capital)
        {
            capitalCount--;
        }
        obj.GetComponent<UnitScript>().targetScriptList.Remove(this);
        if (positions.Count==0 && parentScript==null) Destroy(gameObject);
    }

    public void unassign_noremove(GameObject obj)
    {
        positions.Remove(obj);
        if (obj.GetComponent<UnitScript>().capital)
        {
            capitalCount--;
        }
        if (positions.Count == 0 && parentScript == null) Destroy(gameObject);
    }

    public void Remove()
    {
        foreach (var obj in positions)
        {
            //unassign_noremove(obj.Key);
            obj.Key.GetComponent<UnitScript>().targetScriptList.Remove(this);
        }
        Destroy(gameObject);
    }

}
