using UnityEngine;
using System.Collections.Generic;

namespace Nireus
{
    public enum LayerType
    {
        Scene,          // 场景层;
        FLOAT,          // 通常使用在Scene层和战斗粒子层之上
		POP_UP,			// 弹出面板，不影响其他面板;
        TIPS,           // tips
		GUIDE,			// 引导层;
        EFFECT,         // UI特效层
        STOP_EVENTS,    // 在某些情况暂时阻断用户的触摸输入
		HIDE,			// 隐藏层，不显示的所有GameObject移到这里;
		MAX
	}

	public class LayerManager : Singleton<LayerManager>
	{
        public GameObject guiRoot { get; private set; }
	    public GameObject sceneUIRoot { get; private set; }
	    public GameObject overrideUIRoot { get; private set; }

        private static string[] _layer_names = new string[]
        {
            "Scene Layer",
            "Float Layer",
            "PopUp Layer",
            "Tips Layer",
			"Guide Layer",
			"Effect Layer",
            "Stop Layer",
			"Hide Layer",
		};
        
        private static readonly string NIREUS_LAYER_FREAB_PATH = "Prefabs/UI/NireusLayerPrefab";
        private static readonly string DEPTH_SORT_UI_ROOT = "Prefabs/UI/DepthSortUIRoot";


        private Transform[] _layers = new Transform[(int)LayerType.MAX];
	    private Camera[] _layer_cameras = new Camera[(int) LayerType.MAX];
		private float[] _layer_cameras_depth = new float[(int)LayerType.MAX];
		//private GameObject _layer_prefab = null;

        public void resetLayers()
        {
            for (int i = 0; i < _layers.Length; ++i)
            {
                if (_layers[i] != null) GameObject.Destroy(_layers[i].gameObject);
            }

            guiRoot = GameObject.Find("GUIRoot");
            if (!guiRoot)
            {
                GameDebug.LogError("[LayerManager] GUIRoot not found");
                return;
            }

            var depth_ui_root_asset = UnityEngine.Resources.Load<GameObject>(DEPTH_SORT_UI_ROOT);

            sceneUIRoot = guiRoot.transform.Find("SceneUIRoot").gameObject;
            overrideUIRoot = guiRoot.transform.Find("OverrideUIRoot").gameObject;

            for (int i = 0; i < (int)LayerType.MAX; ++i)
            {
                GameObject obj;
                if (i == (int)LayerType.POP_UP)
                {
                    obj = new GameObject("PopupUIRoot");
                    obj.transform.SetParent(guiRoot.transform);
                    obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    _layers[i] = obj.transform;
                }
                else if (i == (int)LayerType.Scene)
                {
                    _layers[i] = sceneUIRoot.transform.Find("DepthSortUIRoot");
                    _layers[i].transform.position = Vector3.zero;
                }
                else
                {
                    obj = GameObject.Instantiate(depth_ui_root_asset);
                    _layers[i] = obj.transform;
                    var camera = obj.GetComponent<DepthSortUIRoot>();
                    if (i == (int)LayerType.Scene)
                    {
                        _layers[i].transform.SetParent(sceneUIRoot.transform, false);
                        camera.uiCamera.depth = 0;
                    }
                    else if (i == (int)LayerType.FLOAT)
                    {
                        _layers[i].transform.SetParent(overrideUIRoot.transform, false);
                        camera.uiCamera.depth = 6;
                    }
                    else
                    {
                        _layers[i].transform.SetParent(overrideUIRoot.transform, false);
                        camera.uiCamera.depth = 20 + i;
                    }
                    obj.name = _layer_names[i];
                    _layers[i].transform.position = new Vector3((i + 10) * 10,0,0);
                }

                _layer_cameras[i] = _layers[i].GetComponent<DepthSortUIRoot>()?.uiCamera;
				_layer_cameras_depth[i] = null == _layer_cameras[i] ? 0 : _layer_cameras[i].depth;
			}

            getLayer(LayerType.TIPS).gameObject.AddComponent<FlowManager>();
            //getLayer(LayerType.POP_UP).gameObject.AddComponent<PopUpManager>();

            _layers[(int)LayerType.HIDE].gameObject.SetActive(false);

		}

		public void addToLayer(UITemplate ui_base, LayerType layer_type)
		{
			if (layer_type >= LayerType.MAX) return;
			ui_base.transform.SetParent(_layers[(int)layer_type].transform, false);
		}

        public void addToLayer(Transform t, LayerType layer_type)
		{
			if (layer_type >= LayerType.MAX) return;
			t.SetParent(_layers[(int)layer_type].transform, false);
		}

		public Transform getLayer(LayerType layer_type)
		{
			if (layer_type >= LayerType.MAX) return null;
			return _layers[(int)layer_type];
		}

	    public Camera getLayerCamera(LayerType layer_type)
	    {
	        if (layer_type >= LayerType.MAX) return null;
	        return _layer_cameras[(int)layer_type];
        }

		//设置层级， _layer_cameras_depth 中记录的层级不变，用于恢复
		public void SetLayerCameraDepth(LayerType layer_type, float depth)
		{
			Camera camera = getLayerCamera(layer_type);
			if (null == camera)
			{
				GameDebug.LogError($"SetLayerCameraDepth null, layer_type = {layer_type}");
				return;
			}
			camera.depth = depth;
		}

		public void ResetLayerCameraDepth(LayerType layer_type)
		{
			Camera camera = getLayerCamera(layer_type);
			if (null == camera)
			{
				GameDebug.LogError($"ResetLayerCameraDepth null, layer_type = {layer_type}");
				return;
			}
			camera.depth = _layer_cameras_depth[(int)layer_type];
		}

        public void ClearAllChild(LayerType layer_type)
        {
            if (layer_type >= LayerType.MAX) return;
            Transform parent = _layers[(int)layer_type];
            if (parent == null) return;
            Camera camera = getLayerCamera(layer_type);
            if (camera == null) return;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) != camera.transform)
                    GameObject.Destroy(parent.GetChild(i).gameObject);
            }
        }


	}
}
