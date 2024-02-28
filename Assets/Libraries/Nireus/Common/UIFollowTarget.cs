using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nireus
{
    public class UIFollowTarget : MonoBehaviour
    {
        /// <summary>
        /// 3D target that this object will be positioned above.
        /// </summary>

        public Transform target;

        /// <summary>
        /// Game camera to use.
        /// </summary>

        public Camera gameCamera;

        /// <summary>
        /// UI camera to use.
        /// </summary>

        public Camera uiCamera;

        /// <summary>
        /// Whether the children will be disabled when this object is no longer visible.
        /// </summary>

        public bool disableIfInvisible = true;

        Transform mTrans;
        bool mIsVisible = false;
        public Action<bool> OnFollowBecameVisible;
        /// <summary>
        /// Cache the transform;
        /// </summary>

        void Awake() { mTrans = transform; }

        /// <summary>
        /// Find both the UI camera and the game camera so they can be used for the position calculations
        /// </summary>

        void Start()
        {
            if (target != null)
            {
                if (gameCamera == null) gameCamera = Camera.main;//Nireus.Common.FindCameraForLayer(target.gameObject.layer);
                if (uiCamera == null) uiCamera = UICamera.MainUICamera;//  Nireus.Common.FindCameraForLayer(gameObject.layer);
               // SetVisible(true);
            }
            else
            {
                GameDebug.LogError("Expected to have 'target' set to a valid transform", this);
                enabled = false;
            }
        }

        /// <summary>
        /// Enable or disable child objects.
        /// </summary>

        void SetVisible(bool val)
        {
            mIsVisible = val;
            //gameObject.SetActive(val);
            //for (int i = 0, imax = mTrans.childCount; i < imax; ++i)
            //{
            //    mTrans.GetChild(i).gameObject.SetActive(val);
            //}
        }

        /// <summary>
        /// Update the position of the HUD object every frame such that is position correctly over top of its real world object.
        /// </summary>

        void LateUpdate()
        {
            Vector3 pos = gameCamera.WorldToViewportPoint(target.position);

            // Determine the visibility and the target alpha
            bool isVisible = (gameCamera.orthographic || pos.z > 0f) && (!disableIfInvisible || (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f));

            // Update the visibility flag
            if (mIsVisible != isVisible)
            {
                mIsVisible = isVisible;
                if(OnFollowBecameVisible != null)
                    OnFollowBecameVisible(isVisible);
                //SetVisible(isVisible);
            }

            // If visible, update the position
           // if (isVisible)
            {
                UpdatePos(pos);
            }
            OnUpdate(isVisible);
        }


        void UpdatePos(Vector3 pos)
        {
            transform.position = uiCamera.ViewportToWorldPoint(pos);
            pos = mTrans.localPosition;
          //  pos.x = Mathf.FloorToInt(pos.x);
           // pos.y = Mathf.FloorToInt(pos.y);
            pos.z = 0f;
            mTrans.localPosition = pos;
        }
        /// <summary>
        /// Custom update function.
        /// </summary>

        protected virtual void OnUpdate(bool isVisible) { }
    }
}
