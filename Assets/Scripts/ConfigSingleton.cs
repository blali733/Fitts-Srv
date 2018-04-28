using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetworkConfig
{
    public string address;
    public string port;
    public NetworkConfig(string address, string port)
    {
        this.address = address;
        this.port = port;
    }
}

public class ConfigSingleton {
    //Singleton instance
    private static ConfigSingleton instance;

    //Variables:
    private NetworkConfig networkConfig = new NetworkConfig();

    //Instance getter
    public static ConfigSingleton getInstance()
    {
        if (instance == null)
            instance = new ConfigSingleton();
        return instance;
    }

    public NetworkConfig getNetworkConfig()
    {
        return this.networkConfig;
    }

    public void setNetworkConfig(NetworkConfig networkConfig)
    {
        this.networkConfig = networkConfig;
    }
}
