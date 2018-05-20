using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLoop : MonoBehaviour
{

    private string secretKeyR = "_4T%rAcXd794@Vg!_ghtGH=yWv-z5_Cy";
    private string secretKeyW = "bR@RCNYHdj3!_a3#BR=%TPs=*SNY5cX$";
    private string addUserURL = "add_user.php?"; //be sure to add a ? to your url
    private string listUserURL = "list_user.php";
    private string domain;
    private ConfigSingleton _config;

    // Use this for initialization
    void Start()
    {
        Application.targetFrameRate = 10;
        _config = ConfigSingleton.GetInstance();
    }

    public void TestCaseSetup()
    {
        domain = _config.DBDomain;
    }

}
