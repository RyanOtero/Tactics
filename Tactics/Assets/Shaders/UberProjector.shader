// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/UberProjector"
{
	Properties
	{
		_PrimaryTex("Primary Texture", 2D) = "white" {}
		_PrimaryTintColor("Primary Tint", Color) = (255,255,255,255)
		_SecondaryTex("Secondary Texture", 2D) = "white" {}
		_SecondaryTintColor("Secondary Tint", Color) = (255,255,255,255)
		[Toggle] _UseStrobe("Strobe", Float) = 0
		[Toggle] _UseTwoColor("Use Two Colors", Float) = 0
		_High("Multiplier High", Range(0, 2)) = 1
		_Low("Multiplier Low", Range(0, 2)) = 1
		_AngleLimit("Angle Limit (rad)", Range(0,1.58)) = 0
		_Frequency("Frequency", Range(.1, 10)) = 1.25
		_Alpha("Alpha", Range(0,100)) = 100
	}

		Subshader
		{
			Tags { "RenderType" = "Transparent"  "Queue" = "Transparent+101"}
			Pass
			{
				ZWrite Off
				Offset -1, -1
				Fog {Mode OFF}
				ColorMask RGB
				Blend OneMinusSrcAlpha SrcAlpha


				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_fog_exp2
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

				struct v2f
				{
					float4 pos      : SV_POSITION;
					float4 uv       : TEXCOORD0;
					float4 uvSec    : TEXCOORD1;
					half projAngle : TEXCOORD2;
				};

				sampler2D _PrimaryTex;
				sampler2D _SecondaryTex;
				float4 _PrimaryTintColor;
				float4 _SecondaryTintColor;
				float _UseStrobe;
				float _UseTwoColor;
				float4x4 unity_Projector;
				float4x4 unity_ProjectorClip;
				half3 projNormal;
				half _AngleLimit;
				half _High;
				half _Low;
				half _Frequency;
				int _Alpha;


				inline half angleBetween(half3 vector1, half3 vector2)
				{
					return acos(dot(vector1, vector2) / (length(vector1) * length(vector2)));
				}

				v2f vert(appdata_tan v, float3 normal : NORMAL)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv = mul(unity_Projector, v.vertex);
					projNormal = mul(unity_Projector, normal);
					o.projAngle = abs(angleBetween(half3(0, 0, -1), projNormal));
					return o;
				}

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 primaryTex = tex2D(_PrimaryTex, i.uv);
					fixed4 secondaryTex = tex2D(_SecondaryTex, i.uv);
					if (_UseTwoColor == 1)
					{
						primaryTex = primaryTex * _PrimaryTintColor;
						secondaryTex = secondaryTex * _SecondaryTintColor;
					}
					else
					{
						secondaryTex = primaryTex * float4(_Low, _Low, _Low, 1) * _PrimaryTintColor;;
						primaryTex = primaryTex * float4(_High, _High, _High, 1) * _PrimaryTintColor;;
					}
					primaryTex.a = 1 - primaryTex.a;
					secondaryTex.a = 1 - secondaryTex.a;

					if (i.uv.w < 0)
					{
						primaryTex = float4(0, 0, 0, 1);
						secondaryTex = float4(0, 0, 0, 1);
					}
					if (!(1 - step(_AngleLimit, i.projAngle)))
					{
						primaryTex = float4(0, 0, 0, 1);
						secondaryTex = float4(0, 0, 0, 1);
					}
					if (_UseStrobe == 1)
					{
						primaryTex = lerp(float4(0, 0, 0, 1), lerp(primaryTex, secondaryTex, (sin(_Time.w * _Frequency) + 1) * .5f), _Alpha / 100.0f);
					}
					else
					{
						primaryTex = lerp(float4(0, 0, 0, 1), primaryTex, _Alpha / 100.0f);
					}


					return primaryTex;
				}
			ENDCG

			}
		}
}