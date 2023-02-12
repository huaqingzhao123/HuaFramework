using UnityEngine;
using System.Collections.Generic;

namespace Nireus
{
    public class ReflectionMonoBehaviour : MonoBehaviour
    {
        [HideInInspector] public List<MonoBehaviour> allMonoBehaviour = new List<MonoBehaviour>();

        void Start()
        {
            GetAllMonoBehaviour();
        }

        public void GetAllMonoBehaviour()
        {
            MonoBehaviour[] MonoBehaviours = transform.GetComponents<MonoBehaviour>();

            for (int i = 0; i < MonoBehaviours.Length; i++)
            {
                if (MonoBehaviours[i] == this)
                {
                    continue;
                }

                allMonoBehaviour.Add(MonoBehaviours[i]);
            }
        }
    }
}
