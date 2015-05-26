using System.Net.Mime;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {

    void Start()
    {
        //gameObject.GetComponentInChildren<Image>().CrossFadeAlpha(0.0f,5.0f,false);
    }

    public void Click()
    {
        Application.LoadLevel("test");
    }
}
