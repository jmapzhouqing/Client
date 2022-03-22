using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class CoordinateIndex{
    public int x;
    public int z;

    public CoordinateIndex(int x, int z){
        this.x = x;
        this.z = z;
    }
}

public class Coordinate {
    public Vector3 vertice;

    public Coordinate(float x, float y, float z) {
        this.vertice.x = x;
        this.vertice.y = y;
        this.vertice.z = z;
    }

    public Coordinate(Vector3 vertice)
    {
        this.vertice = vertice;
    }

    public Vector3 GetVector3()
    {
        return this.vertice;
    }
}

public class BoundaryCoordinate<T> where T : struct {
    public T min_x;
    public T max_x;
    public T min_z;
    public T max_z;
    public BoundaryCoordinate(T min_x, T max_x, T min_z, T max_z) {
        this.min_x = min_x;
        this.max_x = max_x;
        this.min_z = min_z;
        this.max_z = max_z;
    }
}

public class Grid{
    public BoundaryCoordinate<float> vertice_boundary;
    public BoundaryCoordinate<int> index_boundary;

    public List<Vector2> vertices;

    public float precision;

    public int mesh_segment_number;

    public int index_width;
    public int index_height;

    public Grid(List<Vector2> vertices,float precision,int mesh_segment_number){
        float min_x = vertices.OrderBy(vertice => vertice.x).First().x;
        float max_x = vertices.OrderByDescending(vertice => vertice.x).First().x;

        float min_z = vertices.OrderBy(vertice => vertice.y).First().y;
        float max_z = vertices.OrderByDescending(vertice => vertice.y).First().y;

        this.vertice_boundary = new BoundaryCoordinate<float>(min_x, max_x, min_z, max_z);

        this.index_boundary = new BoundaryCoordinate<int>(Mathf.FloorToInt(min_x / precision), Mathf.FloorToInt(max_x / precision), Mathf.FloorToInt(min_z / precision), Mathf.FloorToInt(max_z / precision));

        this.precision = precision;
        this.mesh_segment_number = mesh_segment_number;

        this.index_width = index_boundary.max_x - index_boundary.min_x;
        this.index_height = index_boundary.max_z - index_boundary.min_z;

        this.vertices = vertices;
    }

    public Grid(List<Vector3> vertices, float precision, int mesh_segment_number) {
        float min_x = vertices.OrderBy(vertice => vertice.x).First().x;
        float max_x = vertices.OrderByDescending(vertice => vertice.x).First().x;

        float min_z = vertices.OrderBy(vertice => vertice.z).First().z;
        float max_z = vertices.OrderByDescending(vertice => vertice.z).First().z;

        this.vertice_boundary = new BoundaryCoordinate<float>(min_x, max_x, min_z, max_z);

        this.index_boundary = new BoundaryCoordinate<int>(Mathf.FloorToInt(min_x / precision), Mathf.FloorToInt(max_x / precision), Mathf.FloorToInt(min_z / precision), Mathf.FloorToInt(max_z / precision));

        this.precision = precision;
        this.mesh_segment_number = mesh_segment_number;

        this.index_width = index_boundary.max_x - index_boundary.min_x;
        this.index_height = index_boundary.max_z - index_boundary.min_z;

        this.vertices = vertices.Select(vertice=>new Vector2(vertice.x,vertice.z)).ToList();
    }

    public Grid(int min_x_index, int max_x_index, int min_z_index, int max_z_index, float precision, int mesh_segment_number){
        this.index_boundary = new BoundaryCoordinate<int>(min_x_index,max_x_index,min_z_index,max_z_index);

        this.vertice_boundary = new BoundaryCoordinate<float>(min_x_index * precision, max_x_index * precision, min_z_index * precision, max_z_index * precision);

        this.precision = precision;

        this.mesh_segment_number = mesh_segment_number;

        this.index_width = max_x_index - min_x_index;
        this.index_height = max_z_index - min_z_index;
    }
    public Grid(float min_x, float max_x, float min_z, float max_z, float precision, int mesh_segment_number){

        this.vertice_boundary = new BoundaryCoordinate<float>(min_x, max_x, min_z, max_z);

        this.index_boundary = new BoundaryCoordinate<int>(Mathf.FloorToInt(min_x / precision), Mathf.FloorToInt(max_x / precision), Mathf.FloorToInt(min_z / precision), Mathf.FloorToInt(max_z / precision));

        this.precision = precision;
        this.mesh_segment_number = mesh_segment_number;

        this.index_width = index_boundary.max_x - index_boundary.min_x;
        this.index_height = index_boundary.max_z - index_boundary.min_z;
    }

    public bool Contains(Grid grid){
        if (index_boundary.min_x <= grid.index_boundary.min_x && index_boundary.max_x >= grid.index_boundary.max_x && index_boundary.min_z <= grid.index_boundary.min_z && index_boundary.max_z >= grid.index_boundary.max_z){
            return true;
        }else{
            return false;
        }
    }

    public bool CheckVerticeInGrid(int x,int z){
        if (x >= index_boundary.min_x && x <= index_boundary.max_x && z >= index_boundary.min_z && z <= index_boundary.max_z){
            return true;
        }else{
            return false;
        }
    }

    public bool CheckPolygonIntersection(Grid grid) {
        bool is_intersection = false;

        BoundaryCoordinate<float> grid_vertice_boundary = grid.vertice_boundary ;

        return !(grid_vertice_boundary.max_z < vertice_boundary.min_z || vertice_boundary.max_z < grid_vertice_boundary.min_z || vertice_boundary.min_x > grid_vertice_boundary.max_x || grid_vertice_boundary.min_x > vertice_boundary.max_x);
    }

    public Polygon CreatePolygon(){
        List<Vector2> vertices = new List<Vector2>();
        vertices.Add(new Vector2(vertice_boundary.min_x, vertice_boundary.min_z));
        vertices.Add(new Vector2(vertice_boundary.min_x, vertice_boundary.max_z));
        vertices.Add(new Vector2(vertice_boundary.max_x, vertice_boundary.max_z));
        vertices.Add(new Vector2(vertice_boundary.max_x, vertice_boundary.min_z));
        Polygon polygon = new Polygon(vertices);
        return polygon;
    }

    public Vector2 GetSize() {
        Vector2 value = Vector2.zero;
        value.x = vertice_boundary.max_x - vertice_boundary.min_x;
        value.y = vertice_boundary.max_z - vertice_boundary.min_z;
        return value;
    }
}

public class Polygon{
    public List<Vector2> polygon_vertices;

    public List<Vector2> interior_vertices;

    public Polygon(List<Vector3> vertices)
    {
        this.polygon_vertices = new List<Vector2>();
        foreach (Vector3 vertice in vertices){
            Vector2 vec = new Vector2(vertice.x, vertice.z);
            this.polygon_vertices.Add(vec);
        }
    }

    public Polygon(List<Vector2> vertices){
        this.polygon_vertices = vertices;
    }

    public Vector4 GetVertices() {
        float min_x = polygon_vertices.OrderBy(vertice => vertice.x).First().x;
        float max_x = polygon_vertices.OrderByDescending(vertice => vertice.x).First().x;
        float min_z = polygon_vertices.OrderBy(vertice => vertice.y).First().y;
        float max_z = polygon_vertices.OrderByDescending(vertice => vertice.y).First().y;

        return new Vector4(min_x,max_x,min_z,max_z);
    }

    public void CreateInteriorPoint(float precison = 0.1f) {
        float min_x = polygon_vertices.OrderBy(vertice => vertice.x).First().x;
        float max_x = polygon_vertices.OrderByDescending(vertice => vertice.x).First().x;
        float min_z = polygon_vertices.OrderBy(vertice => vertice.y).First().y;
        float max_z = polygon_vertices.OrderByDescending(vertice => vertice.y).First().y;

        interior_vertices = new List<Vector2>();

        for (float i = min_x; i < max_x; i += precison) {
            for (float j = min_z; j < max_z; j += precison) {
                this.interior_vertices.Add(new Vector2(i, j));
            }
        }
    }

    public void SetInteriorPoint(List<Vector2> data) {
        this.interior_vertices = data;
    }

    public void Translation(Vector2 center) {
        polygon_vertices = polygon_vertices.Select(vertice => vertice + center).ToList();
    }

    public bool CheckInPolygon(Vector2 checkPoint){
        bool inside = false;
        int pointCount = polygon_vertices.Count;
        Vector2 p1, p2;
        for (int i = 0, j = pointCount - 1; i < pointCount; j = i, i++)
        {
            p1 = polygon_vertices[i];
            p2 = polygon_vertices[j];
            if (checkPoint.y < p2.y){
                if (p1.y <= checkPoint.y){
                    if ((checkPoint.y - p1.y) * (p2.x - p1.x) > (checkPoint.x - p1.x) * (p2.y - p1.y)){
                        inside = !inside;
                    }
                }
            }else if (checkPoint.y < p1.y){
                if ((checkPoint.y - p1.y) * (p2.x - p1.x) < (checkPoint.x - p1.x) * (p2.y - p1.y))
                {
                    inside = !inside;
                }
            }
        }
        return inside;
    }

    public bool CheckInPolygon(Vector2 checkPoint,bool isfast) {
        int vertice_length = this.polygon_vertices.Count;
        int i, j = vertice_length - 1;
        bool oddNodes = false;

        for (i = 0; i < vertice_length; i++){
            Vector2 pre = polygon_vertices[i];
            Vector2 next = polygon_vertices[j];
            if (pre.y < checkPoint.y && next.y >= checkPoint.y || (next.y < checkPoint.y && pre.y >= checkPoint.y)){
                if (pre.x + (checkPoint.y - pre.y) / (next.y - pre.y) * (next.x - pre.x) < checkPoint.x){
                    oddNodes = !oddNodes;
                }
            }
            j = i;
        }

        return oddNodes;
    }

    public bool CheckPolygonIntersection(Polygon other){
        bool is_intersection = false;

        Vector4 vertice = this.GetVertices();

        Vector4 other_vertice = other.GetVertices();
        /*
        foreach (Vector2 vertice in other.polygon_vertices){
            if (this.CheckInPolygon(vertice)){
                is_intersection = true;
                break;
            }
        }

        foreach (Vector2 vertice in this.polygon_vertices){
            if (other.CheckInPolygon(vertice))
            {
                is_intersection = true;
                break;
            }
        }
        return is_intersection;*/

        return !(other_vertice.w < vertice.z || vertice.w < other_vertice.z || vertice.x > other_vertice.y || other_vertice.x > vertice.y);
    }

    public BoundaryCoordinate<float> GetPolygonVerticeBoundary(){

        BoundaryCoordinate<float> vertice_boundary = null;

        float min_x = polygon_vertices.OrderBy(value => value.x).First().x;
        float max_x = polygon_vertices.OrderByDescending(value => value.x).First().x;
        float min_z = polygon_vertices.OrderBy(value => value.y).First().y;
        float max_z = polygon_vertices.OrderByDescending(value => value.y).First().y;

        vertice_boundary = new BoundaryCoordinate<float>(min_x, max_x, min_z, max_z);

        return vertice_boundary;
    }
    public BoundaryCoordinate<int> GetPolygonIndexBoundary(float precision){

        BoundaryCoordinate<int> index_boundary = null;

        int min_x = Mathf.FloorToInt(polygon_vertices.OrderBy(value => value.x).First().x/precision);
        int max_x = Mathf.FloorToInt(polygon_vertices.OrderByDescending(value => value.x).First().x/precision);
        int min_z = Mathf.FloorToInt(polygon_vertices.OrderBy(value => value.y).First().y/precision);
        int max_z = Mathf.FloorToInt(polygon_vertices.OrderByDescending(value => value.y).First().y/precision);

        index_boundary = new BoundaryCoordinate<int>(min_x, max_x, min_z, max_z);

        return index_boundary;
    }

    public Polygon CreateRotationPolygon(Vector2 center,float rotation) {
        List<Vector2> vertices = new List<Vector2>();
        List<Vector2> interior_vertices = new List<Vector2>();

        foreach (Vector2 vertice in this.polygon_vertices) {
            Vector2 trans_vertice = center + SpatialAlternation.Vector2Rotate(Vector2.zero,vertice,rotation);
            vertices.Add(trans_vertice);
        }

        foreach (Vector2 vertice in this.interior_vertices)
        {
            Vector2 trans_vertice = center + SpatialAlternation.Vector2Rotate(Vector2.zero, vertice, rotation);
            interior_vertices.Add(trans_vertice);
        }

        Polygon polygon = new Polygon(vertices);
        polygon.SetInteriorPoint(interior_vertices);
        return polygon;
    }
}

public class CoalDumpInfo{
    public string uuid;
    public string dump_name;
    public List<Vector2> vertices;
    public int level;

    public CoalDumpInfo(){

    }

    public CoalDumpInfo(string uuid) {
        this.uuid = uuid;
    }

    public Grid CreateGrid() {
        float min_x = vertices.OrderBy(vertice => vertice.x).First().x;
        float max_x = vertices.OrderByDescending(vertice => vertice.x).First().x;

        float min_z = vertices.OrderBy(vertice => vertice.y).First().y;
        float max_z = vertices.OrderByDescending(vertice => vertice.y).First().y;

        Grid grid = new Grid(min_x,max_x,min_z,max_z,ConfigurationParameter.precision,ConfigurationParameter.mesh_segment_number);

        return grid;
    }
}

public class StackCoalInfo {
    public string dump_name;
    public int side;
    public float max_z;
    public float min_z;
    public float internal_rotation;
    public float external_rotation;
    public bool is_empty_dump;
    public bool use_config_corner;
}

public class TakeCoalInfo {
    public string dump_name;
    public Vector3 euler;
    public float position;
    public int level;
}
