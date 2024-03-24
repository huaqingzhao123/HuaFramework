/*
 * 作者：Peter Xiang
 * 联系方式：565067150@qq.com
 * 文档: https://github.com/PxGame
 * 创建时间: 2020/12/8 13:37:34
 */

using MoreMountains.NiceVibrations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XMLib;

namespace AliveCell
{
    public partial class GlobalSetting
    {
        [SerializeField]
        private DeviceService.Setting _device = null;

        public static DeviceService.Setting device => Inst._device;
    }

    /// <summary>
    /// DeviceService
    /// </summary>
    public class DeviceService : IServiceInitialize, IDisposable, IMonoUpdate
    {
        [Serializable]
        public class Setting
        {
            public int screenResolution = 720;
            public int preferredRefreshRate = 60;

            [SerializeField]
            protected HapticSetting _haptic;

            public List<HapticInfo> hapticInfos => _haptic.hapticInfos;
            [NonSerialized] protected Dictionary<string, HapticInfo> _cacheHaptic;
            [NonSerialized] protected bool _isInited = false;

            public HapticInfo GetHaptic(string name)
            {
                Initialize();

                if (!_cacheHaptic.TryGetValue(name, out var result))
                {
                    return null;
                    //throw new RuntimeException($"未找到 Haptic:{name} 配置");
                }
                return result;
            }

            private void Initialize()
            {
                if (_isInited)
                {
                    return;
                }
                _isInited = true;

                if (_cacheHaptic == null)
                {
                    _cacheHaptic = new Dictionary<string, HapticInfo>();

                    foreach (var item in hapticInfos)
                    {
                        _cacheHaptic[item.name] = item;
                    }
                }
            }
        }

        public Setting setting => GlobalSetting.device;

        public SuperLogHandler LogHandler = SuperLogHandler.Create("DV");

        public readonly GameInput gameInput = new GameInput();
        public bool cursorActive { get => Cursor.lockState != CursorLockMode.Locked; set => Cursor.lockState = (value ? CursorLockMode.None : CursorLockMode.Locked); }

        public IEnumerator OnServiceInitialize()
        {
            SetResolution(setting.screenResolution, setting.preferredRefreshRate);

            MMVibrationManager.SetHapticsActive(true);
            gameInput.Enable();

#if UNITY_EDITOR
            //if (GlobalSetting.resource.enableDebugger)
            //{
            //    MMVibrationManager.SetDebugMode(true);
            //}
#endif
            yield break;
        }

        public void Dispose()
        {
            gameInput.Disable();
            DisposeHaptic();
        }

        public void OnMonoUpdate()
        {
            UpdateHaptic();
        }

        public void SetResolution(int resolution, int refreshRate)
        {
            int height = 0;
            int width = 0;
            if (Screen.height < Screen.width)
            {
                height = resolution;
                width = Mathf.RoundToInt(resolution / (float)Screen.height * Screen.width);
            }
            else
            {
                width = resolution;
                height = Mathf.RoundToInt(resolution / (float)Screen.width * Screen.height);
            }

#if !UNITY_EDITOR
            Screen.SetResolution(width, height, true, refreshRate);
#endif

            UnityEngine.Application.targetFrameRate = refreshRate;

            LogHandler.Log($"设置分辨率: {width} x {height} ({refreshRate} fps)");
        }

        #region Haptic

        private bool _continuousPlaying = false;
        private float _continuousStartedAt = 0f;
        private HapticInfo _continuousInfo = null;

        public void Haptic(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }
            Haptic(setting.GetHaptic(name));
        }

        private void DisposeHaptic()
        {
            MMVibrationManager.StopAllHaptics(true);
            _continuousPlaying = false;
        }

        public void Haptic(HapticInfo info)
        {
            if (info == null)
            {
                return;
            }
            //LogHandler.Log("Play {0}", info);
            switch (info.hapticMethod)
            {
                case HapticMethods.NativePreset:
                    HapticNativePreset(info);
                    break;

                case HapticMethods.Transient:
                    HapticTransient(info);
                    break;

                case HapticMethods.Continuous:
                    HapticContinuous(info);
                    break;

                case HapticMethods.Stop:
                    HapticStop(info);
                    break;

                case HapticMethods.Advance:
                    HapticAdvance(info);
                    break;
            }
        }

        private void UpdateHaptic()
        {
            if (!_continuousPlaying)
            {
                return;
            }

            float elapsedTime = (_continuousInfo.independTimeScale ? Time.time : Time.unscaledTime) - _continuousStartedAt;

            float remappedTime = MathUtility.Remap01(elapsedTime, 0f, _continuousInfo.duration);
            float intensity = _continuousInfo.intensity * _continuousInfo.intensityScaleCurve.Evaluate(remappedTime);
            float sharpness = _continuousInfo.sharpness * _continuousInfo.sharpnessScaleCurve.Evaluate(remappedTime);
            MMVibrationManager.UpdateContinuousHaptic(intensity, sharpness, _continuousInfo.allowRumble);
            if (_continuousInfo.allowRumble)
            {
#if MOREMOUNTAINS_NICEVIBRATIONS_RUMBLE
                MMNVRumble.RumbleContinuous(intensity, sharpness);
#endif
            }

            if (elapsedTime > _continuousInfo.duration)
            {
                MMVibrationManager.StopContinuousHaptic(_continuousInfo.allowRumble);
                _continuousPlaying = false;
                _continuousInfo = null;
            }
        }

        private void HapticStop(HapticInfo info)
        {
            if (_continuousPlaying)
            {
                _continuousPlaying = false;
                MMVibrationManager.StopContinuousHaptic(info.allowRumble);
            }
        }

        private void HapticContinuous(HapticInfo info)
        {
            if (_continuousPlaying && _continuousInfo != null)
            {
                _continuousPlaying = false;
                MMVibrationManager.StopContinuousHaptic(_continuousInfo.allowRumble);
                _continuousInfo = null;
            }
            _continuousInfo = info;
            _continuousStartedAt = _continuousInfo.independTimeScale ? Time.time : Time.unscaledTime;
            _continuousPlaying = true;

            MMVibrationManager.ContinuousHaptic(
                _continuousInfo.intensity,
                _continuousInfo.sharpness,
                _continuousInfo.duration,
                HapticTypes.SoftImpact,
                App.unityApp);
        }

        private void HapticTransient(HapticInfo info)
        {
            MMVibrationManager.TransientHaptic(
                info.intensity,
                info.sharpness,
                info.allowRumble,
                App.unityApp);
        }

        private void HapticNativePreset(HapticInfo info)
        {
            MMVibrationManager.Haptic(
                info.hapticType,
                true,
                info.allowRumble,
                App.unityApp);
        }

        private void HapticAdvance(HapticInfo info)
        {
            string iosAHAP = string.Empty;
            long[] androidPattern = null;
            int[] androidAmplitudes = null;
            long[] rumblePattern = null;
            int[] rumbleLowFrequencyAmplitudes = null;
            int[] rumbleHighFrequencyAmplitudes = null;

            if (info.AHAPFile != null)
            {
                iosAHAP = info.AHAPFile.text;
            }

            if (info.androidWave != null)
            {
                androidPattern = info.androidWave.WaveForm.Pattern;
                androidAmplitudes = info.androidWave.WaveForm.Amplitudes;
            }

            if (info.rumbleWave != null)
            {
                rumblePattern = info.rumbleWave.WaveForm.Pattern;
                rumbleLowFrequencyAmplitudes = info.rumbleWave.WaveForm.LowFrequencyAmplitudes;
                rumbleHighFrequencyAmplitudes = info.rumbleWave.WaveForm.HighFrequencyAmplitudes;
            }

            MMVibrationManager.AdvancedHapticPattern(
               iosAHAP,//ios
               androidPattern, androidAmplitudes, 1,//android
             rumblePattern, rumbleLowFrequencyAmplitudes, rumbleHighFrequencyAmplitudes, 1,//runmble
               HapticTypes.LightImpact, App.unityApp);
        }

        #endregion Haptic
    }
}