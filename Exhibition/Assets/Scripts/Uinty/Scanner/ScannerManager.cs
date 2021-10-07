using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

using Scanner.Scanister;
using Scanner.Struct;

public class ScannerManager : MonoBehaviour
{
    public void StartScanning() {
        Debug.Log("StartScanning");
        foreach (ScannerPoint scanner in this.GetComponentsInChildren<ScannerPoint>()) {
            scanner.StartDevice();
        }
    }

    public void StopScanning() {
        Debug.Log("StopScanning");
        foreach (ScannerPoint scanner in this.GetComponentsInChildren<ScannerPoint>()){
            scanner.StopDevice();
        }
    }
}
