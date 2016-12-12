using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finish : MonoBehaviour {
  void OnTriggerEnter2D(Collider2D other)
  {
    Debug.Log("OnTriggerEnter");
    if (other.tag == "Player")
    {
      Destroy(this);
    }
  }
}
