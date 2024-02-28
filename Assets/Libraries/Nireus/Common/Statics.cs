using UnityEngine;
using System.Collections;

namespace Nireus
{
	public class Statics : MonoBehaviour
	{
		void Awake ()
		{
			//useGUILayout = false;
		}

		private GUIStyle _label_style;
		// Use this for initialization
		void Start ()
		{
			//Application.targetFrameRate = 60;
			fpstime = 0.0f;
			fps = 0;
			fpstick = 0;
			_label_style = new GUIStyle();
			_label_style.fontSize = 40;
			_label_style.normal.textColor = Color.white;;
		}
		
		// Update is called once per frame
		void Update ()
		{
			fpstime += Time.deltaTime;
			fpstick++;
			
			if (fpstime > 1.0f) {
				fps = fpstick;
				fpstime = 0.0f;
				fpstick = 0;	
			}
		}
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		void OnGUI ()
		{
			float y = Screen.height;
			sb.AppendFormat ("FPS:{0}", fps);
			y -= 60;
			
			GUI.Label (new Rect (0, y, Screen.width, 60), sb.ToString(),_label_style);
			sb.Remove (0, sb.Length);
		}
		
		private int	fps = 0;
		private float fpstime = 0;
		private int fpstick = 0;
	}
}