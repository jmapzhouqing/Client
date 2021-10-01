using System.Collections.Generic;
using UnityEngine;

using Scanner.Struct;

namespace Scanner.DataProcess
{
    public class HolderFixed : CoordinateTransform
    {
        public HolderFixed(Vector3 scanner_cooridnate, Vector3 laser_direction, Vector3 laser_rotation_axis, Vector3 scanner_rotation_axis, Vector3 walk_direction, float holder_origin_field, float laser_origin_field, float origin_distance)
        {
            this.scanner_cooridnate = scanner_cooridnate;
            this.laser_direction = laser_direction.normalized;
            this.laser_rotation_axis = laser_rotation_axis.normalized;
            this.scanner_rotation_axis = scanner_rotation_axis.normalized;
            this.walk_direction = walk_direction.normalized;
            this.holder_origin_field = holder_origin_field;
            this.laser_origin_field = laser_origin_field;
            this.origin_distance = origin_distance;
        }

        public override List<Vector3> Transform(ScannerSector sector){
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

        public List<Vector3> ArmFixed(ScannerSector sector)
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

