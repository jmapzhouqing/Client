using System;

public enum ExceptionCode{
    None = 0,
    InternalError,
    Unauthorized,
    NetworkUnreachable,
    DeviceDisconnect,
    DeviceConnect,
    DeviceConnectionRefused
}

[Serializable]
public class ExceptionHandler:Exception{
    private ExceptionCode code;
    private string message;

    private ExceptionHandler() {
        this.code = ExceptionCode.None;
    }
    public ExceptionHandler(string message, ExceptionCode code):base(message){
        this.code = code;
    }

    public ExceptionHandler(string message,Exception exception, ExceptionCode code):base(message,exception) {
        this.code = code;
    }

    public ExceptionCode GetExceptionCode(){
        return this.code;
    }
}
