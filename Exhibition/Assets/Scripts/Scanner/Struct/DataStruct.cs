using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Scanner.Struct
{

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BroadcastDeviceInfo {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] broadcast_code;
        public byte dev_type; 
        public UInt16 reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
        public byte[] ip;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LMDScandata {
        public UInt16 version_number;
        public UInt16 device_number;
        public UInt32 serial_number;
        public UInt16 device_status;
        public UInt16 telegram_counter;
        public UInt16 scan_counter;
        public UInt32 time_since_start;
        public UInt32 time_transmision;
        public UInt16 status_digital_inputs;
        public UInt16 status_digital_outputs;
        public UInt16 layer_angle;
        public UInt32 scan_frequency;
        public UInt32 measurement_frequency;
        public UInt16 amount_encoder;
        public UInt32 encoder_position;
        public UInt16 encoder_speed;
        public UInt16 amount_channels;
        public string content;
        public UInt32 scale_factor;
        public UInt32 scale_factor_offset;
        public float start_angle;
        public float angular_step;
        public UInt16 amount_data;
    }

    public struct ScannerSector {
        public float rotation;
        public List<RayInfo> rays;
    }

    public struct RayInfo {
        public float degree;
        public float distance;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HolderData {
        public UInt16 control;
        public UInt16 data;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WitData {
        public byte month;
        public byte year;
        public byte hour;
        public byte day;
        public byte second;
        public byte minute;
        public UInt16 msecond;

        public Int16 ax;
        public Int16 ay;
        public Int16 az;

        public Int16 wx;
        public Int16 wy;
        public Int16 wz;

        public Int16 hx;
        public Int16 hy;
        public Int16 hz;

        public UInt16 rollL;
        public UInt16 rollH;
        public UInt16 pitchL;
        public UInt16 pitchH;
        public UInt16 yawL;
        public UInt16 yawH;

        public UInt16 tempture;

        /*
        public Int16 Q0;
        public Int16 Q1;
        public Int16 Q2;
        public Int16 Q3;*/
    }
}
