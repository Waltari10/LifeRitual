Shader "AGF/AGF_Planar_Local_Multiple" {

	Properties {
	_Color ("Diffuse Color", Color) = (1.0, 1.0, 1.0, 1.0)
	_Tiling ("Tiling", Float) = 1
	_Power("Power", Float) = 5
	_TexTop ("Top (RGB)", 2D) = "white" {}
	_TexSides ("Sides (RGB)", 2D) = "white" {}
	}
	
Category {
	SubShader { 
	//#Blend
	ZWrite On
	//#CatTags
	Tags { "RenderType"="Opaque" }
		//Tags { "RenderType"="Opaque" }
		//#LOD
		LOD 400
		//#GrabPass
		CGPROGRAM
		//#LightingModelTag
		#pragma surface surf BlinnPhong vertex:vert 
		//alphatest:_Cutoff
		 
		//#TargetSM
		#pragma target 3.0
		#pragma multi_compile_builtin
		//#UnlitCGDefs
		float _Tiling;
		sampler2D _TexTop, _TexSides;
		float4 _Color;
		float _Power;
		
		struct Input {
			//float4 vertex;
			//#UVDefs
			//float3 normal;
			//float2 uvCoords;
			float3 localPos;
			float3 mylocalNormal;
			INTERNAL_DATA
			half3 signs;
		};
		
		void vert (inout appdata_full v, out Input o) {
			o.mylocalNormal = v.normal;
			o.localPos = v.vertex;
			o.signs = sign(o.mylocalNormal);
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			half3 signs = IN.signs;
			half3 signs2 = IN.signs;
			float4 bumpBase = float4(1.0, 1.0, 1.0, 1.0);
			float4 texBase = 1.0;
			
		//Assign UVs			
			half2 s = float2(_Tiling/10, _Tiling/10);
			half2 tex0 = s * IN.localPos.zy;
			half2 tex1 = s * IN.localPos.zx;
			half2 tex2 = s * IN.localPos.xy;
			half2 tex3 = tex0;
			half2 tex4 = tex1;
			half2 tex5 = tex2;
			
		// =========================
		// 		Assign Your Textures Here.
		//		(Don't forget to declare them at line 31...)
		// =========================
			half4 color0_ = tex2D(_TexSides, tex0).xyzw; 
			half4 color1_ = tex2D(_TexTop, tex1).xyzw; 
			half4 color2_ = tex2D(_TexSides, tex2).xyzw;
			half4 color3_ = tex2D(_TexSides, tex3).xyzw; 
			half4 color4_ = tex2D(_TexSides, tex4).xyzw; 
			half4 color5_ = tex2D(_TexSides, tex5).xyzw;
			
		// Calculate projections
			half3 blendWeights = IN.mylocalNormal;
			blendWeights.x = max(IN.mylocalNormal.x, 0);
			blendWeights.y = max(IN.mylocalNormal.y, 0);
			blendWeights.z = max(IN.mylocalNormal.z, 0);
			half3 blendBottom;
			blendBottom.x = min(IN.mylocalNormal.x, 0);
			blendBottom.y = min(IN.mylocalNormal.y, 0);
			blendBottom.z = min(IN.mylocalNormal.z, 0);
			blendWeights = saturate(pow(blendWeights*0.25, _Power));
			blendBottom = saturate(pow(blendBottom*0.25, _Power));
			half3 blendMult = 1/(blendWeights.x + blendBottom.x + blendWeights.y + blendBottom.y + blendWeights.z + blendBottom.z);			
			blendWeights *= blendMult;
			blendBottom *= blendMult;		
		
		// Combine all that stuff...
			texBase = 
			(color0_ * blendWeights.x) + (color3_ * blendBottom.x) +
			(color1_ * blendWeights.y) + (color4_ * blendBottom.y) + 
			(color2_ * blendWeights.z) + (color5_ * blendBottom.z);
		
		// Assign results to shader.
			o.Albedo = texBase.rgb * _Color;
		}
		ENDCG
		}
	}
	FallBack "Triplanar Textures/Triplanar Local"
}