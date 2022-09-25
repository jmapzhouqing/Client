using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LogManager : MonoBehaviour
{
    private const int LOG_FILE_COUNT = 10;

    public bool EnableLog = true;

    private string LogFileDir = "";
    private string LogFileName = "";
    private StreamWriter LogFileWriter = null;

    private void Awake()
    {
        Application.logMessageReceivedThreaded += OnLogByUnity;
        LogFileDir = Path.Combine(Application.persistentDataPath,"Log");
    }

    private void OnDestroy()
    {
        Application.logMessageReceivedThreaded -= OnLogByUnity;
        this.CloseLog();
    }

    public void CloseLog()
    {
        if (LogFileWriter != null)
        {
            try
            {
                LogFileWriter.Flush();
                LogFileWriter.Close();
                LogFileWriter.Dispose();
                LogFileWriter = null;
            }
            catch (Exception e){
                throw e;
            }
        }
    }

    public void CheckClearLog()
    {
        if (!Directory.Exists(LogFileDir)){
            return;
        }

        DirectoryInfo direction = new DirectoryInfo(LogFileDir);
        var files = direction.GetFiles("*");
        if (files.Length >= LOG_FILE_COUNT)
        {
            var oldfile = files[0];
            var lastestTime = files[0].CreationTime;
            foreach (var file in files)
            {
                if (lastestTime > file.CreationTime)
                {
                    oldfile = file;
                    lastestTime = file.CreationTime;
                }

            }
            oldfile.Delete();
        }
    }

    private string GetLogTime()
    {
        string str = "";

        str = DateTime.Now.ToString("HH:mm:ss.fff") + " ";

        return str;
    }

    private void OnLogByUnity(string condition, string stackTrace, LogType type)
    {
        // 过滤自己的输出
        if (type == LogType.Log){
            return;
        }
        var str = "["+ Enum.GetName(typeof(LogType), type)+ "]" + GetLogTime() + condition + "\n" + stackTrace;
        LogToFile(str);
    }

    private void LogToFile(string message, bool EnableStack = false){

        if (LogFileWriter == null){
            CheckClearLog();
            LogFileName = DateTime.Now.GetDateTimeFormats('s')[0].ToString();
            LogFileName = LogFileName.Replace("-", "_");
            LogFileName = LogFileName.Replace(":", "_");
            LogFileName = LogFileName.Replace(" ", "");
            LogFileName = LogFileName.Replace("T", "_");
            LogFileName = LogFileName + ".log";
            if (string.IsNullOrEmpty(LogFileDir)){
                try{
                    if (!Directory.Exists(LogFileDir)){
                        Directory.CreateDirectory(LogFileDir);
                    }
                }
                catch (Exception exception){
                    Debug.Log("获取 Application.streamingAssetsPath 报错！" + exception.Message, null);
                    return;
                }
            }
            string path = LogFileDir + "/" + LogFileName;

            try
            {
                if (!Directory.Exists(LogFileDir))
                {
                    Directory.CreateDirectory(LogFileDir);
                }
                LogFileWriter = File.AppendText(path);
                LogFileWriter.AutoFlush = true;
            }
            catch (Exception exception2)
            {
                LogFileWriter = null;
                Debug.Log("LogToCache() " + exception2.Message + exception2.StackTrace, null);
                return;
            }
        }
        if (LogFileWriter != null)
        {
            try
            {
                LogFileWriter.WriteLine(message);
                if (EnableStack){
                    //把无关的log去掉
                    var st = StackTraceUtility.ExtractStackTrace();
#if UNITY_EDITOR
                    for (int i = 0; i < 3; i++)
#else
                        for (int i = 0; i < 2; i++)
#endif
                    {
                        st = st.Remove(0, st.IndexOf('\n') + 1);
                    }
                    LogFileWriter.WriteLine(st);
                }
            }
            catch (Exception)
            {
            }
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 100, 60), "Click")) {
            float name = 1.0f / 0.0f;
            Exception e = new Exception("I am error");
            throw e;
        }
    }
}
