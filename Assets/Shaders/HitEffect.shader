Shader "Abyss/HitEffect"
{
    Properties
    {
        _CircleColor ("Circle Color", Color) = (1,1,1,1)
        _CircleSize ("Circle Size", Range(0, 1)) = 0.3
        _CircleFadeDuration ("Circle Fade Duration", Float) = 1.0
        
        _RingColor ("Ring Color", Color) = (1,1,1,1)
        _RingCount ("Ring Count", Int) = 4
        _ExpansionDuration ("Expansion Duration", Float) = 1.0
        _RingFadeDuration ("Ring Fade Duration", Float) = 1.0
        _RingSpawnInterval ("Ring Spawn Interval", Float) = 0.1
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

            float4 _CircleColor;
            float _CircleSize;
            float _CircleFadeDuration;

            float4 _RingColor;
            int _RingCount;
            float _ExpansionDuration;
            float _RingFadeDuration;
            float _RingSpawnInterval;
            float _StartTime;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = (v.uv - 0.5) * 3.0 + 0.5; // Expanding UV range for proper effect
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float dist = length(i.uv - center);
                float timeFactor = (_Time.y - _StartTime);

                // --- Center Circle Effect ---
                float circleAlpha = smoothstep(_CircleSize * 1.2, _CircleSize, dist);
                circleAlpha *= saturate(1.0 - (timeFactor / _CircleFadeDuration));
                float4 circleEffect = float4(_CircleColor.rgb, _CircleColor.a * circleAlpha);
                
                // --- Expanding Rings Effect ---
                float rings = 0.0;
                for (int j = 0; j < _RingCount; j++)
                {
                    float ringStartTime = j * _RingSpawnInterval;
                    float progress = saturate((timeFactor - ringStartTime) / _ExpansionDuration);
                    float ringRadius = pow(progress, 0.6); // Expands fast then slows
                    float ringFade = saturate(1.0 - (progress / _RingFadeDuration));
                    float ring = smoothstep(0.02, 0.0, abs(ringRadius - dist));
                    rings += ring * ringFade;
                }
                float4 ringEffect = float4(_RingColor.rgb, _RingColor.a * rings);

                return circleEffect + ringEffect;
            }
            ENDHLSL
        }
    }
}
