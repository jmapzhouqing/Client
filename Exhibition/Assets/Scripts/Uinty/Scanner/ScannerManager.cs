using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Scanner.Scanister;

public class ScannerManager : MonoBehaviour
{

    private LMS511 left_scanner;
    private LMS511 right_scanner;
    // Start is called before the first frame update
    void Start(){

    }

    // Update is called once per frame
    void Update(){

    }

    public void initialize() {
        //left_scanner = new LMS511();
        //right_scanner = new LMS511();
    }


    public void StartScanning() {
        if (left_scanner != null) {
            left_scanner.Start();
        }

        if (right_scanner != null){
            left_scanner.Start();
        }
    }

    public void StopScanning() {
        if (left_scanner != null){
            left_scanner.Stop();
        }

        if (right_scanner != null) {
            right_scanner.Stop();
        }
    }

    private void OnDestroy(){
        this.StopScanning();
    }

}
