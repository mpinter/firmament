using UnityEngine;
using System.Collections;

public class CameraZoomScript : MonoBehaviour {

    //mostly taken from answer to http://answers.unity3d.com/questions/833108/2d-camera-zoom-smoothing-and-limitations.html

    public float zoomSpeed = 1;
    public float targetOrtho;
    public float smoothSpeed = 20.0f;
    public float minOrtho = 1.0f;
    public float maxOrtho = 155.0f;

    public float keyScrollSpeed = 10.0f;
    public float mouseScrollSpeed = 5.0f;

	void Start () {
        targetOrtho = Camera.main.orthographicSize;
	}
	
	void Update () {
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
        Camera.main.transform.position = new Vector3(Camera.main.transform.position.x + xChange * scrollOrtho * Time.deltaTime, Camera.main.transform.position.y + yChange * scrollOrtho * Time.deltaTime, Camera.main.transform.position.z);
	}
}
