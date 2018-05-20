using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SharedTypes;

public class ConfigSingleton {
    //Singleton instance
    private static ConfigSingleton _instance;
    private List<TestCase> _testCases;

    public List<TestCase> TestCases
    {
        get { return _testCases; }
    }

    //Variables:
    private MyNetworkConfig _networkConfig;
    public string DBDomain { get; set; }
    
    //Instance getter
    public static ConfigSingleton GetInstance()
    {
        return _instance ?? (_instance = new ConfigSingleton());
    }

    private ConfigSingleton()
    {
        _testCases = new List<TestCase>
        {
            new TestCase(20, ColorMode.StaticBlue, DisplayMode.ConstantUnitSize, 75, 125, DistanceMode.Random, 0)
        };
    }

    public MyNetworkConfig GetMyNetworkConfig()
    {
        return this._networkConfig;
    }

    public void SetMyNetworkConfig(MyNetworkConfig networkConfig)
    {
        _networkConfig = networkConfig;
    }
}
