using System;
using System.IO;

using UnityEngine;

public class GridDataPersistence
{

    public static void SaveData(string fileLocation,float precision,Vector3[,] data){
        int row = data.GetLength(0);
        int colum = data.GetLength(1);

        int i = 0;
        int j = 0;
        using (FileStream stream = new FileStream(@"D:\CoalYard\coal_data.txt", FileMode.Create))
        using (BinaryWriter writer = new BinaryWriter(stream)) {
            try
            {
                writer.Write(Convert.ToByte(precision * 100));
                
                writer.Write(BitConverter.GetBytes(Convert.ToUInt16(row)));
                writer.Write(BitConverter.GetBytes(Convert.ToUInt16(colum)));

                for (i = 0; i < row; i++)
                {
                    for (j = 0; j < colum; j++)
                    {
                        writer.Write(BitConverter.GetBytes(Convert.ToUInt16(data[i, j].y * 100)));
                    }
                }
            }
            catch (Exception e) {
                Debug.Log(precision*100);
                Debug.Log(row);
                Debug.Log(colum);
                Debug.Log(i + "#"+j+"#"+data[i,j].y * 100);
            }
            
        } 
    }

    public static void ReadData(string fileLocation,Vector3[,] data) {
        byte[] buffered = File.ReadAllBytes(fileLocation);
        int a = 0;
        int yHeight = 0;
        int colorTemp = 0;
        int meshAccuracy = 0;
        meshAccuracy += buffered[a++] & 0xFF;

        float precision = meshAccuracy / 100.0f;

        int xCnt = 0;
        xCnt += buffered[a++] & 0xFF;
        xCnt += (buffered[a++] & 0xFF) << 8;
        int zCnt = 0;
        zCnt += buffered[a++] & 0xFF;
        zCnt += (buffered[a++] & 0xFF) << 8;

        Debug.Log(xCnt + "#" +zCnt + "#"+ meshAccuracy);

        //data = new Vector3[xCnt, zCnt];

        for (int i = 0; i < data.GetLength(0); i++){
            for (int j = 0; j < data.GetLength(1); j++){
                yHeight = 0;
                yHeight += buffered[a++] & 0xFF;
                yHeight += (buffered[a++] & 0xFF) << 8;

                float y = yHeight / 100.0f;
                data[i, j] = new Vector3(i * precision, y, j * precision);
            }
        }

        /*
        for (int i = 0; i < xCnt; i++)
        {
            for (int j = 0; j < zCnt; j++)
            {
                yHeight = 0;
                yHeight += buffered[a++] & 0xFF;
                yHeight += (buffered[a++] & 0xFF) << 8;

                float y = yHeight / 100.0f;

                data[j, i] = new Vector3(j * precision, y, i * precision);
                a++;
                //RGBList[e, f] = new Color(24 / 255f, 24 / 255f, 24 / 255f, 1f);
            }
        }*/
    }
}
