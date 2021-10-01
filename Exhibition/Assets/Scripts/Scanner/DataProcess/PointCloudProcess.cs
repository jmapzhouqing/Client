using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Scanner.DataProcess
{
    class PointCloudProcess{
        public static void StatisticalOutlierRemoval(int dimension,float precision,Vector3[,] mesh_data){
            int width = mesh_data.GetLength(0);
            int height = mesh_data.GetLength(1);

            float[,] distance = new float[width, height];
            for (int i = dimension; i < width - dimension; i++)
            {
                for (int j = dimension; j < height - dimension; j++)
                {
                    if (Mathf.Abs(mesh_data[i, j].y) > Mathf.Pow(10,-2)){
                        float sum = 0;
                        float variance = 0;
                        int index = 0;
                        for (int m = -1 * dimension; m <= dimension; m++)
                        {
                            for (int n = -1 * dimension; n <= dimension; n++)
                            {
                                if (m != 0 && n != 0)
                                {
                                    int row = m + i;
                                    int col = n + j;

                                    if (Mathf.Abs(mesh_data[row, col].y) > Mathf.Pow(10, -2)){
                                        float value = distance[i, j] = Vector3.Distance(mesh_data[i, j], mesh_data[row, col]);
                                        sum += value;
                                        variance += Mathf.Pow(value, 2);
                                        index++;
                                    }
                                }
                            }
                        }

                        if (index != 0){
                            float average = sum / index;
                            float squared = variance / index - Mathf.Pow(sum, 2) / (index * index);
                            float stddev = Mathf.Sqrt(squared);

                            for (int m = -1 * dimension; m <= dimension; m++)
                            {
                                for (int n = -1 * dimension; n <= dimension; n++)
                                {
                                    int row = m + i;
                                    int col = n + j;

                                    if (distance[row, col] > (average + 2 * stddev)){
                                        mesh_data[row, col].y = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        public static void RadiusOutlierRemoval(float radius,float precision,Vector3[,] mesh_data){
            int width = mesh_data.GetLength(0);
            int height = mesh_data.GetLength(1);

            int dimension = (int)(radius / precision);

            for (int i = dimension; i < width - dimension; i++){
                for (int j = dimension; j < height - dimension; j++){
                    int count = 0;
                    int validate_count = 0;
                    for (int m = -1 * dimension; m <= dimension; m++)
                    {
                        for (int n = -1 * dimension; n <= dimension; n++)
                        {
                            if (m == 0 && n == 0){
                                continue;
                            }

                            if (Mathf.Abs(mesh_data[i, j].y) > Mathf.Pow(10, -2))
                            {
                                int row = m + i;
                                int col = n + j;

                                if (Mathf.Abs(mesh_data[row, col].y) > Mathf.Pow(10, -2)){
                                    float xz_dis = Vector2.Distance(new Vector2(i * precision, j * precision), new Vector2(row * precision, col * precision));

                                    if (xz_dis < radius)
                                    {
                                        count++;
                                    }

                                    float distance = Vector3.Distance(mesh_data[i, j],mesh_data[row, col]);

                                    if (distance <= (radius / Mathf.Sin(30 * Mathf.Deg2Rad)))
                                    {
                                        validate_count++;
                                    }
                                }
                            }
                        }
                    }
                    if (validate_count < 0.5f * count){
                        mesh_data[i, j].y = 0;
                    }
                }
            }
        }
        public static void MLS(Vector3[,] mesh_data){

        }
        public static void Division() {

        }

        public static void PolyData(){

        }
    }
}
