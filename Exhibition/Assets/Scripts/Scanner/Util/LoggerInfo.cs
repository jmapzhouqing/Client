using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class LogHandler : ILogHandler
{
    private FileStream stream;
    private StreamWriter writer;
    private ILogHandler UnityLogHandler = Debug.unityLogger.logHandler;

    public LogHandler(){
        string filePath = Application.persistentDataPath + "/Log1.txt";
        stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        writer = new StreamWriter(stream);
    }

    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args){
        StringBuilder builder = new StringBuilder();

        builder.Append(DateTime.Now).Append("-").Append(logType).Append(":").Append(context.name).Append(":").Append(String.Format(format, args));

        writer.WriteLine(builder);
        writer.Flush();

        UnityLogHandler.LogFormat(logType, context, format, args);
    }

    public void LogException(Exception exception, UnityEngine.Object context){
        UnityLogHandler.LogException(exception, context);
    }

    ~LogHandler(){
        this.Close();
    }

    public void Close() {
        if (writer != null){
            writer.Close();
            writer.Dispose();
        }
    }
}

public class LoggerInfo:MonoBehaviour
{
    private static Logger logger;
    private LogHandler handler;

    void Awake(){
        handler = new LogHandler();
        logger = new Logger(handler);
    }

    public static void Log(Type type,string message,Transform content) {
        logger.Log(LogType.Log, type.Name, message, content);
    }

    private void OnDestroy(){
        if (handler != null) {
            handler.Close();
        }
    }
}
