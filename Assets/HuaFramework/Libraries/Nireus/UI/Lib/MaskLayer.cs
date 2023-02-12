using UnityEngine;

namespace Nireus
{
	public class MaskLayer : MonoBehaviour
	{
		private static MaskLayer _instance;
		public static MaskLayer getInstance() { return _instance; }

		void Awake()
		{
			_instance = this;
		}

		public void active()
		{
			gameObject.SetActive(true);
		}

		public void unactive()
		{
			gameObject.SetActive(false);
		}
	}
}
