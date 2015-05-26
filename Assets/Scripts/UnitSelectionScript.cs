using UnityEngine;
using System.Collections;


//handling both rectangle selection, left and right clicks
public class UnitSelectionScript : MonoComponents
{
    public bool selected = false;

    private PlayerScript playerScript;
    private UnitScript unitScript;

    void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
        unitScript = GetComponent<UnitScript>();
        Init();   
    }
	
	void Update () {
	    if ((unitScript.owner==playerScript.id)&&(!playerScript.performingAction)) checkRectSelect();
        playerScript.processLeftClick(checkClickSelect(0));
        playerScript.processRightClick(checkClickSelect(1));
	}

    private void checkRectSelect()
    {
        bool previous_select = selected;
        if (renderer.isVisible && Input.GetMouseButton(0) && !(Input.mousePosition.x < 300 && Input.mousePosition.y < 55))
        {
            Vector3 camPos = Camera.main.WorldToScreenPoint(transform.position);
            camPos.y = CameraScript.invertProjectY(camPos.y);
            selected = CameraScript.selection.Contains(camPos);
        }
        if (previous_select != selected)
        {
            if (selected)
            {
                unitScript.select();
            }
            else
            {
                unitScript.unselect();
            }
        }
    }

    //todo return object, pass to master
    public GameObject checkClickSelect(int button)
    {
        //race conditions in if, doesn't matter had snoo-snoo
        if ((Input.GetMouseButtonUp(button)) && (CameraScript.downTime < 0.1f))
        {
            Vector3 clickPos = (CameraScript.startClick == -Vector3.one)
                ? Camera.main.ScreenToWorldPoint(CameraScript.lastClickPos)
                : Camera.main.ScreenToWorldPoint(CameraScript.startClick);
            if (renderer.bounds.Contains(clickPos))
            {
                return gameObject;
            }
        }
        return null;
    }
}
