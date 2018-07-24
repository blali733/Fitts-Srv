using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using FittsLibrary;
using UnityEngine.Networking;

[Serializable]
public class ConfigContent
{
    public string ServerPort;
    public string DBDomain;
    public string TestGroup;
    public string ColorSpace;
}

public class ConfigLoader : MonoBehaviour
{

    // Use this for initialization
    private void Start()
    {
        string config = LoadFile();
        ConfigContent configuration = JsonUtility.FromJson<ConfigContent>(config);
        ConfigSingleton configInstance = ConfigSingleton.GetInstance();
        configInstance.SetMyNetworkConfig(new MyNetworkConfig("0.0.0.0", configuration.ServerPort));
        configInstance.DBDomain = configuration.DBDomain;
        configInstance.TestGroup = configuration.TestGroup;
        NetworkManagerEvents.singleton.networkPort = int.Parse(configuration.ServerPort);
        NetworkManagerEvents.singleton.StartServer();
        
    }

    private string LoadFile()
    {
        return File.ReadAllText("./server_config.json");
    }
}
