using System;
using System.Collections.Generic;
using UnityEngine;
using Scanner.Struct;

namespace Scanner.DataProcess
{
    class ArmFixed:CoordinateTransform
    {

        public override List<Vector3> Transform(ScannerSector sector)
        {
            Vector3 scale = new Vector3(1, 1, 1);

            Matrixf matrix = new Matrixf();
            Quaternionf quaternion;
            List<Vector3> points = new List<Vector3>();
            foreach (RayInfo info in sector.rays)
            {
                quaternion = new Quaternionf(this.laser_rotation_axis, info.degree);
                matrix.SetTRS(Vector3.zero, quaternion, scale);

                Vector3 origin = matrix.MultiplyPoint(this.laser_direction * info.distance);

                quaternion = Quaternionf.EulerToQuaternion(new Vector3(sector.pitch, sector.yaw, sector.roll));
                matrix.SetTRS(this.scanner_cooridnate, quaternion, scale);
                origin = matrix.MultiplyPoint(origin);

                origin += this.walk_direction * (sector.distance - this.origin_distance);

                points.Add(origin);
            }

            return points;
        }
    }
}
