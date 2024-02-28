using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Nireus
{
    public class UIDialog : UITemplate
    {
        protected object _show_param;
        Animator _animator = null;

        protected override void OnAwake()
        {
            base.OnAwake();

            var ui_binder = gameObject.GetComponent<UIBinder>();
            if (ui_binder == null) return;
            var root = transform.Find("Root");
            _animator = root?.GetComponent<Animator>();
        }

        public virtual void OnShowDialog(System.Object param = null)
        {
            if (param != null) _show_param = param;
            if (IsShow()) return;
          
            SetVisible(true);
            PlayOpenSound();
        }

        protected virtual void PlayOpenSound()
        {
           // AkSoundEngine.PostEvent("dialog_open", this.gameObject);
        }


        public virtual void OnHideDialog()
        {
            if (!IsShow()) return;

            LayerManager.getInstance().addToLayer(this, LayerType.HIDE);
            SetVisible(false);
		}


        public virtual void CloseDialog()
        {
            if (this == null)
            {
                return;
            }
            //PopUpManager.Instance.RemovePopUp(this);
            /*
            if (_animator != null && _animator.enabled)
            {
                _animator.Play("close_dialog",() =>
                    {
                         PopUpManager.Instance.RemovePopUp(this);
                    });
               
            }
            else
            {
                PopUpManager.Instance.RemovePopUp(this);
            }
            */
           
        }
        private IEnumerator _CloseDialogImpl()
        {
            yield return new WaitForSeconds(0.17f);
            //PopUpManager.Instance.RemovePopUp(this);
        }
        public static void ShowInPopUpView()
        {

        }

        public void SetCameraEnabled(bool enabled)
        {
            //var cam = PopUpManager.Instance.GetUICamera(this);
            //if (cam != null)
            //{
            //    cam.enabled = enabled;
            //}
        }
    }
}
