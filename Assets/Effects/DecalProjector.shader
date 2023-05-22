Shader "Custom/DecalProjector"
{
	Properties
	{
		_MainTex ("Decal Texture", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="true" "DisableBatching"="true" }
		LOD 100
		ZWrite Off
		Cull Off
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha
		Offset -1, -1

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenPos: TEXCOORD1;
			};

			sampler2D _MainTex, _CameraDepthTexture;
			float4 _MainTex_ST;
			float4x4 _WorldToProjector;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.screenPos = ComputeScreenPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed2 screenPos = i.screenPos.xy / i.screenPos.w; // to [0,1]
				const float depth = tex2D(_CameraDepthTexture, screenPos).r;
				const float4 clipPos = float4(screenPos.x * 2 - 1, screenPos.y * 2 - 1, -depth * 2 + 1, 1) * LinearEyeDepth(depth); // clip [-w,w]
				const float4 viewPos = mul(unity_CameraInvProjection, clipPos);		// clip-view
				const float4 worldPos = mul(unity_MatrixInvV, viewPos);				// view-world
				float3 objectPos = mul(unity_WorldToObject, worldPos).xyz;		// world-local-projector
				clip(0.5 - abs(objectPos));
				objectPos += 0.5;
				return tex2D(_MainTex, objectPos.xy);
			}
			ENDCG
		}
	}
}