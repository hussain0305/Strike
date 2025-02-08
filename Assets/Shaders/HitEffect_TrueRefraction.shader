Shader "Abyss/HitEffect_TrueRefraction"
{
    Properties
    {
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _RingWidth ("Ring Width", Range(0, 0.2)) = 0.05
        _TimeMultiplier ("Time Speed", Float) = 1.0
        _FadeDuration ("Fade Duration", Float) = 1.0
        _RefractionStrength ("Refraction Strength", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraOpaqueTexture);  // Grabs the background pixels
            SAMPLER(sampler_CameraOpaqueTexture);

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 screenUV : TEXCOORD1;
            };

            float _RingWidth;
            float _TimeMultiplier;
            float4 _RingColor;
            float _FadeDuration;
            float _RefractionStrength;
            float _StartTime;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);

                // Expand UV space to prevent clipping
                o.uv = (v.uv - 0.5) * 3.0 + 0.5;

                // Screen-space UV for background sampling
                o.screenUV = ComputeScreenPos(o.pos).xy;

                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 dir = normalize(i.uv - center);
                float dist = length(i.uv - center);

                // Time-based growth
                float timeFactor = (_Time.y - _StartTime) * _TimeMultiplier;
                float effectProgress = smoothstep(0, 1, timeFactor);
                float timeOffset = pow(effectProgress, 0.6);

                // 🔥 True Shockwave Refraction: Offset screen UVs
                float refraction = sin(timeFactor * 5.0) * _RefractionStrength;
                float2 screenDistortedUV = i.screenUV + dir * refraction;

                // Sample distorted background pixels
                float4 bgColor = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenDistortedUV);

                // Three expanding rings
                float ring1 = abs(frac(timeOffset) - dist);
                float ring2 = abs(frac(timeOffset - 0.2) - dist);
                float ring3 = abs(frac(timeOffset - 0.4) - dist);

                // Create ring mask
                float ringMask = smoothstep(_RingWidth, 0.0, ring1) +
                                 smoothstep(_RingWidth, 0.0, ring2) +
                                 smoothstep(_RingWidth, 0.0, ring3);

                // Fade out effect
                float alpha = saturate(1.0 - (timeFactor / _FadeDuration));

                // Mix between original background and ring color
                float3 finalColor = lerp(bgColor.rgb, _RingColor.rgb, ringMask * _RingColor.a * alpha);

                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
