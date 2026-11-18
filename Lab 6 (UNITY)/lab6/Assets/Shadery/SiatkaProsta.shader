// Prosta siatka podłoża (jak siatka w widoku Scene) - granatowe tło + jasne linie zanikające z odległością
Shader "Lab6/SiatkaProsta"
{
    Properties
    {
        _KolorTla ("Kolor tła", Color) = (0.13, 0.2, 0.3, 1)
        _KolorLinii ("Kolor linii", Color) = (0.45, 0.55, 0.68, 1)
        _Rozmiar ("Rozmiar oczka", Float) = 2
        _Zasieg ("Zasięg", Float) = 160
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            float4 _KolorTla, _KolorLinii;
            float _Rozmiar, _Zasieg;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionWS = TransformObjectToWorld(i.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                return o;
            }

            float Linie(float2 uv)
            {
                float2 g = abs(frac(uv - 0.5) - 0.5) / fwidth(uv);
                return 1.0 - min(min(g.x, g.y), 1.0);
            }

            half4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.positionWS.xz / _Rozmiar;
                float linie = Linie(uv) * 0.35 + Linie(uv / 10.0) * 0.6;
                float odl = length(i.positionWS.xz - _WorldSpaceCameraPos.xz);
                float zanik = saturate(1.0 - odl / _Zasieg);
                return half4(lerp(_KolorTla.rgb, _KolorLinii.rgb, linie * zanik), 1.0);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 vert(float4 positionOS : POSITION) : SV_POSITION { return TransformObjectToHClip(positionOS.xyz); }
            half frag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
