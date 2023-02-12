/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2021/1/3 23:16:42
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// RecorderListItem
    /// </summary>
    public class RecorderListItem : ResourceItem
    {
        [SerializeField]
        protected Text _nameText;

        [SerializeField]
        protected Text _timeText;

        //protected RecordData _data;
        protected RecorderPanel _panel;

        //public RecordData data => _data;

        public void Initialize(RecorderPanel panel/*, RecordData data*/)
        {
            _panel = panel;
            //_nameText.text = string.Format("{0} {1}分{2}秒", data.mapName, data.gameTime.Minutes, data.gameTime.Seconds);
            //_timeText.text = data.date.ToString("yyyy-MM-dd HH:mm:ss");
            //_data = data;
        }

        public void OnPlayButton()
        {
            _panel.OnPlayRecoder(this);
        }

        public void OnDeleteButton()
        {
            _panel.OnDeleteRecoder(this);
        }
    }
}