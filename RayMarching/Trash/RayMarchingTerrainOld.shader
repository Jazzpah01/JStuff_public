Shader "Unlit/RayMarchingTerrain"
{
    Properties
    {
        _Scale("Scale", float) = 100
        _Stretch("Stretch", float) = 1000
        _Offset("Offset", Vector) = (0,0,0)

        _ViewDistance("ViewDistance", float) = 500
        _NearPlane("NearPlane", float) = 0.01
        _Step("Step", float) = 0.1
        _StepDelta("StepDelta", float) = 0.01

        _SkyColor("SkyColor", Color) = (0,0,1)
        _VoidColor("VoidColor", Color) = (1,0,1)

        _Seed("Seed", Vector) = (0.4125363, 0.12675688, 0.98167999)
        _Depth("Depth", Integer) = 40
        _MinDepth("MinDepth", Integer) = 10

        _DistanceFactor("DistanceFactor", float) = 0.95
        _HeightDifferenceFactor("HeightDifferenceFactor", float) = 0.2

        _DepthThreshold("DepthThreshold", float) = 1.0

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
    }
    SubShader
    {
        //Tags { "RenderType"="Opaque" }
        Tags { "RenderType" = "AlphaTest" "DisableBatching" = "True" }
        LOD 100

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma enable_d3d11_debug_symbols
            #pragma multi_compile_fog
            //#pragma surface surf Lambert
            // make fog work
            //#pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                //tangent
                //normal
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 positionW : TEXCOORD1;
                float3 rayDirection : TEXCOORD2;
                float3 rayOrigin : TEXCOORD3;
                //tangent interpo
                //normal interpo
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _Scale;
            float _Stretch;

            float _ViewDistance;
            float _NearPlane;
            float _Step;
            float _StepDelta;

            fixed3 _VoidColor;
            fixed3 _SkyColor;

            float _HeightDifferenceFactor;
            float _DistanceFactor;

            float3 rayOrigin;
            float3 rayDirection;
            float3 rayHit;
            float3 raySurface;

            int _Depth;
            int _MinDepth;
            float3 _Seed;

            float _DepthThreshold;

            float stored = 1000;


            //https://stackoverflow.com/questions/2049582/how-to-determine-if-a-point-is-in-a-2d-triangle
            float sign(float2 p1, float2 p2, float2 p3)
            {
                return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
            }

            bool pointInTriangle(float2 pt, float2 v1, float2 v2, float2 v3)
            {
                float d1, d2, d3;
                bool has_neg, has_pos;

                d1 = sign(pt, v1, v2);
                d2 = sign(pt, v2, v3);
                d3 = sign(pt, v3, v1);

                has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
                has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

                return !(has_neg && has_pos);
            }

            // PI = 3.14159265359f
            float rand(float seed0, float seed1) {
                return frac((seed0 + 3.14159265359f) * (seed1 + 3.14159265359f)) * 2.0f - 1.0f;
            }

            //uint getTargetDepthOld(uint maxDepth, float distance) {
            //    uint targetDepth = 0;
            //    float n = sqrt(2.0f);
            //    float s2 = n;
            //    for (uint i = 0; i < _Depth && s2 / (1.0f + distance * distance) * _DepthThreshold > s2 / (float)n; i++)
            //    {
            //        n = sqrt((n / 2.0f) * (n / 2.0f));
            //        targetDepth += 1;
            //    }
            //    return targetDepth;
            //}

            uint getTargetDepth(float minDistance, float maxDistance, float distance) {
                distance -= minDistance / _Stretch;
                maxDistance -= minDistance / _Stretch;
                //float max = 1.0f;
                //float min = 1.0f / (1.0f + (maxDistance / _Stretch) * (maxDistance / _Stretch));
                //float t = 1.0f / (1.0f + (distance / _Stretch) * (distance / _Stretch));

                float max = (maxDistance / _Stretch) * (maxDistance / _Stretch);
                float min = (minDistance / _Stretch) * (minDistance / _Stretch);
                float t = (distance / _Stretch) * (distance / _Stretch);

                float mix = lerp((float)_Depth, (float)_MinDepth, (t - min) / (max - min));
                return (uint)mix;
            }

            float rightTriangle(float2 p, uint depth,
                inout float cachedDepth, inout float4 cachedA, inout float4 cachedB, inout float4 cachedC) {
                float h = _HeightDifferenceFactor;
                float d = _DistanceFactor;

                // x and y are coordinates. z is height. w is random number
                float4 a = float4(0.0f, 1.0f, 0.0f, 0.4321558372f);
                float4 b = float4(0.0f, 0.0f, 0.0f, -0.9770926823f);
                float4 c = float4(1.0f, 0.0f, 0.0f, 0.2236396734f);

                uint initialDepth = 0;

                if (!pointInTriangle(p, a.xy, b.xy, c.xy)) {
                    return -2;
                }

                if (cachedDepth != 0.0f && cachedDepth < depth && pointInTriangle(p, cachedA.xy, cachedB.xy, cachedC.xy)) {
                    a = cachedA;
                    b = cachedB;
                    c = cachedC;
                    initialDepth = cachedDepth;

                    cachedA = a;
                    cachedB = b;
                    cachedC = c;
                    cachedDepth = initialDepth;
                }
                else {
                    cachedA = float4(0.0f, 0.0f, 0.0f, 0.0f);
                    cachedB = float4(0.0f, 0.0f, 0.0f, 0.0f);
                    cachedC = float4(0.0f, 0.0f, 0.0f, 0.0f);
                    cachedDepth = initialDepth;
                }

                // Non linear = 1.55
                // linear = 1.5
                for (int i = initialDepth; i < depth; i++)
                {
                    float r = rand(a.w, c.w);
                    float h = (a.z + c.z) / 2 + r * length(a.xy - c.xy) * d;
                    float4 ac = float4((a.xy + c.xy) / 2.0f, h, r);

                    float distA = length(p - a.xy);
                    float distC = length(p - c.xy);
                    if (distA < distC)
                    {
                        c = b;
                        b = ac;
                    }
                    else if (distA > distC)
                    {
                        a = c;
                        c = b;
                        b = ac;
                    }
                    else if (a.r > c.r)
                    {
                        c = b;
                        b = ac;
                    }
                    else
                    {
                        a = c;
                        c = b;
                        b = ac;
                    }

                    if (i == (uint)depth - 3) {
                        // Cache the triangle just before
                        cachedA = a;
                        cachedB = b;
                        cachedC = c;
                        cachedDepth = i + 1;
                    }
                }

                return (a.z + b.z + c.z) / 3.0f;
            }

            float flat(float2 p) {
                return 0;
            }

            //https://iquilezles.org/articles/terrainmarching/
            bool castRay(float3 origin, float3 surface, float3 direction)
            {
                rayOrigin = origin;
                raySurface = surface;
                rayDirection = direction;

                float surfaceDistance = length(origin - surface);

                float dt = _Step + surfaceDistance / _Step * _StepDelta;
                //float dt = _Step;
                const float mint = max(surfaceDistance, _NearPlane);
                const float maxt = _ViewDistance;

                float3 lastPoint = surface;

                float lh = 0.0f;
                float ly = 0.0f;

                float cachedDepth = 0.0f;
                float4 cachedA = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float4 cachedB = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float4 cachedC = float4(0.0f, 0.0f, 0.0f, 0.0f);

                for (float t = mint; t < maxt; t += dt)
                {
                    const float3 p = origin + direction * t;

                    if (p.y > _Scale) {
                        return false;
                    }

                    uint targetDepth = getTargetDepth(_NearPlane, _ViewDistance, t);

                    float h = rightTriangle(p.xz / _Stretch, targetDepth,
                        cachedDepth, cachedA, cachedB, cachedC) * _Scale;

                    if (p.y < h)
                    {
                        //rayHit = binarySearch(p, lastPoint, 0.01f, _Depth);
                        rayHit = rayOrigin + rayDirection * (t - dt + dt * (lh - ly) / (p.y - ly - h + lh));
                        return true;
                    }

                    lh = h;
                    ly = p.y;

                    lastPoint = p;
                    dt = dt + _StepDelta;
                }
                return false;
            }

            //https://stackoverflow.com/questions/49640250/calculate-normals-from-heightmap
            //https://www.ultraengine.com/community/topic/16244-calculate-normals-from-heightmap/
            float3 getNormal(float3 p, float distance) {
                //float diff = max(distance / (float)_Stretch * distance / (float)_Stretch * 50.0f, 0.25f);
                //float diff = distance / (float)_Stretch * distance / (float)_Stretch * 200.0f;

                uint targetDepth = getTargetDepth(_NearPlane, _ViewDistance, distance);
                //float diff = max(distance / (float)_Stretch / (float)targetDepth * 5.0f * 40.0f, 0.5f);
                //float diff = (1.1412f / (float)targetDepth);

                float n = sqrt(2.0f) * _Stretch;
                for (uint i = 0; i < targetDepth; i++)
                {
                    n = sqrt((n / 2.0f) * (n / 2.0f));
                }

                float diff = max(n * 2.0f, 10.0f);
                //float diff = distance * 2.0f;

                float2 Rxz = float2(p.x + diff, p.z);
                float2 Lxz = float2(p.x - diff, p.z);
                float2 Txz = float2(p.x, p.z + diff);
                float2 Bxz = float2(p.x, p.z - diff);

                float cachedDepth = 0.0f;
                float4 cachedA = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float4 cachedB = float4(0.0f, 0.0f, 0.0f, 0.0f);
                float4 cachedC = float4(0.0f, 0.0f, 0.0f, 0.0f);

                float R = rightTriangle(Rxz / _Stretch, targetDepth,
                    cachedDepth, cachedA, cachedB, cachedC) * _Scale;
                float L = rightTriangle(Lxz / _Stretch, targetDepth,
                    cachedDepth, cachedA, cachedB, cachedC) * _Scale;
                float T = rightTriangle(Txz / _Stretch, targetDepth,
                    cachedDepth, cachedA, cachedB, cachedC) * _Scale;
                float B = rightTriangle(Bxz / _Stretch, targetDepth,
                    cachedDepth, cachedA, cachedB, cachedC) * _Scale;

                float3 retval = float3(2.0f * diff * (L - R), 4.0f * diff * diff, 2.0f * diff * (B - T));

                return normalize(retval);
            }

            half3 _LightColor0;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.positionW = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0f));

                o.rayOrigin = _WorldSpaceCameraPos;
                o.rayDirection = o.positionW - _WorldSpaceCameraPos;

                return o;
            }

            fixed4 frag(v2f i, out float outDepth : SV_Depth) : SV_Target
            {
                float3 direction = normalize(i.rayDirection.xyz);//normalize(i.positionW.xyz - _WorldSpaceCameraPos);
                float3 origin = i.rayOrigin;
                float3 surfacePosition = i.positionW;

                fixed3 albedo = fixed3((fixed3)_VoidColor);

                bool hit = castRay(origin, surfacePosition, direction);

                if (hit) {
                    fixed3 Gray = fixed3(0.5f, 0.5f, 0.5f);
                    fixed3 Blue = fixed3(0.0f, 0.0f, 1.0f);
                    fixed3 Black = fixed3(0.0f, 0.0f, 0.0f);

                    fixed3 Green = fixed3(0.0f, 1.0f, 0.0f);
                    fixed3 DarkGreen = fixed3(0.0f, 0.5f, 0.0f);
                    fixed3 Brown = fixed3(0.9f, 0.6f, 0.2f);

                    fixed3 LightBrown = fixed3(0.7f, 0.56f, 0.2f);

                    float t = (rayHit.y + _Scale) / 2 / _Scale;

                    albedo = lerp(LightBrown, Brown, t);

                    

                    //// Testing optimisation
                    //// r:
                    //// 40->1.00005
                    //uint td = getTargetDepth(_NearPlane, _ViewDistance, length(rayHit - origin));
                    //float c = ((float)td-_MinDepth) / ((float)_Depth - _MinDepth);
                    //albedo = fixed3(c,c,c);
                    //return float4(albedo, 1.0f);
                    //// End Test

                    // world space surface normal
                    float3 worldNormal = getNormal(rayHit, length(rayHit.xyz - _WorldSpaceCameraPos.xyz));

                    //https://bgolus.medium.com/rendering-a-sphere-on-a-quad-13c92025570c#c582
                    // basic lighting
                    float3 worldLightDir = _WorldSpaceLightPos0.xyz;
                    float ndotl = saturate(dot(worldNormal, worldLightDir));
                    float3 lighting = _LightColor0 * ndotl;

                    // ambient lighting
                    float3 ambient = ShadeSH9(float4(worldNormal, 1.0f));
                    lighting += ambient;

                    // Depth
                    float4 clipPos = UnityWorldToClipPos(rayHit);
                    outDepth = clipPos.z / clipPos.w;

                    fixed4 acc = fixed4(albedo, 1.0f);

                    //return float4(ambient, 1.0f);
                    //return float4(worldNormal, 1.0f);
                    return clamp(float4(lighting, 1.0f), 0.0f, 1.0f) * float4(albedo, 1.0f);
                }
                else {
                    clip(-1);
                    return float4(_SkyColor, 1);
                }
            }
            ENDCG


        }
    }
}



//if (pointInTriangle(p, ac, a, b)) {
//    c = b;
//    b = ac;
//}
//else if (pointInTriangle(p, ac, b, c)) {
//    a = c;
//    c = b;
//    b = ac;
//}
//else {
//    float distA = length(p - a.xy);
//    float distC = length(p - c.xy);
//    if (distA < distC)
//    {
//        c = b;
//        b = ac;
//    }
//    else
//    {
//        a = c;
//        c = b;
//        b = ac;
//    }
//}