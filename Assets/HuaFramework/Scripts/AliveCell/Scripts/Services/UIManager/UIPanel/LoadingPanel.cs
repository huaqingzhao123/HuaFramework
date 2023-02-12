/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/8 12:46:50
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;
using DG.Tweening;
using UnityEngine.UI;

namespace AliveCell
{
    /// <summary>
    /// LoadingPanel
    /// </summary>
    public class LoadingPanel : UIPanel
    {
        public override string panelName => "Loading";

        public override int sortWeight => 999;

        [Header("Icon")] [SerializeField] protected Image iconImage;
        [SerializeField] protected RectTransform iconRect;
        [SerializeField] protected float flashSpeed;
        [SerializeField] protected AnimationCurve flashCurve;

        [Header("Mask")] [SerializeField] protected RectTransform upMask;
        [SerializeField] protected RectTransform downMask;
        [SerializeField] protected float yScaleTime;
        [SerializeField] protected float exitScaleTime;
        [SerializeField] protected AnimationCurve yScaleCurve;

        public override void OnStateChange(UIPanelStatus status, bool enterOrExit)
        {
            base.OnStateChange(status, enterOrExit);

            switch (status)
            {
                case UIPanelStatus.None:
                    break;

                case UIPanelStatus.Enter when enterOrExit:
                    {
                        canvasGroup.alpha = 1;
                        canvasGroup.blocksRaycasts = true;
                        canvasGroup.interactable = true;

                        upMask.localScale = Vector3.one;
                        downMask.localScale = Vector3.one;

                        Sequence seq = DOTween.Sequence();
                        seq.Append(iconImage.DOFade(1f, flashSpeed / 2.0f).ChangeStartValue(Color.clear).SetLoops(2, LoopType.Yoyo));
                        seq.Join(iconRect.DOScale(Vector3.one * 1.1f, flashSpeed / 2.0f).ChangeStartValue(Vector3.one).SetLoops(2, LoopType.Yoyo));
                        seq.OnComplete(() =>
                        {
                            if (this.status != UIPanelStatus.Exit)
                            {
                                seq.Restart();
                                seq.Play();
                            }
                            else
                            {
                                seq.OnComplete(null);
                            }
                        });
                        seq.Play();
                    }
                    break;

                case UIPanelStatus.Exit when enterOrExit:
                    {
                        Sequence seq = BeginTween();
                        seq.AppendInterval(flashSpeed);
                        seq.Append(upMask.DOScaleY(0, yScaleTime).SetEase(yScaleCurve));
                        seq.Join(downMask.DOScaleY(0, yScaleTime).SetEase(yScaleCurve));
                        seq.Join(canvasGroup.DOFade(0.0f, yScaleTime * exitScaleTime).SetDelay(yScaleTime * (1.0f - exitScaleTime)));
                        seq.OnComplete(() =>
                        {
                            canvasGroup.alpha = 0f;
                            canvasGroup.blocksRaycasts = false;
                            canvasGroup.interactable = false;
                        });
                        EndTween(seq);
                    }
                    break;

                default:
                    break;
            }
        }
    }
}