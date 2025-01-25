Shader "Abyss/VerticalGradientUI"
{
    Properties
    {
        _ColorTop ("Top Color", Color) = (1, 0, 0, 1) // Red
        _ColorBottom ("Bottom Color", Color) = (0, 0, 1, 1) // Blue
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "RenderType" = "Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Properties
            fixed4 _ColorTop;
            fixed4 _ColorBottom;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0; // UV coordinates, if available
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex); // Calculate world-space position
                o.uv = v.uv; // Pass the UV coordinates to the fragment shader
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // The y-coordinate of the UV will determine the gradient (0 at the bottom, 1 at the top)
                float gradientFactor = i.uv.y; // Use the vertical UV coordinate (0 at the bottom, 1 at the top)
                return lerp(_ColorBottom, _ColorTop, gradientFactor); // Linearly interpolate between the colors
            }
            ENDCG
        }
    }
}
