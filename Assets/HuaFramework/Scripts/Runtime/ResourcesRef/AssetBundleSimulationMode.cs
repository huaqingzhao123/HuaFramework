using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using HuaFramework.Managers;

namespace HuaFramework.ResourcesRef
{
    public class AssetBundleSimulationMode
    {
#if UNITY_EDITOR
        private const string SimulationPath = "HuaFramework/Simulation Mode";

        public static bool SimulationMode
        {
            get { return ResManager.SimulationMode; }
            set { ResManager.SimulationMode = value; }
        }

        [MenuItem(SimulationPath)]
        private static void ToggleSimulationMode()
        {
            SimulationMode = !SimulationMode;
        }


        [MenuItem(SimulationPath, true)]
        private static bool ToggleSimulationModeValidate()
        {
            //设置菜单状态
            Menu.SetChecked(SimulationPath, SimulationMode);
            return true;
        }

#endif

    }

}
