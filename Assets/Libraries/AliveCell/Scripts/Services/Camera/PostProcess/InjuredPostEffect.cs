// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>
#if UNITY_POST_PROCESSING_STACK_V2

using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    [Serializable]
    [PostProcess(typeof(InjuredPostEffectRenderer), PostProcessEvent.AfterStack, "XMLib/InjuredPostEffect", true)]
    public sealed class InjuredPostEffect : PostProcessEffectSettings
    {
        [Tooltip("Value")]
        [Range(0, 1)]
        public FloatParameter Value = new FloatParameter { value = 0.2f };

        [Tooltip("Alpha")]
        [Range(0, 1)]
        public FloatParameter Alpha = new FloatParameter { value = 1f };

        [Tooltip("Color")]
        public ColorParameter Color = new ColorParameter { value = new Color(1f, 1f, 1f, 1f) };

        [Tooltip("Overlay Texture")]
        public TextureParameter OverlayTex = new TextureParameter { };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            return base.IsEnabledAndSupported(context)
                && Value > 0f
                && Alpha > 0f
                && Color.value.a > 0f;
        }
    }

    public sealed class InjuredPostEffectRenderer : PostProcessEffectRenderer<InjuredPostEffect>
    {
        private int _Value = Shader.PropertyToID("_Value");
        private int _Alpha = Shader.PropertyToID("_Alpha");
        private int _Color = Shader.PropertyToID("_Color");
        private int _OverlayTex = Shader.PropertyToID("_OverlayTex");

        public override void Render(PostProcessRenderContext context)
        {
            Shader shader = Shader.Find("XMLib/InjuredPostEffect");
            var sheet = context.propertySheets.Get(shader);
            sheet.properties.SetFloat(_Value, settings.Value);
            sheet.properties.SetFloat(_Alpha, settings.Alpha);
            sheet.properties.SetColor(_Color, settings.Color);
            if (settings.OverlayTex.value != null) sheet.properties.SetTexture(_OverlayTex, settings.OverlayTex);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}

#endif