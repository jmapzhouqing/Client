using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.IO;
using Newtonsoft.Json;

public class DataRead
{
    public static List<CoalDumpInfo> ReadCoalDumpData() {
        List<CoalDumpInfo> coal_dump = new List<CoalDumpInfo>();
        using (TextReader reader = new StreamReader(@"E:\CoalYard\info.txt", false))
        {
            string data = reader.ReadToEnd();
            List<string> data_list = JsonConvert.DeserializeObject<List<string>>(data);
            foreach (string item in data_list) {
                CoalDumpInfo info = JsonUtility.FromJson<CoalDumpInfo>(item);
                coal_dump.Add(info);
            }
        }
        return coal_dump;
    }

    private void ReadByteData() {

    }

    private void ReadTextData() {

    }

    public static void SaveCoalDumpData(List<CoalDumpInfo> infos){
        using (TextWriter writer = new StreamWriter(@"E:\CoalYard\info.txt",false)) {
            List<string> data_list = new List<string>();
            foreach (CoalDumpInfo info in infos) {
                if (info.vertices.Count > 4) {
                    info.vertices = info.vertices.Take(4).ToList();
                }

                string data = JsonUtility.ToJson(info);
                data_list.Add(data);
            }
            string result = JsonConvert.SerializeObject(data_list);
            writer.Write(result);
        }
    }
}
