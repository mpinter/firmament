using UnityEngine;
using System.Collections;

public class FixRotationScript : MonoBehaviour {

/*	Quaternion rotation;
 */
    private Vector3 pos;
  void Awake()
  {
       pos = transform.position;
  }
 
  void LateUpdate()
  {
      //transform.position = pos;
      /*   if (transform.rotation != Quaternion.Euler(0, 0, 0))
      {
          transform.rotation = Quaternion.Euler(0, 0, 0);
      }
       // transform.rotation = rotation;
 */
  }
}
