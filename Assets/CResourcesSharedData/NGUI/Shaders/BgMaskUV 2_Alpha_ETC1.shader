// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "DGM/ItemIcon_BG_Mask_UV 2_Alpha_ETC1"
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
						half4 color : COLOR;
						float2 texcoord : TEXCOORD0;
					};

					struct v2f
					{
						float4 vertex : POSITION;
						half4 color : COLOR;
						float2 texcoord : TEXCOORD0;
						float4 worldPos : TEXCOORD1;
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
					float4 _ClipArgs0 = float4(1000.0, 1000.0, 0.0, 1.0);
					float4 _ClipRange1 = float4(0.0, 0.0, 1.0, 1.0);
					float4 _ClipArgs1 = float4(1000.0, 1000.0, 0.0, 1.0);
					v2f o;
					v2f vert(appdata_t v)
					{
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.color = v.color;
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						o.worldPos.xy = v.vertex.xy * _ClipRange0.zw + _ClipRange0.xy;
						o.worldPos.zw = Rotate(v.vertex.xy, _ClipArgs1.zw) * _ClipRange1.zw + _ClipRange1.xy;
						return o;
					}

					fixed4 frag(v2f IN) : COLOR
					{
						fixed4 col1 = tex2D(_BgTex, IN.texcoord);
						IN.texcoord.x += _OffX;
						IN.texcoord.y += _OffY;
						IN.texcoord = IN.texcoord*float2(1.0 / _ScaleX, 1.0 / _ScaleY);//缩放

						// First clip region
						float2 factor = (float2(1.0, 1.0) - abs(IN.worldPos.xy)) * _ClipArgs0.xy;
							float f = min(factor.x, factor.y);

						// Second clip region
						factor = (float2(1.0, 1.0) - abs(IN.worldPos.zw)) * _ClipArgs1.xy;
						f = min(f, min(factor.x, factor.y));

						// Sample the texture
						half4 col;
						col = tex2D(_MainTex, IN.texcoord);
						col.a = tex2D(_AlphaTex, IN.texcoord).r;
						col = col * IN.color;

						float product0 = clipProduct(IN.worldPos.xy, _Offset40);
						float product = clipProduct(IN.worldPos.zw, _Offset41);
						product = max(product0, product);
						float f1 = clamp(f, 0.0, 1.0);
						col.a *= max(product, f1)*ceil(f1);


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