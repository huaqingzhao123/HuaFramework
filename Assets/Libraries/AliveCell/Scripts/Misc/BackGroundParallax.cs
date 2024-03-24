using System;
using System.Collections;
using System.Collections.Generic;
using AliveCell;
using UnityEngine;

[Serializable]
public class BackGroundData
{
    public Transform transform;
    [Header("相机移动插值 * 缩放比例 * 这个 ")]
    public float parallaxScaleReductionFactor;
    [Header("初始位置偏移")]
    public float oriOffset;
    
}
public class BackGroundParallax : MonoBehaviour
{

    public BackGroundData[] backgrounds;//= new List<BackGroundData>();//所有的背景层，这里，离玩家最近的背景的下标i最小，当i足够大时，背景和相机是一起位移的，比如真实世界里，人走动的时候，周围的房屋都向后移动，而天空中的星星却看起来没有移动过
    [Header("插值缩放比例")]
    public float parallaxScale;//相对相机的移动补偿的比例值
    //public float parallaxScaleReductionFactor;//对于不同的背景层的移动补偿的差值比例
    public float smoothing;

    private Transform cam;
    private Vector3 previousCamPos;//相机在上一帧的位置

    private Vector3 oriCameraPos;
    //private CinemachineVirtualCamera cam;
    
// Use this for initialization
    // void Start () {
    //     cam = Camera.main.transform;
    //     previousCamPos = cam.position;
    // }
    private List<Vector3> oriPos = new List<Vector3>();
    protected bool _bool_record = false;
    private void Awake()
    {
        if (_bool_record == false)
        {
            oriPos.Clear();
            for (int i = 0; i < backgrounds.Length; i++)
            {
                oriPos.Add(backgrounds[i].transform.position);
            }
            _bool_record = true;
        }
        
    }
    public void SetInitCamera()
    {
        for (int i = 0; i < oriPos.Count; i++)
        {
            backgrounds[i].transform.position = oriPos[i];
        }
        OffsetBg(0);
    }
    public void SetCamera(Transform camera)
    {
        
        cam = camera;
        //previousCamPos = cam.position;
        oriCameraPos = cam.position;
        
    }
    public void UpdateBG()
    {
        if (cam == null)
        {
            return;
        }
        //if (App.game.gamePassStageState == GamePassStageState.ShowEnterStage
        //    || App.game.gamePassStageState == GamePassStageState.ShowPassStageOver)
        //{
        //    return;
        //}
        Vector3 targetPos = cam.position;
        if (targetPos.x < CameroFollowPlayer.leftPos)
        {
            targetPos.x = CameroFollowPlayer.leftPos;
        }
        else if (targetPos.x > CameroFollowPlayer.rightPos)
        {
            targetPos.x = CameroFollowPlayer.rightPos;
        }
        targetPos.x = Mathf.Clamp(targetPos.x, CameroFollowPlayer.leftPos, CameroFollowPlayer.rightPos);
        oriCameraPos.x = Mathf.Clamp(oriCameraPos.x, CameroFollowPlayer.leftPos, CameroFollowPlayer.rightPos);
        
        //float parallax = (previousCamPos.x - cam.position.x) * parallaxScale;//根据相机的单帧位移获得补偿值大小
        float offetCameraPosX = targetPos.x - oriCameraPos.x;
        OffsetBg(offetCameraPosX);
        
    }
    protected void OffsetBg(float offetCameraPosX)
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            //获得位移的x值，i*parallaxScaleReductionFactor得到不同背景层的补偿差
            float backgroundTargetPosX = oriPos[i].x + offetCameraPosX * backgrounds[i].parallaxScaleReductionFactor;

            Vector3 backgroundTargetPos = new Vector3(backgroundTargetPosX, backgrounds[i].transform.position.y, backgrounds[i].transform.position.z);

            Vector3 tempPos = backgroundTargetPos - new Vector3(backgrounds[i].oriOffset, 0, 0);

            
            backgrounds[i].transform.position = Vector3.Lerp(backgrounds[i].transform.position, tempPos, smoothing * Time.deltaTime);
            


        }
    }
}