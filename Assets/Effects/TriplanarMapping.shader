// https://www.ronja-tutorials.com/post/010-triplanar-mapping/
Shader "Custom/TriplanarMapping"{
	//show values to edit in inspector
	Properties{
		_Color ("Overall Tint Color", Color) = (0, 0, 0, 1)
		_EmissionColor("Emission Color", Color) = (1, 0, 0, 1)
		[Toggle(EMISSION_FLICKER)]_EmissionFlicker("Will Emission flicker?", float) = 0
		_FlickerSpeed("Emission Flicker Speed", float) = 1.0
		_MainTex ("Side Texture", 2D) = "white" {}
		[NoScaleOffset]_MainTexNormal ("Side Normal Map", 2D) = "bump"{}
		[NoScaleOffset]_MainTexEmission ("Side Emission Mask", 2D) = "black"{}
		_SecondTex ("Top Texture", 2D) = "black" {}
		[NoScaleOffset]_SecondTexNormal ("Top Normal Map", 2D) = "bump"{}
		[NoScaleOffset]_SecondTexEmission ("Top Emission Mask", 2D) = "black"{}
		_Sharpness ("Blend sharpness", Range(1, 5)) = 1
	}

	SubShader{
		//the material is completely non-transparent and is rendered at the same time as the other opaque geometry
		Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

			CGPROGRAM

			#include "UnityCG.cginc"
			
			#pragma surface surf Standard vertex:vertShader
			#pragma shader_feature EMISSION_FLICKER

			//texture and transforms of the texture
			sampler2D _MainTex, _MainTexNormal, _MainTexEmission, _SecondTex, _SecondTexNormal, _SecondTexEmission;
			float4 _MainTex_ST, _SecondTex_ST;

			fixed4 _Color, _EmissionColor;
			float _Sharpness, _FlickerSpeed;
			
			struct Input {
	            float3 worldPos;
				float3 worldNormal;
	            INTERNAL_DATA
			};
	
			void vertShader(inout appdata_full v, out Input o)
			{
				UNITY_INITIALIZE_OUTPUT(Input,o);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			}

			// Transform world normal to tangent normal
			float3 WorldToTangentNormalVector(Input IN, float3 normalWS) {
	            float3 t2w0 = WorldNormalVector(IN, float3(1,0,0));
	            float3 t2w1 = WorldNormalVector(IN, float3(0,1,0));
	            float3 t2w2 = WorldNormalVector(IN, float3(0,0,1));
	            float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
	            return normalize(mul(t2w, normalWS));
			}
			
			// Unity surface shader (PBR Based)
			void surf(Input i, inout SurfaceOutputStandard o)
			{
				i.worldNormal = WorldNormalVector(i, float3(0,0,1));
				//calculate UV coordinates for three projections
				float2 uv_front = TRANSFORM_TEX(i.worldPos.xy, _MainTex);
				float2 uv_side = TRANSFORM_TEX(i.worldPos.zy, _MainTex);
				float2 uv_top = TRANSFORM_TEX(i.worldPos.xz, _SecondTex);
				
				//read texture at uv position of the three projections
				// front and back is trivial in our case so use side texture
				fixed4 col_front = tex2D(_MainTex, uv_front);	
				fixed4 col_side = tex2D(_MainTex, uv_side);
				fixed4 col_top = tex2D(_SecondTex, uv_top);
				
				//generate weights from world normals
				float3 weights = i.worldNormal;
				//show texture on both sides of the object (positive and negative)
				weights = abs(weights);
				//make the transition sharper
				weights = pow(weights, _Sharpness);
				//make it so the sum of all components is 1
				weights = weights / (weights.x + weights.y + weights.z);

				//combine weights with projected colors
				col_front *= weights.z;
				col_side *= weights.x;
				col_top *= weights.y;

				//combine the projected colors
				fixed4 col_blended = col_front + col_side + col_top;
				
				// tangent space normal maps
				fixed3 normal_side = UnpackNormal(tex2D(_MainTexNormal, uv_side));
				fixed3 normal_top = UnpackNormal(tex2D(_SecondTexNormal, uv_top));
				fixed3 normal_front = UnpackNormal(fixed4(0.5,0.5,1,0.5));

				// Get the sign (-1 or 1) of the surface normal
				fixed3 axisSign = sign(i.worldNormal);
				// Flip tangent normal z to account for surface normal facing
				normal_side.z *= axisSign.x;
				normal_top.z *= axisSign.y;
				normal_front.z *= axisSign.z;
				
				// blend normal by swizzle tangent axis to match world space axis
				fixed3 normal_blended = normalize(
					normal_side.zyx * weights.x +
					normal_top.xzy * weights.y +
					normal_front.xyz * weights.z
				);

				// emission mask blend
				fixed emission_front = tex2D(_MainTexEmission, uv_front).r;	
				fixed emission_side = tex2D(_MainTexEmission, uv_side).r;
				fixed emission_top = tex2D(_SecondTexEmission, uv_top).r;
				emission_front *= weights.z;
				emission_side *= weights.x;
				emission_top *= weights.y;

				fixed emission_blended = emission_front + emission_side + emission_top;
				fixed3 emission_color = _EmissionColor * emission_blended;

				#ifdef EMISSION_FLICKER
					   emission_color =lerp(0.0, emission_color, sin(_Time.y * _FlickerSpeed) * 0.5 + 0.5);
				#endif
				
				o.Albedo = col_blended;
				o.Normal = WorldToTangentNormalVector(i, normal_blended);	// surface input requires tangent space normal
				o.Metallic = 0.0;
				o.Alpha = 1.0;
				o.Emission = emission_color;
				o.Occlusion = 1.0;
				o.Smoothness = 0.0;
			}
			ENDCG
	}
	FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}
