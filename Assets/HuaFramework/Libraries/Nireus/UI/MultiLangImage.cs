using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Nireus
{

    [Serializable]
    public class MultiLangImage : UIBehaviour
    {
        public string _ImagePath;
        
        public bool bSetNativeSize = true;

        [FilePath(Extensions = "png, jpg")] 
        [Title("图片名字")]
        public string ImageName
        {
            get
            {
                return _ImagePath;
            }
            set
            {
                _ImagePath = Path.GetFileName(value);
            }
        }

        protected override void Awake()
        {
            ImageName = Path.GetFileName(ImageName);

            if (!string.IsNullOrEmpty(ImageName))
            {
                if (ImageName.Contains(".png") || ImageName.Contains(".jpg"))
                {
                    ImageName = ImageName.Substring(0, ImageName.IndexOf('.'));
                }
            }
            var image = transform.GetComponent<Image>();
            //var sprite = MultiLanguageSpriteManager.Instance.GetUISprite(ImageName);
            //if(null != image && null != sprite)
            //{
            //    image.sprite = sprite;
            //    if (bSetNativeSize)
            //    {
            //        image.SetNativeSize();
            //    }
            //}
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
        }
#endif
    }
}
