using System;
using System.Collections;
using System.Collections.Generic;
using Nireus;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIImageSpriteSwitch : MonoBehaviour
{
    [Serializable]
    private struct NamedSprite
    {
        public string name;
        public Sprite sprite;
    }

    private Image imageComponent;

    [SerializeField]
    private List<NamedSprite> spriteList = new List<NamedSprite>();

    public void SwitchSprite(string key, bool setNativeSize = false)
    {
        if (imageComponent == null)
        {
            imageComponent = GetComponent<Image>();
            if (imageComponent == null) return;
        }

        imageComponent.sprite = GetSpriteWithKey(key);

        if (setNativeSize)
        {
            imageComponent.SetNativeSize();
        }
    }

    private Sprite GetSpriteWithKey(string key)
    {
        if (spriteList == null)
        {
            return null;
        }

        foreach (var namedSprite in spriteList)
        {
            if (namedSprite.name == key)
            {
                return namedSprite.sprite;
            }
        }

        return null;
    }
}
