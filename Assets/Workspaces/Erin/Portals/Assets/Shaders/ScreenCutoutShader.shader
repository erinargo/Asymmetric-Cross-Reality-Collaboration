Shader "Unlit/ScreenCutoutShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Inflation("Inflation", float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Overlay" }
        Lighting Off
        Cull Back
        ZWrite On
        ZTest Less
        
        LOD 100

        Fog { Mode Off }

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
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1; // Screen space position
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _Inflation;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                // Apply inflation to vertices
                o.vertex = UnityObjectToClipPos(v.vertex + v.normal * _Inflation);
                
                // Calculate screen position (Unity handles stereo automatically here)
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            sampler2D _MainTex;

            // Fragment shader
            fixed4 frag(v2f i) : SV_Target
            {
                // Use screen position normalized to [0, 1]
                i.screenPos /= i.screenPos.w;

                // Convert screen position to texture UV space
                fixed4 col = tex2D(_MainTex, i.screenPos.xy);

                // Return the texture color
                return col;
            }
            ENDCG
        }
    }
}
