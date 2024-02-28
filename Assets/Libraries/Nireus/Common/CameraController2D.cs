using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nireus
{
    public class CameraController2D //: SingletonBehaviour<CameraController2D>
    {

        public class CameraParas
        {
            public float _cameraSize;
            public float _cameraSizeDefault = 5f;
            public float _cameraSizeMin = 3f;
            public float _cameraSizeMax = 6f;
            public float _minX;
            public float _maxX;
            public float _minY;
            public float _maxY;
            public float _zoomSensitivityMul = 4.6f;
            public float _moveSensitivityMul = 1f;
            public float _moveSpeedMul = 20f;
            public float _zoomSpeedMul = 20f;
            public bool _supportZoom;
            public bool freeBounds;
            public Vector3 position;

            public void CopyTo(CameraParas target)
            {
                target._cameraSize = _cameraSize;
                target._cameraSizeDefault = _cameraSizeDefault;
                target._cameraSizeMin = _cameraSizeMin;
                target._cameraSizeMax = _cameraSizeMax;
                target._minX = _minX;
                target._maxX = _maxX;
                target._minY = _minY;
                target._maxY = _maxY;
                target._zoomSensitivityMul = _zoomSensitivityMul;
                target._moveSensitivityMul = _moveSensitivityMul;
                target._moveSpeedMul = _moveSpeedMul;
                target._zoomSpeedMul = _zoomSpeedMul;
                target._supportZoom = _supportZoom;
                target.freeBounds = freeBounds;
                target.position = position;
            }
        }
        struct TimeVector3
        {
            public Vector3 pos;
            public float time;
            public TimeVector3(Vector3 pos, float time)
            {
                this.pos = pos;
                this.time = time;
            }
        }


        protected float _positionZ = -50f;

        protected Camera _camera = null;
        protected GameObject dragObject;
        protected Transform _cam_transform;
        public Transform camTransform { get { return _cam_transform; } }
        protected Transform followTransform;

        private bool _dragging = false;
        private Vector2 _last_pos = Vector2.zero;
        private Touch _oldTouch1;
        private Touch _oldTouch2;



        protected Vector3 _offset;
        public bool enabledCtrl { set; get; }

        public Rect screenWorldBox2D
        {
            get; private set;
        }
        public Transform targetTransform
        {
            get { return dragObject.transform; }
        }
        private List<TimeVector3> mousePosList = new List<TimeVector3>();
        protected Vector3 inertia;
        private float inertiaScale = 4.5f;
        private float inertiaDamping = 50f;


        public CameraParas paras { get; private set; }
        CameraParas parasSave;


        public void Init(Camera camera, float minX, float maxX, float minY, float maxY,
            float orthographicSize = 6.4f, float orthographicSizeMin = 6.4f, float orthographicSizeMax = 6.4f,
            float moveSensitivityMul = 1f, float zoomSensitivityMul = 4.51f, float moveSpeedMul = 20f, float zoomSpeedMul = 20f
            )
        {
            this._camera = camera;
            _offset = Vector3.zero;
            enabledCtrl = true;
            paras = new CameraParas();
            parasSave = new CameraParas();
            paras._cameraSizeDefault = orthographicSize;
            paras._cameraSize = orthographicSize;
            paras._cameraSizeMin = orthographicSizeMin;
            paras._cameraSizeMax = orthographicSizeMax;
            paras._supportZoom = orthographicSizeMin != 0 && orthographicSizeMax != 0;
            _camera.orthographicSize = paras._cameraSize;
            paras._moveSensitivityMul = moveSensitivityMul;
            paras._zoomSensitivityMul = zoomSensitivityMul;
            paras._moveSpeedMul = moveSpeedMul;
            paras._zoomSpeedMul = zoomSpeedMul;

            paras._minX = minX;// - _fieldMaxX / 2f;
            paras._maxX = maxX;
            paras._minY = minY;// - _fieldMaxY / 2f;
            paras._maxY = maxY;// - _minY;
            paras.freeBounds = false;

            if (_camera == null)
                _camera = Camera.main;

            if (dragObject == null)
                dragObject = new GameObject("DragTarget");

            _cam_transform = _camera.transform;
            dragObject.transform.position = new Vector3(0, 0, _positionZ);
            _dragging = false;
            _last_pos = Vector2.zero;
            inertia = Vector3.zero;
            _CalaScreenToWorldRect();
            paras.CopyTo(parasSave);
        }


        void _CalaScreenToWorldRect()
        {
            //minY    right
            //[       ]
            //left     maxX
            //Vector3 left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 10));
            //Vector3 right = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 10));
            //Vector3 minY = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, 10));
            //Vector3 minX = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, 10));
            Vector3 left = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, 0));
            Vector3 right = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
            screenWorldBox2D = new Rect(left.x, left.y, right.x, right.y);
        }


        public void FocusPositionImmediately(Vector2 worldPosition)
        {
            FocusPosition(new Vector3(worldPosition.x, worldPosition.y, _positionZ), true);
        }
        public void FocusPosition(Transform followTransform, bool immediately = true)
        {
            this.followTransform = followTransform;
        }

        public float GetCameraNowSize()
        {
            return this._camera.orthographicSize;
        }

        public void ResumeFocus(bool immediately = true)
        {
            float cameraSize = this.paras._cameraSize;
            Vector3 worldPos = this.paras.position;
            if (followTransform != null)
            {
                FocusPosition(this.followTransform, immediately);
                SetCameraZoom(cameraSize, immediately);
            }
            else
            {
                FocusPosition(worldPos, cameraSize, immediately);
            }
        }

        public void FocusPosition(Vector3 worldPosition, float cameraZoom, bool immediately = true, bool freeBounds = false)
        {
            FocusPosition(worldPosition, immediately, freeBounds);
            SetCameraZoom(cameraZoom, immediately);
        }

        public void FocusPosition(Vector3 worldPosition, bool immediately = true, bool freeBounds = false)
        {
            followTransform = null;
            Vector3 nextPosition = worldPosition;
            inertia = Vector3.zero;
            Vector3 moveTo;
            paras.freeBounds = freeBounds;
            if (freeBounds)
            {
                moveTo = worldPosition;
            }
            else
            {
                moveTo = ApplyBoundsForDragObj(worldPosition);
            }

            dragObject.transform.position = new Vector3(moveTo.x, moveTo.y, _positionZ);
            if (immediately)
            {
                _camera.transform.position = dragObject.transform.position + _offset;
            }

        }
        public void Save(bool savePosOnly = false)
        {
            paras.position = this.dragObject.transform.position;
            paras._cameraSize = this.GetCameraNowSize();
            paras.CopyTo(parasSave);
        }
        public void SavePos()
        {
            parasSave.position = this.dragObject.transform.position;
        }

        public void Load()
        {
            parasSave.CopyTo(paras);
        }


        public virtual Vector3 ApplyBoundsForDragObj(Vector3 worldPosition)
        {
            Vector3 moveTo = worldPosition;
            var cameraHalfWidth = this._camera.orthographicSize * ((float)Screen.width / Screen.height);

            moveTo.x = Mathf.Clamp(moveTo.x, paras._minX + cameraHalfWidth, paras._maxX - cameraHalfWidth);
            moveTo.y = Mathf.Clamp(moveTo.y, paras._minY + this._camera.orthographicSize, paras._maxY - this._camera.orthographicSize);
            return moveTo;
        }

        public virtual Vector3 ApplyBoundsForCamera(Vector3 worldPosition)
        {
            Vector3 moveTo = new Vector3(dragObject.transform.position.x, dragObject.transform.position.y, dragObject.transform.position.z);

            var cameraHalfWidth = this._camera.orthographicSize * ((float)Screen.width / Screen.height);
            moveTo.x = Mathf.Clamp(moveTo.x, paras._minX + cameraHalfWidth, paras._maxX - cameraHalfWidth);
            moveTo.y = Mathf.Clamp(moveTo.y, paras._minY + this._camera.orthographicSize, paras._maxY - this._camera.orthographicSize);
            return moveTo;
        }


        public void SetCameraZoom(float cameraSize, bool immediately)
        {
            paras._cameraSize = Mathf.Clamp(cameraSize, paras._cameraSizeMin, paras._cameraSizeMax);
            if (immediately)
            {
                _camera.orthographicSize = paras._cameraSize;
            }
        }

        void _UpdateZoom(float speed)
        {
            if (paras._supportZoom == false) return;
            var nextSize = _camera.orthographicSize + speed;
            paras._cameraSize = Mathf.Clamp(nextSize, paras._cameraSizeMin, paras._cameraSizeMax);
        }


        protected virtual void UpdateDrag(Vector3 moveToDelta)
        {
            Vector3 moveToDelta2D = moveToDelta * paras._moveSensitivityMul;

            Vector3 moveTo = dragObject.transform.position + moveToDelta2D;

            var cameraHalfWidth = this._camera.orthographicSize * ((float)Screen.width / Screen.height);

            moveTo.x = Mathf.Clamp(moveTo.x, paras._minX + cameraHalfWidth, paras._maxX - cameraHalfWidth);
            moveTo.y = Mathf.Clamp(moveTo.y, paras._minY + this._camera.orthographicSize, paras._maxY - this._camera.orthographicSize);

            dragObject.transform.position = new Vector3(moveTo.x, moveTo.y, _positionZ);
        }


        public void Update()
        {
            if (_cam_transform == null) return;
            Debug.DrawLine(_cam_transform.position, dragObject.transform.position, Color.blue);
            if (enabledCtrl)
            {
                if (InputManager.getTouchCount() == 1)
                {
                    if (_dragging == false && InputManager.getTouchDown())
                    {
                        if (!InputManager.IsPointerOverUI())
                        {
                            _dragging = true;
                            _last_pos = InputManager.getScreenPosition();

                            inertia = Vector3.zero;
                            mousePosList.Clear();
                        }
                    }

                    else if (InputManager.getTouchMoved() && _dragging)
                    {
                        Vector2 curPos = InputManager.getScreenPosition();
                        Vector3 p1 = this._camera.ScreenToWorldPoint(new Vector3(_last_pos.x, _last_pos.y, 0));
                        Vector3 p2 = this._camera.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y, 0));
                        mousePosList.Add(new TimeVector3(curPos, Time.realtimeSinceStartup));
                        UpdateDrag(p1 - p2);
                        _last_pos = InputManager.getScreenPosition();
                    }
                    else if (InputManager.getTouchUp())
                    {
                        _last_pos = InputManager.getScreenPosition();
                        if (_dragging)
                        {
                            Vector3 nowMousePoint = new Vector3(_last_pos.x, _last_pos.y, -Camera.main.transform.position.z);
                            inertia = (Camera.main.ScreenToWorldPoint(nowMousePoint) - Camera.main.ScreenToWorldPoint(GetPrevMousePos())) * inertiaScale;
                        }
                        _dragging = false;
                    }
                }
                else if (Input.GetAxis("Mouse ScrollWheel") != 0)//zoom out
                {
                    _UpdateZoom((Input.GetAxis("Mouse ScrollWheel") > 0 ? -1f : 1));
                }

                if (_dragging && InputManager.getTouchUp())
                {
                    _dragging = false;
                    _last_pos = InputManager.getScreenPosition();
                }

                if (Input.touchCount > 1)
                {
                    Touch newTouch1 = Input.GetTouch(0);
                    Touch newTouch2 = Input.GetTouch(1);
                    _dragging = false;
                    _last_pos = newTouch2.position;
                    if (newTouch2.phase == TouchPhase.Began)
                    {
                        _oldTouch1 = newTouch1;
                        _oldTouch2 = newTouch2;
                        inertia = Vector3.zero;
                    }
                    else if (newTouch1.phase == TouchPhase.Moved || newTouch2.phase == TouchPhase.Moved)
                    {
                        _last_pos = newTouch2.position;
                        float oldDistance = Vector2.Distance(_oldTouch1.position, _oldTouch2.position);
                        float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);
                        float scaleFactor = (newDistance - oldDistance) * -paras._zoomSensitivityMul;
                        _UpdateZoom(scaleFactor);
                    }
                    _oldTouch1 = newTouch1;
                    _oldTouch2 = newTouch2;
                }
            }

            UpdateFollow();

            if (paras._cameraSize != _camera.orthographicSize)
            {
                _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, paras._cameraSize, Time.deltaTime * paras._zoomSpeedMul);
            }

            UpdatePosition();

            _CalaScreenToWorldRect();

        }

        protected Vector3 UpdateInertia()
        {
            if (inertia != Vector3.zero)
            {
                Vector3 speed = -inertia * Time.deltaTime;
                if (Mathf.Abs(inertia.y) > 0.5f || (Mathf.Abs(inertia.x) > 0.5f))
                    inertia -= (inertia / inertiaDamping);
                else
                    inertia = Vector3.zero;
                return new Vector3(speed.x, speed.y, 0);
            }
            return Vector3.zero;
        }


        protected virtual void UpdatePosition()
        {
            Vector3 inertiaSpeed = UpdateInertia();
            dragObject.transform.position += inertiaSpeed;

            Vector3 moveTo;
            if (paras.freeBounds == false)
            {
                moveTo = ApplyBoundsForCamera(dragObject.transform.position);
            }
            else
                moveTo = dragObject.transform.position;

            _camera.transform.position = Vector3.Lerp(_camera.transform.position, moveTo, Time.deltaTime * paras._moveSpeedMul) + _offset;
        }

        void UpdateFollow()
        {
            if (followTransform != null)
            {
                Vector3 moveTo = followTransform.position;
                var cameraHalfWidth = this._camera.orthographicSize * ((float)Screen.width / Screen.height);

                moveTo.x = Mathf.Clamp(moveTo.x, paras._minX + cameraHalfWidth, paras._maxX - cameraHalfWidth);
                moveTo.y = Mathf.Clamp(moveTo.y, paras._minY + this._camera.orthographicSize, paras._maxY - this._camera.orthographicSize);

                dragObject.transform.position = new Vector3(moveTo.x, moveTo.y, _positionZ);
            }
        }

        public void SetCameraOffset(Vector3 vector3)
        {
            _offset = vector3;
        }


        Vector3 GetPrevMousePos()
        {
            float time = Time.realtimeSinceStartup - 0.1f;
            for (int i = 0; i < mousePosList.Count; i++)
            {
                TimeVector3 v = mousePosList[i];
                if (v.time >= time)
                {
                    return v.pos;
                }
            }
            if (mousePosList.Count == 0)
            {
                return InputManager.getScreenPosition();
            }
            else
            {
                return mousePosList[mousePosList.Count - 1].pos;
            }

        }

        public Vector2 GetCameraPosition()
        {
            return _camera.transform.position;
        }

        public Vector3 GetCameraLocalScale()
        {
            return _camera.transform.localScale;
        }
    }
}
