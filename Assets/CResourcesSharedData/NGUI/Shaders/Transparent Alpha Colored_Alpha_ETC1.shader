// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Transparent Alpha Colored_Alpha_ETC1"
{
	Properties
	{
		_MainColor("Main Color", color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_MainTex_ALPHA ("Alpha",2D) = "white" {}
	}

		SubShader
	{
		LOD 200

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
				Offset -1,-1
				Blend SrcAlpha OneMinusSrcAlpha

				CGPROGRAM
#pragma vertex vert
#pragma fragment frag			
#include "UnityCG.cginc"
#include "shaderCommon.cginc"

				sampler2D _MainTex;
				fixed4 _MainColor;
				sampler2D _AlphaTex;

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
				};

				v2f o;

				v2f vert(appdata_t v)
				{
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.texcoord = v.texcoord;
					return o;
				}

				fixed4 frag(v2f IN) : COLOR
				{
					half4 col;
					col = _MainColor * tex2D_RGB_A(_MainTex, _AlphaTex, IN.texcoord);
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
				Offset -1,-1
				ColorMask RGB
				Blend SrcAlpha OneMinusSrcAlpha
				ColorMaterial AmbientAndDiffuse

				SetTexture[_MainTex]
				{
					Combine Texture * Primary
				}
			}
	}
}
