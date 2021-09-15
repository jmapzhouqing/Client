using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Scanner.Util
{
    class DataConvert
    {
        public static T ConvertByteStruct<T>(byte[] data, int offset = 0,bool reversal = false) where T:struct{
            T value = default(T);

            int size = Marshal.SizeOf(typeof(T));

            if ((data.Length - offset) < size){
                return value;
            }

            IntPtr intPtr = Marshal.AllocHGlobal(size);

            if (reversal) {
                Type type = typeof(T);

                object obj = type.Assembly.CreateInstance(type.FullName);

                int reversestartoffset = offset;

                foreach (var field in type.GetFields()){
                    object fieldValue = field.GetValue(obj);

                    TypeCode typeCode = Type.GetTypeCode(field.FieldType);
                    if (fieldValue != null){
                        Array.Reverse(data, reversestartoffset, Marshal.SizeOf(fieldValue));
                        reversestartoffset += Marshal.SizeOf(fieldValue);
                    }else{
                        MarshalAsAttribute attribute = field.GetCustomAttribute<MarshalAsAttribute>();
                        if (attribute != null)
                        {
                            reversestartoffset += attribute.SizeConst;
                        }
                    }
                }
            }

            Marshal.Copy(data, offset, intPtr, size);

            value = Marshal.PtrToStructure<T>(intPtr);

            Marshal.FreeHGlobal(intPtr);

            return value;
        }

        public static int GetLength<T>() {
            return Marshal.SizeOf<T>();
        }
        public static byte[] ConvertStructByte<T>(Object data) where T:struct
        {
            int length = Marshal.SizeOf(typeof(T));

            byte[] result = new byte[length];

            IntPtr intPtr = Marshal.AllocHGlobal(length);

            Marshal.StructureToPtr(data,intPtr,false);

            Marshal.Copy(intPtr, result, 0, length);

            Marshal.FreeHGlobal(intPtr);

            return result;
        }

        public static void AddStringToBuffer(List<byte> list, string data){
            byte[] value = Encoding.UTF8.GetBytes(data);
            list.AddRange(value);
        }

        public static int AddNumberToBuffer<T>(List<byte> list, T value) where T : struct
        {
            int width = Marshal.SizeOf(value);

            ulong data;

            if (ulong.TryParse(value.ToString(),out data)){
                for (int i = width - 1; i >= 0; i--){
                    list.Add((byte)(data >> (8 * i) & 0xFF));
                }
            }

            return width;
        }

        public static int InsertNumberToBuffer<T>(List<byte> list, int offset,T value) where T : struct
        {
            int width = Marshal.SizeOf(value);

            ulong data;

            if (ulong.TryParse(value.ToString(), out data))
            {
                for (int i = width - 1; i >= 0; i--)
                {
                    list.Insert(offset++,(byte)(data >> (8 * i) & 0xFF));
                }
            }

            return width;
        }

        public static T GetNumberFromBuffer<T>(byte[] data, ref int pos) where T:struct{
            T result = default(T);

            int width = Marshal.SizeOf(result);

            ulong value = 0;

            for (int i = 0; i < width; i++){
                value += ((ulong)data[pos + width - 1 - i]) << (8 * i);
            }

            result = (T)System.Convert.ChangeType(value, typeof(T));

            pos += width;

            return result;
        }

        public static string GetStringFromBuffer(byte[] data,ref int position, int length) {
            string value = Encoding.UTF8.GetString(data, position, length);
            position += length;
            return value;
        }

        public static string GetStringFromBuffer(byte[] data, int position, int length){
            string value = Encoding.UTF8.GetString(data, position, length);
            return value;
        }

        public static void CaculateSickChecksum(List<byte> list) {
            byte checksum = 0x0;
            for (int i = 8; i < list.Count; i++) {
                checksum ^= list[i];
            }
            list.Add(checksum);
        }

        public static byte CaculateHolderChecksum(IEnumerable<byte> list,int offset){
            byte value = 0x00;
            for (int i = offset; i < list.Count(); i++) {
                value += list.ElementAt(i);
            }

            return value;
        }

        public static byte CaculateHolderChecksum(byte[] list, int offset,int length)
        {
            byte value = 0x00;
            for (int i = offset; i < offset+length; i++){
                value += list[i];
            }

            return value;
        }

        public static int CaculateModbusCRC(byte[] data,int offset,int length) {
            int CRC = 0x0000FFFF;
            int POLYNOMIAL = 0x0000A001;

            int i, j;
            for (i = offset; i < offset+length; i++)
            {
                CRC ^= (int)data[i];
                for (j = 0; j < 8; j++)
                {
                    if ((CRC & 0x00000001) == 1)
                    {
                        CRC >>= 1;
                        CRC ^= POLYNOMIAL;
                    }
                    else{
                        CRC >>= 1;
                    }
                }
            }
            CRC = ((CRC & 0x0000FF00) >> 8) | ((CRC & 0x000000FF) << 8);
            return CRC;
        }
        

        public static void PrintValue<T>(T value) where T:struct {
            foreach (FieldInfo field in typeof(T).GetFields(BindingFlags.Instance|BindingFlags.NonPublic |BindingFlags.Public)){
                Console.WriteLine("{0} = {1}", field.Name, field.GetValue(value));
            }
        }
    }
}
 