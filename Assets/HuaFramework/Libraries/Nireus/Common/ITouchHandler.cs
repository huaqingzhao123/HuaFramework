using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Nireus
{
    public interface ITouchHandler
    {
        bool OnTouchBegan(Vector2 screen_pt);
        void OnTouchMoved(Vector2 screen_pt);
        void OnTouchEnded(Vector2 screen_pt);

        // void OnTouchCancelled(Vector2 screen_pt);
    }
}
