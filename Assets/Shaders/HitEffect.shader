Shader "Abyss/HitEffect"
{
    Properties
    {
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _RingWidth ("Ring Width", Range(0, 0.2)) = 0.05
        _TimeMultiplier ("Time Speed", Float) = 1.0
        _FadeDuration ("Fade Duration", Float) = 1.0
        _DistortionStrength ("Distortion Strength", Range(0, 0.1)) = 0.02
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

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _RingWidth;
            float _TimeMultiplier;
            float4 _RingColor;
            float _FadeDuration;
            float _DistortionStrength;
            float _StartTime;

            // Simple pseudo-random function for jitter
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                
                // Expand UVs beyond 0-1 range to prevent cut-off
                o.uv = (v.uv - 0.5) * 3.0 + 0.5;
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = length(i.uv - center);
                
                // Time progression with slowing expansion
                float timeFactor = (_Time.y - _StartTime) * _TimeMultiplier;
                float effectProgress = smoothstep(0, 1, timeFactor);
                float timeOffset = pow(effectProgress, 0.6); // Slows as it expands

                // Jitter effect
                float jitter = (random(i.uv + timeFactor) - 0.5) * _DistortionStrength;

                // Three expanding rings with offsets
                float ring1 = abs(frac(timeOffset + jitter) - dist);
                float ring2 = abs(frac(timeOffset - 0.2 + jitter) - dist);
                float ring3 = abs(frac(timeOffset - 0.4 + jitter) - dist);

                // Create ring mask
                float ringMask = smoothstep(_RingWidth, 0.0, ring1) +
                                 smoothstep(_RingWidth, 0.0, ring2) +
                                 smoothstep(_RingWidth, 0.0, ring3);

                // Fade out effect
                float alpha = saturate(1.0 - (timeFactor / _FadeDuration));

                return float4(_RingColor.rgb, ringMask * _RingColor.a * alpha);
            }
            ENDHLSL
        }
    }
}
