// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DGM/ItemIcon_BG_Filter_Addtive_Alpha_ETC1"
{
	Properties
	{
		_AddBgTex("Alpha (A)", 2D) = "white" {}
		_MainTex("Base (RGB), Alpha (A)", 2D) = "white" {}
		_AlphaTex("Base (RGB), Alpha (A)", 2D) = "white" {}
		_BgAlpha("BgAlpha", Float) = 1.0
		_Brightness("Brightness", Float) = 0.46
	}
	SubShader
		{
			LOD 100

			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Fog{ Mode Off }
			Offset -1,-1
			Blend SrcAlpha OneMinusSrcAlpha

			Pass
				{
					CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"
#include "shaderCommon.cginc"

					struct appdata_t
					{
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
						fixed4 color : COLOR;
					};

					struct v2f
					{
						float4 vertex : SV_POSITION;
						half2 texcoord : TEXCOORD0;
						fixed4 color : COLOR;
						//half2  uv      	: TEXCOORD0;
					};

					sampler2D _AddBgTex;
					sampler2D _MainTex;
					half4     _MainTex_ST;
					sampler2D _AlphaTex;
					fixed _BgAlpha;
					fixed _Brightness;

					v2f vert(appdata_t v)
					{
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);
						//o.texcoord = v.texcoord;
						o.color = v.color;
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						return o;
					}

					fixed4 frag(v2f i) : COLOR
					{
						fixed4 textureColor1 = tex2D(_AddBgTex, i.texcoord) * i.color;
						fixed4 textureColor2 = tex2D_RGB_A(_MainTex, _AlphaTex, i.texcoord);
						float2 al = textureColor2.a;
							fixed4 colMask = (textureColor2 + textureColor1 * _BgAlpha)* _Brightness;// *0.46;
						colMask.a = al;
						return colMask;
					}
						ENDCG
				}
		}
		SubShader
					{
						LOD 100

						Tags
						{
							"Queue" = "Transparent"
							"IgnoreProjector" = "True"
							"RenderType" = "Transparent"
						}

						Pass
							{
								Cull Off
								Lighting Off
								ZWrite Off
								Fog{ Mode Off }
								ColorMask RGB
								Blend SrcAlpha OneMinusSrcAlpha
								ColorMaterial AmbientAndDiffuse
								SetTexture[_AlphaTex] {combine texture}
								SetTexture[_MainTex] {combine texture, previous}
							}
					}
					Fallback "Unlit/Text"
}