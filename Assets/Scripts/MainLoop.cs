using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLoop : MonoBehaviour
{
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
