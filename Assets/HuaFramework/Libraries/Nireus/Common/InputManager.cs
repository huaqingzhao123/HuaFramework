using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Nireus
{
    public class InputManager
    {
        private static Vector2 prevClickPos;
        public static int getTouchCount()
        {
#if UNITY_EDITOR
            return (Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0)|| Input.GetMouseButton(0)) ? 1 : 0;
#elif UNITY_ANDROID || UNITY_IPHONE
 			return Input.touchCount;			
#else
            return Input.GetMouseButton(0) ? 1 : Input.GetMouseButton(0) && Input.GetMouseButton(1) ? 2 : 0;
#endif
        }


        public static Vector2 getLastTouchPosition()
        {
#if UNITY_EDITOR
            return Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IPHONE
			//if(Input.touchSupported){
			    return Input.GetTouch(Input.touchCount - 1).position;
			//}
			//return Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }

        public static Vector2 getMoveOffset()
        {
#if UNITY_EDITOR
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#elif UNITY_ANDROID || UNITY_IPHONE
			if(Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved){
			return Input.GetTouch(0).deltaPosition;
			}
			return Vector2.zero;
#else
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
        }

        public static Vector2 getScreenPosition()
        {
#if UNITY_EDITOR
			return Input.mousePosition;
#elif UNITY_ANDROID || UNITY_IPHONE
			if(Input.touchCount == 1){
			return Input.GetTouch(0).position;
			}
			return Vector2.zero;
#else
            return Input.mousePosition;
#endif
        }


        public static float getScaleOffset(ref float pre_distance)
        {
#if UNITY_EDITOR
			return Input.mouseScrollDelta.y;
#elif UNITY_ANDROID || UNITY_IPHONE
			float offset = 0.0f;
			if( Input.multiTouchEnabled && Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved && Input.GetTouch(0).phase == TouchPhase.Moved){      //限定两个手指才能缩放
			Touch touch_one = Input.GetTouch(0);
			Touch touch_two = Input.GetTouch(1);
			float distance = Vector2.Distance(touch_one.position, touch_two.position);
			float offset_dis = Vector2.Distance(touch_one.deltaPosition, touch_two.deltaPosition);
			if(distance > pre_distance){
			offset = offset_dis;
			}else{
			offset = -offset_dis;
			}
			pre_distance = distance;
			}
			return offset;
#else
            return Input.mouseScrollDelta.y;
#endif
        }

        public static bool getTouchDown()      //默认是左键点击,对于移动端来说部分左右键，因此就用默认的左键代替一根手指
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonDown(0);
#elif UNITY_ANDROID || UNITY_IPHONE
			if(Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began){            
			    return true;
			}
			return false;
#else
            return Input.GetMouseButtonDown(0);
#endif
        }

        public static bool getTouchMoved()      //默认是左键点击,对于移动端来说部分左右键，因此就用默认的左键代替一根手指
        {
#if UNITY_EDITOR
            return Input.GetMouseButton(0);
#elif UNITY_ANDROID || UNITY_IPHONE
			if(Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved){
			    return true;
			}
			return false;
#else
            return Input.GetMouseButton(0);
#endif
        }

        public static bool getTouchUp()
        {
#if UNITY_EDITOR
            return Input.GetMouseButtonUp(0);
#elif UNITY_ANDROID || UNITY_IPHONE
			if(Input.touchCount == 1 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled)){
			    return true;
			}
			return false;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        public static bool getTouchClickUp()
        {
#if UNITY_EDITOR
            if (Input.GetMouseButtonDown(0))
            {
                prevClickPos = Input.mousePosition;
            }
            return Input.GetMouseButtonUp(0) && Vector2.Distance( prevClickPos ,Input.mousePosition) < 10f;
#elif UNITY_ANDROID || UNITY_IPHONE
            if(Input.touchCount == 1 && (Input.GetTouch(0).phase == TouchPhase.Began)){
                prevClickPos = Input.mousePosition;
            }
			if(Input.touchCount == 1 && (Input.GetTouch(0).phase == TouchPhase.Ended)){
			    return Vector2.Distance( Input.GetTouch(0).position, prevClickPos) < 10f;
			}
			return false;
#else
            return Input.GetMouseButtonUp(0);
#endif
        }

        public static int getTouchAnyUp()
        {
#if UNITY_EDITOR
			return Input.GetMouseButtonUp(0) ? 0 : -1;
#elif UNITY_ANDROID || UNITY_IPHONE
                for (int i = 0; i < Input.touchCount; i++)
                {
                    var item = Input.GetTouch(i);
                    if (item.phase == TouchPhase.Ended || item.phase == TouchPhase.Canceled)
                    {
                        return i;
                    }
                }
                return -1;
#else
            return Input.GetMouseButtonUp(0) ? 0 : -1;
#endif
        }


        public static void Update()
        {
            if (getTouchDown())
            {

            }
        }

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

    }
}