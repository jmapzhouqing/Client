using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class DeviceStatusControl : MonoBehaviour
{
    public abstract void SetStatus(string value);
}
