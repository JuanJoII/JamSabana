Shader "Custom/ToonShader_URP"
{
    Properties
    {
        [Header(Base)]
        _MainTex        ("Albedo Texture", 2D)      = "white" {}
        [HDR] _Color    ("Albedo Color", Color)     = (1,1,1,1)

        [Header(Toon Shading)]
        _RampTex        ("Ramp Texture (1D)", 2D)   = "white" {}
        _ShadowColor    ("Shadow Color", Color)     = (0.9,0.8,1,1)
        _ShadowThreshold("Shadow Threshold", Range(0,1)) = 0.5
        _ShadowSmooth   ("Shadow Smoothness", Range(0,0.5)) = 0.05

        [Header(Specular)]
        _SpecularColor  ("Specular Color", Color)   = (1,1,1,1)
        _Glossiness     ("Glossiness", Range(1,256)) = 64
        _SpecularThreshold("Specular Threshold", Range(0,1)) = 0.6
        _SpecularSmooth ("Specular Smoothness", Range(0,0.1)) = 0.0

        [Header(Rim Light)]
        [HDR]_RimColor  ("Rim Color", Color)        = (1,1,1,1)
        _RimThreshold   ("Rim Threshold", Range(0,1)) = 0.7
        _RimSmooth      ("Rim Smoothness", Range(0,0.1)) = 0.05
        _RimIntensity   ("Rim Intensity", Range(0,3)) = 1.0

        [Header(Outline)]
        _OutlineColor   ("Outline Color", Color)    = (0,0,0,1)
        _OutlineWidth   ("Outline Width", Range(0,0.1)) = 0.003

        [Header(Emission)]
        [HDR]_EmissionColor("Emission Color", Color) = (0,0,0,1)
        _EmissionMap    ("Emission Map", 2D)         = "black" {}

        [Header(Stencil)]
        _Stencil        ("Stencil ID", Float)        = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]
        _StencilComp    ("Stencil Comp", Int)        = 8
        [Enum(UnityEngine.Rendering.StencilOp)]
        _StencilOp      ("Stencil Op", Int)          = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"  = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue"       = "Geometry"
        }

        // ─────────────────────────────────────────
        // PASS 1 — Outline (back-face extrusion)
        // ─────────────────────────────────────────
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            Stencil
            {
                Ref   [_Stencil]
                Comp  [_StencilComp]
                Pass  [_StencilOp]
            }

            HLSLPROGRAM
            #pragma vertex   vert_outline
            #pragma fragment frag_outline
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float  _OutlineWidth;
                // declara el resto para evitar warnings de CBUFFER
                float4 _Color;
                float4 _MainTex_ST;
                float4 _ShadowColor;
                float  _ShadowThreshold;
                float  _ShadowSmooth;
                float4 _SpecularColor;
                float  _Glossiness;
                float  _SpecularThreshold;
                float  _SpecularSmooth;
                float4 _RimColor;
                float  _RimThreshold;
                float  _RimSmooth;
                float  _RimIntensity;
                float4 _EmissionColor;
            CBUFFER_END

            struct Attributes_O
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };
            struct Varyings_O
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings_O vert_outline(Attributes_O v)
            {
                Varyings_O o;
                // Extruye vértices en dirección de la normal
                float3 pos = v.positionOS.xyz + normalize(v.normalOS) * _OutlineWidth;
                o.positionHCS = TransformObjectToHClip(pos);
                return o;
            }
            half4 frag_outline(Varyings_O i) : SV_Target
            {
                return half4(_OutlineColor.rgb, 1);
            }
            ENDHLSL
        }

        // ─────────────────────────────────────────
        // PASS 2 — Toon Lit (luz URP real)
        // ─────────────────────────────────────────
        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back

            Stencil
            {
                Ref   [_Stencil]
                Comp  [_StencilComp]
                Pass  [_StencilOp]
            }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            // Keywords URP necesarios
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
            TEXTURE2D(_RampTex);    SAMPLER(sampler_RampTex);
            TEXTURE2D(_EmissionMap);SAMPLER(sampler_EmissionMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShadowColor;
                float  _ShadowThreshold;
                float  _ShadowSmooth;
                float4 _SpecularColor;
                float  _Glossiness;
                float  _SpecularThreshold;
                float  _SpecularSmooth;
                float4 _RimColor;
                float  _RimThreshold;
                float  _RimSmooth;
                float  _RimIntensity;
                float4 _EmissionColor;
                float4 _OutlineColor;
                float  _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float  fogFactor   : TEXCOORD3;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs posInputs = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(v.normalOS);

                o.positionHCS = posInputs.positionCS;
                o.positionWS  = posInputs.positionWS;
                o.normalWS    = nrmInputs.normalWS;
                o.uv          = TRANSFORM_TEX(v.uv, _MainTex);
                o.fogFactor   = ComputeFogFactor(posInputs.positionCS.z);
                return o;
            }

            // Stepped con smoothstep para no tener aliasing
            half ToonStep(half threshold, half smooth, half value)
            {
                return smoothstep(threshold - smooth, threshold + smooth, value);
            }

            half4 frag(Varyings i) : SV_Target
            {
                // ── Base color ────────────────────────────
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv) * _Color;

                float3 N = normalize(i.normalWS);
                float3 V = normalize(GetCameraPositionWS() - i.positionWS);

                // ── Luz principal URP ─────────────────────
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(i.positionWS));
                float3 L = normalize(mainLight.direction);
                float3 H = normalize(L + V);

                float  NdotL    = dot(N, L);
                float  NdotH    = dot(N, H);
                float  NdotV    = dot(N, V);
                float  shadow   = mainLight.shadowAttenuation;

                // ── Diffuse toon ──────────────────────────
                half   rampU    = NdotL * 0.5 + 0.5;           // remapea [-1,1] → [0,1]
                half   rampSample = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex,
                                    float2(rampU, 0.5)).r;
                half   toonDiff = ToonStep(_ShadowThreshold, _ShadowSmooth,
                                           rampSample * shadow);
                float3 diffuse  = lerp(_ShadowColor.rgb, mainLight.color, toonDiff);

                // ── Specular toon ─────────────────────────
                half   spec     = pow(max(NdotH, 0), _Glossiness);
                half   toonSpec = ToonStep(_SpecularThreshold, _SpecularSmooth, spec);
                float3 specular = _SpecularColor.rgb * toonSpec * mainLight.color;

                // ── Rim light ─────────────────────────────
                half   rim      = 1.0 - NdotV;
                half   toonRim  = ToonStep(_RimThreshold, _RimSmooth, rim);
                float3 rimLight = _RimColor.rgb * toonRim * _RimIntensity;

                // ── Luces adicionales (point/spot) ────────
                float3 additionalDiffuse = 0;
                #ifdef _ADDITIONAL_LIGHTS
                uint addLightCount = GetAdditionalLightsCount();
                for (uint li = 0; li < addLightCount; li++)
                {
                    Light addLight = GetAdditionalLight(li, i.positionWS);
                    float addNdotL = dot(N, normalize(addLight.direction));
                    half  addToon  = ToonStep(_ShadowThreshold, _ShadowSmooth,
                                             addNdotL * addLight.distanceAttenuation);
                    additionalDiffuse += addLight.color * addToon;
                }
                #endif

                // ── Emission ──────────────────────────────
                half3 emission = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap,
                                 i.uv).rgb * _EmissionColor.rgb;

                // ── Componer ──────────────────────────────
                half3 color = albedo.rgb * (diffuse + additionalDiffuse)
                            + specular
                            + rimLight
                            + emission;

                // ── Fog ───────────────────────────────────
                color = MixFog(color, i.fogFactor);

                return half4(color, albedo.a);
            }
            ENDHLSL
        }

        // ─────────────────────────────────────────
        // PASS 3 — Shadow Caster (URP)
        // ─────────────────────────────────────────
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest  LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert_shadow
            #pragma fragment frag_shadow
            #pragma multi_compile_shadowcaster
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShadowColor;
                float  _ShadowThreshold; float _ShadowSmooth;
                float4 _SpecularColor;   float _Glossiness;
                float  _SpecularThreshold; float _SpecularSmooth;
                float4 _RimColor;        float _RimThreshold;
                float  _RimSmooth;       float _RimIntensity;
                float4 _EmissionColor;   float4 _OutlineColor;
                float  _OutlineWidth;
            CBUFFER_END

            struct Attr_S { float4 positionOS:POSITION; float3 normalOS:NORMAL; float2 uv:TEXCOORD0; };
            struct Vary_S { float4 positionHCS:SV_POSITION; };

            Vary_S vert_shadow(Attr_S v)
            {
                Vary_S o;
                float3 posWS  = TransformObjectToWorld(v.positionOS.xyz);
                float3 nrmWS  = TransformObjectToWorldNormal(v.normalOS);
                float4 posCS  = TransformWorldToHClip(ApplyShadowBias(posWS, nrmWS, 0));
                #if UNITY_REVERSED_Z
                    posCS.z = min(posCS.z, posCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    posCS.z = max(posCS.z, posCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif
                o.positionHCS = posCS;
                return o;
            }
            half4 frag_shadow(Vary_S i) : SV_Target { return 0; }
            ENDHLSL
        }

        // ─────────────────────────────────────────
        // PASS 4 — Depth Normals (para SSAO, etc.)
        // ─────────────────────────────────────────
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex   vert_dn
            #pragma fragment frag_dn
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST; float4 _Color;
                float4 _ShadowColor; float _ShadowThreshold; float _ShadowSmooth;
                float4 _SpecularColor; float _Glossiness;
                float _SpecularThreshold; float _SpecularSmooth;
                float4 _RimColor; float _RimThreshold; float _RimSmooth; float _RimIntensity;
                float4 _EmissionColor; float4 _OutlineColor; float _OutlineWidth;
            CBUFFER_END

            struct Attr_D  { float4 posOS:POSITION; float3 normOS:NORMAL; };
            struct Vary_D  { float4 posHCS:SV_POSITION; float3 normWS:TEXCOORD0; };

            Vary_D vert_dn(Attr_D v)
            {
                Vary_D o;
                o.posHCS  = TransformObjectToHClip(v.posOS.xyz);
                o.normWS  = TransformObjectToWorldNormal(v.normOS);
                return o;
            }
            half4 frag_dn(Vary_D i) : SV_Target
            {
                return half4(normalize(i.normWS) * 0.5 + 0.5, 1);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}