/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/10/31 15:08:22
 */

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// MatchingPanel
    /// </summary>
    public class MatchingPanel : UIPanel
    {
        public override string panelName => "Matching";

        public override int sortWeight => 2;

        [SerializeField]
        protected Text _title;

        [SerializeField]
        protected CanvasGroup _glow;

        public void OnCancelMatchingButton()
        {
            App.Trigger(EventTypes.UI_CancelMatch);
        }

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.None:
                    break;

                case UIPanelStatus.Enter when enterOrExit:
                    {

                        string oldTest = _title.text;
                        _title.DOText("...", 0.85f, true).SetLoops(-1, LoopType.Yoyo).SetRelative().OnKill(() => { _title.text = oldTest; });
                        _glow.DOFade(1f, 0.85f).ChangeStartValue(0f).SetLoops(-1, LoopType.Yoyo);
                    }
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    {
                        App.camera.fsm.ChangeState<CameraStates.None>();
                        _title.DOKill(true);
                        _glow.DOKill(true);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}