using System;
using System.IO;
using System.IO.Ports;


using System.Threading;
using System.Threading.Tasks;

using Scanner.Struct;

using UnityEngine;

namespace Scanner.Serial
{
    class SerialCorrespond{
        private double receive_ticks;

        private DeviceStatus transfer_status;

        private SerialPort port;

        private System.Timers.Timer heart_timer;

        private System.Timers.Timer ticks_timer;

        private byte[] heartbeat_data;

        private bool isOpen = false;

        byte[] recv_buffer = new byte[1024];

        public delegate void DataReceiveHandle(byte[] buff, int offset, int length);
        public delegate void StatusChangedHandle(DeviceStatus status);
        public delegate void ErrorHandle(ExceptionHandler exception);

        public event DataReceiveHandle DataReceived;
        public event StatusChangedHandle StatusChanged;
        public event ErrorHandle Error;

        private Task read_task;

        private CancellationTokenSource cancel_source;
        CancellationToken cancel_token;

        protected DeviceStatus StatusMonitor{
            get { return this.transfer_status; }
            set{
                if (!this.transfer_status.Equals(value)){
                    this.transfer_status = value;
                    this.OnStatusChanged(transfer_status);
                }
            }
        }
        protected double LastReceiveTicks{
            get{
                return this.receive_ticks;
            }

            set{
                if (this.transfer_status.Equals(DeviceStatus.NotConnect)){
                    this.StatusMonitor = DeviceStatus.Connect;
                }else{
                    if (!this.transfer_status.Equals(DeviceStatus.Working)){
                        this.StatusMonitor = DeviceStatus.OnLine;
                    }
                }

                this.receive_ticks = value;

                this.StartMonitorTicks(1000);
            }
        }

        public SerialCorrespond(String name, Int32 baud_rate, Parity parity = Parity.None, Int32 data_bits = 8, StopBits stop_bits = StopBits.One)
        {
            try{
                port = new SerialPort(name, baud_rate, parity, data_bits, stop_bits);
                port.ReadTimeout = 5000;
                port.ReadBufferSize = 102400;
                port.ReadBufferSize = 102400;
                //port.ErrorReceived += SerialPort_ErrorReceived;
                //port.DataReceived += SerialPort_DataReceived;
            }catch (IOException e){
                this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
            }
        }

        protected void StartHeart(int interval)
        {
            heart_timer = new System.Timers.Timer(interval);
            heart_timer.AutoReset = true;
            heart_timer.Enabled = true;
            heart_timer.Elapsed += new System.Timers.ElapsedEventHandler(HeartTimerUp);
        }

        public virtual void StopHeart()
        {
            if (heart_timer != null){
                heart_timer.Stop();
                heart_timer.Close();
            }
        }

        private void HeartTimerUp(object sender, System.Timers.ElapsedEventArgs e)
        {
            System.Timers.Timer timer = sender as System.Timers.Timer;
            try
            {
                double ticks = DateTime.Now.Ticks * Math.Pow(10, -4);

                if (Math.Abs(ticks - receive_ticks) > timer.Interval)
                {
                    this.SendData(heartbeat_data);
                }
            }
            catch (Exception exception)
            {
                this.OnError(new ExceptionHandler(exception.Message, ExceptionCode.InternalError));
            }
        }
        private void StartMonitorTicks(int interval){
            if (ticks_timer != null){
                ticks_timer.Stop();
                ticks_timer.Close();
            }
            ticks_timer = new System.Timers.Timer(interval);
            ticks_timer.AutoReset = false;
            ticks_timer.Enabled = true;
            ticks_timer.Elapsed += new System.Timers.ElapsedEventHandler(TicksTimerUp);
        }

        private void TicksTimerUp(object sender, System.Timers.ElapsedEventArgs e){
            if (!this.transfer_status.Equals(DeviceStatus.NotConnect) && !this.transfer_status.Equals(DeviceStatus.DisConnect)){
                this.StatusMonitor = DeviceStatus.OffLine;
            }
        }

        public void StartReceiveData(int delay) {
            cancel_source = new CancellationTokenSource();
            cancel_token = cancel_source.Token;

            read_task = new Task(() => {
                ReadDataAsync(delay);
            });
            read_task.Start();
        }

        public void StopReceiveData() {
            if (cancel_source != null) {
                cancel_source.Cancel();
            }
        }

        public void Open(){
            try{
                if (port != null){
                    port.Open();
                    if (port.IsOpen){
                        //this.StartHeart(500);
                        this.UpdateReceiveTicks();
                    }
                }
            }catch (Exception e){
                this.OnError(new ExceptionHandler(e.Message, ExceptionCode.InternalError));
            }
        }

        public void Close() {
            this.StopReceiveData();
            if (port != null && port.IsOpen) {
                port.Close();
                port.Dispose();
            }
        }

        public void SendData(byte[] data){
            if(port.IsOpen) {
                port.Write(data, 0, data.Length);
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e){
            Console.WriteLine("Error");
        }
        private async void ReadDataAsync(int delay){
            while (true){
                if (cancel_token.IsCancellationRequested){
                    break;
                }
                if (port.IsOpen){
                    int length = 0;
                    if(port.BytesToRead != 0){
                        this.UpdateReceiveTicks();
                        length = port.Read(recv_buffer, 0, recv_buffer.Length);
                        this.OnDataReceived(recv_buffer,0,length);
                    }
                }
                await Task.Delay(delay);
            }
        }
        public void UpdateReceiveTicks(){
            this.LastReceiveTicks = DateTime.Now.Ticks * Math.Pow(10, -4);
        }
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e){
            int length = 0;
            try{
                while(port.BytesToRead!=0){
                    length = port.Read(recv_buffer, 0, recv_buffer.Length);
                    this.OnDataReceived(recv_buffer,0,length);
                }
            }
            catch (Exception error) {
                this.OnError(new ExceptionHandler(error.Message,ExceptionCode.InternalError));
            }

            /*
            byte[] byteTemp = new byte[1000];

            if (bClosing) return;
            try 
            {
                bListening = true;
                UInt16 usLength = 0;
                try
                {
                    usLength = (UInt16)spSerialPort.Read(RxBuffer, usRxLength, 700);
                }
                catch (Exception err)
                {
                    //MessageBox.Show(err.Message);
                    //return;
                }
                usRxLength += usLength;
                while (usRxLength >= 11)
                {
                    UpdateData Update = new UpdateData(DecodeData);
                    RxBuffer.CopyTo(byteTemp, 0);
                    if (!((byteTemp[0] == 0x55) & ((byteTemp[1] & 0x50) == 0x50)))
                    {
                        for (int i = 1; i < usRxLength; i++) RxBuffer[i - 1] = RxBuffer[i];
                        usRxLength--;
                        continue;
                    }
                    if (((byteTemp[0] + byteTemp[1] + byteTemp[2] + byteTemp[3] + byteTemp[4] + byteTemp[5] + byteTemp[6] + byteTemp[7] + byteTemp[8] + byteTemp[9]) & 0xff) == byteTemp[10])
                        this.Invoke(Update, byteTemp);
                    for (int i = 11; i < usRxLength; i++) RxBuffer[i - 11] = RxBuffer[i];
                    usRxLength -= 11;
                }

                Thread.Sleep(10);
            }
            finally
            {
                bListening = false;//我用完了，ui可以关闭串口了。   
            }*/
        }

        protected void OnDataReceived(byte[] buffer, int offset, int length)
        {
            if (this.DataReceived != null)
            {
                this.DataReceived(buffer, offset, length);
            }
        }

        protected void OnStatusChanged(DeviceStatus status){
            if (this.StatusChanged != null){
                this.StatusChanged(status);
            }
        }

        protected void OnError(ExceptionHandler exception){
            if (this.Error != null){
                this.Error(exception);
            }
        }
    }

   
}