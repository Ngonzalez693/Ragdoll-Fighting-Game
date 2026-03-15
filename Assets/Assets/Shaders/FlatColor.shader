Shader "Custom/CartoonShading"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ColorBoost ("Color Boost", Range(0,2)) = 1.2 // Increases color intensity
        _ShadowColor ("Shadow Color", Color) = (0.5, 0.5, 0.5, 1) // Slightly colorful shadows
        _Threshold ("Light Threshold", Range(0,1)) = 0.6 // Controls cel shading sharpness
        _OutlineThickness ("Outline Thickness", Range(0,0.05)) = 0.02 // For outlines
        _OutlineColor ("Outline Color", Color) = (0,0,0,1) // Black outlines
    }
    SubShader
    {
        Tags {"Queue"="Geometry"}
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
            };

            sampler2D _MainTex;
            float _ColorBoost;
            fixed4 _ShadowColor;
            float _Threshold;
            float _OutlineThickness;
            fixed4 _OutlineColor;

            // Function to boost colors
            fixed4 BoostColors(fixed4 color)
            {
                float3 boosted = pow(color.rgb, 1.0 / _ColorBoost); // Increases vibrancy
                return fixed4(boosted, color.a);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = normalize(WorldSpaceViewDir(v.vertex));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample texture and boost color vibrancy
                fixed4 texColor = tex2D(_MainTex, i.uv);
                texColor = BoostColors(texColor);

                // Simple cel shading based on light direction
                float lightIntensity = saturate(dot(i.worldNormal, float3(0, 1, 0)));
                float shadowStep = step(_Threshold, lightIntensity);

                // Colorful shadowing
                fixed4 finalColor = lerp(_ShadowColor, texColor, shadowStep);

                // Outline effect (thicker edges for cartoon effect)
                float edge = dot(i.viewDir, i.worldNormal);
                edge = smoothstep(0.2, 0.3, edge);
                finalColor = lerp(_OutlineColor, finalColor, edge + _OutlineThickness);

                return finalColor;
            }
            ENDCG
        }
    }
}
