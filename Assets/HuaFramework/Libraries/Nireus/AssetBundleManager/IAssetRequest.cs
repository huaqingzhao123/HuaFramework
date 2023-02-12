using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nireus;
public interface IAssetRequest
{
    UnityEngine.Object asset { get; }
    bool isDone { get; }
    string path{ get; }
    System.Object info{ get; }
}
