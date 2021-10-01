using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

using HslCommunication;
using HslCommunication.Profinet.AllenBradley;


public class DataMonitor<T> where T:class,new()
{
    public List<string> points;

    private AllenBradleyNet allenBradleyNet;

    private Task task;
    private CancellationTokenSource cancel_token_source;
    private CancellationToken cancel_token;

    private bool connect_station;

    public delegate void DataUpdate(T data);
    public event DataUpdate OnDataUpdate;

    public delegate void ErrorCallback(Exception e);
    public event ErrorCallback OnError;

    private T data;

    public DataMonitor(string ip){
        try{
            if (!HslCommunication.Authorization.SetAuthorizationCode("a4908a2c-4fb6-4a14-a418-d183c5a6e448")){
                allenBradleyNet = new AllenBradleyNet(ip);
                allenBradleyNet.ConnectTimeOut = 1000;
                allenBradleyNet.SetPersistentConnection();
            }else{
                this.OnError(new ExceptionHandler("HslCommunication 授权未成功",ExceptionCode.Unauthorized));
            }
        }catch(Exception e){

        }
    }

    public void Connect(){
        Task task = new Task(() =>{
            if (allenBradleyNet != null){
                OperateResult connect = allenBradleyNet.ConnectServer();
                connect_station = connect.IsSuccess;
                if (connect_station){
                    this.StartReadData(100);
                }else {
                    this.OnError(new ExceptionHandler("PLC断连",ExceptionCode.DeviceDisconnect));
                }
            }
        });
        task.Start();
    }

    public void DisConnect(){
        this.StopReadData();
        if (allenBradleyNet!=null) {
            allenBradleyNet.ConnectClose();
        }
    }

    public void StartReadData(int delay) {
        cancel_token_source = new CancellationTokenSource();
        cancel_token = cancel_token_source.Token;

        task = new Task(async ()=>{
            while (true) {
                if (cancel_token.IsCancellationRequested) {
                    return;
                }
                this.ReadData();
                await Task.Delay(delay);
            }
        });
        task.Start();
    }

    public void StopReadData(){
        if (cancel_token_source != null){
            cancel_token_source.Cancel();
        }
    }

    private void ReadData(){
        OperateResult<T> operate = allenBradleyNet.Read<T>();
        if(operate.IsSuccess){
            data = operate.Content;
            if (this.OnDataUpdate != null){
                this.OnDataUpdate(data);
            }
        }else{
            this.OnError(new ExceptionHandler("PLC断连", ExceptionCode.DeviceDisconnect));
        }
    }
}
