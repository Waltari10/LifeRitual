Shader "AGF/AGF_Rim_Lighting_02"
{
	Properties 
	{
_MainTex("_MainTex", 2D) = "black" {}
_NormalMap("_NormalMap", 2D) = "bump" {}
_Glossiness("_Glossiness", Float) = 100
_SpecularColor("_SpecularColor", Color) = (1,1,1,1)
_SpecIntensity("_SpecIntensity", Float) = 1
_SpecDirection("_SpecDirection", Vector) = (-1,1,0,0)
_SelfIllumination("_SelfIllumination", Range(0,1) ) = 0
_RimlightColor("_RimlightColor", Color) = (1,1,1,1)
_RimlightPower("_RimlightPower", Float) = 3
_RimlightIntensity("_RimlightIntensity", Float) = 0

	}
	
	SubShader 
	{
		Tags
		{
"Queue"="Geometry"
"IgnoreProjector"="False"
"RenderType"="Opaque"

		}

		
Cull Back
ZWrite On
ZTest LEqual
ColorMask RGBA
Fog{
}


		CGPROGRAM
#pragma surface surf BlinnPhongEditor  vertex:vert
#pragma target 2.0


sampler2D _MainTex;
sampler2D _NormalMap;
float _Glossiness;
float4 _SpecularColor;
float _SpecIntensity;
float4 _SpecDirection;
float _SelfIllumination;
float4 _RimlightColor;
float _RimlightPower;
float _RimlightIntensity;

			struct EditorSurfaceOutput {
				half3 Albedo;
				half3 Normal;
				half3 Emission;
				half3 Gloss;
				half Specular;
				half Alpha;
				half4 Custom;
			};
			
			inline half4 LightingBlinnPhongEditor_PrePass (EditorSurfaceOutput s, half4 light)
			{
half3 spec = light.a * s.Gloss;
half4 c;
c.rgb = (s.Albedo * light.rgb + light.rgb * spec);
c.a = s.Alpha;
return c;

			}

			inline half4 LightingBlinnPhongEditor (EditorSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
			{
				half3 h = normalize (lightDir + viewDir);
				
				half diff = max (0, dot ( lightDir, s.Normal ));
				
				float nh = max (0, dot (s.Normal, h));
				float spec = pow (nh, s.Specular*128.0);
				
				half4 res;
				res.rgb = _LightColor0.rgb * diff;
				res.w = spec * Luminance (_LightColor0.rgb);
				res *= atten * 2.0;

				return LightingBlinnPhongEditor_PrePass( s, res );
			}
			
			struct Input {
				float2 uv_MainTex;
float2 uv_NormalMap;
float3 viewDir;

			};

			void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input,o);
			
float4 VertexOutputMaster0_0_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_1_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_2_NoInput = float4(0,0,0,0);
float4 VertexOutputMaster0_3_NoInput = float4(0,0,0,0);

			}
			

			void surf (Input IN, inout EditorSurfaceOutput o) {
				o.Normal = float3(0.0,0.0,1.0);
				o.Alpha = 1.0;
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Gloss = 0.0;
				o.Specular = 0.0;
				o.Custom = 0.0;
				
float4 Sampled2D0=tex2D(_MainTex,IN.uv_MainTex.xy);
float4 Tex2DNormal0=float4(UnpackNormal( tex2D(_NormalMap,(IN.uv_NormalMap.xyxy).xy)).xyz, 1.0 );
float4 Add0=float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ) + _SpecDirection;
float4 Fresnel0=(1.0 - dot( normalize( Add0.xyz), normalize( Tex2DNormal0.xyz ) )).xxxx;
float4 Invert0= float4(1.0, 1.0, 1.0, 1.0) - Fresnel0;
float4 Pow0=pow(Invert0,_Glossiness.xxxx);
float4 Saturate0=saturate(Pow0);
float4 Multiply2=_SpecularColor * Saturate0;
float4 Multiply0=_SpecIntensity.xxxx * Multiply2;
float4 Multiply1=Sampled2D0 * _SelfIllumination.xxxx;
float4 Add1=Multiply0 + Multiply1;
float4 Fresnel1=(1.0 - dot( normalize( float4( IN.viewDir.x, IN.viewDir.y,IN.viewDir.z,1.0 ).xyz), normalize( Tex2DNormal0.xyz ) )).xxxx;
float4 Pow1=pow(Fresnel1,_RimlightPower.xxxx);
float4 Saturate1=saturate(Pow1);
float4 Multiply3=Saturate1 * _RimlightColor;
float4 Multiply4=Multiply3 * _RimlightIntensity.xxxx;
float4 Add2=Add1 + Multiply4;
float4 Master0_3_NoInput = float4(0,0,0,0);
float4 Master0_4_NoInput = float4(0,0,0,0);
float4 Master0_5_NoInput = float4(1,1,1,1);
float4 Master0_7_NoInput = float4(0,0,0,0);
float4 Master0_6_NoInput = float4(1,1,1,1);
o.Albedo = Sampled2D0;
o.Normal = Tex2DNormal0;
o.Emission = Add2;

				o.Normal = normalize(o.Normal);
			}
		ENDCG
	}
	Fallback "Diffuse"
}