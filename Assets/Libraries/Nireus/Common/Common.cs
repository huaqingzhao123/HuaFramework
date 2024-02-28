
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Nireus
{
	public static class Common
    {
        public static UnityEngine.Font GameUIFont = UnityEngine.Resources.Load("GameFontYaHei", typeof(UnityEngine.Font)) as UnityEngine.Font;
        public static UnityEngine.Font GameBoldFont = UnityEngine.Resources.Load("FZZhengHeiFont", typeof(UnityEngine.Font)) as UnityEngine.Font;
        public static UnityEngine.Font GameSkillFont = UnityEngine.Resources.Load("skillNameXml", typeof(UnityEngine.Font)) as UnityEngine.Font;
        public static bool hasChinese(string s)
		{
			return Regex.IsMatch(s, @"[\u4e00-\u9fa5]");
		}

        public static Camera FindCameraForLayer(int layer)
        {
            int layerMask = 1 << layer;

            Camera cam;

            cam = Camera.main;
            if (cam && (cam.cullingMask & layerMask) != 0) return cam;

            Camera[] cameras = new Camera[Camera.allCamerasCount];
            int camerasFound = Camera.GetAllCameras(cameras);
            for (int i = 0; i < camerasFound; ++i)
            {
                cam = cameras[i];
                if (cam && cam.enabled && (cam.cullingMask & layerMask) != 0)
                    return cam;
            }
            return null;
        }

    }
}
