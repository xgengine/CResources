#ifndef SHADER_COMMON_CG_INCLUDED
#define SHADER_COMMON_CG_INCLUDED

//mainTex store the rgb, alphaTex store the alpha, get the rgba compositely.
fixed4 tex2D_RGB_A(sampler2D mainTex, sampler2D alphaTex, half2 texcoord)
{
    fixed4 color = tex2D(mainTex, texcoord);
    fixed4 alphaColor = tex2D(alphaTex, texcoord);
    color.a = alphaColor.r;

    return color;
}

#endif
