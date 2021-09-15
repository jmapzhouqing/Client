using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoalDumpManager : MonoBehaviour
{
    public CoalDumpInfo info;

    public Grid grid;

    public int level;

    public Material material;

    public Dictionary<string, BlockManager> blockManagers;
    
    // Start is called before the first frame update
    void Awake(){
        blockManagers = new Dictionary<string, BlockManager>();
    }

    // Update is called once per frame
    void Update(){
        
    }

    public void UpdateCoalYard(int x,int z){
        foreach (KeyValuePair<string, BlockManager> item in blockManagers) {
            if (item.Value.CheckVerticeInGrid(x,z)) {
                item.Value.UpdateBlock(x,z);
                break;
            }
        }
    }
    public void UpdateCoalYard(List<Vector3> vertices){

    }

    public void ErasureGridData(int start_x,int end_x,int start_z,int end_z){
        for (int i = start_x; i <= end_x; i++) {
            for (int j = start_z; j <= end_z; j++) {
                int x = i + start_x;
                int y = j + start_z;
                Vector3 vertice = new Vector3(x*0.1f,0,y*0.1f);
                this.UpdateCoalYard(x,y);
            }
        }
    }

    public void AddChild(string key,BlockManager blockManager) {
        blockManagers.Add(key, blockManager);
    }

    public bool CheckVerticeInGrid(int x,int z){
        return this.grid.CheckVerticeInGrid(x,z);
    }
}
