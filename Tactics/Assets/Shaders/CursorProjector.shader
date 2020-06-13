// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/CursorProjector"
{
	Properties
	{
		_PrimaryTex("Primary Texture", 2D) = "white" {}
		_SecondaryTex("Secondary Texture", 2D) = "white" {}
		_AngleLimit("Angle Limit (rad)", Range(0,1.58)) = 0
	}

		Subshader
	{
		Tags { "RenderType" = "Transparent"  "Queue" = "Transparent+101"}
		Pass
		{
			ZWrite Off
			Offset -1, -1

			Fog { Mode Off }

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
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			half3 projNormal;
			half _AngleLimit;

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
				fixed4 secTex = tex2D(_SecondaryTex, i.uv);
				primaryTex.a = 1 - primaryTex.a;
				secTex.a = 1 - secTex.a;

				if (i.uv.w < 0)
				{
					primaryTex = float4(0, 0, 0, 1);
					secTex = float4(0, 0, 0, 1);
				}
				if (!(1 - step(_AngleLimit, i.projAngle))) {
					primaryTex = float4(0, 0, 0, 1);
					secTex = float4(0, 0, 0, 1);
				}

				primaryTex = lerp(primaryTex, secTex, (sin(_Time.w * 1.25) + 1) * .5f);
				return primaryTex;
			}
		ENDCG

		}
	}
}