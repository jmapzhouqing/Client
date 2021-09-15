using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class LogCallBack : MonoBehaviour
{

    private StreamWriter writer;
    // Start is called before the first frame update
    void Start(){
        string path = Application.persistentDataPath;
        if (!Directory.Exists(path)){
            Directory.CreateDirectory(path);
        }

        path = Path.Combine(path ,"log.txt");

        if (File.Exists(path)){
            File.Delete(path);
        }

        FileInfo file = new FileInfo(path);
        writer = file.CreateText();

        Application.logMessageReceived += LogCallback;
    }

    void LogCallback(string condition, string stackTrace, LogType type){
        string content = "";
        content += System.DateTime.Now + ":" + type.ToString() + ": " + "\r\n" +
         "condition" + ": " + condition + "\r\n" +
         "stackTrace" + ": " + stackTrace + "\r\n";

        writer.Write(content);
    }

    void OnDestroy()
    {
        if (writer != null){
            writer.Close();
            writer.Dispose();
        }
        Application.logMessageReceived -= LogCallback;
    }
}
