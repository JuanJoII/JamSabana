Shader "Custom/AimLineURP"
{
    Properties
    {
        [Header(Color)]
        [HDR] _Color ("Color principal", Color) = (1, 0.35, 0.1, 1)
        [HDR] _CoreColor ("Color del centro (brillo)", Color) = (1, 0.9, 0.6, 1)
        _Intensity ("Intensidad", Range(0, 5)) = 1.5

        [Header(Dashes)]
        _DashAmount ("Cantidad de dashes (0 = linea solida)", Range(0, 1)) = 1
        _DashCount ("Dashes por linea", Float) = 12
        _DashFill ("Relleno del dash", Range(0.05, 1)) = 0.6
        _DashSoft ("Suavidad del dash", Range(0.001, 0.5)) = 0.15
        _Speed ("Velocidad de movimiento", Float) = 1.5

        [Header(Shape)]
        _EdgeSoft ("Suavidad de bordes (ancho)", Range(0.01, 1)) = 0.5
        _CoreWidth ("Ancho del centro brillante", Range(0.05, 1)) = 0.35
        _FadeIn ("Fade al inicio", Range(0, 0.5)) = 0.05
        _FadeOut ("Fade al final", Range(0, 0.5)) = 0.1

        [Header(Pulse)]
        _PulseSpeed ("Velocidad del pulso", Float) = 4
        _PulseAmount ("Fuerza del pulso", Range(0, 1)) = 0.2

        [Header(Render)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend (10 = alpha, 1 = additive)", Float) = 1
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "AimLine"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            ZTest [_ZTest]
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _CoreColor;
                float _Intensity;
                float _DashAmount;
                float _DashCount;
                float _DashFill;
                float _DashSoft;
                float _Speed;
                float _EdgeSoft;
                float _CoreWidth;
                float _FadeIn;
                float _FadeOut;
                float _PulseSpeed;
                float _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // uv.x = largo de la linea, uv.y = ancho (0..1)
                float across = abs(IN.uv.y * 2.0 - 1.0);

                // Dashes que se mueven hacia adelante
                float f = frac(IN.uv.x * _DashCount - _Time.y * _Speed);
                float dash = smoothstep(0.0, _DashSoft, f) *
                             (1.0 - smoothstep(_DashFill - _DashSoft, _DashFill, f));
                dash = lerp(1.0, dash, _DashAmount);

                // Bordes suaves a lo ancho
                float edge = 1.0 - smoothstep(1.0 - _EdgeSoft, 1.0, across);

                // Centro brillante
                float core = 1.0 - smoothstep(0.0, _CoreWidth, across);

                // Fade en las puntas
                float fadeIn = (_FadeIn > 0.0001) ? smoothstep(0.0, _FadeIn, IN.uv.x) : 1.0;
                float fadeOut = (_FadeOut > 0.0001) ? (1.0 - smoothstep(1.0 - _FadeOut, 1.0, IN.uv.x)) : 1.0;

                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;

                float3 col = lerp(_Color.rgb, _CoreColor.rgb, core) * _Intensity * pulse;
                col *= IN.color.rgb; // color del Gradient del LineRenderer

                float alpha = dash * edge * fadeIn * fadeOut * _Color.a * IN.color.a;

                return half4(col, saturate(alpha));
            }
            ENDHLSL
        }
    }

    Fallback Off
}
