Shader "Custom/RayMarchingSurfaceShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)

		_Scale("Scale", float) = 100
		_Stretch("Stretch", Range(1,100000)) = 1000
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

		_Error("Error", float) = 1.0
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		//#pragma surface surf Standard fullforwardshadows
		//#pragma surface surf Lambert vertex:vert
		#pragma surface surf Lambert vertex:vert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		//#include "AutoLight.cginc"

		#include "UnityStandardUtils.cginc"
		#include "RayMarchingLib.cginc"

		struct Input
		{
			float2 uv_MainTex;
			float3 worldPos;
			float3 viewDir;
			float3 wTangent;
			float3 wNormal;
			INTERNAL_DATA
		};

		fixed3 _Color;

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

		int _Depth;
		int _MinDepth;
		float3 _Seed;

		float _DepthThreshold;

		float _Error;

		float _ZDepth;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.wTangent = normalize(UnityObjectToWorldDir(v.tangent.xyz));
			o.wNormal = normalize(UnityObjectToWorldNormal(v.normal.xyz));
		}

		//https://iquilezles.org/articles/terrainmarching/
		//https://bgolus.medium.com/rendering-a-sphere-on-a-quad-13c92025570c#c582
		void surf(Input IN, inout SurfaceOutput o)
		{
			// Construct options struct
			raymarch_options options;
			options.minDistance = _NearPlane;
			options.maxDistance = _ViewDistance;
			options.minDepth = _MinDepth;
			options.maxDepth = _Depth;
			options.scale = _Scale;
			options.stretch = _Stretch;
			options.step = _Step;
			options.stepDelta = _StepDelta;
			options.distanceFactor = _DistanceFactor;
			options.error = _Error;

			// Shader
			float3 origin = _WorldSpaceCameraPos;
			float3 surfacePosition = IN.worldPos;
			float3 direction = normalize(surfacePosition - origin);

			float hit = castRay(options, origin, surfacePosition);

			clip(hit);

			float3 rayHit = origin + direction * hit;

			fixed3 albedo = fixed3((fixed3)_VoidColor);

			float t = (rayHit.y + options.scale) / 2 / options.scale;

			// world space terrain normal
			float3 terrainNormal = getNormal(options, rayHit, length(rayHit.xyz - _WorldSpaceCameraPos.xyz));

			o.Albedo = _Color;
			o.Normal = worldToTangentNormalVector(IN.wNormal, IN.wTangent, terrainNormal);

			// Calculate depth for shadows
			float4 clipPos = UnityWorldToClipPos(float4(rayHit, 1.0f));
			_ZDepth = clipPos.z / clipPos.w;
		}
		ENDCG
	}
		FallBack "Diffuse"
}









//void surf(Input IN, inout SurfaceOutputStandard o)
//{
//    // World space
//    float3 cameraPosition = _WorldSpaceCameraPos;
//
//    float3 direction = normalize(IN.worldPos - cameraPosition);
//
//    fixed3 albedo = _VoidColor;
//
//    bool hit = false;
//
//    if (length(IN.worldPos - cameraPosition) < _ViewDistance) {
//        hit = castRay(_WorldSpaceCameraPos, IN.worldPos, direction);
//    }
//
//    if (hit) {
//        fixed3 Gray = fixed3(0.5f, 0.5f, 0.5f);
//        fixed3 Blue = fixed3(0.0f, 0.0f, 1.0f);
//        fixed3 Black = fixed3(0.0f, 0.0f, 0.0f);
//
//        fixed3 Green = fixed3(0.0f, 1.0f, 0.0f);
//        fixed3 DarkGreen = fixed3(0.0f, 0.5f, 0.0f);
//        fixed3 Brown = fixed3(0.9f, 0.6f, 0.2f);
//
//        float t = (rayHit.y + _Scale) / 2 / _Scale;
//
//        albedo = fixed3(lerp(DarkGreen, Brown, t));
//
//        // Fog
//        albedo = fixed3(lerp(albedo, _SkyColor, length(rayHit - _WorldSpaceCameraPos) / _ViewDistance));
//    }
//    else if (!hit) {
//        albedo = _SkyColor;
//    }
//
//    float3 worldNormal = getNormal(rayHit);
//
//    // Generated stuff
//    // Albedo comes from a texture tinted by color
//    //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
//    o.Albedo = albedo;
//    // Metallic and smoothness come from slider variables
//    o.Metallic = _Metallic;
//    o.Smoothness = _Glossiness;
//    o.Emission = 0;
//    //o.Normal = WorldToTangentNormalVector(IN, worldNormal);
//
//    if (hit) {
//        o.Alpha = 1.0f;
//    }
//    else {
//        o.Alpha = 0.0f;
//    }
//}