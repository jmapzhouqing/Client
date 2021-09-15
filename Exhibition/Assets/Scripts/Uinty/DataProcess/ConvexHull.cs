using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Hull {
    public float x;
    public float z;
    public float angle;
    public float distance;

    public Hull(float x, float z, float angle, float distance) {
        this.x = x;
        this.z = z;
        this.angle = angle;
        this.distance = distance;
    }
}

public class ConvexHull
{
    public static float calculateBearingToPoint(float currentBearing, float currentX, float currentY, float targetX, float targetY){
        float X = targetX - currentX, Y = targetY - currentY, targetAngle;

        if (X == 0)
        {
            if (Y >= 0)
                targetAngle = -currentBearing;
            else
                targetAngle = 180 - currentBearing;
        }
        else
        {
            if (X > 0)
            {
                if (Y >= 0)
                    targetAngle = 90 - currentBearing - 180 * Mathf.Atan(Y / X) / Mathf.PI;
                else
                    targetAngle = 90 - currentBearing - (360 + 180 * Mathf.Atan(Y / X) / Mathf.PI);
            }
            else
            {
                targetAngle = 90 - currentBearing - (180 + 180 * Mathf.Atan(Y / X) / Mathf.PI);
            }
        }

        if (targetAngle - 360.0f >= 0)
            return targetAngle - 360.0f;
        else
        {
            if (targetAngle + 360.0f <= 0)
                return targetAngle + 720.0f;
            else
            {
                if (targetAngle < 0)
                    return targetAngle + 360.0f;
                else
                    return targetAngle;
            }
        }
    }

    public static List<Vector2> convexHull(List<Vector3> points)
    {
        List<Vector2> Hull = new List<Vector2>();

        if (points.Count < 3){
            return Hull;
        }

        Vector3 first_point = points.OrderBy(s => s.z).FirstOrDefault();

        points.Remove(first_point);

        List<Hull> all = (from n in points
                          let vector = new Vector2(n.x - first_point.x, n.z - first_point.z)
                          let angle = Convert.ToSingle(Math.Round(Vector2.Angle(vector, Vector2.right),1))
                          let distance = vector.magnitude
                          orderby angle,distance
                          group new Hull(n.x,n.z,angle,distance) by angle into g 
                          let first = g.OrderByDescending(s => s.distance).First()
                          select new Hull(first.x,first.z,g.Key,first.distance)).ToList();

      
        Hull.Add(new Vector2(first_point.x,first_point.z));

        for (int i = 0; i < all.Count; i++) {
            Vector2 vec = new Vector2(all[i].x, all[i].z);
            Calc(Hull,vec);
        }

        return Hull;
    }

    private static void Calc(List<Vector2> hull,Vector2 point) {
        if (hull.Count == 1) {
            hull.Add(point);
        }else{
            Vector2 center = hull[hull.Count - 1];
            Vector2 fir = hull[hull.Count - 2];

            if (CheckDirection(fir, center, point))
            {
                hull.Add(point);
            }else{
                hull.RemoveAt(hull.Count - 1);
                Calc(hull,point);
            }
        }
    }

    private static bool CheckDirection(Vector2 fir, Vector2 center, Vector2 third){
        Vector2 first = fir - center;
        Vector2 second = third - center;
        return (first.x*second.y - first.y*second.x) < 0;
    }


}
