Shader "Unlit/ScreenCutoutShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags{ "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Overlay" }
        Lighting Off
        Cull Back
        ZWrite On
        ZTest Less

        Fog{ Mode Off }

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
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1; // Screen space position
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex); // Transform to clip space
                o.screenPos = ComputeScreenPos(o.vertex); // Compute screen space position
                return o;
            }

            sampler2D _MainTex;

            // Fragment shader
            fixed4 frag(v2f i) : SV_Target
            {
                // Use screen position normalized to [0, 1]
                i.screenPos /= i.screenPos.w;

                // Convert screen position to texture UV space
                fixed4 col = tex2D(_MainTex, float2(i.screenPos.x, i.screenPos.y));

                // Return the texture color
                return col;
            }
            ENDCG
        }
    }
}
