// using System;
// using UnityEngine;
// using UnityEngine.Rendering.PostProcessing;
//
// namespace AliveCell
// {
//     public partial class CameraEffectSetting
//     {
//         public CameraEffects.InjuredSetting injured;
//     }
//
//     namespace CameraEffects
//     {
//         [Serializable]
//         public class InjuredSetting
//         {
//             [Range(0, 1)]
//             public float valueAlpha = 1f;
//
//             public float inTime = 1f;
//             public float outTime = 1f;
//         }
//
//         public class InjuredEffect : ICameraEffect
//         {
//             private InjuredPostEffect postEffect;
//             private float Value { get => postEffect.Value; set => postEffect.Value.value = value; }
//             private float Alpha { get => postEffect.Alpha; set => postEffect.Alpha.value = value; }
//
//             InjuredSetting setting => effectSetting.injured;
//
//             private Vector2 _value;
//             private Vector2 _alpha;
//             private float _inTime;
//             private float _outTime;
//             private bool _isInOrOut;
//             private float _timer;
//             private bool _playing;
//             
//             public InjuredEffect(CameraEffect effect) : base(effect)
//             {
//             }
//             public void Play()
//             {
//                 Play(setting.valueAlpha, setting.valueAlpha, setting.inTime, setting.outTime);
//             }
//
//             public void Play(float value, float alpha, float inTime, float outTime)
//             {
//                 _value.x = Value;
//                 _alpha.x = Alpha;
//                 _value.y = value;
//                 _alpha.y = alpha;
//                 _inTime = inTime;
//                 _outTime = outTime;
//                 _timer = 0f;
//                 _playing = true;
//                 _isInOrOut = true;
//             }
//
//             public override void Initialize()
//             {
//                 postEffect = profile.GetSetting<InjuredPostEffect>();
//                 Reset();
//             }
//             public override void Reset()
//             {
//                 Value = 0f;
//                 Alpha = 0f;
//                 _value = Vector2.zero;
//                 _alpha = Vector2.zero;
//                 _inTime = 0f;
//                 _outTime = 0f;
//                 _isInOrOut = true;
//                 _timer = 0f;
//                 _playing = false;
//             }
//             public override void Update(float deltaTime)
//             {
//                 if (!_playing)
//                 {
//                     return;
//                 }
//                 float scale = 0f;
//                 _timer += deltaTime;
//                 if (_isInOrOut)
//                 {
//                     scale = Mathf.Clamp01(_timer / _inTime);
//                     if (_timer > _inTime)
//                     {
//                         _timer -= _inTime;
//                         _isInOrOut = false;
//                         _value.x = _value.y;
//                         _alpha.x = _alpha.y;
//                         _value.y = 0f;
//                         _alpha.y = 0f;
//                     }
//                 }
//                 if (!_isInOrOut)
//                 {
//                     scale = Mathf.Clamp01(_timer / _outTime);
//                     if (_timer > _outTime)
//                     {
//                         _playing = false;
//                     }
//                 }
//
//                 Value = Mathf.Lerp(_value.x, _value.y, scale);
//                 Alpha = Mathf.Lerp(_alpha.x, _alpha.y, scale);
//             }
//         }
//     }
// }