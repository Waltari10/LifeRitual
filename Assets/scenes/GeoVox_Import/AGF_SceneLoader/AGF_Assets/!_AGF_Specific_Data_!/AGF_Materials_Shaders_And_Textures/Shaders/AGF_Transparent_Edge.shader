// Upgrade NOTE: replaced 'PositionFog()' with multiply of UNITY_MATRIX_MVP by position
// Upgrade NOTE: replaced 'V2F_POS_FOG' with 'float4 pos : SV_POSITION'

Shader "AGF/AGF_Transparent_Edge" {
    Properties {
        _Color ("Tint (RGB)", Color) = (1,1,1,1)
        _RampTex ("Facing Ratio Ramp (RGB)", 2D) = "white" {}
    }
    SubShader {
        ZWrite Off
        Tags { "Queue" = "Transparent" }
        Blend One One
        Pass {
            
            CGPROGRAM 
            // profiles arbfp1 
            // vertex vert
            #include "UnityCG.cginc" 

            struct v2f {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD0;
            };
            
            v2f vert (appdata_base v) {
                v2f o;
                o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
                float3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
                o.uv = float4( dot(viewDir,v.normal), 0.5, 0.0, 1.0 );
                return o;
            }
            ENDCG 

            SetTexture [_RampTex] {constantColor[_Color] combine texture * constant} 
        }
    }
    Fallback Off
}