using UnityEngine;

namespace Nireus
{
	public class UIScene : UITemplate
	{
        protected object _show_param;
      
        public virtual void OnShowScene(System.Object param = null)
        {
            if (IsShow()) return;
            if (param != null) _show_param = param;
            LayerManager.getInstance().addToLayer(this, LayerType.Scene);
            SetVisible(true);
        }

        public virtual void OnHideScene()
        {
            if (!IsShow()) return;
            LayerManager.getInstance().addToLayer(this, LayerType.HIDE);
            SetVisible(false);
		}
        
        public virtual void OnPush() {      }
        public virtual void OnPushEnd(System.Object param = null) { }
		public virtual void OnPop() { }
        public virtual void OnTop() { }
        public virtual void OnLeaveTop() { }
    }
}
