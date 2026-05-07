Shader "ROB/Skybox/ProceduralDayNight"
{
    Properties
    {
        [Header(Cycle)]
        _TimeOfDay ("Time Of Day", Range(0,1)) = 0.35

        [Header(Sun  Moon Directions)]
        _SunDir ("Sun Direction", Vector) = (0,1,0,0)
        _MoonDir ("Moon Direction", Vector) = (0,-1,0,0)

        [Header(Sky Colors  Day)]
        _ZenithDay ("Zenith Day", Color) = (0.22, 0.55, 0.95, 1)
        _HorizonDay ("Horizon Day", Color) = (0.70, 0.85, 1.00, 1)

        [Header(Sky Colors  Sunset)]
        _ZenithSunset ("Zenith Sunset", Color) = (0.16, 0.20, 0.42, 1)
        _HorizonSunset ("Horizon Sunset", Color) = (1.00, 0.48, 0.18, 1)

        [Header(Sky Colors  Night)]
        _ZenithNight ("Zenith Night", Color) = (0.015, 0.025, 0.07, 1)
        _HorizonNight ("Horizon Night", Color) = (0.06, 0.09, 0.16, 1)

        [Header(Ground  Lower Horizon)]
        _GroundDay ("Ground Day", Color) = (0.35, 0.38, 0.40, 1)
        _GroundNight ("Ground Night", Color) = (0.01, 0.015, 0.03, 1)
        _HorizonSoftness ("Horizon Softness", Range(0.01, 2.0)) = 0.55

        [Header(Sun Disc)]
        _SunColor ("Sun Color", Color) = (1.0, 0.88, 0.68, 1)
        _SunSize ("Sun Size", Range(0.0001, 0.05)) = 0.012
        _SunSharpness ("Sun Sharpness", Range(1, 256)) = 80

        [Header(Moon Disc)]
        _MoonColor ("Moon Color", Color) = (0.78, 0.84, 1.0, 1)
        _MoonSize ("Moon Size", Range(0.0001, 0.08)) = 0.02
        _MoonSharpness ("Moon Sharpness", Range(1, 256)) = 70

        [Header(Stars)]
        _StarIntensity ("Star Intensity", Range(0, 3)) = 1.0
        _StarThreshold ("Star Threshold", Range(0.8, 0.9999)) = 0.985
        _StarScale ("Star Scale", Float) = 220.0

        [Header(Clouds)]
        _CloudCoverage ("Cloud Coverage", Range(0,1)) = 0.52
        _CloudSoftness ("Cloud Softness", Range(0.001,1)) = 0.18
        _CloudScale ("Cloud Scale", Float) = 2.8
        _CloudSpeed ("Cloud Speed", Float) = 0.008
        _CloudHeightFadeStart ("Cloud Height Fade Start", Range(-1,1)) = -0.05
        _CloudHeightFadeEnd ("Cloud Height Fade End", Range(-1,1)) = 0.65
        _CloudLightDay ("Cloud Light Day", Color) = (1,1,1,1)
        _CloudDarkDay ("Cloud Dark Day", Color) = (0.78,0.84,0.90,1)
        _CloudLightSunset ("Cloud Light Sunset", Color) = (1.0,0.72,0.56,1)
        _CloudDarkSunset ("Cloud Dark Sunset", Color) = (0.62,0.38,0.34,1)
        _CloudLightNight ("Cloud Light Night", Color) = (0.14,0.18,0.25,1)
        _CloudDarkNight ("Cloud Dark Night", Color) = (0.05,0.07,0.11,1)

        [Header(Cloud Lighting)]
        _CloudSunInfluence ("Cloud Sun Influence", Range(0,4)) = 1.2
        _CloudRim ("Cloud Rim", Range(0,4)) = 1.1
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _TimeOfDay;
            float4 _SunDir;
            float4 _MoonDir;

            float4 _ZenithDay;
            float4 _HorizonDay;
            float4 _ZenithSunset;
            float4 _HorizonSunset;
            float4 _ZenithNight;
            float4 _HorizonNight;

            float4 _GroundDay;
            float4 _GroundNight;
            float _HorizonSoftness;

            float4 _SunColor;
            float _SunSize;
            float _SunSharpness;

            float4 _MoonColor;
            float _MoonSize;
            float _MoonSharpness;

            float _StarIntensity;
            float _StarThreshold;
            float _StarScale;

            float _CloudCoverage;
            float _CloudSoftness;
            float _CloudScale;
            float _CloudSpeed;
            float _CloudHeightFadeStart;
            float _CloudHeightFadeEnd;
            float4 _CloudLightDay;
            float4 _CloudDarkDay;
            float4 _CloudLightSunset;
            float4 _CloudDarkSunset;
            float4 _CloudLightNight;
            float4 _CloudDarkNight;
            float _CloudSunInfluence;
            float _CloudRim;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise2d(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash21(i);
                float b = hash21(i + float2(1,0));
                float c = hash21(i + float2(0,1));
                float d = hash21(i + float2(1,1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float a = 0.5;

                v += noise2d(p) * a; p = p * 2.02; a *= 0.5;
                v += noise2d(p) * a; p = p * 2.03; a *= 0.5;
                v += noise2d(p) * a; p = p * 2.01; a *= 0.5;
                v += noise2d(p) * a; p = p * 2.04; a *= 0.5;
                v += noise2d(p) * a;

                return v;
            }

            float3 GetSkyGradient(float3 dir, float sunFactor, float sunsetFactor)
            {
                float up01 = saturate(dir.y * 0.5 + 0.5);
                float horizonT = pow(up01, max(0.01, _HorizonSoftness));

                float3 zenith = lerp(_ZenithNight.rgb, _ZenithDay.rgb, sunFactor);
                float3 horizon = lerp(_HorizonNight.rgb, _HorizonDay.rgb, sunFactor);
                float3 ground = lerp(_GroundNight.rgb, _GroundDay.rgb, sunFactor);

                zenith = lerp(zenith, _ZenithSunset.rgb, sunsetFactor);
                horizon = lerp(horizon, _HorizonSunset.rgb, sunsetFactor);

                float skyMask = smoothstep(-0.02, 0.12, dir.y);
                float3 skyColor = lerp(horizon, zenith, horizonT);
                float3 lowerBlend = lerp(ground, horizon, smoothstep(-0.35, 0.08, dir.y));

                return lerp(lowerBlend, skyColor, skyMask);
            }

            float GetSunsetFactor(float t)
            {
                float dawn = smoothstep(0.20, 0.30, t) * (1.0 - smoothstep(0.30, 0.40, t));
                float dusk = smoothstep(0.60, 0.70, t) * (1.0 - smoothstep(0.70, 0.80, t));
                return saturate(max(dawn, dusk) * 1.35);
            }

            float3 DirToCloudUV(float3 dir)
            {
                float2 uv = dir.xz / max(0.15, (dir.y + 0.45));
                return float3(uv, dir.y);
            }

            float StarField(float3 dir)
            {
                float2 uv = normalize(dir).xz / max(0.1, abs(dir.y) + 0.2);
                uv *= _StarScale;

                float2 gv = frac(uv) - 0.5;
                float2 id = floor(uv);

                float rnd = hash21(id);
                float size = lerp(0.02, 0.18, hash21(id + 13.1));
                float2 offset = (float2(hash21(id + 1.7), hash21(id + 8.3)) - 0.5) * 0.6;
                float d = length(gv - offset);

                float star = smoothstep(size, 0.0, d);
                star *= step(_StarThreshold, rnd);

                float twinkle = 0.65 + 0.35 * sin(_Time.y * 2.5 + rnd * 20.0);
                return star * twinkle;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.viewDir = normalize(mul((float3x3)unity_ObjectToWorld, v.vertex.xyz));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.viewDir);
                float3 sunDir = normalize(_SunDir.xyz);
                float3 moonDir = normalize(_MoonDir.xyz);

                float sunFactor = saturate(smoothstep(0.18, 0.35, _TimeOfDay) * (1.0 - smoothstep(0.72, 0.84, _TimeOfDay))
                                 + smoothstep(0.84, 1.0, _TimeOfDay) * 0.0);

                // mesmo comportamento visual do seu script: dia forte no meio, noite nos extremos
                sunFactor = saturate(
                    max(
                        smoothstep(0.20, 0.35, _TimeOfDay) * (1.0 - smoothstep(0.65, 0.80, _TimeOfDay)),
                        0.0
                    )
                );

                float nightFactor = 1.0 - sunFactor;
                float sunsetFactor = GetSunsetFactor(_TimeOfDay);

                float3 col = GetSkyGradient(dir, sunFactor, sunsetFactor);

                // Sol
                float sunDot = dot(dir, sunDir);
                float sunDisc = pow(saturate((sunDot - (1.0 - _SunSize)) / max(0.0001, _SunSize)), _SunSharpness);
                float sunHalo = pow(saturate(sunDot), 24.0) * 0.25;
                col += (_SunColor.rgb * sunDisc + _SunColor.rgb * sunHalo) * sunFactor;

                // Lua
                float moonDot = dot(dir, moonDir);
                float moonDisc = pow(saturate((moonDot - (1.0 - _MoonSize)) / max(0.0001, _MoonSize)), _MoonSharpness);
                float moonHalo = pow(saturate(moonDot), 18.0) * 0.10;
                col += (_MoonColor.rgb * moonDisc + _MoonColor.rgb * moonHalo) * nightFactor;

                // Estrelas
                float starsVisible = saturate(nightFactor * smoothstep(0.02, 0.25, dir.y + 0.05));
                float stars = StarField(dir) * _StarIntensity * starsVisible;
                col += stars;

                // Nuvens
                float3 cuv = DirToCloudUV(dir);
                float cloudHeightMask = smoothstep(_CloudHeightFadeStart, _CloudHeightFadeEnd, dir.y);
                float2 cloudUV = cuv.xy * _CloudScale;
                cloudUV += float2(_Time.y * _CloudSpeed, _Time.y * _CloudSpeed * 0.65);

                float n1 = fbm(cloudUV);
                float n2 = fbm(cloudUV * 1.9 + 7.31);
                float cloudNoise = lerp(n1, n2, 0.35);

                float coverage = 1.0 - _CloudCoverage;
                float cloudMask = smoothstep(coverage - _CloudSoftness, coverage + _CloudSoftness, cloudNoise);
                cloudMask *= cloudHeightMask;

                float sunLightOnClouds = saturate(dot(float3(cloudUV.x, 0.5, cloudUV.y), normalize(float3(sunDir.x, abs(sunDir.y), sunDir.z))) * 0.5 + 0.5);
                float rim = pow(saturate(1.0 - abs(dir.y)), 2.0) * _CloudRim;

                float3 cloudLight = lerp(_CloudLightNight.rgb, _CloudLightDay.rgb, sunFactor);
                float3 cloudDark = lerp(_CloudDarkNight.rgb, _CloudDarkDay.rgb, sunFactor);

                cloudLight = lerp(cloudLight, _CloudLightSunset.rgb, sunsetFactor);
                cloudDark = lerp(cloudDark, _CloudDarkSunset.rgb, sunsetFactor);

                float lightTerm = saturate(sunLightOnClouds * _CloudSunInfluence + rim * 0.35);
                float3 cloudCol = lerp(cloudDark, cloudLight, lightTerm);

                col = lerp(col, cloudCol, cloudMask * 0.92);

                return float4(saturate(col), 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}