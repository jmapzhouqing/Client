using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Scanner.Livox
{
    enum DeviceType{
        DeviceHub = 0,              //Livox Hub
        DeviceLidarMid40 = 1,      //Mid-40
        DeviceLidarTele = 2,      //Tele
        DeviceLidarHorizon = 3,  //Horizon
        DeviceLidarMid70 = 6,   //Livox Mid-70
        DeviceLidarAvia = 7    //Avia
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SdkPreamble
    {
        public byte sof;
        public byte version;
        public UInt16 length;
        public byte packet_type;
        public UInt16 seq_num;
        public UInt16 preamble_crc;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SdkPacket
    {
        public byte sof;
        public byte version;
        public UInt16 length;
        public byte packet_type;
        public UInt16 seq_num;
        public UInt16 preamble_crc;
        public byte cmd_set;
        public byte cmd_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]
        public byte[] data;
        public UInt32 crc;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BroadcastDeviceInfo
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string broadcast_code;
        byte dev_type;
        UInt16 reserved;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct HandshakeRequest
    {
        public UInt32 ip_addr;
        public UInt16 data_port;
        public UInt16 cmd_port;
        public UInt16 sensor_port;
    }

    struct DeviceInfo
    {
        public string broadcast_code;
        public byte dev_type;
        public IPEndPoint remote_address;
        public IPEndPoint command_address;
        public IPEndPoint data_address;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct DeviceInformationResponse{
        public byte ret_code;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] firmware_version;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxEthPacket{
        public byte version;              /**< Packet protocol version. */
        public byte slot;                 /**< Slot number used for connecting LiDAR. */
        public byte id;                   /**< LiDAR id. */
        public byte rsvd;                 /**< Reserved. */
        public UInt32 err_code;           /**< Device error status indicator information. */
        public byte timestamp_type;       /**< Timestamp type. */
        public byte data_type;            /** Point cloud coordinate format, refer to \ref PointDataType.*/
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] timestamp;         /**< Nanosecond or UTC format timestamp. */
    }

    enum PointDataType{
        Cartesian,               /**< Cartesian coordinate point cloud. */
        Spherical,               /**< Spherical coordinate point cloud. */
        ExtendCartesian,         /**< Extend cartesian coordinate point cloud. */
        ExtendSpherical,         /**< Extend spherical coordinate point cloud. */
        DualExtendCartesian,     /**< Dual extend cartesian coordinate  point cloud. */
        DualExtendSpherical,     /**< Dual extend spherical coordinate point cloud. */
        Imu,                     /**< IMU data. */
        TripleExtendCartesian,   /**< Triple extend cartesian coordinate  point cloud. */
        TripleExtendSpherical,   /**< Triple extend spherical coordinate  point cloud. */
        MaxPointDataType         /**< Max Point Data Type.*/
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxSdkVersion
    {
        public int major;      /**< major number */
        public int minor;      /**< minor number */
        public int patch;      /**< patch number */
    }

    /** Cartesian coordinate format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxRawPoint
    {
        public Int32 x;            /**< X axis, Unit:mm */
        public Int32 y;            /**< Y axis, Unit:mm */
        public Int32 z;            /**< Z axis, Unit:mm */
        public Int32 reflectivity; /**< Reflectivity */
    }

    /** Spherical coordinate format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxSpherPoint
    {
        public UInt32 depth;       /**< Depth, Unit: mm */
        public UInt16 theta;       /**< Zenith angle[0, 18000], Unit: 0.01 degree */
        public UInt16 phi;         /**< Azimuth[0, 36000], Unit: 0.01 degree */
        public UInt16 reflectivity; /**< Reflectivity */
    }

    /** Standard point cloud format */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxPoint
    {
        public float x;              /**< X axis, Unit:m */
        public float y;              /**< Y axis, Unit:m */
        public float z;              /**< Z axis, Unit:m */
        public byte reflectivity; /**< Reflectivity */
    }

    /** Extend cartesian coordinate format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxExtendRawPoint
    {
        public Int32 x;            /**< X axis, Unit:mm */
        public Int32 y;            /**< Y axis, Unit:mm */
        public Int32 z;            /**< Z axis, Unit:mm */
        public byte reflectivity; /**< Reflectivity */
        public byte tag;          /**< Tag */
    }

    /** Extend spherical coordinate format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxExtendSpherPoint
    {
        public UInt32 depth;       /**< Depth, Unit: mm */
        public UInt16 theta;       /**< Zenith angle[0, 18000], Unit: 0.01 degree */
        public UInt16 phi;         /**< Azimuth[0, 36000], Unit: 0.01 degree */
        public byte reflectivity; /**< Reflectivity */
        public byte tag;          /**< Tag */
    }

    /** Dual extend cartesian coordinate format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxDualExtendRawPoint{
        public Int32 x1;            /**< X axis, Unit:mm */
        public Int32 y1;            /**< Y axis, Unit:mm */
        public Int32 z1;            /**< Z axis, Unit:mm */
        public byte reflectivity1; /**< Reflectivity */
        public byte tag1;          /**< Tag */
        public Int32 x2;            /**< X axis, Unit:mm */
        public Int32 y2;            /**< Y axis, Unit:mm */
        public Int32 z2;            /**< Z axis, Unit:mm */
        public byte reflectivity2; /**< Reflectivity */
        public byte tag2;          /**< Tag */
    }

    /** Dual extend spherical coordinate format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxDualExtendSpherPoint{
        public UInt16 theta;        /**< Zenith angle[0, 18000], Unit: 0.01 degree */
        public UInt16 phi;          /**< Azimuth[0, 36000], Unit: 0.01 degree */
        public UInt32 depth1;       /**< Depth, Unit: mm */
        public byte reflectivity1; /**< Reflectivity */
        public byte tag1;          /**< Tag */
        public UInt32 depth2;       /**< Depth, Unit: mm */
        public byte reflectivity2; /**< Reflectivity */
        public byte tag2;          /**< Tag */
    }

    /** Triple extend cartesian coordinate format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxTripleExtendRawPoint{
        public Int32 x1;            /**< X axis, Unit:mm */
        public Int32 y1;            /**< Y axis, Unit:mm */
        public Int32 z1;            /**< Z axis, Unit:mm */
        public byte reflectivity1; /**< Reflectivity */
        public byte tag1;          /**< Tag */
        public Int32 x2;            /**< X axis, Unit:mm */
        public Int32 y2;            /**< Y axis, Unit:mm */
        public Int32 z2;            /**< Z axis, Unit:mm */
        public byte reflectivity2; /**< Reflectivity */
        public byte tag2;          /**< Tag */
        public Int32 x3;            /**< X axis, Unit:mm */
        public Int32 y3;            /**< Y axis, Unit:mm */
        public Int32 z3;            /**< Z axis, Unit:mm */
        public byte reflectivity3; /**< Reflectivity */
        public byte tag3;          /**< Tag */
    }

    /** Triple extend spherical coordinate format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxTripleExtendSpherPoint{
        public UInt16 theta;        /**< Zenith angle[0, 18000], Unit: 0.01 degree */
        public UInt16 phi;          /**< Azimuth[0, 36000], Unit: 0.01 degree */
        public UInt32 depth1;       /**< Depth, Unit: mm */
        public byte reflectivity1; /**< Reflectivity */
        public byte tag1;          /**< Tag */
        public UInt32 depth2;       /**< Depth, Unit: mm */
        public byte reflectivity2; /**< Reflectivity */
        public byte tag2;          /**< Tag */
        public UInt32 depth3;       /**< Depth, Unit: mm */
        public byte reflectivity3; /**< Reflectivity */
        public byte tag3;          /**< Tag */
    }

    /** IMU data format. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LivoxImuPoint{
        public float gyro_x;        /**< Gyroscope X axis, Unit:rad/s */
        public float gyro_y;        /**< Gyroscope Y axis, Unit:rad/s */
        public float gyro_z;        /**< Gyroscope Z axis, Unit:rad/s */
        public float acc_x;         /**< Accelerometer X axis, Unit:g */
        public float acc_y;         /**< Accelerometer Y axis, Unit:g */
        public float acc_z;         /**< Accelerometer Z axis, Unit:g */
    }

    /** LiDAR error code. */
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct LidarErrorCode{
        public UInt32 temp_status;      /**< 0: Temperature in Normal State. 1: High or Low. 2: Extremely High or Extremely Low. */
        public UInt32 volt_status;      /**< 0: Voltage in Normal State. 1: High. 2: Extremely High. */
        public UInt32 motor_status;     /**< 0: Motor in Normal State. 1: Motor in Warning State. 2:Motor in Error State, Unable to Work. */
        public UInt32 dirty_warn;       /**< 0: Not Dirty or Blocked. 1: Dirty or Blocked. */
        public UInt32 firmware_err;     /**< 0: Firmware is OK. 1: Firmware is Abnormal, Need to be Upgraded. */
        public UInt32 pps_status;       /**< 0: No PPS Signal. 1: PPS Signal is OK. */
        public UInt32 device_status;    /**< 0: Normal. 1: Warning for Approaching the End of Service Life. */
        public UInt32 fan_status;       /**< 0: Fan in Normal State. 1: Fan in Warning State. */
        public UInt32 self_heating;     /**< 0: Normal. 1: Low Temperature Self Heating On. */
        public UInt32 ptp_status;       /**< 0: No 1588 Signal. 1: 1588 Signal is OK. */
        /** 0: System dose not start time synchronization.
         * 1: Using PTP 1588 synchronization.
         * 2: Using GPS synchronization.
         * 3: Using PPS synchronization.
         * 4: System time synchronization is abnormal.(The highest priority synchronization signal is abnormal)
        */
        public UInt32 time_sync_status;
        public UInt32 rsvd;            /**< Reserved. */
        public UInt32 system_status;    /**< 0: Normal. 1: Warning. 2: Error. */
    }
}
