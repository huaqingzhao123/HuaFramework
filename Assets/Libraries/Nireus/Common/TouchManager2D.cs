using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nireus
{
    public class TouchManager2D
    {
        public static Vector2 getScreenPosition()
        {
#if UNITY_EDITOR
            return Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IPHONE
            if (Input.touchSupported && Input.touchCount > 0){
			    return Input.GetTouch(0).position;
		    }
		    return Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }
        public static bool getTouchBegan()      //默认是左键点击,对于移动端来说部分左右键，因此就用默认的左键代替一根手指
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonDown(0);
#elif UNITY_ANDROID || UNITY_IPHONE
            if (Input.touchSupported && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began){
			    return true;
		    }
		    return false;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public static bool getTouchEnded()
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonUp(0);
#elif UNITY_ANDROID || UNITY_IPHONE
            if (Input.touchSupported && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended){
			    return true;
		    }
		    return false;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        public static bool getTouchMoved()
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(0);
#elif UNITY_ANDROID || UNITY_IPHONE
            if (Input.touchSupported && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved){
			    return true;
		    }
		    return false;
#else 
            return Input.GetMouseButton(0);
#endif
        }

        private static bool _IsTouchOverUI = false;
        public static bool IsPointerOverUI()
        {
            EventSystem event_sys = EventSystem.current;
#if UNITY_EDITOR
            return event_sys.IsPointerOverGameObject();
#elif UNITY_ANDROID || UNITY_IPHONE
            return IsPointerOverUIForMobile(Input.GetTouch(0).position);
#else
            return event_sys.IsPointerOverGameObject();
#endif
        }

        public static bool IsPointerOverUIForMobile(Vector2 scenePosition)
        {
            PointerEventData eventDataCurPos = new PointerEventData(EventSystem.current);
            eventDataCurPos.position = new Vector2(scenePosition.x, scenePosition.y);
            List<RaycastResult> resultList = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurPos, resultList);
            return resultList.Count > 0;
            //  return event_sys.IsPointerOverGameObject(Input.GetTouch(0).fingerId)
            //&& event_sys.currentSelectedGameObject != null;
        }


        public static Collider2D[] HitTest(Camera camera, Vector2 screen_pt)
        {
            return Physics2D.OverlapPointAll(camera.ScreenToWorldPoint(screen_pt));
        }

        //private static Camera _ui_camera = null;
        //public static void SetUICamera(Camera camera) { _ui_camera = camera; }
        private static Camera _3d_camera = null;
        public static void Set3dCamera(Camera camera) { _3d_camera = camera; }
        private static List<ITouchHandler> _ray_target = new List<ITouchHandler>();
        public static void UpdateTouchCheck()
        {
            if (getTouchBegan())
            {
                Vector2 screen_pt = getScreenPosition();
                if (_ray_target != null)
                {
                    foreach (ITouchHandler handler in _ray_target)
                    {
                        handler.OnTouchEnded(screen_pt);
                    }
                }
                if (!IsPointerOverUI() && _3d_camera != null)
                {
                    TriggerTouchBegan(_3d_camera, screen_pt);
                }
            }
            else if (_ray_target != null)
            {
                if (getTouchMoved())
                {
                    Vector2 screen_pt = getScreenPosition();
                    foreach (ITouchHandler handler in _ray_target)
                    {
                        handler.OnTouchMoved(screen_pt);
                    }
                }
                else if (getTouchEnded())
                {
                    Vector2 screen_pt = getScreenPosition();
                    foreach (ITouchHandler handler in _ray_target)
                    {
                        handler.OnTouchEnded(screen_pt);
                    }
                    _ray_target.Clear();
                }
            }
        }

        private static void TriggerTouchBegan(Camera camera, Vector2 screen_pt)
        {
            _ray_target.Clear();
            Collider2D[] hits = HitTest(camera, screen_pt);
            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit.transform != null)
                {
                    var handler = hit.transform.GetComponent<ITouchHandler>();
                    if (handler != null && _ray_target.Contains(handler) == false)
                    {
                        _ray_target.Add(handler);
                        if (handler.OnTouchBegan(screen_pt))
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}
