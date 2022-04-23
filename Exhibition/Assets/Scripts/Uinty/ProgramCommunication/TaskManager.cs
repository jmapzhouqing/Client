using UnityEngine;

class TaskManager : MonoBehaviour
{

    public static TaskManager instance;

    private SpatialAnalysis spatialAnalysis;

    private void Awake()
    {
        spatialAnalysis = FindObjectOfType<SpatialAnalysis>();
    }

    public void StartTakeCoal(string[] data)
    {
        UIManager.instance.Refresh(delegate () {
            UIManager.instance.TakeCoalExhibition(data[3]);
        });
        spatialAnalysis.take_coal_status = true;
    }

    public void StopTakeCoal()
    {
        UIManager.instance.Refresh(delegate () {
            UIManager.instance.ClearInterface();
        });
        spatialAnalysis.take_coal_status = false;
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
