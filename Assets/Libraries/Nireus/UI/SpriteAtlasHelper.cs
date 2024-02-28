using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace Nireus
{
    public class SpriteAtlasHelper : MonoBehaviour
    {
        public SpriteAtlas spriteAtlas;

        public Sprite GetSprite(string spriteName)
        {
            if (spriteAtlas == null || string.IsNullOrWhiteSpace(spriteName))
            {
                return null;
            }
            
            return spriteAtlas.GetSprite(spriteName);
        }
        
    }

}

