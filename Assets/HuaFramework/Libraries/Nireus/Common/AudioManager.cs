//#define TEST_AUDIO //开启测试UI
using UnityEngine;
using System.Collections.Generic;


namespace Nireus
{
    /// <summary>
    /// 音乐与音效管理类.
    /// </summary>
    public class AudioManager : SingletonBehaviour<AudioManager>, IAssetLoaderReceiver
    {
        public readonly int AudioSourceClipLimit = 30;//同时只能存活N个音效音源.
        public delegate void PlayAudioDelegate(object userData);
        private AssetLoadInfo assetLoadinfo;
        private float CurrentSoundVolume
        {
            get { return soundMuted ? 0 : soundVolume; }
        }

        private float CurrentMusicVolume
        {
            get { return musicMuted ? 0 : musicVolume; }
        }

        private AudioConfig musicConfig;

        private string baseSoundPath => PathConst.BUNDLE_RES_AUDIO_SFX;
        private string baseMusicPath => PathConst.BUNDLE_RES_AUDIO_MUSIC;

        private AudioListener audioListener;

        private List<AudioConfig> audioTempList = new List<AudioConfig>();

        public enum PlayState
        {
            NotStart,
            Normal,
            Pause,// Mark as end delete later
            End,
        }

        /// <summary>
        /// 音效静音开关
        /// </summary>
        private bool soundMuted = false;
        public bool SoundMuted
        {
            get { return soundMuted; }
            set
            {
                if (soundMuted != value)
                {
                    soundMuted = value;
                    UpdateAllAudioSourceVolume(false);
                }
            }
        }

        private float soundVolume = 1.0f;
        public float SoundVolume
        {
            get { return soundVolume; }
            set
            {
                if (soundVolume != value)
                {
                    soundVolume = value;
                    UpdateAllAudioSourceVolume(false);
                }
            }
        }


        private bool musicMuted = false;
        public bool MusicMuted
        {
            get { return musicMuted; }
            set
            {
                if (musicMuted != value)
                {
                    musicMuted = value;
                    UpdateAllAudioSourceVolume(true);
                }
            }
        }

        private float musicVolume = 1.0f;
        public float MusicVolume
        {
            get { return musicVolume; }
            set
            {
                if (musicVolume != value)
                {
                    musicVolume = value;
                    UpdateAllAudioSourceVolume(true);
                }
            }
        }

        public override void Initialize()
        {
            audioListener = gameObject.GetComponent<AudioListener>();
            if (audioListener == null)
                audioListener = gameObject.AddComponent<AudioListener>();
            AudioListener.volume = 1f;
            assetLoadinfo = new AssetLoadInfo("", null, this);
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

       void Destroy()
        {
            if (musicConfig != null)
                musicConfig.audioSource.clip = null;
            Destroy(audioListener);
            List<AudioConfig> list = new List<AudioConfig>(spawnedAudio);
            list.AddRange(despawnedAudio);
            foreach (var item in list)
                Destroy(item.audioSource.gameObject);

            spawnedAudio.Clear();
            despawnedAudio.Clear();
        }

        public void FreeMemory()
        {
            if (musicConfig != null)
                musicConfig.audioSource.clip = null;
            foreach (var item in despawnedAudio)
                Destroy(item.audioSource.gameObject);
            despawnedAudio.Clear();
        }
        public bool IsSoundPlaying(string soundName)
        {
            for (int i = 0; i < spawnedAudio.Count; i++)
            {
                var item = spawnedAudio[i];
                if (item.isMusic == false
                    && item.audioSource.clip.name.Equals(soundName, System.StringComparison.CurrentCultureIgnoreCase))
                    return item.playState != PlayState.End;
            }

            return false;
        }

        public AudioConfig GetPlayingSound(string soundName)
        {
            for (int i = 0; i < spawnedAudio.Count; i++)
            {
                var item = spawnedAudio[i];
                if (item.isMusic == false
                    && item.audioSource.clip.name.Equals(soundName, System.StringComparison.CurrentCultureIgnoreCase)
                     && item.playState != PlayState.End)
                    return item;
            }
            return null;
        }

        public void PlaySound(AudioClip audioClip, bool loop = false, float volume = 1, float delay = 0f, PlayAudioDelegate endDel = null, object userData = null, float timeLimit = 0f)
        {
            if (audioClip == null)
                return;

            var batchingSound = GetRepeatSound(audioClip, loop, delay, Time.frameCount, userData);

            if (batchingSound != null)
            {
                batchingSound.volume = Mathf.Max(volume, batchingSound.volume);
                batchingSound.endDel += endDel;

                batchingSound.UpdateVolume(CurrentSoundVolume);
            }
            else
            {
                if (spawnedAudio.Count >= AudioSourceClipLimit)
                {
                    return;
                }
                var audioData = SpawnAudio();
                audioData.isMusic = false;
                audioData.audioSource.clip = audioClip;
                audioData.audioSource.loop = loop;
                audioData.loop = loop;
                audioData.createdFrameCount = Time.frameCount;
                audioData.createdTime = Time.time;
                audioData.volume = volume;
                audioData.delay = delay;
                audioData.endDel += endDel;
                audioData.userData = userData;
                audioData.timeLimit = timeLimit;

                audioData.UpdateVolume(CurrentSoundVolume);

                if (delay == 0)
                {
                    audioData.audioSource.Play();
                    audioData.playState = PlayState.Normal;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="audioName"></param>
        /// <param name="loop"></param>
        /// <param name="volume"></param>
        /// <param name="delay"></param>
        /// <param name="endDel"></param>
        /// <param name="userData"></param>
        /// <param name="timeLimit">指定时间内只能播放一次</param>
        public void PlaySound(string audioName, bool loop = false, float volume = 1f, float delay = 0f, PlayAudioDelegate endDel = null, object userData = null, float timeLimit = 0f)
        {
            if (string.IsNullOrEmpty(audioName))
            {
                return;
            }
            //指定时间内还存在
            var audioConfig = GetPlayingSound(audioName);
            if (audioConfig != null && audioConfig.timeLimit > 0)
            {
                return;
            }
            assetLoadinfo.assetPath = baseSoundPath + audioName;
            //声音堵塞主线程加载播放
            //var audioClip = AssetManager.getInstance().loadSync(assetLoadinfo) as AudioClip;
            var audioClip  = AssetManager.Instance.loadSync<AudioClip>(assetLoadinfo.assetPath);
            if (audioClip != null)
                audioClip.name = audioName;

            PlaySound(audioClip, loop, volume, delay, endDel, userData, timeLimit);
        }
        public void PlayVoice(string audioName, bool loop = false, float volume = 1f, float delay = 0f, PlayAudioDelegate endDel = null, object userData = null, float timeLimit = 0f)
        {
            if (string.IsNullOrEmpty(audioName))
            {
                return;
            }
            //指定时间内还存在
            var audioConfig = GetPlayingSound(audioName);
            if (audioConfig != null && audioConfig.timeLimit > 0)
            {
                return;
            }
            assetLoadinfo.assetPath = audioName;
            ///////
            //////
            //////
            if (!dialogspwan.Contains(audioName))
            {
                dialogspwan.Add(audioName);
            }
            //声音堵塞主线程加载播放
            var audioClip = AssetManager.Instance.loadSync<AudioClip>(assetLoadinfo.assetPath);
            if (audioClip != null)
                audioClip.name = audioName;

            PlaySound(audioClip, loop, volume, delay, endDel, userData, timeLimit);
        }
        public void StopSound(string soundName)
        {
            for (int i = 0; i < spawnedAudio.Count; ++i)
            {
                var audioData = spawnedAudio[i];
                if (audioData.isMusic == false
                    && audioData.audioSource != null
                    && audioData.audioSource.clip != null
                    && audioData.audioSource.clip.name.Equals(soundName))
                    StopAudio(audioData); 
            }
            DespawnEndAudios();
            
        }
        public void StopVoice(string soundName)
        {
            for (int i = 0; i < spawnedAudio.Count; ++i)
            {
                var audioData = spawnedAudio[i];
                if (audioData.isMusic == false
                    && audioData.audioSource != null
                    && audioData.audioSource.clip != null
                    && audioData.audioSource.clip.name.Equals(soundName))
                {
                    StopAudio(audioData);
                    audioData.audioSource.clip = null;

                }
            }
        }
        public bool IsMusicPlaying(string soundName)
        {
            int dotIndex = soundName.LastIndexOf('.');
            if (dotIndex != -1)
                soundName = soundName.Substring(0, dotIndex);

            var item = musicConfig;
            if (item != null
                && item.isMusic
                && item.playState != PlayState.End
                && item.audioSource != null
                && item.audioSource.clip != null
                && item.audioSource.clip.name.Equals(soundName, System.StringComparison.CurrentCultureIgnoreCase))
                return true;


            return false;
        }

        private AudioConfig _GetPlayingMusic()
        {
            var item = musicConfig;
            if (item.isMusic
                && item.playState != PlayState.End
                && item.audioSource != null
                && item.audioSource.clip != null
                )
                return item;
            return null;
        }

        private void PlayMusic(AudioClip audioClip, string asset_path, bool loop = true, float volume = 1f, float delay = 0f)
        {
            if (audioClip == null)
                return;

            if (IsMusicPlaying(audioClip.name))
                return;


            var audioData = musicConfig;
            audioData.isMusic = true;
            audioData.audioSource.clip = audioClip;
            audioData.audioSource.loop = loop;
            audioData.loop = loop;
            audioData.createdFrameCount = Time.frameCount;
            audioData.createdTime = Time.time;
            audioData.volume = volume;
            audioData.delay = delay;
            audioData.asset_path = asset_path;

            audioData.UpdateVolume(CurrentMusicVolume);

            if (delay == 0)
            {
                audioData.audioSource.Play();
                audioData.playState = PlayState.Normal;
            }
        }

        public void PlayMusic(string audioName, bool loop = true, float volume = 1f, float delay = 0f)
        {
            if (IsMusicPlaying(audioName)) return;

            if (musicConfig == null)
            {
                musicConfig = CreateAudioConfig();
                musicConfig.Reset();
                musicConfig.isMusic = true;
            }
            StopMusic();

            var audioData = musicConfig;
            audioData.isMusic = true;
            audioData.volume = volume;
            audioData.loop = loop;
            string name = baseMusicPath + audioName;
            //AssetManager.getInstance().load(this, name, audioData, AssetLoadType.COMMON, 2);
            AssetManager.Instance.loadSync<AudioClip>(name);
        }


        public void PauseMusic()
        {
            var audioData = musicConfig;
            if (audioData == null)
                return;

            audioData.audioSource.Pause();

            audioData.playState = PlayState.Pause;
        }

        public void UnPauseMusic()
        {
            var audioData = musicConfig;
            if (audioData == null)
                return;

            audioData.audioSource.UnPause();
            audioData.playState = PlayState.Normal;
        }

        public void StopMusic()
        {
            var audioData = musicConfig;
            if (audioData.audioSource == null || audioData.audioSource.clip == null)
                return;

            if (audioData.playState == PlayState.End)
                return;
            StopAudio(audioData);
            audioData.audioSource.clip = null;
            AssetManager.Instance.UnloadAsset(audioData.asset_path);
        }
        public void StopDialogspwan()
        {
            if (dialogspwan.Count>0)
            {
                foreach (var item in dialogspwan)
                {
                    if (item!=null)
                    {
                        AssetManager.Instance.UnloadAsset(item);
                    }

                }
                dialogspwan.Clear();
            }

        }
        #region Pool
        private List<AudioConfig> spawnedAudio = new List<AudioConfig>();
        private List<AudioConfig> despawnedAudio = new List<AudioConfig>();
        private List<string> dialogspwan = new List<string>();

        private AudioConfig CreateAudioConfig()
        {
            var audioData = new AudioConfig();
            //3D
            audioData.audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();
            audioData.audioSource.playOnAwake = false;
            audioData.audioSource.transform.SetParent(this.gameObject.transform);
            DontDestroyOnLoad(audioData.audioSource.gameObject);

            return audioData;
        }

        private AudioConfig SpawnAudio()
        {
            AudioConfig audioData = null;
            if (despawnedAudio.Count != 0)
            {
                audioData = despawnedAudio[despawnedAudio.Count - 1];
                despawnedAudio.RemoveAt(despawnedAudio.Count - 1);
            }
            else
            {
                audioData = CreateAudioConfig();
            }
            spawnedAudio.Add(audioData);
            audioData.Reset();

            return audioData;
        }

        private void DespawnAudio(AudioConfig audioData)
        {
            audioData.audioSource.Stop();
            audioData.audioSource.clip = null;
            audioData.playState = PlayState.NotStart;
            audioData.endDel = null;
            audioData.userData = null;

            spawnedAudio.Remove(audioData);
            despawnedAudio.Add(audioData);
        }

        private void DespawnEndAudios()
        {
            audioTempList.Clear();

            for (int i = 0; i < spawnedAudio.Count; ++i)
            {
                var audioData = spawnedAudio[i];
                if (audioData.playState == PlayState.End)
                    audioTempList.Add(audioData);
            }

            for (int i = 0; i < audioTempList.Count; ++i)
                DespawnAudio(audioTempList[i]);

        }
        #endregion


        void Update()
        {
            for (int i = 0; i < spawnedAudio.Count; ++i)
            {
                var audioData = spawnedAudio[i];

                if (audioData.audioSource == null)
                    continue;

                if (audioData.isMusic)
                    UpdateMusicState(audioData);
                else
                    UpdateSoundState(audioData);
            }
            DespawnEndAudios();
        }

        private void UpdateSoundState(AudioConfig audioData)
        {
            if (audioData.playState == PlayState.NotStart)
            {
                // Update delay play
                if (audioData.audioSource.isPlaying == false && Time.time - audioData.createdTime >= audioData.delay)
                    audioData.audioSource.Play();

                if (audioData.audioSource.isPlaying)
                    audioData.playState = PlayState.Normal;
            }

            if (audioData.playState == PlayState.Normal && audioData.audioSource.isPlaying == false && audioData.timeLimit <= 0)
                StopAudio(audioData);

            if (audioData.timeLimit > 0)
                audioData.timeLimit -= Time.deltaTime;
        }

        private void UpdateMusicState(AudioConfig audioData)
        {
            if (audioData.playState == PlayState.NotStart)
            {// Update delay play
                if (audioData.audioSource.isPlaying == false && Time.time - audioData.createdTime >= audioData.delay)
                    audioData.audioSource.Play();

                if (audioData.audioSource.isPlaying)
                {
                    audioData.playState = PlayState.Normal;
                }
            }

            if (audioData.playState == PlayState.Normal)
            {
                if (audioData.loop && !audioData.audioSource.isPlaying)
                    audioData.playState = PlayState.NotStart;
            }
        }

        private void StopAudio(AudioConfig audioData)
        {
            if (audioData.audioSource != null)
                audioData.audioSource.Stop();

            audioData.playState = PlayState.End;
            if (audioData.endDel != null)
                audioData.endDel(audioData.userData);
        }

        private void UpdateAllAudioSourceVolume(bool music)
        {
            if (musicConfig != null)
            {
                musicConfig.UpdateVolume(CurrentMusicVolume);
            }
            foreach (var item in spawnedAudio)
                if (item.isMusic == music)
                    item.UpdateVolume(music ? CurrentMusicVolume : CurrentSoundVolume);
        }

        //同一时间相同声音只播放一个.
        private AudioConfig GetRepeatSound(AudioClip audioClip, bool loop, float delay, int createdframeCount, object userData)
        {
            foreach (var item in spawnedAudio)
                if (item.isMusic == false
                    && item.audioSource.clip == audioClip
                    && Mathf.Approximately(item.delay, delay)
                    && item.createdFrameCount == createdframeCount
                    && item.loop == loop
                    && item.userData == userData)
                    return item;

            return null;
        }

        public void CleanAllObject()
        {
            StopMusic();

            for (int i = 0; i < spawnedAudio.Count; ++i)
            {
                var audioData = spawnedAudio[i];

                if (audioData.isMusic == false)
                    StopAudio(audioData);

                DespawnAudio(audioData);
            }
        }
        //获取声音播放的时间
        public float GetAudioClip(string audioName)
        {
            var audioClip = AssetManager.getInstance().loadSync(assetLoadinfo) as AudioClip;
            if (audioClip != null)
            {
                return audioClip.length;
            }
            else
            {
                return float.MaxValue;
            }
        }
        public void OnAssetLoadReceive(string asset_path, UnityEngine.Object asset_data, System.Object userData)
        {
            var tmpData = userData as AudioConfig;
            var data = asset_data as AudioClip;
            var loop = tmpData.loop;
            var volume = tmpData.volume;
            var delay = tmpData.delay;
            var timeLimit = tmpData.timeLimit;
            if (tmpData.isMusic)
            {
                PlayMusic(data, asset_path, loop, volume, delay);
            }
            else
            {
                PlaySound(data, loop, tmpData.volume, delay, null, null, timeLimit);
            }
        }

        public void OnAssetLoadError(string url, object info)
        {
        }

        public void OnAssetLoadProgress(string url, object info, float progress, int bytesLoaded, int bytesTotal)
        {
        }


        public class AudioConfig
        {
            public bool isMusic;
            public AudioSource audioSource;
            public float createdFrameCount;//哪帧创建的
            public float createdTime;//创建的时间, timeScale不影响            
            public PlayState playState;//播放器状态
            public float volume;
            public float delay;
            public bool loop;
            public float timeLimit;//限制此时间内只播放同名声音一个.
            public PlayAudioDelegate endDel;
            public object userData;
            public string asset_path;

            public void Reset()
            {
                isMusic = false;
                audioSource.clip = null;
                audioSource.volume = 0;
                audioSource.loop = false;
                audioSource.spatialBlend = 0;//0 2d 1 3d
                createdFrameCount = 0;
                createdTime = 0;
                playState = PlayState.NotStart;
                volume = 0;
                delay = 0;
                timeLimit = 0;
                endDel = null;
                userData = null;
            }

            public void UpdateVolume(float globalVolume)
            {
                if (audioSource == null && Mathf.Approximately(globalVolume, audioSource.volume) == false)
                    return;
                audioSource.volume = globalVolume * 1f;
            }
        }


#if TEST_AUDIO
        void OnGUI()
        {
            SoundMuted = GUILayout.Toggle(SoundMuted, "SoundMuted");
            SoundVolume = GUILayout.HorizontalSlider(SoundVolume, 0, 1);
            MusicMuted = GUILayout.Toggle(MusicMuted, "MusicMuted");
            MusicVolume = GUILayout.HorizontalSlider(MusicVolume, 0, 1);

            if (GUILayout.Button("Play sound"))
                PlaySound("bow_dead.mp3", 0);

            if (GUILayout.Button("Play sound delay"))
                PlaySound("win.mp3", 1);

            if (GUILayout.Button("stop music"))
                StopMusic();         
        }
#endif
    }
}