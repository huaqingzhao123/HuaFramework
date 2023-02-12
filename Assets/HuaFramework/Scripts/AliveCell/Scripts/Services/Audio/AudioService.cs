/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/9/24 16:12:11
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    /// <summary>
    /// AudioService
    /// </summary>
    public class AudioService : IServiceInitialize, IDisposable, IMonoUpdate
    {
        [InjectObject] protected ResourceService res { get; set; }

        protected List<AudioBase> audios = new List<AudioBase>();
        private Transform audioRoot;
        public SuperLogHandler LogHandler = SuperLogHandler.Create("AS");

        public AudioBase Play(int prefabID)
        {
            return Play(prefabID, Vector3.zero);
        }

        public BGAudio PlayBGM(int prefabID, bool forceChange = false)
        {
            if (!forceChange)
            {
                foreach (var item in audios)
                {
                    if (item is BGAudio bga)
                    {
                        bga.Stop();
                    }
                }
            }
            else
            {
                for (int i = 0; i < audios.Count; i++)
                {
                    if (audios[i] is BGAudio bga)
                    {
                        audios.RemoveAt(i);
                        res.DestroyGO(bga);
                        i--;
                    }
                }
            }

            GameObject obj = res.CreateGO(prefabID);
            obj.transform.SetParent(audioRoot);

            BGAudio audio = obj.GetComponent<BGAudio>();
            audios.Add(audio);
            audio.Initialize(this);
            audio.Play();

            return audio;
        }

        public AudioBase Play(int prefabID, Vector3 position)
        {
            GameObject obj = res.CreateGO(prefabID);
            obj.transform.SetParent(audioRoot);
            obj.transform.position = position;

            AudioBase audio = obj.GetComponent<AudioBase>();
            audios.Add(audio);
            audio.Initialize(this);
            audio.Play();
            return audio;
        }

        public IEnumerator OnServiceInitialize()
        {
            audioRoot = new GameObject("[AC]Audio").transform;
            GameObject.DontDestroyOnLoad(audioRoot.gameObject);

            App.On<ISubScene>(EventTypes.Scene_UnInitialize, OnSceneUnInitialize);

            yield break;
        }

        public void Dispose()
        {
            App.Off(this);
            if (null != audioRoot)
            {
                GameObject.Destroy(audioRoot.gameObject);
                audioRoot = null;
                audios.Clear();
            }
        }

        public void OnMonoUpdate()
        {
            for (int i = 0; i < audios.Count; i++)
            {
                AudioBase audio = audios[i];
                if (!audio.isPlaying)
                {//回收
                    audios.RemoveAt(i);
                    res.DestroyGO(audio);
                    i--;
                }
            }
        }

        private void OnSceneUnInitialize(ISubScene obj)
        {
            foreach (var audio in audios)
            {//移除依赖场景的音频
                if (audio is SceneAudio)
                {
                    audio.Stop();
                }
            }
        }
    }
}