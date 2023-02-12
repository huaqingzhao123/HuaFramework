using UnityEngine;

namespace Nireus
{
    public class AnimLayoutGroupAlpha : AnimLayoutGroupBase
    {
        public float alpha_default = 0;
        
        protected override void _clearData()
        {
            base._clearData();
        }

        protected override void _addAnimationChild(Transform child)
        {
            base._addAnimationChild(child);

            var canvas_group = child.GetComponent<CanvasGroup>();
            if (canvas_group != null)
            {
                canvas_group.alpha = alpha_default;
            }
        }
    }
}
