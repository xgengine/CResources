#ifndef CLIPSHADER_COMMON_CG_INCLUDED
#define CLIPSHADER_COMMON_CG_INCLUDED

float4 _Offset40 = float4(0.0, 0.0, 0.0, 0.0);
float4 _Offset41 = float4(0.0, 0.0, 0.0, 0.0);

float2 Rotate (float2 v, float2 rot)
{
	float2 ret;
	ret.x = v.x * rot.y - v.y * rot.x;
	ret.y = v.x * rot.x + v.y * rot.y;
	return ret;
}

float clipProduct(float2 worldPos, float4 offset4)
{
    float a1 = ceil(max((worldPos.x- offset4.x), 0));
    float a2 =  ceil(max(( offset4.z-worldPos.x), 0));
    float a3 = ceil(max((worldPos.y- offset4.y), 0));
    float a4 =  ceil(max((offset4.w-worldPos.y), 0));
    float product = min(a1*a2*a3*a4, 1.0f);
	return product;
}

#endif
