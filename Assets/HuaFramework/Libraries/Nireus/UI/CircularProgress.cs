using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nireus
{
    public abstract class CircularProgressExtension : MonoBehaviour
    {
        public abstract void onValueChanged(float min, float max, float value);
    }

    [ExecuteInEditMode]
    [RequireComponent(typeof(Image))]
    public class CircularProgress: MonoBehaviour
    {
        public CircularProgressExtension extesion = null;

        [Range(0, 1)]
        public float min = 1;


        [Range(0, 1)]
        public float max = 1;



        [Range(0, 1)]
        public float value = 0;

        private Image image = null;
        private Color image_color = Color.white;

        private bool color_change = false;

        private void Start()
        {
            image = gameObject.GetComponent<Image>();
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Radial360;
            if(color_change)
            {
                image.color = image_color;
            }
        }

        public void SetColor(Color color)
        {
            if (image != null)
            {
                image.color = color;
            }
            else
            {
                color_change = true;
                image_color = color;
            }
        }

        private float _last_value = -1;

        private void Update()
        {
            if (_last_value == value)
            {
                return;
            }
            _last_value = value;
            image.fillAmount = min + (max - min) * value;
            if(extesion != null)
            {
                extesion.onValueChanged(min, max, value);
            }
            
        }
    }
}
