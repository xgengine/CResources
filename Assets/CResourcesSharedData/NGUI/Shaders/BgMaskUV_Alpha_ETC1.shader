// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DGM/ItemIcon_BG_Mask_UV_Alpha_ETC1"
{
	Properties
	{
		_BgTex("Alpha (A)", 2D) = "white" {}
		_MainTex("Base (RGB), Alpha (A)", 2D) = "white" {}
		_AlphaTex("Base (RGB), Alpha (A)", 2D) = "white" {}
		_ScaleX("scaleX", float) = 1//X方向 
			_ScaleY("scaleY", float) = 1
			_OffX("offx", float) = 0
			_OffY("offy", float) = 0
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

					sampler2D _BgTex;
					sampler2D _MainTex;
					half4     _MainTex_ST;
					sampler2D _AlphaTex;
					float _ScaleX;
					float _ScaleY;
					float _OffX;
					float _OffY;

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
						fixed4 col1 = tex2D(_BgTex, i.texcoord) * i.color;
						i.texcoord.x += _OffX;
						i.texcoord.y += _OffY;
						i.texcoord = i.texcoord*float2(1.0 / _ScaleX, 1.0 / _ScaleY);//缩放
						fixed4 colMask = tex2D_RGB_A(_MainTex, _AlphaTex, i.texcoord);
						colMask.a = colMask.a * col1.g;
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