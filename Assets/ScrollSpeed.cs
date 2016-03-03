using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScrollSpeed : MonoBehaviour {

	// Use this for initialization
	void OnEnable ()
    {
        GetComponent<ScrollRect>().scrollSensitivity = 100;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
