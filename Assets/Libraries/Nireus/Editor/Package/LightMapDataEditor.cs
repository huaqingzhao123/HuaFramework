using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace Nireus.Editor
{
	public class LightMapDataEditor : UnityEditor.Editor
	{
		// 把renderer上面的lightmap信息保存起来，以便存储到prefab上面
		[MenuItem("Nireus/Lightmap/Save", false, 501)]
		static void SaveLightmapInfo()
		{
			GameObject go = Selection.activeGameObject;
			if (go == null) return;
			Nireus.LightmapData data = go.GetComponent<Nireus.LightmapData>();
			if (data == null)
			{
				data = go.AddComponent<Nireus.LightmapData>();
			}

			data.SaveLightmap();
			EditorUtility.SetDirty(go);
		}
		// 把保存的lightmap信息恢复到renderer上面
		[MenuItem("Nireus/Lightmap/Load", false, 502)]
		static void LoadLightmapInfo()
		{
			GameObject go = Selection.activeGameObject;
			if (go == null) return;
			Nireus.LightmapData data = go.GetComponent<Nireus.LightmapData>();
			if (data == null) return;

			data.LoadLightmap();
			EditorUtility.SetDirty(go);
			//new GameObject();
		}

		[MenuItem("Nireus/Lightmap/Clear", false, 503)]
		static void ClearLightmapInfo()
		{
			GameObject go = Selection.activeGameObject;
			if (go == null) return;
			Nireus.LightmapData data = go.GetComponent<Nireus.LightmapData>();
			if (data == null) return;

			data.m_RendererInfo.Clear();
			EditorUtility.SetDirty(go);
		}
	}
}
