using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nireus {
    class VecAnimClipOverrides:List<KeyValuePair<AnimationClip,AnimationClip>> {
        public VecAnimClipOverrides(int capacity) : base(capacity) { }
        public AnimationClip this[string name] {
            get { return this.Find(x => x.Key.name.Equals(name)).Value; }
            set {
                int index = this.FindIndex(x => x.Key.name.Equals(name));
                if (index != -1)
                    this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
            }
        }
    }
}
