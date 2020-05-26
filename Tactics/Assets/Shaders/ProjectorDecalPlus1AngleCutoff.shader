// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Projector/DecalPlus1AngleCutoff"
{
	Properties
	{
		_AlphaTex("Alpha", 2D) = "white" {}
		_TintColor("Tint", Color) = (255,255,255,255)
		_AlphaValue("Alpha Value", Range(0,100)) = 100
		_FalloffTex("FallOff", 2D) = "" {}
		_AngleLimit("Angle Limit (rad)", Range(0,1.58)) = 0
	}

		Subshader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+1" }
		Pass
	{
		ZWrite Off
		Offset -1, -1
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
		float4 uvFalloff : TEXCOORD1;
		half projAngle : TEXCOORD2;
		UNITY_FOG_COORDS(2)
	};

	sampler2D _AlphaTex;
	float4x4 unity_Projector;
	float4x4 unity_ProjectorClip;
	float4 _TintColor;
	int _AlphaValue;
	half3 projNormal;
	sampler2D _FalloffTex;
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
		o.uvFalloff = mul(unity_ProjectorClip, v.vertex);
		projNormal = mul(unity_Projector, normal);
		o.projAngle = abs(angleBetween(half3(0, 0, -1), projNormal));
		UNITY_TRANSFER_FOG(o, o.pos);
		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 tex = tex2D(_AlphaTex, i.uv);
		fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
		fixed4 res = tex * texF.a;
		tex.a = 1 - tex.a;

		if (i.uv.w < 0)
		{
			tex = float4(0,0,0,1);
		}
		if (1 - step(_AngleLimit, i.projAngle))
		{
			tex = tex * _TintColor;
		}
		else {
			tex = float4(0, 0, 0, 1);
		}
		
		tex = lerp(float4(0, 0, 0, 1), tex, _AlphaValue / 100.0f);
		UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(0, 0, 0, 0));
		return tex;
	}
		ENDCG

	}
	}
}