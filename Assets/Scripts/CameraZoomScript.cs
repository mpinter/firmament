using UnityEngine;
using System.Collections;

public class CameraZoomScript : MonoBehaviour {

    //mostly taken from answer to http://answers.unity3d.com/questions/833108/2d-camera-zoom-smoothing-and-limitations.html

    public float zoomSpeed = 1;
    public float targetOrtho;
    public float smoothSpeed = 20.0f;
    public float minOrtho = 1.0f;
    public float maxOrtho = 100.0f;

    public float keyScrollSpeed = 10.0f;
    public float mouseScrollSpeed = 5.0f;

    public float sceneLimitX = 450;
    public float sceneLimitY = 280;

    public bool minimapActive=true;

    public GameObject minimapCamera;

	void Start () {
        targetOrtho = Camera.main.orthographicSize;
	}

    //from stackOverflow
    public float horzExtent()
    {
       return Camera.main.orthographicSize * Screen.width / Screen.height;
    }
	
	void Update () {
	    if (Input.GetKeyDown(KeyCode.M))
	    {
	        
	    }
	    if (Input.GetMouseButtonDown(0))
	    {
	        float x = Input.mousePosition.x - (Screen.width - 400);
            float y = Input.mousePosition.y - (Screen.height - 258);
	        if (x > 0 && y > 0)
	        {
	            goToPosition(minimapCamera.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(x,y)));
	        }
	    }
        if (Input.GetAxis("Mouse ScrollWheel") < 0) // forward
         {
             targetOrtho+=2.0f;
             //if (targetOrtho < maxOrtho) GameObject.FindGameObjectWithTag("Background").transform.localScale += Vector3.one*2;
         }
         if (Input.GetAxis("Mouse ScrollWheel") > 0) // back
         {
             targetOrtho -= 2.0f;
             //if (targetOrtho > minOrtho) GameObject.FindGameObjectWithTag("Background").transform.localScale -= Vector3.one * 2;
         }
        targetOrtho=Mathf.Clamp(targetOrtho, minOrtho, maxOrtho);
        Camera.main.orthographicSize = Mathf.MoveTowards(Camera.main.orthographicSize, targetOrtho, smoothSpeed * Time.deltaTime);
	    float xChange = 0;
	    float yChange = 0;
	    if (Input.GetKey(KeyCode.UpArrow))
	    {
	        yChange += keyScrollSpeed;
	    }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            yChange -= keyScrollSpeed;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            xChange -= keyScrollSpeed;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            xChange += keyScrollSpeed;
        }
        if (Input.mousePosition.y==Screen.height)
        {
            yChange += mouseScrollSpeed;
        }
        if (Input.mousePosition.y == 0)
        {
            yChange -= mouseScrollSpeed;
        }
        if (Input.mousePosition.x == Screen.width)
        {
            xChange += mouseScrollSpeed;
        }
        if (Input.mousePosition.x == 0)
        {
            xChange -= mouseScrollSpeed;
        }
        float scrollOrtho = Mathf.Clamp(targetOrtho/10, 1, 10);
        goToPosition(new Vector3(Camera.main.transform.position.x + xChange * scrollOrtho * Time.deltaTime, Camera.main.transform.position.y + yChange * scrollOrtho * Time.deltaTime, Camera.main.transform.position.z));
	}

    public void goToPosition(Vector3 position)
    {
        float x, y;
        float lowerx = position.x - horzExtent();
        float upperx = position.x + horzExtent();
        float lowery = position.y - Camera.main.orthographicSize;
        float uppery = position.y + Camera.main.orthographicSize;
        x = (lowerx < 0) ? position.x - lowerx : position.x;
        x = (upperx > sceneLimitX) ? x - (upperx-sceneLimitX) : x;
        y = (lowery < 0) ? position.y - lowery : position.y;
        y = (uppery > sceneLimitY) ? y - (uppery - sceneLimitY) : y;
        Camera.main.transform.position = new Vector3(x,y, Camera.main.transform.position.z); 
        /*Debug.Log("===============");
        Debug.Log(lowerx);
        Debug.Log(upperx);
        Debug.Log(lowery);
        Debug.Log(uppery);
        Debug.Log(x);
        Debug.Log(y);*/
    }
}
