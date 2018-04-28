using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLoop : MonoBehaviour {

    private string secretKeyR = "_4T%rAcXd794@Vg!_ghtGH=yWv-z5_Cy";
    private string secretKeyW = "bR@RCNYHdj3!_a3#BR=%TPs=*SNY5cX$";
    public string addUserURL = "http://blali733.dynu.com/unitydb/add_user.php?"; //be sure to add a ? to your url
    public string listUserURL = "http://blali733.dynu.com/unitydb/list_user.php";

    // Use this for initialization
    void Start () {
        Application.targetFrameRate = 10;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
