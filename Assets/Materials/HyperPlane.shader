Shader "Custom/HyperPlane"
{
    Properties
    {
        _Color ("Base Color", Color) = (1,1,1,0.2) // Set alpha to 0.5 for translucency
        _GridSpacing ("Grid Spacing", Float) = 0.1
        _GridThickness ("Grid Thickness", Float) = 0.01
        _GridColor ("Grid Color", Color) = (0,0,0,1) // Black grid lines
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True"}
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha // Enable blending for transparency
        ZWrite Off // Disable depth writing for proper transparency handling
        Cull Off // Render both sides of the plane

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            fixed _GridSpacing;
            fixed _GridThickness;
            fixed4 _GridColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.vertex.xy; // Assuming xy plane, adjust if necessary
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Determine the fractional part of the uv coordinates within each grid cell
                float2 gridUV = fmod(abs(i.uv), _GridSpacing);
                // Check if we're within the thickness threshold to render a grid line
                bool inGridLine = gridUV.x < _GridThickness || gridUV.y < _GridThickness;
                // Mix the base color and grid color based on whether we're in a grid line
                fixed4 color = lerp(_Color, _GridColor, inGridLine ? 1 : 0);
                return color;
            }
            ENDCG
        }
    }
}
