using UnityEngine;
using System.Collections;


//handling both rectangle selection, left and right clicks
public class UnitSelectionScript : MonoComponents
{
    public bool selected = false;

    void Start()
    {
        Init();   
    }
	
	void Update () {
	    checkRectSelect();
        checkClickSelect(0);
        checkClickSelect(1);
	}

    private void checkRectSelect()
    {
        if (renderer.isVisible && Input.GetMouseButton(0))
        {
            Vector3 camPos = Camera.main.WorldToScreenPoint(transform.position);
            camPos.y = CameraScript.invertProjectY(camPos.y);
            selected = CameraScript.selection.Contains(camPos);
        }
        /*
        if (selected)
        {
            renderer.material.color = Color.red;
        }
        else
        {
            renderer.material.color = Color.white;
        }
         * */
    }

    //todo return object, pass to master
    private GameObject checkClickSelect(int button)
    {
        //race conditions in if, doesn't matter had snoo-snoo
        if ((Input.GetMouseButtonUp(button)) && (CameraScript.downTime < 0.3f))
        {
            Vector3 clickPos = (CameraScript.startClick == -Vector3.one)
                ? Camera.main.ScreenToWorldPoint(CameraScript.lastClickPos)
                : Camera.main.ScreenToWorldPoint(CameraScript.startClick);
            if (renderer.bounds.Contains(clickPos))
            {
                return gameObject;
            }
        }
    }
}
