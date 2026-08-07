Shader "KingmakerGunslinger/DoubleSidedDiffuse"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150
        Cull Off
        CGPROGRAM
        #pragma surface surf Lambert
        sampler2D _MainTex;
        fixed4 _Color;
        struct Input { float2 uv_MainTex; };
        void surf(Input input, inout SurfaceOutput output)
        {
            fixed4 color = tex2D(_MainTex, input.uv_MainTex) * _Color;
            output.Albedo = color.rgb;
            output.Alpha = color.a;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
