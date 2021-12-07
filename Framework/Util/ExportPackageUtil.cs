using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HuaFramework.Utility
{

    public class ExportPackageUtil
    {
        public static string GetPackageName()
        {
            return "HuaFramework" + DateTime.Now.ToString("_yyyyMMdd_HH");
        }

    }
}