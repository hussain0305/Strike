Shader "Abyss/VerticalGradient"
{
    Properties
    {
        _ColorTop ("Top Color", Color) = (1, 0, 0, 1) // Red by default
        _ColorBottom ("Bottom Color", Color) = (0, 0, 1, 1) // Blue by default
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Properties from the inspector
            fixed4 _ColorTop;
            fixed4 _ColorBottom;

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);

                // Use the y-component of the position in world space
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv.y = worldPos.y;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Normalize the UV to range between 0 and 1
                float gradientFactor = saturate((i.uv.y + 1.0) * 0.5); // Normalized from -1 to 1 => 0 to 1
                return lerp(_ColorBottom, _ColorTop, gradientFactor);
            }
            ENDCG
        }
    }
}
