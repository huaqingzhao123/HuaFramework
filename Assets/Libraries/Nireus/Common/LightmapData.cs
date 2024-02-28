using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nireus
{
	public class LightmapData : MonoBehaviour
	{
		[System.Serializable]
		public struct RendererInfo
		{
			public Renderer renderer;
			public int lightmapIndex;
			public Vector4 lightmapOffsetScale;
		}

		[System.Serializable]
		public struct TerrainInfo
		{
			public Terrain terrain;
			public int lightmapIndex;
			public Vector4 lightmapOffsetScale;
		}

		public List<RendererInfo> m_RendererInfo = new List<RendererInfo>();
		public List<TerrainInfo> m_TerrainInfo = new List<TerrainInfo>();

		void Awake()
		{
			LoadLightmap();
			GameDebug.Log("--------------------------------------------");
			var renderers = GetComponentsInChildren<MeshRenderer>();
			foreach (var item in renderers)
			{
				GameDebug.Log(item.lightmapIndex);
			}

			//		if (m_RendererInfo == null || m_RendererInfo.Count == 0)
			//			return;
			//		
			//		var lightmaps = LightmapSettings.lightmaps;
			//		var combinedLightmaps = new LightmapData[lightmaps.Length + m_Lightmaps.Count];
			//		
			//		lightmaps.CopyTo(combinedLightmaps, 0);
			//		for (int i = 0; i < m_Lightmaps.Count; i++)
			//		{
			//			combinedLightmaps[i+lightmaps.Length] = new LightmapData();
			//			combinedLightmaps[i+lightmaps.Length].lightmapFar = m_Lightmaps[i];
			//		}
			//		
			//		ApplyRendererInfo(m_RendererInfo, lightmaps.Length);
			//		LightmapSettings.lightmaps = combinedLightmaps;
		}

		public void SaveLightmap()
		{
			m_RendererInfo.Clear();

			var renderers = GetComponentsInChildren<MeshRenderer>();
			foreach (MeshRenderer r in renderers)
			{
				if (r.lightmapIndex != -1)
				{
					RendererInfo info = new RendererInfo();
					info.renderer = r;
					info.lightmapOffsetScale = r.lightmapScaleOffset;
					info.lightmapIndex = r.lightmapIndex;

					//                Texture2D lightmap = LightmapSettings.lightmaps[r.lightmapIndex].lightmapFar;
					//
					//                info.lightmapIndex = m_Lightmaps.IndexOf(lightmap);
					//                if (info.lightmapIndex == -1) {
					//                    info.lightmapIndex = m_Lightmaps.Count;
					//                    m_Lightmaps.Add(lightmap);
					//                }

					m_RendererInfo.Add(info);
				}
			}

			var terrains = GetComponentsInChildren<Terrain>();
			foreach (Terrain t in terrains)
			{
				if (t.lightmapIndex != -1)
				{
					TerrainInfo info = new TerrainInfo();
					info.terrain = t;
					info.lightmapOffsetScale = t.lightmapScaleOffset;
					info.lightmapIndex = t.lightmapIndex;

					m_TerrainInfo.Add(info);
				}
			}
		}

		public void LoadLightmap()
		{
			if (m_RendererInfo.Count <= 0) return;

			foreach (var item in m_RendererInfo)
			{
				item.renderer.lightmapIndex = item.lightmapIndex;
				item.renderer.lightmapScaleOffset = item.lightmapOffsetScale;
			}

			foreach (var item in m_TerrainInfo)
			{
				item.terrain.lightmapIndex = item.lightmapIndex;
				item.terrain.lightmapScaleOffset = item.lightmapOffsetScale;
			}
		}
	}
}
