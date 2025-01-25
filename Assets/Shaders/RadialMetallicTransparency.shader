Shader "Abyss/RadialMetallicTransparency"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {} // Base texture
        _Color ("Base Color", Color) = (1,1,1,1) // Tint color
        _InnerAlpha ("Inner Alpha", Range(0, 1)) = 1.0 // Transparency at center
        _OuterAlpha ("Outer Alpha", Range(0, 1)) = 0.5 // Transparency at edges
        _MetallicToggle ("Enable Metallic Appearance", Float) = 0 // Metallic toggle (0 = Off, 1 = On)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            fixed4 _Color;
            float _InnerAlpha;
            float _OuterAlpha;
            float _MetallicToggle;

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the base texture
                fixed4 texColor = tex2D(_MainTex, i.uv) * _Color;

                // Calculate distance from center (radial gradient)
                float2 uvCentered = i.uv - float2(0.5, 0.5); // UV centered at (0.5, 0.5)
                float dist = length(uvCentered) * 2.0; // Scale to normalize distance [0-1]

                // Lerp between inner and outer alpha
                float alpha = lerp(_InnerAlpha, _OuterAlpha, saturate(dist));

                // Apply metallic appearance if enabled
                if (_MetallicToggle > 0.5)
                {
                    // Simulate metallic effect with highlights
                    float highlight = smoothstep(0.3, 0.0, dist); // Highlight near center
                    float edgeHighlight = smoothstep(1.0, 0.8, dist); // Highlight near edges
                    texColor.rgb += highlight * 0.2 + edgeHighlight * 0.1; // Add subtle highlights
                }

                // Combine texture color with alpha
                texColor.a *= alpha;

                return texColor;
            }
            ENDCG
        }
    }

    FallBack "Transparent/Diffuse"
}
