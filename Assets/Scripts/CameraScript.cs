using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{
    //rect select
    public Texture2D selectionHighlight = null;
    public static Rect selection=new Rect(0,0,0,0);
    public static Vector3 lastClickPos = -Vector3.one;
    public static Vector3 startClick = -Vector3.one;

    //click select
    public static float downTime;

    private PlayerScript playerScript;
    private MasterScript masterScript;

    void Start()
    {
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>();
    }

	void Update ()
	{
	    checkSelection();
	}

    //done following a tutorial -> https://www.youtube.com/watch?v=2wgeDQlwnQ0
    private void checkSelection()
    {
        //if (Input.GetMouseButtonDown(0) && !playerScript.performingAction && Input.mousePosition.x > 300 && Input.mousePosition.y > 55) playerScript.unselectAll(); //ugly haxxor - maybe place elsewhere?
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            startClick = new Vector3(Input.mousePosition.x, Input.mousePosition.y,10f);
            downTime = 0;
        }
        else {
            if (Input.GetMouseButton(1))
            {
                downTime += Time.deltaTime;
                playerScript.performingAction = false;
            }
            else if (Input.GetMouseButton(0))
            {
                downTime += Time.deltaTime;
                selection=new Rect(startClick.x, invertProjectY(startClick.y), Input.mousePosition.x-startClick.x, invertProjectY(Input.mousePosition.y)-invertProjectY(startClick.y));
                if (selection.width < 0)
                {
                    selection.x += selection.width;
                    selection.width = -selection.width;
                }
                if (selection.height < 0)
                {
                    selection.y += selection.height;
                    selection.height = -selection.height;
                }
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            lastClickPos = startClick;
            startClick = -Vector3.one;
            selection = new Rect(0, 0, 0, 0);
        }
        if (Input.GetMouseButtonUp(1))
        {
            lastClickPos = startClick;
            startClick = -Vector3.one;
            selection = new Rect(0, 0, 0, 0);
        }
    }

    private void OnGUI()
    {
        if (startClick != -Vector3.one)
        {
            GUI.color=new Color(1,1,1,0.5f);
            GUI.DrawTexture(selection,selectionHighlight);
        }
    }

    public static float invertProjectY(float y)
    {
        return Screen.height - y;
    }
}
