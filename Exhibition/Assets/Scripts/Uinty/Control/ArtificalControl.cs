using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArtificalControl : MonoBehaviour
{
    private ScannerManager scanner_manager;

    public Image image;

    private bool is_working = false;
    // Start is called before the first frame update
    void Awake(){
        scanner_manager = FindObjectOfType<ScannerManager>();
    }

    public void ArtificalCoalInventory(){
        is_working = !is_working;
        if (is_working) {
            scanner_manager.StartScanning();
        } else{
            scanner_manager.StopScanning();
        }
    }
    
}
