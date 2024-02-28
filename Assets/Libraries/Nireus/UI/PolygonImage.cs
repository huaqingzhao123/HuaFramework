using UnityEngine;
using UnityEngine.UI;

namespace Nireus
{
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PolygonImage:Image
    {
        protected PolygonImage()
        {
            useLegacyMeshGeneration = true;
        }

        [HideInInspector]
        [SerializeField]
        private PolygonCollider2D _collider;

        #if UNITY_EDITOR
        //package all时这里会报错，所以加个判断
        protected override void Reset()
        {
            base.Reset();
            _collider = gameObject.AddComponentIfNeeded<PolygonCollider2D>();

            float w = rectTransform.sizeDelta.x * 0.5f + 0.1f;
            float h = rectTransform.sizeDelta.y * 0.5f + 0.1f;

            _collider.points = new Vector2[] {
                new Vector2(-w, -h),
                new Vector2(w, -h),
                new Vector2(w, h),
                new Vector2(-w, h),
            };
        }
        #endif

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            return _collider.OverlapPoint(eventCamera.ScreenToWorldPoint(screenPoint));
        }
    }
}
