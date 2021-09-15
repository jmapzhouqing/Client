using System;
using System.IO;

using UnityEngine;

public class GridDataPersistence
{
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

        Color[,] RGBList = new Color[xCnt, zCnt];
        for (int e = 0; e < xCnt; e++)
        {
            for (int f = 0; f < zCnt; f++){
                yHeight = 0;
                yHeight += buffered[a++] & 0xFF;
                yHeight += (buffered[a++] & 0xFF) << 8;

                float y = yHeight / 100.0f;


                data[f, e] = new Vector3(f * precision, y, e * precision);
                a++;
                //RGBList[e, f] = new Color(24 / 255f, 24 / 255f, 24 / 255f, 1f);
            }
        }
    }
}
