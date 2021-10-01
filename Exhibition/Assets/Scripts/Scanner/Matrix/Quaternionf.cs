using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Quaternionf
{

    public float x;

    public float y;

    public float z;

    public float w;

    public Quaternionf()
    {

    }

    public Quaternionf(float x, float y, float z, float w)
    {
        this.x = x;
        this.y = y;
        this.z = z;
        this.w = w;
    }

    public Quaternionf(Vector3 vector, float rotation)
    {
        vector *= Mathf.Sin(rotation / 2.0f * Mathf.Deg2Rad);

        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
        this.w = Mathf.Cos(rotation / 2.0f * Mathf.Deg2Rad);
    }

    public static Quaternionf EulerToQuaternion(Vector3 euler)
    {

        euler *= Mathf.Deg2Rad;

        float cX = Mathf.Cos(euler.x / 2.0f);
        float sX = Mathf.Sin(euler.x / 2.0f);

        float cY = Mathf.Cos(euler.y / 2.0f);
        float sY = Mathf.Sin(euler.y / 2.0f);

        float cZ = Mathf.Cos(euler.z / 2.0f);
        float sZ = Mathf.Sin(euler.z / 2.0f);

        Quaternionf qX = new Quaternionf(sX, 0.0f, 0.0f, cX);
        Quaternionf qY = new Quaternionf(0.0f, sY, 0.0f, cY);
        Quaternionf qZ = new Quaternionf(0.0f, 0.0f, sZ, cZ);

        Quaternionf q = (qY * qX) * qZ;

        return q;
    }

    public static float SqrMagnitude(Quaternionf q)
    {
        return q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w;
    }

    public static Quaternionf operator *(Quaternionf pre, Quaternionf next)
    {
        return new Quaternionf(pre.w * next.x + pre.x * next.w + pre.y * next.z - pre.z * next.y,
            pre.w * next.y + pre.y * next.w + pre.z * next.x - pre.x * next.z,
            pre.w * next.z + pre.z * next.w + pre.x * next.y - pre.y * next.x,
            pre.w * next.w - pre.x * next.x - pre.y * next.y - pre.z * next.z);
    }


}
