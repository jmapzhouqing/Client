using HslCommunication;
using HslCommunication.Profinet.Siemens;
using System;

namespace Unattended.PLC
{
    /// <summary>
    /// 西门子PLC工具类
    /// </summary>
    public class SiemensPLCTool : PLCBusiness
    {
        private static readonly Lazy<SiemensPLCTool> lazy = new Lazy<SiemensPLCTool>(() => new SiemensPLCTool());

        /// <summary>
        /// 西门子PLC工具类单例
        /// </summary>
        public static SiemensPLCTool SiemensPLCToolInstance { get { return lazy.Value; } }

        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnect = false;

        private SiemensS7Net siemensTcpNet;

        private SiemensPLCTool()
        {

        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="PLCType">PLC类型</param>
        /// <param name="ip">ip地址</param>
        /// <returns></returns>
        public bool Init(string PLCType, string ip)
        {
            var ret = false;
            try
            {
                if (IsAuthorization())
                {
                    siemensTcpNet = new SiemensS7Net(PLCTypeTransition(PLCType), ip)
                    {
                        ConnectTimeOut = 5000
                    };
                    IsConnect = ret = Connect();
                }
            }
            catch (Exception)
            {
                //日志
            }
            return ret;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        private bool Connect()
        {
            var ret = false;
            try
            {
                OperateResult connect = siemensTcpNet.ConnectServer();
                ret = connect.IsSuccess;
            }
            catch (Exception)
            {
                //日志
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
                siemensTcpNet.ConnectClose();
                ret = true;
                IsConnect = false;
            }
            catch (Exception)
            {
                //日志
            }
            return ret;
        }

        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="adress">地址</param>
        /// <returns></returns>
        public Object Read(string dataType, string adress)
        {
            Object ret = null;
            try
            {
                if (IsConnect)
                {
                    switch (dataType)
                    {
                        case "Int":
                            break;
                        case "Bool":
                            break;
                        case "Float":
                            ret = siemensTcpNet.ReadFloat(adress).Content;
                            break;
                    }
                }
            }
            catch (Exception)
            {
                //日志
            }
            return ret;
        }

        /// <summary>
        /// 西门子PLC类型转换
        /// </summary>
        /// <param name="PLCType"></param>
        /// <returns></returns>
        private SiemensPLCS PLCTypeTransition(string PLCType)
        {
            SiemensPLCS ret = 0;
            try
            {
                switch (PLCType)
                {
                    case "S200":
                        ret = SiemensPLCS.S200;
                        break;
                    case "S200Smart":
                        ret = SiemensPLCS.S200Smart;
                        break;
                    case "S300":
                        ret = SiemensPLCS.S300;
                        break;
                    case "S400":
                        ret = SiemensPLCS.S400;
                        break;
                    case "S1200":
                        ret = SiemensPLCS.S1200;
                        break;
                    case "S1500":
                        ret = SiemensPLCS.S1500;
                        break;
                }
            }
            catch (Exception)
            {
                //日志
            }
            return ret;
        }
    }
}
