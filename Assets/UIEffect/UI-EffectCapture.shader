Shader "UI/Hidden/UI-EffectCapture"
{
	Properties
	{
		[PerRendererData] _MainTex("Main Texture", 2D) = "white" {}
	}

	SubShader
	{
		ZTest Always
		Cull Off
		ZWrite Off
		Fog{ Mode off }

		Pass
		{
			Name "Default"

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#if UNITY_VERSION >= 540
			#pragma target 2.0
			#endif
			
			#include "UnityCG.cginc"
			
			// vvvv [For UIEffect] vvvv : Define keyword and include.
			#pragma shader_feature __ UI_TONE_GRAYSCALE UI_TONE_SEPIA UI_TONE_NEGA UI_TONE_PIXEL UI_TONE_HUE
			#pragma shader_feature __ UI_COLOR_ADD UI_COLOR_SUB UI_COLOR_SET
			#pragma shader_feature __ UI_BLUR_FAST UI_BLUR_MEDIUM UI_BLUR_DETAIL
			#include "UI-Effect.cginc"

			// ^^^^ [For UIEffect] ^^^^
			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord  : TEXCOORD0;
				
				// vvvv [For UIEffect] vvvv
				#if defined (UI_COLOR)						// Add color effect factor.
				fixed4 colorFactor : COLOR1;
				#endif

				#if defined (UI_TONE) || defined (UI_BLUR)	// Add other effect factor.
				half4 effectFactor : TEXCOORD2;
				#endif
				// ^^^^ [For UIEffect] ^^^^
			};

			sampler2D _MainTex;
			half4 _EffectFactor;
			float4 _MainTex_TexelSize;
			fixed4 _ColorFactor;

			v2f vert(appdata_img v)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
			
				#if UNITY_UV_STARTS_AT_TOP
				OUT.texcoord = half2(v.texcoord.x, 1 - v.texcoord.y);
				#else
				OUT.texcoord = v.texcoord;
				#endif

				// vvvv [For UIEffect] vvvv : Calculate effect parameter.
				#if defined (UI_TONE) || defined (UI_BLUR)
				OUT.effectFactor = _EffectFactor;
				#endif

				#if UI_TONE_PIXEL
				OUT.effectFactor.xy = max(2, (1-OUT.effectFactor.x) * _MainTex_TexelSize.zw);
				#endif

				#if UI_TONE_HUE
				OUT.effectFactor.y = sin(OUT.effectFactor.x*3.14159265359*2);
				OUT.effectFactor.x = cos(OUT.effectFactor.x*3.14159265359*2);
				#endif
				
				#if defined (UI_COLOR)
				OUT.colorFactor = UnpackToVec4(IN.uv1.y);
				#endif
				// ^^^^ [For UIEffect] ^^^^
				
				return OUT;
			}


			fixed4 frag(v2f IN) : SV_Target
			{
				#if UI_TONE_PIXEL
				IN.texcoord = round(IN.texcoord * IN.effectFactor.xy) / IN.effectFactor.xy;
				#endif
				
				#if defined (UI_BLUR)
				half4 color = Tex2DBlurring(_MainTex, IN.texcoord, _EffectFactor.z * _MainTex_TexelSize.xy * 2);
				#else
				half4 color = tex2D(_MainTex, IN.texcoord);
				#endif

				#if UI_TONE_HUE
				color.rgb = shift_hue(color.rgb, _EffectFactor.x, _EffectFactor.y);
				#elif defined (UI_TONE)
				color = ApplyToneEffect(color, _EffectFactor.x);	// Tone effect.
				#endif
				
				#if defined (UI_COLOR)
				color = ApplyColorEffect(color, _ColorFactor);	// Color effect.
				#endif

				color.a = 1;
				return color;
			}
		ENDCG
		}
	}
}
