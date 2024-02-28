using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Nireus
{
    public class AnimationEventListener : MonoBehaviour
    {
        public event UnityAction<string> onAnimationEvent;

        public void OnAnimationEvent(string eventName)
        {
            onAnimationEvent?.Invoke(eventName);
        }
    }
}


