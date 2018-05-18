using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using SharedTypes;

[Serializable]
public class ConfigContent
{
    public string ServerIp;
    public string ServerPort;

    public ConfigContent() { }

    public ConfigContent(string serverIp, string serverPort)
    {
        ServerIp = serverIp;
        ServerPort = serverPort;
    }
}

public class ConfigLoader : MonoBehaviour
{

    // Use this for initialization
    private void Start()
    {
        string config = LoadFile();
        ConfigContent configuration = JsonUtility.FromJson<ConfigContent>(config);
        ConfigSingleton configInstance = ConfigSingleton.GetInstance();
        configInstance.SetMyNetworkConfig(new MyNetworkConfig(configuration.ServerIp, configuration.ServerPort));
    }

    private string LoadFile()
    {
        return File.ReadAllText("./config.json");
    }
}
