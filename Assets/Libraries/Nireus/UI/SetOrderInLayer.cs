using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Nireus
{

    public class SetOrderInLayer : MonoBehaviour
    {
        public int order_in_layer = 0;

        [ContextMenu("更新")]
        public void doSet()
        {
            var childs = this.gameObject.GetComponentsInChildren<ParticleSystem>();

            foreach(var child in childs)
            {
                var render = child.GetComponent<Renderer>();

                if(render)
                {
                    render.sortingOrder = order_in_layer;
                }
            }
        }

    }
}
