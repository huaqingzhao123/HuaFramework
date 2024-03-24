using AliveCell;
using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameroFollowPlayer : MonoBehaviour
{
    private CinemachineBrain mainCamera;
    private Transform player; //玩家位置
    public Vector3 offsetPos; //玩家与相机的位置偏移
    public bool isRefreshBG = true;
    public float PosY = 1;
    public static float leftPos = -5;
    public static float rightPos = 5;
    public float smooth = 1;

    private bool isPause = false;
    private Vector3 _reset_point = new Vector3(-5, -1, -113);
    public Action moveCallBack = null;
    private bool isBossMove = false;
    private Vector3 bossCreatePos = Vector3.zero;
    public void SetFollow(Transform player)
    {
        this.player = player;
    }
    public void SetFollowBoss(Transform boss)
    {
        this.player = boss;
        Vector3 aimPos = player.position + offsetPos;
        Vector3 tempPos = aimPos;
        tempPos.x = Mathf.Clamp(tempPos.x, leftPos, rightPos);
        tempPos.y = PosY;
        transform.position = tempPos;
    }
    public void SetPause(bool isPause)
    {
        this.isPause = isPause;
    }
    public void SetFollowBoss(Vector3 aimPos, Action moveCallBack)
    {
        isBossMove = true;
        this.player = null;
        this.moveCallBack = moveCallBack;
        Vector3 aimPosTo = aimPos + offsetPos;
        Vector3 tempPos = aimPosTo;
        tempPos.x = Mathf.Clamp(tempPos.x, leftPos, rightPos);
        tempPos.y = PosY;
        transform.position = tempPos;
        bossCreatePos = tempPos;
        mainCamera = transform.parent.Find("Main Camera").GetComponent<CinemachineBrain>();
    }
    public void ResetCameraPoint()
    {
        transform.position = _reset_point;
    }
    //只用于BOSS
    private void Update()
    {
        if (isBossMove)
        {
            UpdateFollowPlayer();
        }
    }
    public bool IsCameraArrivePos()
    {
        Vector3 aimPos = Vector3.zero;
        if (isBossMove) // 生成boss  移动到点
        {
            aimPos = bossCreatePos;
        }
        else
        {
            aimPos = player.position + offsetPos;
        }
        Vector3 tempPos = aimPos;
        tempPos.x = Mathf.Clamp(tempPos.x, leftPos, rightPos);

        tempPos.y = PosY;
        float dis = Vector3.Distance(transform.position, tempPos);
        if (dis < 0.01f)
        {
            return true;
        }
        return false;
    }
    public void UpdateFollowPlayer()
    {
        if (isPause) return;
        if (player == null && isBossMove == false) return;
  
        Vector3 aimPos = Vector3.zero;
        if (isBossMove) // 生成boss  移动到点
        {
            aimPos = bossCreatePos;
        }
        else
        {
            aimPos = player.position + offsetPos;
        }
        //Vector3 aimPos = player.position + offsetPos;
        Vector3 tempPos = aimPos;
        tempPos.x = Mathf.Clamp(tempPos.x,leftPos,rightPos);
        
        tempPos.y = PosY;
        transform.position = Vector3.Lerp(transform.position, tempPos, smooth * Time.fixedDeltaTime);
        if (moveCallBack!=null)
        {
            if(mainCamera.IsBlending == false)
            {
                moveCallBack.Invoke();
                moveCallBack = null;
                isBossMove = false;
            }            
        }
        
    }


    
}