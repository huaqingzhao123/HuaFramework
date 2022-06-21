namespace HuaFramework.Managers
{
    using HuaFramework.ResourcesRef;
    using HuaFramework.Singleton;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public partial class ResManager : MonoSingleton<ResManager>
    {

        public List<ResData> AssetDatas = new List<ResData>();

#if UNITY_EDITOR
        private const string SimulationModeKey = "Simulation Mode";
        private static int _simulationMode = -1;
        public static bool SimulationMode
        {
            get
            {
                if (_simulationMode == -1)
                    _simulationMode = UnityEditor.EditorPrefs.GetBool(SimulationModeKey, true) ? 1 : 0;
                return _simulationMode != 0;
            }
            set
            {
                _simulationMode = value ? 1 : 0;
                UnityEditor.EditorPrefs.SetBool(SimulationModeKey, value);
            }

        }
#endif
        public static bool IsSimulationModeLogic
        {
            get
            {
#if UNITY_EDITOR
                return SimulationMode;
#endif
                return false;
            }

        }

        private void OnGUI()
        {
            if (Input.GetKey(KeyCode.F1))
            {
                GUILayout.BeginVertical();
                AssetDatas.ForEach((item) =>
                {
                    GUILayout.Label(string.Format("资源:{0}的引用数量为:{1},状态为:{2}", item.Name, item.RefCount, item.AssetState.ToString()));
                });
                GUILayout.EndVertical();
            }
        }
        private void OnDestroy()
        {
            AssetDatas.Clear();
        }
    }
}

