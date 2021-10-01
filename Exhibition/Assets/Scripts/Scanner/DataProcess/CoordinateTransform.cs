using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Scanner.Struct;

namespace Scanner.DataProcess
{
    public abstract class CoordinateTransform
    {
        protected Vector3 scanner_cooridnate;
        protected Vector3 laser_direction;
        protected Vector3 laser_rotation_axis;
        protected Vector3 scanner_rotation_axis;
        protected Vector3 scanner_pitch_axis;
        protected Vector3 walk_direction;
        protected float holder_origin_field;
        protected float laser_origin_field;
        protected float origin_distance;

        public abstract List<Vector3> Transform(ScannerSector sector);
    }
}
