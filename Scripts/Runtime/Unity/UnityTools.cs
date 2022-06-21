using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Unity {

    public class UnityTools 
    {
        
        public static string ProjectRoot
        {
            get
            {
                return Application.dataPath.Replace("/Assets", "");
            }
        }
        public static string Library
        {
            get
            {
                return Application.dataPath.Replace("Assets", "Library");
            }
        }
    }



}

