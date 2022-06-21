using HuaFramework.Singleton;
using HuaFramework.Utility;
using HuaFramework.Utility.ObjectPool;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HuaFramework.Managers
{
    public class AudioManager : MonoSingleton<AudioManager>
    {
        SimpleObjectPool<AudioSource> _audioSourcePool;
        private Dictionary<string, AudioClip> _audioCaches = new Dictionary<string, AudioClip>();

        private AudioSource _bgMusicAudioSource;

        public AudioSource PlayCommonAudio(string audioName,string path = "Audio/",bool isLoop = false, Action callback=null)
        {
            InitPool();
            var audioSource = _audioSourcePool.SpawnObject();//从池子取一个
            if (_audioCaches.TryGetValue(audioName, out AudioClip clip))
                audioSource.clip = clip;
            else
            {
                //加载音乐
                audioSource.clip = LoadAudioByResources(audioName, path);
            }
            audioSource.loop = isLoop;
            audioSource.Play();
            OnPlayMusicComplete(isLoop,audioSource,callback);
            return audioSource;
        }
        public void PlayBgAudio(string audioName, string path = "Audio/",bool isLoop = true, Action callback = null)
        {
            InitPool();
            if (!_bgMusicAudioSource)
                _bgMusicAudioSource = _audioSourcePool.SpawnObject();//从池子取一个
            if (_audioCaches.TryGetValue(audioName, out AudioClip clip))
                _bgMusicAudioSource.clip = clip;
            else
            {
                //加载音乐
                _bgMusicAudioSource.clip = LoadAudioByResources(audioName, path);
            }
            _bgMusicAudioSource.Play();
            _bgMusicAudioSource.loop = isLoop;
            OnPlayMusicComplete(isLoop, _bgMusicAudioSource, callback);
        }

        public bool StopAudio(AudioSource audioSource)
        {
            if (!audioSource) return false;
            audioSource.Pause();
            _audioSourcePool.UnSpawnObject(audioSource);
            return true;
        }

        private AudioClip LoadAudioByResources(string audioName, string path)
        {
            var clip= Resources.Load<AudioClip>(path + audioName);
            if (!_audioCaches.ContainsKey(audioName))
                _audioCaches.Add(audioName, clip);
                return clip;
        }

        /// <summary>
        /// 异步播放音效
        /// </summary>
        /// <param name="audioPah"></param>
        /// <param name="isLoop"></param>
        /// <param name="callback"></param>
        private void AsyncPlayAudio(string audioPah,bool isLoop, Action callback)
        {

        }

        public void InitPool()
        {
            if (_audioSourcePool == null)
                _audioSourcePool = new SimpleObjectPool<AudioSource>(() =>
                  {
                      return gameObject.AddComponent<AudioSource>();
                  }, null, 10);
        }

        private void OnPlayMusicComplete(bool isLoop,AudioSource  audioSource, Action callback)
        {
            if (!isLoop)
                callback += () =>
                {
                    Debug.LogError("回收前池子的数量为:"+_audioSourcePool.CurCount);
                    _audioSourcePool.UnSpawnObject(audioSource);
                    Debug.LogError("回收后池子的数量为:" + _audioSourcePool.CurCount);
                };
            Delay(audioSource.clip.length, callback);
        }

    }

}
