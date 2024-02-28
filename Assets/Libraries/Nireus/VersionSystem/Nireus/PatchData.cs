#if !ASSET_RESOURCES && (!UNITY_IOS || USE_SUB_PACKAGE) 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatchData {

    public int data_length;
    public int file_count;
    public string patch_md5;
    //string[] files_names;
    //string[] files_md5;
}

#endif