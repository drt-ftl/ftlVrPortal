using UnityEngine;
using System.Collections;

public class stopper : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void OnGUI () {
		if (Input.GetKey (KeyCode.Escape))
			Application.Quit ();
	}
}
