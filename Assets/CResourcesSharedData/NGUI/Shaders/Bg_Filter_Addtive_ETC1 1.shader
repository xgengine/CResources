// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/DGM/ItemIcon_BG_Filter_Addtive 1_Alpha_ETC1"
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
						float2 worldPos : TEXCOORD1;
					};

					fixed _BgAlpha;
					fixed _Brightness;
					sampler2D _AddBgTex;
					sampler2D _MainTex;
					sampler2D _AlphaTex;

					float4 _ClipRange0 = float4(0.0, 0.0, 1.0, 1.0);
					float2 _ClipArgs0 = float2(1000.0, 1000.0);
					v2f o;

					v2f vert(appdata_t v)
					{
						o.vertex = UnityObjectToClipPos(v.vertex);
						o.color = v.color;
						o.texcoord = v.texcoord;
						o.worldPos = v.vertex.xy * _ClipRange0.zw + _ClipRange0.xy;
						return o;
					}

					half4 frag(v2f IN) : COLOR
					{

						// Softness factor
						float2 factor = (float2(1.0, 1.0) - abs(IN.worldPos)) * _ClipArgs0;

						
						half4 col;
						col = tex2D(_MainTex, IN.texcoord);
						col.a = tex2D(_AlphaTex, IN.texcoord).r;
						col = col  * IN.color;

						float product = clipProduct(IN.worldPos, _Offset40);
						col.a *= max(product, clamp(min(factor.x, factor.y), 0.0, 1.0));

						// Sample the texture
						fixed4 textureColor1 = tex2D(_AddBgTex, IN.texcoord) * IN.color;
						float2 al = col.a;
						fixed4 colMask = (col + textureColor1 * _BgAlpha)* _Brightness;// textureColor2* textureColor1 *  _Brightness; // (textureColor2 + textureColor1 * _BgAlpha)* _Brightness;// *0.46;
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