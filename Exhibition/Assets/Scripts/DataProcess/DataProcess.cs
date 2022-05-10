using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scanner.DataProcess
{

    class VectorCompare : IComparer<Vector3>
    {
        public int Compare(Vector3 x, Vector3 y)
        {
            return x.y.CompareTo(y.y);
        }
    }

    class Index
    {
        public int valida_number { get; set; } = 0;
        public int index_x { get; set; } = 0;
        public int index_z { get; set; } = 0;

        public Index(int valida_number, int index_x, int index_z)
        {
            this.valida_number = valida_number;
            this.index_x = index_x;
            this.index_z = index_z;
        }
    }

    public class DataProcess
    {
        public static void RadiusFilter(float radius, float[,] mesh_data, float precision)
        {
            int width = mesh_data.GetLength(0);
            int height = mesh_data.GetLength(1);

            int width_center = (width - 1) / 2;
            int height_center = (height - 1) / 2;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (Vector2.Distance(new Vector2((i - width_center) * precision, -1 * (j - height_center) * precision), Vector2.zero) > radius)
                    {
                        mesh_data[i, j] = 0;
                    }
                    else if (mesh_data[i, j] <= Math.Pow(10, -2))//对于煤场区域内的点，如果是0点，补成0.01；避免漏地面丑
                    {
                        mesh_data[i, j] = 0.011f;
                    }
                }
            }
        }

        public static void InnerRadiusFilter(float radius, float[,] mesh_data, float precision)
        {
            int width = mesh_data.GetLength(0);
            int height = mesh_data.GetLength(1);

            int width_center = (width - 1) / 2;
            int height_center = (height - 1) / 2;

            int dimension = (int)(radius / precision);

            for (int i = (int)Math.Round((double)(width / 2 - dimension - 1)); i < (int)Math.Round((double)(width / 2 + dimension + 1)); i++)
            {
                for (int j = (int)Math.Round((double)(height / 2 - dimension - 1)); j < (int)Math.Round((double)(height / 2 + dimension + 1)); j++)
                {
                    if (Vector2.Distance(new Vector2((i - width_center) * precision, 1 * (j - height_center) * precision), Vector2.zero) < radius)
                    {
                        mesh_data[i, j] = 0;
                    }
                }
            }
        }

        public static void EliminateLoop(float[,] mesh_data, int dimension, int times)
        {
            try
            {
                int width = mesh_data.GetLength(0);
                int height = mesh_data.GetLength(1);

                int center_index = dimension / 2;
                while (true)
                {
                    int deal_number = 0;
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            float correct_data = mesh_data[i, j];
                            if (correct_data > Mathf.Pow(10, -2))
                            {
                                float result = 0;
                                int index = 0;

                                float difference = 0;

                                for (int m = -center_index; m <= center_index; m++)
                                {
                                    for (int n = -center_index; n <= center_index; n++)
                                    {
                                        int row_index = i + m;
                                        int column_index = j + n;

                                        if (row_index < 0 || row_index > (width - 1) || column_index < 0 || column_index > (height - 1))
                                        {
                                            continue;
                                        }

                                        float origin_data = mesh_data[row_index, column_index];
                                        if (m != 0 && n != 0)
                                        {
                                            result += origin_data;
                                            index++;
                                        }
                                    }
                                }

                                if (index != 0)
                                {
                                    result /= index;

                                    for (int m = -center_index; m <= center_index; m++)
                                    {
                                        for (int n = -center_index; n <= center_index; n++)
                                        {
                                            int row_index = i + m;
                                            int column_index = j + n;

                                            if (row_index < 0 || row_index > (width - 1) || column_index < 0 || column_index > (height - 1))
                                            {
                                                continue;
                                            }

                                            float origin_data = mesh_data[row_index, column_index];
                                            if (m != 0 && n != 0)
                                            {
                                                difference += Mathf.Abs(origin_data - result);
                                            }
                                        }
                                    }

                                    difference /= index;

                                    float ratio = Mathf.Abs(correct_data - result) / difference;

                                    if (ratio > times || ratio < 1 / times)
                                    {
                                        mesh_data[i, j] = 0;
                                        deal_number++;
                                    }
                                }
                            }
                        }
                    }

                    if (deal_number == 0)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        public static void Denoising(int extrude_number, float gradient_value, float[,] mesh_data, float precision)
        {
            int width = mesh_data.GetLength(0);
            int height = mesh_data.GetLength(1);

            int index = int.MaxValue;
            int pre_index = int.MinValue;
            while (index != pre_index)
            {
                pre_index = index;
                index = 0;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 1; j < height; j++)
                    {
                        float fir_gradient = Mathf.Atan((mesh_data[i, j] - mesh_data[i, j - 1]) / precision) * Mathf.Rad2Deg;
                        if (fir_gradient > gradient_value || fir_gradient < gradient_value * -1)
                        {
                            for (int k = j + 1; k < height; k++)
                            {
                                float gradient = Mathf.Atan((mesh_data[i, k] - mesh_data[i, k - 1]) / precision) * Mathf.Rad2Deg;
                                if ((gradient > gradient_value || gradient < gradient_value * -1) && fir_gradient * gradient < 0)
                                {
                                    if (k - j < extrude_number)
                                    {
                                        index++;
                                        for (int m = j - 1; m <= k; m++)
                                        {
                                            mesh_data[i, m] = 0;
                                        }
                                    }
                                    j = k;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            index = int.MaxValue;
            pre_index = int.MinValue;
            while (index != pre_index)
            {
                pre_index = index;
                index = 0;

                for (int i = 0; i < height; i++)
                {
                    for (int j = 1; j < width; j++)
                    {
                        float fir_gradient = Mathf.Atan((mesh_data[j, i] - mesh_data[j - 1, i]) / precision) * Mathf.Rad2Deg;
                        if (fir_gradient > gradient_value || fir_gradient < gradient_value * -1)
                        {
                            for (int k = j + 1; k < height; k++)
                            {
                                float gradient = Mathf.Atan((mesh_data[k, i] - mesh_data[k - 1, i]) / precision) * Mathf.Rad2Deg;
                                if ((gradient > gradient_value || gradient < gradient_value * -1) && fir_gradient * gradient < 0)
                                {
                                    if (k - j < extrude_number)
                                    {
                                        index++;
                                        for (int m = j - 1; m <= k; m++)
                                        {
                                            mesh_data[m, i] = 0;
                                        }
                                    }
                                    j = k;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }


        public static void CorrectDataArea(int dimension, int beta, float[,] mesh_data, float times)
        {
            int grid_width = mesh_data.GetLength(0);
            int grid_height = mesh_data.GetLength(1);

            float[,] template = new float[dimension, dimension];
            int[] center = { dimension / 2, dimension / 2 };
            for (int i = 0; i < dimension; i++)
            {
                for (int j = 0; j < dimension; j++)
                {
                    float distance = Mathf.Sqrt(Mathf.Pow(i - center[0], 2) + Mathf.Pow(j - center[1], 2));
                    template[i, j] = Mathf.Pow(1.0f / distance, beta);
                }
            }

            List<Index> list = new List<Index>();

            int center_index = dimension / 2;

            for (int i = center_index; i < grid_width - center_index; i++)
            {
                for (int j = center_index; j < grid_height - center_index; j++)
                {

                    float correct_data = mesh_data[i, j];
                    if (correct_data < Mathf.Pow(10, -2))
                    {
                        int valid_number = 0;

                        for (int m = -center_index; m < center_index; m++)
                        {
                            for (int n = -center_index; n < center_index; n++)
                            {
                                float origin_data = mesh_data[i + m, j + n];
                                if (m != 0 && n != 0 && origin_data > Mathf.Pow(10, -2))
                                {
                                    valid_number++;
                                }
                            }
                        }
                        list.Add(new Index(valid_number, i, j));
                    }
                }
            }

            list.Sort((first, second) =>
            {
                return second.valida_number.CompareTo(first.valida_number);
            });

            foreach (Index value in list)
            {
                int index_x = value.index_x;
                int index_z = value.index_z;

                float correct_data = mesh_data[index_x, index_z];
                float result = 0;
                float denominator = 0;
                int valid_number = 0;

                for (int m = -center_index; m < center_index; m++)
                {
                    for (int n = -center_index; n < center_index; n++)
                    {
                        float origin_data = mesh_data[index_x + m, index_z + n];
                        if (m != 0 && n != 0 && origin_data > Mathf.Pow(10, -2))
                        {
                            result += origin_data * template[center_index + m, center_index + n];
                            denominator += template[center_index + m, center_index + n];
                            valid_number++;
                        }
                    }
                }

                if (valid_number > (Mathf.Pow(dimension, 2) * times))
                {

                    mesh_data[index_x, index_z] = result / denominator;
                }
                else
                {
                    mesh_data[index_x, index_z] = 0;
                }
            }
        }

        public static void ClearData(float[,] mesh_data, List<Rect> areas)
        {
            try
            {
                if (areas != null)
                {
                    int width = mesh_data.GetLength(0);
                    int height = mesh_data.GetLength(1);
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            foreach (Rect area in areas)
                            {
                                if (area.Contains(new Vector2(i, j)))
                                {
                                    mesh_data[i, j] = 0;
                                    break;
                                }
                                //对于煤场区域内的点，如果是0点，补成0.01；避免漏地面丑
                                if (mesh_data[i, j] <= Math.Pow(10, -2))
                                {
                                    mesh_data[i, j] = 0.011f;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
               
            }
        }

        public static void CorrectDataRect(float[,] mesh_data, float precision, int dimension, float ratio = 0.45f, int pre_validator = 0)
        {
            try
            {
                int width = mesh_data.GetLength(0);
                int height = mesh_data.GetLength(1);

                int width_center = (width - 1) / 2;
                int height_center = (height - 1) / 2;

                int validator = 0;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        float value = mesh_data[i, j];

                        if (Mathf.Abs(value) < Mathf.Pow(10, -2))
                        {
                            int start_row = i - dimension < 0 ? 0 : i - dimension;
                            int end_row = i + dimension > width - 1 ? width - 1 : i + dimension;
                            int start_column = j - dimension < 0 ? 0 : j - dimension;
                            int end_column = j + dimension > height - 1 ? height - 1 : j + dimension;

                            int number = 0;
                            List<Vector3> data = new List<Vector3>();
                            for (int m = start_row; m <= end_row; m++)
                            {
                                for (int n = start_column; n < end_column; n++)
                                {
                                    if (m != i && n != j)
                                    {
                                        float vertice_y = mesh_data[m, n];
                                        if (Mathf.Abs(vertice_y) > Mathf.Pow(10, -2))
                                        {
                                            number++;
                                            data.Add(new Vector3(m * precision, vertice_y, n * precision));
                                        }
                                    }
                                }
                            }

                            validator++;

                            if (number > Mathf.Pow(2 * dimension, 2) * ratio)
                            {
                                validator--;
                                float result = Convert.ToSingle(MLS(new Vector2(i * precision, j * precision), data.ToArray(), precision, dimension));
                                if (Mathf.Abs(result) < 1000)
                                {
                                    mesh_data[i, j] = result;
                                }
                            }
                        }
                    }
                }

                if (validator != pre_validator)
                {
                    CorrectDataRect(mesh_data, precision, dimension, ratio, validator);
                }
            }
            catch (Exception e)
            {
            }
        }

        public static void CorrectDataCircle(float[,] mesh_data, float precision, int dimension, float radius, float ratio = 0.45f, int pre_validator = 0)
        {
            try
            {
                int width = mesh_data.GetLength(0);
                int height = mesh_data.GetLength(1);

                int width_center = (width - 1) / 2;
                int height_center = (height - 1) / 2;

                int validator = 0;

                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        float value = mesh_data[i, j];

                        float distance = Vector2.Distance(new Vector2((i - width_center) * precision, -1 * (j - height_center) * precision), Vector2.zero);

                        if (distance < radius && Mathf.Abs(value) < Mathf.Pow(10, -2))
                        {
                            int start_row = i - dimension < 0 ? 0 : i - dimension;
                            int end_row = i + dimension > width - 1 ? width - 1 : i + dimension;
                            int start_column = j - dimension < 0 ? 0 : j - dimension;
                            int end_column = j + dimension > height - 1 ? height - 1 : j + dimension;

                            int number = 0;
                            List<Vector3> data = new List<Vector3>();
                            for (int m = start_row; m <= end_row; m++)
                            {
                                for (int n = start_column; n < end_column; n++)
                                {
                                    distance = Vector2.Distance(new Vector2((m - width_center) * precision, -1 * (n - height_center) * precision), Vector2.zero);
                                    if (distance < radius && m != i && n != j)
                                    {
                                        float vertice_y = mesh_data[m, n];
                                        if (Mathf.Abs(vertice_y) > Mathf.Pow(10, -2))
                                        {
                                            number++;
                                            data.Add(new Vector3(m * precision, vertice_y, n * precision));
                                        }
                                    }
                                }
                            }

                            validator++;

                            if (number > Mathf.Pow(2 * dimension, 2) * ratio)
                            {
                                validator--;
                                float result = Convert.ToSingle(MLS(new Vector2(i * precision, j * precision), data.ToArray(), precision, dimension));
                                if (Mathf.Abs(result) < 1000)
                                {
                                    mesh_data[i, j] = result;
                                }
                            }
                        }
                    }
                }

                if (validator != pre_validator)
                {
                    CorrectDataCircle(mesh_data, precision, dimension, radius, ratio, validator);
                }
            }
            catch (Exception e)
            {
            }
        }

        private static double MLS(Vector2 center, Vector3[] data, float precision, int dimension)
        {
            double z_value = 0;
            int number = data.Length;

            int M = 6;
            int N = data.Length;

            double[,] P = new double[N, M];
            double[] W = new double[N];

            double[,] A = new double[M, M];
            double[,] B = new double[M, 1];

            double d = 2 * dimension * precision;

            for (int i = 0; i < number; i++)
            {
                Vector3 vertice = data[i];
                P[i, 0] = 1;
                P[i, 1] = vertice.x;
                P[i, 2] = vertice.z;
                P[i, 3] = Mathf.Pow(vertice.x, 2);
                P[i, 4] = vertice.x * vertice.z;
                P[i, 5] = Mathf.Pow(vertice.z, 2);
                W[i] = W_mat(d, new Vector2(vertice.x, vertice.z), center);
            }

            A = A_mat(W, P, M, N);
            B = B_mat(data, W, P, M, N);

            double[,] A_Inversion = Matrix_Inversion(A);

            if (A_Inversion != null)
            {
                double[,] result = MatrixMult(A_Inversion, B);

                z_value = result[0, 0] + result[1, 0] * center.x + result[2, 0] * center.y + result[3, 0] * Mathf.Pow(center.x, 2) + result[4, 0] * center.y * center.x + result[5, 0] * Mathf.Pow(center.y, 2);
            }

            return z_value;
        }

        private static double[,] Matrix_Inversion(double[,] Array)
        {
            int m = 0;
            int n = 0;
            m = Array.GetLength(0);
            n = Array.GetLength(1);
            double[,] array = new double[2 * m + 1, 2 * n + 1];
            for (int k = 0; k < 2 * m + 1; k++)  //初始化数组
            {
                for (int t = 0; t < 2 * n + 1; t++)
                {
                    array[k, t] = 0.0f;
                }
            }
            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    array[i, j] = Array[i, j];
                }
            }

            for (int k = 0; k < m; k++)
            {
                for (int t = n; t <= 2 * n; t++)
                {
                    if ((t - k) == m)
                    {
                        array[k, t] = 1.0f;
                    }
                    else
                    {
                        array[k, t] = 0;
                    }
                }
            }
            //得到逆矩阵
            for (int k = 0; k < m; k++)
            {
                if (array[k, k] != 1)
                {
                    double bs = array[k, k];
                    array[k, k] = 1;
                    for (int p = k + 1; p < 2 * n; p++)
                    {
                        array[k, p] /= bs;
                    }
                }
                for (int q = 0; q < m; q++)
                {
                    if (q != k)
                    {
                        double bs = array[q, k];
                        for (int p = 0; p < 2 * n; p++)
                        {
                            array[q, p] -= bs * array[k, p];
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            double[,] NI = new double[m, n];
            for (int x = 0; x < m; x++)
            {
                for (int y = n; y < 2 * n; y++)
                {
                    NI[x, y - n] = array[x, y];
                }
            }
            return NI;
        }

        private static double[,] MatrixMult(double[,] A, double[,] B)
        {
            //matrix1是m*n矩阵，matrix2是n*p矩阵，则result是m*p矩阵   
            int A_row = A.GetLength(0);
            int A_colum = A.GetLength(1);

            int B_row = B.GetLength(0);
            int B_colum = B.GetLength(1);

            if (A_colum != B_row)
            {
                return null;
            }

            double[,] result = new double[A_row, B_colum];

            for (int i = 0; i < A_row; i++)
            {
                for (int j = 0; j < B_colum; j++)
                {
                    for (int k = 0; k < A_colum; k++)
                    {
                        result[i, j] += (A[i, k] * B[k, j]);
                    }
                }
            }
            return result;
        }

        private static double W_mat(double d, Vector2 origin, Vector2 center)
        {
            double s = Vector2.Distance(origin, center) / d;


            if (s <= 0.5f)
            {
                return (2.0f / 3) - 4 * Math.Pow(s, 2) + 4 * Math.Pow(s, 3);
            }
            else if (s <= 1)
            {
                return (4.0f / 3) - 4 * s + 4 * Math.Pow(s, 2) - (4.0f / 3) * Math.Pow(s, 3);
            }
            else
            {
                return 0;
            }
        }

        private static double[,] A_mat(double[] W, double[,] P, int M, int N)
        {
            double[,] A = new double[M, M];
            for (int i = 0; i < M; i++)
            {
                for (int j = 0; j < M; j++)
                {
                    double value = 0;
                    for (int k = 0; k < N; k++)
                    {
                        value += W[k] * P[k, i] * P[k, j];
                    }
                    A[i, j] = value;
                }
            }
            return A;
        }

        private static double[,] B_mat(Vector3[] data, double[] W, double[,] P, int M, int N)
        {
            double[,] B = new double[M, 1];
            for (int i = 0; i < M; i++)
            {
                double value = 0;
                for (int j = 0; j < N; j++)
                {
                    value += W[j] * P[j, i] * data[j].y;
                }
                B[i, 0] = value;
            }
            return B;
        }

        public static float Calculate(Vector3[] points, float meshAccuracy){
            float volume = 0;
            Array.Sort(points, new VectorCompare());

            volume += points[0].y * 0.5F * meshAccuracy * meshAccuracy;
            Vector3 a = new Vector3(points[0].x - points[1].x, 0, points[0].z - points[1].z);
            Vector3 b = new Vector3(points[2].x - points[1].x, 0, points[2].z - points[1].z);

            float h = Vector3.Cross(a, b).magnitude / b.magnitude;
            volume += 1.0F / 3.0F * 0.5F * (points[1].y + points[2].y - 2 * points[0].y) * Vector2.Distance(new Vector2(points[2].x, points[2].z), new Vector2(points[1].x, points[1].z)) * h;

            return volume;
        }
    }
}
