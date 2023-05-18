Shader "Custom/Skybox"
{
    Properties
    {
        _MainSkyColor ("Main Color", Color) = (0.3, 0.56, 0.81, 1)
        _HorizonColor ("Horizon Color", Color) = (0.56, 0.9, 0.99, 1)
        [Range(1.0, 25.0)]_HorizonFalloff ("Horizon Falloff", float) = 2.0
        _SunBloomColor ("Sun Bloom Color", Color) = (1, 0.92, 0.48, 1)
        [Range(1.0, 25.0)]_SunBloomFalloff ("Sun Bloom Falloff", float) = 4.0
        _SunColor ("Sun Color", Color) = (1,1,1,1)
//        [NoScaleOffset] _SunZenithGrad ("Main sky color gradient", 2D) = "white" {}
//        [NoScaleOffset] _ViewZenithGrad ("Horizon color gradient", 2D) = "white" {}
//        [NoScaleOffset] _SunViewGrad ("Sun bloom color gradient", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"  }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewDirWS : TEXCOORD0;
            };

            sampler2D _SunZenithGrad, _ViewZenithGrad, _SunViewGrad;
            float4 _MainSkyColor, _HorizonColor, _SunBloomColor, _SunColor;
            float _SunBloomFalloff, _HorizonFalloff;

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewDirWS = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float3 viewDir = normalize(i.viewDirWS);
                float3 sunDir = _WorldSpaceLightPos0.xyz;
                float sunViewDot = (dot(sunDir, viewDir) + 1.0) * 0.5;
                float sunZenithDot = (sunDir.y + 1.0) * 0.5;
                float viewZenithDot = (viewDir.y + 1.0) * 0.5;
                float sunMask = step(0.995, pow(sunViewDot,4));
                float vzMask = pow(1.0 - abs(viewZenithDot - 0.5), _HorizonFalloff);
                float svMask = pow(saturate(sunViewDot), _SunBloomFalloff);
                float3 skyColor = _MainSkyColor;
                skyColor += vzMask * _HorizonColor + svMask * _SunBloomColor + _SunColor * sunMask;
                return float4(skyColor, 1.0);
            }
            ENDCG
        }
    }
}
