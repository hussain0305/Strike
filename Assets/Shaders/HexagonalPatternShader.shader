Shader "Abyss/HexagonalPatternShader"
{
    Properties
    {
        _MainTex ("iChannel0", 2D) = "white" {}
        _SecondTex ("iChannel1", 2D) = "white" {}
        _ThirdTex ("iChannel2", 2D) = "white" {}
        _FourthTex ("iChannel3", 2D) = "white" {}
        _Mouse ("Mouse", Vector) = (0.5, 0.5, 0.5, 0.5)
        [ToggleUI] _GammaCorrect ("Gamma Correction", Float) = 1
        _Resolution ("Resolution (Change if AA is bad)", Range(1, 1024)) = 1
        _Scale ("Pattern Scale", Float) = 1.0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Built-in properties
            sampler2D _MainTex;   float4 _MainTex_TexelSize;
            sampler2D _SecondTex; float4 _SecondTex_TexelSize;
            sampler2D _ThirdTex;  float4 _ThirdTex_TexelSize;
            sampler2D _FourthTex; float4 _FourthTex_TexelSize;
            float4 _Mouse;
            float _GammaCorrect;
            float _Resolution;
            float _Scale;

            // GLSL Compatability macros
            #define glsl_mod(x,y) (((x)-(y)*floor((x)/(y))))
            #define texelFetch(ch, uv, lod) tex2Dlod(ch, float4((uv).xy * ch##_TexelSize.xy + ch##_TexelSize.xy * 0.5, 0, lod))
            #define textureLod(ch, uv, lod) tex2Dlod(ch, float4(uv, 0, lod))
            #define iResolution float3(_Resolution, _Resolution, _Resolution)
            #define iFrame (floor(_Time.y / 60))
            #define iChannelTime float4(_Time.y, _Time.y, _Time.y, _Time.y)
            #define iDate float4(2020, 6, 18, 30)
            #define iSampleRate (44100)
            #define iChannelResolution float4x4(                      \
                _MainTex_TexelSize.z,   _MainTex_TexelSize.w,   0, 0, \
                _SecondTex_TexelSize.z, _SecondTex_TexelSize.w, 0, 0, \
                _ThirdTex_TexelSize.z,  _ThirdTex_TexelSize.w,  0, 0, \
                _FourthTex_TexelSize.z, _FourthTex_TexelSize.w, 0, 0)

            // Global access to uv data
            static v2f vertex_output;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

#define S(r, v) smoothstep(9./iResolution.y, 0., abs(v-r))
            static const float2 s = float2(1, 1.7320508);
            static const float3 baseCol = float3(0.05098, 0.25098, 0.2784);
            static const float borderThickness = 0.02;
            static const float isolineOffset = 0.4;
            static const float isolineOffset2 = 0.325;
            float calcHexDistance(float2 p)
            {
                p = abs(p);
                return max(dot(p, s*0.5), p.x);
            }

            float random(float2 co)
            {
                return frac(sin(dot(co.xy, float2(12.9898, 78.233)))*43758.547);
            }

            float4 calcHexInfo(float2 uv)
            {
                float4 hexCenter = round(float4(uv, uv-float2(0.5, 1.))/s.xyxy);
                float4 offset = float4(uv-hexCenter.xy*s, uv-(hexCenter.zw+0.5)*s);
                return dot(offset.xy, offset.xy)<dot(offset.zw, offset.zw) ? float4(offset.xy, hexCenter.xy) : float4(offset.zw, hexCenter.zw);
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                
                float2 uv = 3.*(2.*fragCoord - iResolution.xy) / (iResolution.y * _Scale);
                uv.x += _Time.y * 0.25;

                float4 hexInfo = calcHexInfo(uv);
                float totalDist = calcHexDistance(hexInfo.xy) + borderThickness;
                float rand = random(hexInfo.zw);
                float angle = atan2(hexInfo.y, hexInfo.x) + rand * 5. + _Time.y;
                float3 isoline = S(isolineOffset, totalDist) * baseCol * step(3. + rand * 0.5, glsl_mod(angle, 6.28)) + S(isolineOffset2, totalDist) * baseCol * step(4. + rand * 1.5, glsl_mod(angle + rand * 2., 6.28));
                float sinOffset = sin(_Time.y + rand * 8.);
                float aa = 5. / iResolution.y;
                fragColor.rgb = (smoothstep(0.51, 0.51 - aa, totalDist) + pow(1. - max(0., 0.5 - totalDist), 20.) * 1.5) * (baseCol + rand * float3(0., 0.1, 0.09)) + isoline + baseCol * smoothstep(0.2 + sinOffset, 0.2 + sinOffset - aa, totalDist);
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            ENDCG
        }
    }
}
