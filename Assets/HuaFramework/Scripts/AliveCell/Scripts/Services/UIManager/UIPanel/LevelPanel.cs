/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/24 16:35:15
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// LevelPanel
    /// </summary>
    public class LevelPanel : UIPanel
    {
        public override string panelName => "Level";

        public override int sortWeight => 1;

        public void OnChangeLevelButton(int levelID)
        {
            App.Trigger(EventTypes.UI_ChangeLevel, levelID);
        }

        public void OnBackButton()
        {
            App.Trigger(EventTypes.UI_LevelListBack);
        }

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.Enter when enterOrExit:
                    //App.camera.fsm.ChangeState<CameraStates.MW.LevelListDisplay>();
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    App.camera.fsm.ChangeState<CameraStates.None>();
                    break;
            }
        }
    }
}