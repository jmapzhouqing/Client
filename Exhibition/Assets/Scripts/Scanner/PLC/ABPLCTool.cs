using HslCommunication;
using HslCommunication.Profinet.AllenBradley;
using System;

namespace Unattended.PLC
{
    /// <summary>
    /// ABPLC工具类
    /// </summary>
    public class ABPLCTool : PLCBusiness
    {
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnect = false;

        private AllenBradleyNet allenBradleyNet;

        public ABPLCTool()
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        public bool Init(string ip)
        {
            var ret = false;
            try
            {
                allenBradleyNet = new AllenBradleyNet(ip);
                allenBradleyNet.ConnectTimeOut = 5000;
                OperateResult connect = allenBradleyNet.ConnectServer();
                ret = IsConnect = connect.IsSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ret;
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <returns></returns>
        public bool Close()
        {
            var ret = false;
            try
            {
                if (IsConnect)
                {
                    allenBradleyNet.ConnectClose();
                }
            }
            catch (Exception)
            {
                //
            }
            return ret;
        }

        /// <summary>
        /// 写入值
        /// </summary>
        /// <param name="adress">地址</param>
        /// <param name="val">值</param>
        /// <returns></returns>
        public bool Write(string adress, string val)
        {
            var ret = false;
            try
            {

            }
            catch (Exception)
            {

            }
            return ret;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="dataType">数据类型(1:Byte;2:Bool;3:Int;4:Float)</param>
        /// <param name="adress">地址</param>
        /// <returns></returns>
        public string ReadData(int dataType, string adress){
            var ret = "";
            try
            {
                if (IsConnect)
                {
                    switch (dataType)
                    {
                        case 1:
                            ret = allenBradleyNet.ReadByte(adress).Content.ToString();
                            break;
                        case 2:
                            ret = allenBradleyNet.ReadBool(adress).Content == true ? "1" : "0";
                            break;
                        case 3:
                            ret = allenBradleyNet.ReadInt32(adress).Content.ToString();
                            break;
                        case 4:
                            ret = allenBradleyNet.ReadFloat(adress).Content.ToString();
                            break;
                    }
                }
            }
            catch (Exception)
            {
                ret = "";
                //日志
            }
            return ret;
        }

        /// <summary>
        /// 模拟读取
        /// </summary>
        /// <returns></returns>
        public string SimulationReadData()
        {
            var ret = "";
            try
            {

            }
            catch(Exception ex)
            {

            }
            return ret;
        }

        /// <summary>
        /// 批量读取
        /// </summary>
        /// <returns></returns>
        public byte[] BatchRead(string[] adress)
        {
            byte[] obj = null;
            try
            {
                var result = allenBradleyNet.BuildReadCommand(adress);
                if (result.IsSuccess)
                {
                    var data = result.Content;
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                //日志
            }
            return obj;
        }
    }
}
