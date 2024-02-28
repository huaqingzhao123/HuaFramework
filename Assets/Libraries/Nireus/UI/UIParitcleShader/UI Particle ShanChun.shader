Shader "UI/Particles/ShanChun_add" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

	_Tex_u_speed("Tex_u_speed", Float) = 0
	_Tex_v_Speed("Tex_v_Speed", Float) = 0
	[MaterialToggle] _UV_ON("UV_ON", Float) = 0
	_wenli("wenli", 2D) = "white" {}
	[MaterialToggle] _rongjie("rongjie", Float) = 1
	[MaterialToggle] _rongjie_UV("rongjie_UV", Float) = 0
	_DissUPan("DissU Pan", Float) = 0
	_DissVPan("DissV Pan", Float) = 0
	_Difusecolr("Difusecolr", Color) = (0.5,0.5,0.5,1)

	_StencilComp ("Stencil Comparison", Float) = 8
    _Stencil ("Stencil ID", Float) = 0
    _StencilOp ("Stencil Operation", Float) = 0
    _StencilWriteMask ("Stencil Write Mask", Float) = 255
    _StencilReadMask ("Stencil Read Mask", Float) = 255

    _ColorMask ("Color Mask", Float) = 15

    [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
	Blend One One
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off
	
	SubShader {

		Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

		Pass {
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_particles
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

            #pragma multi_compile __ UNITY_UI_ALPHACLIP

			sampler2D _MainTex;
			fixed4 _TintColor;

			uniform float _Tex_u_speed;
			uniform float _Tex_v_Speed;
			uniform fixed _UV_ON;
			uniform sampler2D _wenli; uniform float4 _wenli_ST;
			uniform fixed _rongjie;
			uniform fixed _rongjie_UV;
			uniform float _DissUPan;
			uniform float _DissVPan;
			uniform float4 _Difusecolr;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				UNITY_FOG_COORDS(1)
				#ifdef SOFTPARTICLES_ON
				float4 projPos : TEXCOORD2;
				#endif
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t IN)
			{
				v2f v;
				v.vertex = UnityObjectToClipPos(IN.vertex);
				#ifdef SOFTPARTICLES_ON
				v.projPos = ComputeScreenPos (v.vertex);
				COMPUTE_EYEDEPTH(v.projPos.z);
				#endif
				v.color = IN.color;
				v.texcoord =IN.texcoord;
				v.texcoord1 = IN.texcoord1;
				UNITY_TRANSFER_FOG(v,v.vertex);
				return v;
			}

			sampler2D_float _CameraDepthTexture;
			float _InvFade;
			
			fixed4 frag (v2f IN, float facing : VFACE) : COLOR
			{
				float isFrontFace = (facing >= 0 ? 1 : 0);
				float faceSign = (facing >= 0 ? 1 : -1);

				float4 node_6785 = _Time;
				float2 _UV_ON_var = lerp((float2((_Tex_u_speed*node_6785.g), (node_6785.g*_Tex_v_Speed)) + IN.texcoord), (IN.texcoord + float2(IN.texcoord1.b, IN.texcoord1.a)), _UV_ON);
				float4 _Textures_var = tex2D(_MainTex, TRANSFORM_TEX(_UV_ON_var, _MainTex));
				float4 node_8864 = _Time;
				float2 _rongjie_UV_var = lerp(IN.texcoord, (float2((_DissUPan*node_8864.g), (node_8864.g*_DissVPan)) + IN.texcoord), _rongjie_UV);
				float4 _wenli_var = tex2D(_wenli, TRANSFORM_TEX(_rongjie_UV_var, _wenli));
				float3 emissive = (_Difusecolr.rgb*(_Textures_var.rgb*_Textures_var.a)*(IN.color.rgb*IN.color.a)*lerp(1.0, step(IN.texcoord1.r, _wenli_var.r), _rongjie)*_Difusecolr.a);
				float4 finalColor = fixed4(emissive,1);
				//return fixed4(finalColor, 1);

				#ifdef SOFTPARTICLES_ON
				float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(IN.projPos)));
				float partZ = IN.projPos.z;
				float fade = saturate (_InvFade * (sceneZ-partZ));
				IN.color.a *= fade;
				#endif
				
				fixed4 col = 1.0f * IN.color * _TintColor * finalColor;
				UNITY_APPLY_FOG_COLOR(IN.fogCoord, col, fixed4(0,0,0,0)); // fog towards black due to our blend mode
				return col;
			}
			ENDCG 
		}
	}	
}
}
