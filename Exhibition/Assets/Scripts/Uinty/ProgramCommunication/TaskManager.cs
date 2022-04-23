using UnityEngine;

class TaskManager : MonoBehaviour
{

    public static TaskManager instance;

    private SpatialAnalysis spatialAnalysis;

    private void Awake()
    {
        instance = this;
        spatialAnalysis = FindObjectOfType<SpatialAnalysis>();
    }

    public void StartTakeCoal(string[] data)
    {
        UIManager.instance.TakeCoalExhibition(data[3]);
        spatialAnalysis.take_coal_status = true;
        
    }

    public void StopTakeCoal()
    {
        Debug.Log("StopTake");
        UIManager.instance.Refresh(delegate () {
            UIManager.instance.ClearInterface();
        });
        spatialAnalysis.take_coal_status = false;
        Debug.Log("Stop2Take");
    }

    public void StopStackCoal()
    {
        UIManager.instance.Refresh(delegate () {
            UIManager.instance.ClearInterface();
        });
    }

    public void StartStackCoal(string[] data)
    {
        UIManager.instance.Refresh(delegate () {
            UIManager.instance.StackCoalExhibition(data[3]);
        });
    }

    public void TaskCommandReceived() {

    }
}
