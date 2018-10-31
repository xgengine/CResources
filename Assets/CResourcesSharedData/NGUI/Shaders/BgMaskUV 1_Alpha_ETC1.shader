// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DGM/ItemIcon_BG_Mask_UV 1_Alpha_ETC1"
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
#include "clipShaderCommon.cginc"

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
						float2 worldPos : TEXCOORD1;
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
					float4 _ClipRange0 = float4(0.0, 0.0, 1.0, 1.0);
					float2 _ClipArgs0 = float2(1000.0, 1000.0);
					v2f o;
					v2f vert(appdata_t v)
					{
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.color = v.color;
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.worldPos = v.vertex.xy * _ClipRange0.zw + _ClipRange0.xy;
						return o;
					}

					fixed4 frag(v2f IN) : COLOR
					{
						fixed4 col1 = tex2D(_BgTex, IN.texcoord);
						IN.texcoord.x += _OffX;
						IN.texcoord.y += _OffY;
						IN.texcoord = IN.texcoord*float2(1.0 / _ScaleX, 1.0 / _ScaleY);//缩放

						// Softness factor
						float2 factor = (float2(1.0, 1.0) - abs(IN.worldPos)) * _ClipArgs0;

						// Sample the texture
						half4 col;
						col = tex2D(_MainTex, IN.texcoord);
						col.a = tex2D(_AlphaTex, IN.texcoord).r;
						col = col  * IN.color;
						//				col.a *= clamp( min(factor.x, factor.y), 0.0, 1.0);

						float product = clipProduct(IN.worldPos, _Offset40);
						col.a *= max(product, clamp(min(factor.x, factor.y), 0.0, 1.0));


						

						col.a = col.a * col1.g;
						return col;
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