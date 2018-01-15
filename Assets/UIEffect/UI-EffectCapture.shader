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

			v2f_img vert(appdata_img v)
			{
				v2f_img o;
				o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			
				#if UNITY_UV_STARTS_AT_TOP
				o.uv = half2(v.texcoord.x, 1 - v.texcoord.y);
				#else
				o.uv = v.texcoord;
				#endif
			
				return o;
			}

			sampler2D _MainTex;
			half3 _EffectFactor;
			fixed4 _ColorFactor;

			fixed4 frag(v2f_img IN) : SV_Target
			{
				#if UI_TONE_PIXEL
				float pixelRate = max(1,(1 - _EffectFactor.x) * 256);
				IN.uv = round(IN.uv * pixelRate) / pixelRate;
				#endif
				
				#if defined (UI_BLUR)
				half4 color = Tex2DBlurring(_MainTex, IN.uv, _EffectFactor.z);
				#else
				half4 color = tex2D(_MainTex, IN.uv);
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
