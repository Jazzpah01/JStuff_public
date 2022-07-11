Shader "Unlit/RayMarchingOld"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

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

            float rand(float seed0, float seed1) {
                return frac(seed0 * 3.14159265359f + seed1 * 3.14159265359f) * 2.0f - 1.0f;
            }

            float rightTriangle(float2 p, int depth) {
                float h = 0.65f;
                float d = 0.2f;

                float4 a = float4(0.0f, 1.0f, 0.0f, 0.4271358372f);
                float4 b = float4(0.0f, 0.0f, 0.0f, 0.9770988823f);
                float4 c = float4(1.0f, 0.0f, 0.0f, 0.2960396734f);

                for (int i = 0; i < depth; i++)
                {
                    float r = rand(a.w, c.w);
                    float h = (a.z + c.z) / 2.0f + r * length(a.xy - c.xy);
                    float4 ac = float4((a.xy + c.xy) / 2.0f, h, r);

                    if (pointInTriangle(p, ac, a, b)) {
                        c = b;
                        b = ac;
                    }
                    else if (pointInTriangle(p, ac, a, c)) {
                        a = c;
                        c = b;
                        b = ac;
                    }
                    else {
                        return -1.0f;
                    }
                }

                return (a.z + b.z + c.z) / 3.0f;
            }

            bool castRay(const float3 ro, const float3 rd, float resT)
            {
                const float dt = 0.01f;
                const float mint = 0.001f;
                const float maxt = 10.0f;
                for (float t = mint; t < maxt; t += dt)
                {
                    const float3 p = ro + rd * t;
                    if (p.y < rightTriangle(p.xz, 12))
                    {
                        resT = t - 0.5f * dt;
                        return true;
                    }
                }
                return false;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // sample the texture
                //fixed4 col = i.color;//tex2D(_MainTex, i.uv);
                // apply fog
                //UNITY_APPLY_FOG(i.fogCoord, col);

                fixed4 col = fixed4(1,1,1,1);

                return col;
            }
            ENDCG

            

            
        }
    }
}
