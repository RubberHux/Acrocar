Shader "Custom/VerticalFogIntersection"
{
    Properties
    {
        _Color("Main Color", Color) = (1, 1, 1, .5)
        _IntersectionColor("IntersectionColor", Color) = (0.5,0.5,0.5,0.5)
        _Noise("Noise Texture", 2D) = "black"{}
        _NoiseTilingAndSpeed("Noise Tiling", Vector) = (0,0,0,0)
        _IntersectionThresholdMax("Intersection Threshold Max", float) = 1
        [Toggle(_IgnoreDepth)]_IgnoreDepth("Ignore Depth (For cloud effect)", float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType"="Transparent"  }
  
        Pass
        {
           Blend SrcAlpha OneMinusSrcAlpha
           ZWrite Off
           Cull off
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma multi_compile_fog
           #pragma shader_feature _IgnoreDepth
           #include "UnityCG.cginc"
  
           struct appdata
           {
               float4 vertex : POSITION;
           };
  
           struct v2f
           {
               float4 scrPos : TEXCOORD0;
               UNITY_FOG_COORDS(1)
               float4 vertex : SV_POSITION;
               float3 worldPos : TEXCOORD2;
           };
  
           sampler2D _CameraDepthTexture, _Noise;
           float4 _NoiseTilingAndSpeed;
           float4 _Color;
           float4 _IntersectionColor;
           float _IntersectionThresholdMax;
  
           v2f vert(appdata v)
           {
               v2f o;
               o.vertex = UnityObjectToClipPos(v.vertex);
               o.scrPos = ComputeScreenPos(o.vertex);
               o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
               UNITY_TRANSFER_FOG(o,o.vertex);
               return o;   
           }
  
  
            half4 frag(v2f i) : SV_TARGET
            {
               float depth = LinearEyeDepth (tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));
               float noise2x = 0.15 * tex2D(_Noise, i.worldPos.xz * 0.05);
               float thresholdOffset = tex2D(_Noise, i.worldPos.xz * 0.02 + _NoiseTilingAndSpeed.xy + _NoiseTilingAndSpeed.zw * _Time.y + noise2x).r;
               float threshold = max(_IntersectionThresholdMax - 0.1 * thresholdOffset,0.01);
#ifdef _IgnoreDepth
               thresholdOffset = tex2D(_Noise, i.worldPos.xz * _NoiseTilingAndSpeed.xy + _NoiseTilingAndSpeed.zw * _Time.y).r;
               float diff = saturate(thresholdOffset);
               float realDiff = 1;
               fixed4 col = lerp(fixed4(_Color.rgb, 0.0), _Color, thresholdOffset);
               return col;
#else
               float diff = saturate(threshold * (depth - i.scrPos.w));
               float realDiff = saturate(0.05 * (depth - i.scrPos.w));
               fixed4 baseCol = fixed4(_Color.rgb, 0.1);
               fixed4 col = lerp(fixed4(_Color.rgb, 0.1), _Color, diff * diff * diff * (diff * (6 * diff - 15) + 10));
               col.rgb *= _IntersectionColor.rgb * realDiff;
               UNITY_APPLY_FOG(i.fogCoord, col);
               return col;
#endif
            }
  
            ENDCG
        }
    }
}