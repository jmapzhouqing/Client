using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


public class LogTest : MonoBehaviour
{
    private static ILogger logger = Debug.unityLogger;
    private static string kTAG = "MyGameTag";
    private LogHandler myFileLogHandler;

    void Start()
    {
        myFileLogHandler = new LogHandler();

        logger.Log(kTAG, "MyGameClass Start.");

    }
}
