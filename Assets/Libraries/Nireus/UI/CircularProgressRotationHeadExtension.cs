using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nireus
{
    public class CircularProgressRotationHeadExtension : CircularProgressExtension
    {
        public GameObject head = null;

        public float start_rotation = 0;

        public bool dir = true;

        public override void onValueChanged(float min, float max, float value)
        {
            if (head == null)
            {
                return;
            }
            head.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, start_rotation + (dir ? 1 : -1) * (min + (max - min) * value * 360)));
        }
    }

}
