using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Nireus
{
    public class UIEffectScrollViewCamera
    {
        public UiEffectLyaerMask cur_layer { get { return _cur_layer;} }
        private const int render_texture_max = 4;
        private UiEffectLyaerMask _cur_layer;
        private List<ParticleSystem> _effect_root_list = new List<ParticleSystem>();
        private Data _cur_data;
        private static int _counter;       
        private static Dictionary<int, Data> s_cache_dict = new Dictionary<int, Data>();
        public static UiEffectLyaerMask[] layer_mask_list = new UiEffectLyaerMask[render_texture_max]
            {
                UiEffectLyaerMask.Scroll0,UiEffectLyaerMask.Scroll1,UiEffectLyaerMask.Scroll2,UiEffectLyaerMask.Scroll3,

            };

        class Data
        {
            public GameObject image_game_object;
            public UiEffectLyaerMask layer_mask;
            public Transform scroll_content;
        }


        public UIEffectScrollViewCamera()
        {
        }
        //template=列表中项目的原始模型
        public UIEffectScrollViewCamera(GameObject template)
        {
            _effect_root_list.Clear();
            _FindEffectRoot(template.transform);
        }

        //用于列表中的模板，
        public void SetGameObjectTemplate(GameObject template,Transform scroll_content)
        {
            _effect_root_list.Clear();
            _FindEffectRoot(template.transform);
            _AllocLayerMask(scroll_content);
            _SetEffectRenderLayer(_cur_layer);
        }
        /// <summary>
        /// SetGameObjectTemplate 后调用这个方法
        /// </summary>
        public void Show()
        {
            _cur_data.image_game_object.SetActive(true);
        }

        //scroll_content 列表中的容器，这个不需要SetGameObjectTemplate，但是会递归
        public void Show(Transform scroll_content,bool recurrence_content = false,bool is_dirty_data = false)
        {
            if (is_dirty_data)
            {
                _effect_root_list.Clear();
            }
            if (recurrence_content)
            {
                _AllocLayerMask(scroll_content);
                if (_effect_root_list.Count == 0)
                {
                    _effect_root_list.Clear();
                    _FindEffectRoot(scroll_content.transform);
                }
                _SetEffectRenderLayer(_cur_layer);
            }
            else
            {
                _AllocLayerMask(scroll_content);
                _SetEffectRenderLayer(_cur_layer);
            }
            _cur_data.image_game_object.SetActive(true);
        }
        public void Hide()
        {
            if (_cur_data != null && _cur_data.image_game_object.activeSelf)
            {
                _cur_data.image_game_object.SetActive(false);
            }
        }

        public void Refrash(Transform scroll_content)
        {
            _effect_root_list.Clear();
            _FindEffectRoot(scroll_content.transform);
            _SetEffectRenderLayer(_cur_layer);
        }

        public void SetTemplatePrefab(GameObject gameObject)
        {
            _effect_root_list.Clear();
            _FindEffectRoot(gameObject.transform);
        }

        void _AllocLayerMask(Transform scroll_content)
        {
            int find = _FindContent(scroll_content);
            _cur_data = find >= 0 ? s_cache_dict[find] :  _GetImageGameObject();
            if (_cur_data.scroll_content != null)
            {
                LayerManager.getInstance().addToLayer(_cur_data.image_game_object.transform,LayerType.HIDE);                
            }
            _cur_data.scroll_content = scroll_content;
            _cur_data.image_game_object.transform.SetParent(scroll_content.parent, false);
            _cur_layer = _cur_data.layer_mask;
        }


        int _FindContent(Transform scroll_content)
        {
            foreach (var kv in s_cache_dict)
            {
                if (kv.Value.scroll_content == scroll_content)
                {
                    return kv.Key;
                }
            }
            return -1;
        }

        private void _SetEffectRenderLayer(UiEffectLyaerMask layer)
        {
            foreach (var t in _effect_root_list)
            {
                _SetLayer(t.gameObject, (int)layer);
            }
        }


        void _SetLayer(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform)
            {
                _SetLayer(t.gameObject, layer);
            }
        }

        //获取到每个特效根节点。
        void _FindEffectRoot(Transform cur_transform)
        {
            var child_count = cur_transform.childCount;
            for (var i = 0; i < child_count; i++)
            {
                var transform_child = cur_transform.GetChild(i);
                var p = transform_child.GetComponent<ParticleSystem>();
                if (p != null)
                {
                   // if(_effect_root_list.Contains(p) == false)
                    _effect_root_list.Add(p);
                }
                else if(transform_child.childCount > 0)
                {
                    _FindEffectRoot(transform_child);
                }
            }
        }

        //分配一个镜头，注镜头是有限的。
        private static Data _GetImageGameObject()
        {
            Data data;
            if (s_cache_dict.TryGetValue(_counter, out data) == false)
            {
                var go = GameObject.Instantiate<GameObject>(UnityEngine.Resources.Load<GameObject>("UIEffect/UIEffectPrefabScroll" + _counter));
                data = new Data();
                data.image_game_object = go;
                data.layer_mask = layer_mask_list[_counter];
                s_cache_dict.Add(_counter, data);
            }
            _counter++;
            if (_counter > render_texture_max)
            {
                _counter = 0;
            }
            return data;
        }
    }

}