Shader "Hidden/Renkai/CompetitiveGrade"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Exposure;
            float _Contrast;
            float _Saturation;
            float _BloomStrength;
            float _BloomThreshold;
            float _Sharpness;
            float _Vignette;
            float _CoolShadowStrength;

            float3 SampleSoft(float2 uv)
            {
                float2 t = _MainTex_TexelSize.xy;
                float3 sum = tex2D(_MainTex, uv).rgb * 0.28;
                sum += tex2D(_MainTex, uv + float2(t.x, 0)).rgb * 0.12;
                sum += tex2D(_MainTex, uv - float2(t.x, 0)).rgb * 0.12;
                sum += tex2D(_MainTex, uv + float2(0, t.y)).rgb * 0.12;
                sum += tex2D(_MainTex, uv - float2(0, t.y)).rgb * 0.12;
                sum += tex2D(_MainTex, uv + t).rgb * 0.06;
                sum += tex2D(_MainTex, uv - t).rgb * 0.06;
                sum += tex2D(_MainTex, uv + float2(t.x, -t.y)).rgb * 0.06;
                sum += tex2D(_MainTex, uv + float2(-t.x, t.y)).rgb * 0.06;
                return sum;
            }

            float3 Tonemap(float3 x)
            {
                float a = 2.51;
                float b = 0.03;
                float c = 2.43;
                float d = 0.59;
                float e = 0.14;
                return saturate((x * (a * x + b)) / (x * (c * x + d) + e));
            }

            fixed4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;
                float3 source = tex2D(_MainTex, uv).rgb;
                float3 soft = SampleSoft(uv);

                float3 bright = max(soft - _BloomThreshold, 0.0);
                float3 color = source + bright * _BloomStrength;
                color += (source - soft) * _Sharpness;
                color *= exp2(_Exposure);

                color = (color - 0.5) * _Contrast + 0.5;

                float luma = dot(color, float3(0.2126, 0.7152, 0.0722));
                color = lerp(luma.xxx, color, _Saturation);

                float shadowMask = saturate(1.0 - luma * 1.55);
                color += float3(-0.018, 0.010, 0.045) * shadowMask * _CoolShadowStrength;

                float2 centered = uv * 2.0 - 1.0;
                float vignette = saturate(1.0 - dot(centered, centered) * _Vignette);
                color *= lerp(1.0, vignette, 0.72);

                color = Tonemap(max(color, 0.0));
                return fixed4(color, 1.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
