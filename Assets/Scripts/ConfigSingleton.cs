using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTypes;

public class ConfigSingleton {
    //Singleton instance
    private static ConfigSingleton _instance;

    //Variables:
    private MyNetworkConfig _networkConfig = new MyNetworkConfig();

    //Instance getter
    public static ConfigSingleton GetInstance()
    {
        return _instance ?? (_instance = new ConfigSingleton());
    }

    public MyNetworkConfig GetNetworkConfig()
    {
        return this._networkConfig;
    }

    public void SetNetworkConfig(MyNetworkConfig networkConfig)
    {
        _networkConfig = networkConfig;
    }
}
