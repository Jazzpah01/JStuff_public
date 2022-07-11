// Upgrade NOTE: replaced 'defined FOG_COMBINED_WITH_WORLD_POS' with 'defined (FOG_COMBINED_WITH_WORLD_POS)'

Shader "Custom/RayMarchingSurfaceShader Gen"
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


		// ------------------------------------------------------------
		// Surface shader code generated out of a CGPROGRAM block:


		// ---- forward rendering base pass:
		Pass {
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }

	CGPROGRAM
		// compile directives
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma multi_compile_fog
		#pragma multi_compile_fwdbase
		#include "HLSLSupport.cginc"
		#define UNITY_INSTANCED_LOD_FADE
		#define UNITY_INSTANCED_SH
		#define UNITY_INSTANCED_LIGHTMAPSTS
		#include "UnityShaderVariables.cginc"
		#include "UnityShaderUtilities.cginc"
		// -------- variant for: <when no other keywords are defined>
		#if !defined(INSTANCING_ON)
		// Surface shader code generated based on:
		// vertex modifier: 'vert'
		// writes to per-pixel normal: YES
		// writes to emission: no
		// writes to occlusion: no
		// needs world space reflection vector: no
		// needs world space normal vector: no
		// needs screen space position: no
		// needs world space position: YES
		// needs view direction: no
		// needs world space view direction: no
		// needs world space position for lighting: YES
		// needs world space view direction for lighting: no
		// needs world space view direction for lightmaps: no
		// needs vertex color: no
		// needs VFACE: no
		// needs SV_IsFrontFace: no
		// passes tangent-to-world matrix to pixel shader: YES
		// reads from normal: no
		// 0 texcoords actually used
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"

		#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
		#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
		#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

		// Original surface shader snippet:
		#line 33 ""
		#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
		#endif
		/* UNITY: Original start of shader */
				// Physically based Standard lighting model, and enable shadows on all light types
				////#pragma surface surf Standard fullforwardshadows
				////#pragma surface surf Lambert vertex:vert
				//#pragma surface surf Lambert vertex:vert

				// Use shader model 3.0 target, to get nicer looking lighting
				//#pragma target 3.0

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
				// //#pragma instancing_options assumeuniformscaling
				UNITY_INSTANCING_BUFFER_START(Props)
					// put more per-instance properties here
				UNITY_INSTANCING_BUFFER_END(Props)

				void vert(inout appdata_full v, out Input o) {
					UNITY_INITIALIZE_OUTPUT(Input, o);
					o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
					o.wNormal = UnityObjectToWorldNormal(v.normal);
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


				// vertex-to-fragment interpolation data
				// no lightmaps:
				#ifndef LIGHTMAP_ON
				// half-precision fragment shader registers:
				#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
				#define FOG_COMBINED_WITH_TSPACE
				struct v2f_surf {
				  UNITY_POSITION(pos);
				  float4 tSpace0 : TEXCOORD0;
				  float4 tSpace1 : TEXCOORD1;
				  float4 tSpace2 : TEXCOORD2;
				  float3 custompack0 : TEXCOORD3; // wTangent
				  float3 custompack1 : TEXCOORD4; // wNormal
				  #if UNITY_SHOULD_SAMPLE_SH
				  half3 sh : TEXCOORD5; // SH
				  #endif
				  UNITY_LIGHTING_COORDS(6,7)
				  #if SHADER_TARGET >= 30
				  float4 lmap : TEXCOORD8;
				  #endif
				  UNITY_VERTEX_INPUT_INSTANCE_ID
				  UNITY_VERTEX_OUTPUT_STEREO
				};
				#endif
				// high-precision fragment shader registers:
				#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
				struct v2f_surf {
				  UNITY_POSITION(pos);
				  float4 tSpace0 : TEXCOORD0;
				  float4 tSpace1 : TEXCOORD1;
				  float4 tSpace2 : TEXCOORD2;
				  float3 custompack0 : TEXCOORD3; // wTangent
				  float3 custompack1 : TEXCOORD4; // wNormal
				  #if UNITY_SHOULD_SAMPLE_SH
				  half3 sh : TEXCOORD5; // SH
				  #endif
				  UNITY_FOG_COORDS(6)
				  UNITY_SHADOW_COORDS(7)
				  #if SHADER_TARGET >= 30
				  float4 lmap : TEXCOORD8;
				  #endif
				  UNITY_VERTEX_INPUT_INSTANCE_ID
				  UNITY_VERTEX_OUTPUT_STEREO
				};
				#endif
				#endif
				// with lightmaps:
				#ifdef LIGHTMAP_ON
				// half-precision fragment shader registers:
				#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
				#define FOG_COMBINED_WITH_TSPACE
				struct v2f_surf {
				  UNITY_POSITION(pos);
				  float4 tSpace0 : TEXCOORD0;
				  float4 tSpace1 : TEXCOORD1;
				  float4 tSpace2 : TEXCOORD2;
				  float3 custompack0 : TEXCOORD3; // wTangent
				  float3 custompack1 : TEXCOORD4; // wNormal
				  float4 lmap : TEXCOORD5;
				  UNITY_LIGHTING_COORDS(6,7)
				  UNITY_VERTEX_INPUT_INSTANCE_ID
				  UNITY_VERTEX_OUTPUT_STEREO
				};
				#endif
				// high-precision fragment shader registers:
				#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
				struct v2f_surf {
				  UNITY_POSITION(pos);
				  float4 tSpace0 : TEXCOORD0;
				  float4 tSpace1 : TEXCOORD1;
				  float4 tSpace2 : TEXCOORD2;
				  float3 custompack0 : TEXCOORD3; // wTangent
				  float3 custompack1 : TEXCOORD4; // wNormal
				  float4 lmap : TEXCOORD5;
				  UNITY_FOG_COORDS(6)
				  UNITY_SHADOW_COORDS(7)
				  UNITY_VERTEX_INPUT_INSTANCE_ID
				  UNITY_VERTEX_OUTPUT_STEREO
				};
				#endif
				#endif

				// vertex shader
				v2f_surf vert_surf(appdata_full v) {
				  UNITY_SETUP_INSTANCE_ID(v);
				  v2f_surf o;
				  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
				  UNITY_TRANSFER_INSTANCE_ID(v,o);
				  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				  Input customInputData;
				  vert(v, customInputData);
				  o.custompack0.xyz = customInputData.wTangent;
				  o.custompack1.xyz = customInputData.wNormal;
				  o.pos = UnityObjectToClipPos(v.vertex);
				  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
				  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
				  #ifdef DYNAMICLIGHTMAP_ON
				  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
				  #endif
				  #ifdef LIGHTMAP_ON
				  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				  #endif

				  // SH/ambient and vertex lights
				  #ifndef LIGHTMAP_ON
					#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
					  o.sh = 0;
					  // Approximated illumination from non-important point lights
					  #ifdef VERTEXLIGHT_ON
						o.sh += Shade4PointLights(
						  unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
						  unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
						  unity_4LightAtten0, worldPos, worldNormal);
					  #endif
					  o.sh = ShadeSHPerVertex(worldNormal, o.sh);
					#endif
				  #endif // !LIGHTMAP_ON

				  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
				  #ifdef FOG_COMBINED_WITH_TSPACE
					UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
				  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
					UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
				  #else
					UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
				  #endif
				  return o;
				}

				// fragment shader
				fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
				  UNITY_SETUP_INSTANCE_ID(IN);
				// prepare and unpack data
				Input surfIN;
				#ifdef FOG_COMBINED_WITH_TSPACE
				  UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
				#elif defined (FOG_COMBINED_WITH_WORLD_POS)
				  UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
				#else
				  UNITY_EXTRACT_FOG(IN);
				#endif
				#ifdef FOG_COMBINED_WITH_TSPACE
				  UNITY_RECONSTRUCT_TBN(IN);
				#else
				  UNITY_EXTRACT_TBN(IN);
				#endif
				UNITY_INITIALIZE_OUTPUT(Input,surfIN);
				surfIN.uv_MainTex.x = 1.0;
				surfIN.worldPos.x = 1.0;
				surfIN.viewDir.x = 1.0;
				surfIN.wTangent.x = 1.0;
				surfIN.wNormal.x = 1.0;
				surfIN.wTangent = IN.custompack0.xyz;
				surfIN.wNormal = IN.custompack1.xyz;
				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
				#ifndef USING_DIRECTIONAL_LIGHT
				  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
				#else
				  fixed3 lightDir = _WorldSpaceLightPos0.xyz;
				#endif
				surfIN.worldPos = worldPos;
				#ifdef UNITY_COMPILER_HLSL
				SurfaceOutput o = (SurfaceOutput)0;
				#else
				SurfaceOutput o;
				#endif
				o.Albedo = 0.0;
				o.Emission = 0.0;
				o.Specular = 0.0;
				o.Alpha = 0.0;
				o.Gloss = 0.0;
				fixed3 normalWorldVertex = fixed3(0,0,1);
				o.Normal = fixed3(0,0,1);

				// call surface function
				surf(surfIN, o); outDepth = _ZDepth;

				// compute lighting & shadowing factor
				UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
				fixed4 c = 0;
				float3 worldN;
				worldN.x = dot(_unity_tbn_0, o.Normal);
				worldN.y = dot(_unity_tbn_1, o.Normal);
				worldN.z = dot(_unity_tbn_2, o.Normal);
				worldN = normalize(worldN);
				o.Normal = worldN;

				// Setup lighting environment
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;
				// Call GI (lightmaps/SH/reflections) lighting function
				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.atten = atten;
				#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				  giInput.lightmapUV = IN.lmap;
				#else
				  giInput.lightmapUV = 0.0;
				#endif
				#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
				  giInput.ambient = IN.sh;
				#else
				  giInput.ambient.rgb = 0.0;
				#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
				#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
				  giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
				#endif
				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
				  giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				  giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				  giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				  giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				  giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
				#endif
				LightingLambert_GI(o, giInput, gi);

				// realtime lighting: call lighting function
				c += LightingLambert(o, gi);
				UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
				UNITY_OPAQUE_ALPHA(c.a);
				return c;
			  }


			  #endif

					// -------- variant for: INSTANCING_ON 
					#if defined(INSTANCING_ON)
					// Surface shader code generated based on:
					// vertex modifier: 'vert'
					// writes to per-pixel normal: YES
					// writes to emission: no
					// writes to occlusion: no
					// needs world space reflection vector: no
					// needs world space normal vector: no
					// needs screen space position: no
					// needs world space position: YES
					// needs view direction: no
					// needs world space view direction: no
					// needs world space position for lighting: YES
					// needs world space view direction for lighting: no
					// needs world space view direction for lightmaps: no
					// needs vertex color: no
					// needs VFACE: no
					// needs SV_IsFrontFace: no
					// passes tangent-to-world matrix to pixel shader: YES
					// reads from normal: no
					// 0 texcoords actually used
					#include "UnityCG.cginc"
					#include "Lighting.cginc"
					#include "AutoLight.cginc"

					#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
					#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
					#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

					// Original surface shader snippet:
					#line 33 ""
					#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
					#endif
					/* UNITY: Original start of shader */
							// Physically based Standard lighting model, and enable shadows on all light types
							////#pragma surface surf Standard fullforwardshadows
							////#pragma surface surf Lambert vertex:vert
							//#pragma surface surf Lambert vertex:vert

							// Use shader model 3.0 target, to get nicer looking lighting
							//#pragma target 3.0

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
							// //#pragma instancing_options assumeuniformscaling
							UNITY_INSTANCING_BUFFER_START(Props)
								// put more per-instance properties here
							UNITY_INSTANCING_BUFFER_END(Props)

							void vert(inout appdata_full v, out Input o) {
								UNITY_INITIALIZE_OUTPUT(Input, o);
								o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
								o.wNormal = UnityObjectToWorldNormal(v.normal);
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


							// vertex-to-fragment interpolation data
							// no lightmaps:
							#ifndef LIGHTMAP_ON
							// half-precision fragment shader registers:
							#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
							#define FOG_COMBINED_WITH_TSPACE
							struct v2f_surf {
							  UNITY_POSITION(pos);
							  float4 tSpace0 : TEXCOORD0;
							  float4 tSpace1 : TEXCOORD1;
							  float4 tSpace2 : TEXCOORD2;
							  float3 custompack0 : TEXCOORD3; // wTangent
							  float3 custompack1 : TEXCOORD4; // wNormal
							  #if UNITY_SHOULD_SAMPLE_SH
							  half3 sh : TEXCOORD5; // SH
							  #endif
							  UNITY_LIGHTING_COORDS(6,7)
							  #if SHADER_TARGET >= 30
							  float4 lmap : TEXCOORD8;
							  #endif
							  UNITY_VERTEX_INPUT_INSTANCE_ID
							  UNITY_VERTEX_OUTPUT_STEREO
							};
							#endif
							// high-precision fragment shader registers:
							#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
							struct v2f_surf {
							  UNITY_POSITION(pos);
							  float4 tSpace0 : TEXCOORD0;
							  float4 tSpace1 : TEXCOORD1;
							  float4 tSpace2 : TEXCOORD2;
							  float3 custompack0 : TEXCOORD3; // wTangent
							  float3 custompack1 : TEXCOORD4; // wNormal
							  #if UNITY_SHOULD_SAMPLE_SH
							  half3 sh : TEXCOORD5; // SH
							  #endif
							  UNITY_FOG_COORDS(6)
							  UNITY_SHADOW_COORDS(7)
							  #if SHADER_TARGET >= 30
							  float4 lmap : TEXCOORD8;
							  #endif
							  UNITY_VERTEX_INPUT_INSTANCE_ID
							  UNITY_VERTEX_OUTPUT_STEREO
							};
							#endif
							#endif
							// with lightmaps:
							#ifdef LIGHTMAP_ON
							// half-precision fragment shader registers:
							#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
							#define FOG_COMBINED_WITH_TSPACE
							struct v2f_surf {
							  UNITY_POSITION(pos);
							  float4 tSpace0 : TEXCOORD0;
							  float4 tSpace1 : TEXCOORD1;
							  float4 tSpace2 : TEXCOORD2;
							  float3 custompack0 : TEXCOORD3; // wTangent
							  float3 custompack1 : TEXCOORD4; // wNormal
							  float4 lmap : TEXCOORD5;
							  UNITY_LIGHTING_COORDS(6,7)
							  UNITY_VERTEX_INPUT_INSTANCE_ID
							  UNITY_VERTEX_OUTPUT_STEREO
							};
							#endif
							// high-precision fragment shader registers:
							#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
							struct v2f_surf {
							  UNITY_POSITION(pos);
							  float4 tSpace0 : TEXCOORD0;
							  float4 tSpace1 : TEXCOORD1;
							  float4 tSpace2 : TEXCOORD2;
							  float3 custompack0 : TEXCOORD3; // wTangent
							  float3 custompack1 : TEXCOORD4; // wNormal
							  float4 lmap : TEXCOORD5;
							  UNITY_FOG_COORDS(6)
							  UNITY_SHADOW_COORDS(7)
							  UNITY_VERTEX_INPUT_INSTANCE_ID
							  UNITY_VERTEX_OUTPUT_STEREO
							};
							#endif
							#endif

							// vertex shader
							v2f_surf vert_surf(appdata_full v) {
							  UNITY_SETUP_INSTANCE_ID(v);
							  v2f_surf o;
							  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
							  UNITY_TRANSFER_INSTANCE_ID(v,o);
							  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
							  Input customInputData;
							  vert(v, customInputData);
							  o.custompack0.xyz = customInputData.wTangent;
							  o.custompack1.xyz = customInputData.wNormal;
							  o.pos = UnityObjectToClipPos(v.vertex);
							  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
							  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
							  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
							  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
							  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
							  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
							  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
							  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
							  #ifdef DYNAMICLIGHTMAP_ON
							  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
							  #endif
							  #ifdef LIGHTMAP_ON
							  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
							  #endif

							  // SH/ambient and vertex lights
							  #ifndef LIGHTMAP_ON
								#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
								  o.sh = 0;
								  // Approximated illumination from non-important point lights
								  #ifdef VERTEXLIGHT_ON
									o.sh += Shade4PointLights(
									  unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
									  unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
									  unity_4LightAtten0, worldPos, worldNormal);
								  #endif
								  o.sh = ShadeSHPerVertex(worldNormal, o.sh);
								#endif
							  #endif // !LIGHTMAP_ON

							  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
							  #ifdef FOG_COMBINED_WITH_TSPACE
								UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
							  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
								UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
							  #else
								UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
							  #endif
							  return o;
							}

							// fragment shader
							fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
							  UNITY_SETUP_INSTANCE_ID(IN);
							// prepare and unpack data
							Input surfIN;
							#ifdef FOG_COMBINED_WITH_TSPACE
							  UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
							#elif defined (FOG_COMBINED_WITH_WORLD_POS)
							  UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
							#else
							  UNITY_EXTRACT_FOG(IN);
							#endif
							#ifdef FOG_COMBINED_WITH_TSPACE
							  UNITY_RECONSTRUCT_TBN(IN);
							#else
							  UNITY_EXTRACT_TBN(IN);
							#endif
							UNITY_INITIALIZE_OUTPUT(Input,surfIN);
							surfIN.uv_MainTex.x = 1.0;
							surfIN.worldPos.x = 1.0;
							surfIN.viewDir.x = 1.0;
							surfIN.wTangent.x = 1.0;
							surfIN.wNormal.x = 1.0;
							surfIN.wTangent = IN.custompack0.xyz;
							surfIN.wNormal = IN.custompack1.xyz;
							float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
							#ifndef USING_DIRECTIONAL_LIGHT
							  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
							#else
							  fixed3 lightDir = _WorldSpaceLightPos0.xyz;
							#endif
							surfIN.worldPos = worldPos;
							#ifdef UNITY_COMPILER_HLSL
							SurfaceOutput o = (SurfaceOutput)0;
							#else
							SurfaceOutput o;
							#endif
							o.Albedo = 0.0;
							o.Emission = 0.0;
							o.Specular = 0.0;
							o.Alpha = 0.0;
							o.Gloss = 0.0;
							fixed3 normalWorldVertex = fixed3(0,0,1);
							o.Normal = fixed3(0,0,1);

							// call surface function
							surf(surfIN, o); outDepth = _ZDepth;

							// compute lighting & shadowing factor
							UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
							fixed4 c = 0;
							float3 worldN;
							worldN.x = dot(_unity_tbn_0, o.Normal);
							worldN.y = dot(_unity_tbn_1, o.Normal);
							worldN.z = dot(_unity_tbn_2, o.Normal);
							worldN = normalize(worldN);
							o.Normal = worldN;

							// Setup lighting environment
							UnityGI gi;
							UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
							gi.indirect.diffuse = 0;
							gi.indirect.specular = 0;
							gi.light.color = _LightColor0.rgb;
							gi.light.dir = lightDir;
							// Call GI (lightmaps/SH/reflections) lighting function
							UnityGIInput giInput;
							UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
							giInput.light = gi.light;
							giInput.worldPos = worldPos;
							giInput.atten = atten;
							#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
							  giInput.lightmapUV = IN.lmap;
							#else
							  giInput.lightmapUV = 0.0;
							#endif
							#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
							  giInput.ambient = IN.sh;
							#else
							  giInput.ambient.rgb = 0.0;
							#endif
							giInput.probeHDR[0] = unity_SpecCube0_HDR;
							giInput.probeHDR[1] = unity_SpecCube1_HDR;
							#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
							  giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
							#endif
							#ifdef UNITY_SPECCUBE_BOX_PROJECTION
							  giInput.boxMax[0] = unity_SpecCube0_BoxMax;
							  giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
							  giInput.boxMax[1] = unity_SpecCube1_BoxMax;
							  giInput.boxMin[1] = unity_SpecCube1_BoxMin;
							  giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
							#endif
							LightingLambert_GI(o, giInput, gi);

							// realtime lighting: call lighting function
							c += LightingLambert(o, gi);
							UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
							UNITY_OPAQUE_ALPHA(c.a);
							return c;
						  }


						  #endif


						  ENDCG

						  }

		// ---- forward rendering additive lights pass:
		Pass {
			Name "FORWARD"
			Tags { "LightMode" = "ForwardAdd" }
			ZWrite Off Blend One One

	CGPROGRAM
							  // compile directives
							  #pragma vertex vert_surf
							  #pragma fragment frag_surf
							  #pragma target 3.0
							  #pragma multi_compile_instancing
							  #pragma multi_compile_fog
							  #pragma skip_variants INSTANCING_ON
							  #pragma multi_compile_fwdadd
							  #include "HLSLSupport.cginc"
							  #define UNITY_INSTANCED_LOD_FADE
							  #define UNITY_INSTANCED_SH
							  #define UNITY_INSTANCED_LIGHTMAPSTS
							  #include "UnityShaderVariables.cginc"
							  #include "UnityShaderUtilities.cginc"
							  // -------- variant for: <when no other keywords are defined>
							  #if !defined(INSTANCING_ON)
							  // Surface shader code generated based on:
							  // vertex modifier: 'vert'
							  // writes to per-pixel normal: YES
							  // writes to emission: no
							  // writes to occlusion: no
							  // needs world space reflection vector: no
							  // needs world space normal vector: no
							  // needs screen space position: no
							  // needs world space position: YES
							  // needs view direction: no
							  // needs world space view direction: no
							  // needs world space position for lighting: YES
							  // needs world space view direction for lighting: no
							  // needs world space view direction for lightmaps: no
							  // needs vertex color: no
							  // needs VFACE: no
							  // needs SV_IsFrontFace: no
							  // passes tangent-to-world matrix to pixel shader: YES
							  // reads from normal: no
							  // 0 texcoords actually used
							  #include "UnityCG.cginc"
							  #include "Lighting.cginc"
							  #include "AutoLight.cginc"

							  #define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
							  #define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
							  #define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

							  // Original surface shader snippet:
							  #line 33 ""
							  #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
							  #endif
							  /* UNITY: Original start of shader */
									  // Physically based Standard lighting model, and enable shadows on all light types
									  ////#pragma surface surf Standard fullforwardshadows
									  ////#pragma surface surf Lambert vertex:vert
									  //#pragma surface surf Lambert vertex:vert

									  // Use shader model 3.0 target, to get nicer looking lighting
									  //#pragma target 3.0

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
									  // //#pragma instancing_options assumeuniformscaling
									  UNITY_INSTANCING_BUFFER_START(Props)
										  // put more per-instance properties here
									  UNITY_INSTANCING_BUFFER_END(Props)

									  void vert(inout appdata_full v, out Input o) {
										  UNITY_INITIALIZE_OUTPUT(Input, o);
										  o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
										  o.wNormal = UnityObjectToWorldNormal(v.normal);
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


									  // vertex-to-fragment interpolation data
									  struct v2f_surf {
										UNITY_POSITION(pos);
										float3 tSpace0 : TEXCOORD0;
										float3 tSpace1 : TEXCOORD1;
										float3 tSpace2 : TEXCOORD2;
										float3 worldPos : TEXCOORD3;
										float3 custompack0 : TEXCOORD4; // wTangent
										float3 custompack1 : TEXCOORD5; // wNormal
										UNITY_LIGHTING_COORDS(6,7)
										UNITY_FOG_COORDS(8)
										UNITY_VERTEX_INPUT_INSTANCE_ID
										UNITY_VERTEX_OUTPUT_STEREO
									  };

									  // vertex shader
									  v2f_surf vert_surf(appdata_full v) {
										UNITY_SETUP_INSTANCE_ID(v);
										v2f_surf o;
										UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
										UNITY_TRANSFER_INSTANCE_ID(v,o);
										UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
										Input customInputData;
										vert(v, customInputData);
										o.custompack0.xyz = customInputData.wTangent;
										o.custompack1.xyz = customInputData.wNormal;
										o.pos = UnityObjectToClipPos(v.vertex);
										float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
										float3 worldNormal = UnityObjectToWorldNormal(v.normal);
										fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
										fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
										fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
										o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
										o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
										o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
										o.worldPos.xyz = worldPos;

										UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
										UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
										return o;
									  }

									  // fragment shader
									  fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
										UNITY_SETUP_INSTANCE_ID(IN);
									  // prepare and unpack data
									  Input surfIN;
									  #ifdef FOG_COMBINED_WITH_TSPACE
										UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
									  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
										UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
									  #else
										UNITY_EXTRACT_FOG(IN);
									  #endif
									  #ifdef FOG_COMBINED_WITH_TSPACE
										UNITY_RECONSTRUCT_TBN(IN);
									  #else
										UNITY_EXTRACT_TBN(IN);
									  #endif
									  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
									  surfIN.uv_MainTex.x = 1.0;
									  surfIN.worldPos.x = 1.0;
									  surfIN.viewDir.x = 1.0;
									  surfIN.wTangent.x = 1.0;
									  surfIN.wNormal.x = 1.0;
									  surfIN.wTangent = IN.custompack0.xyz;
									  surfIN.wNormal = IN.custompack1.xyz;
									  float3 worldPos = IN.worldPos.xyz;
									  #ifndef USING_DIRECTIONAL_LIGHT
										fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
									  #else
										fixed3 lightDir = _WorldSpaceLightPos0.xyz;
									  #endif
									  surfIN.worldPos = worldPos;
									  #ifdef UNITY_COMPILER_HLSL
									  SurfaceOutput o = (SurfaceOutput)0;
									  #else
									  SurfaceOutput o;
									  #endif
									  o.Albedo = 0.0;
									  o.Emission = 0.0;
									  o.Specular = 0.0;
									  o.Alpha = 0.0;
									  o.Gloss = 0.0;
									  fixed3 normalWorldVertex = fixed3(0,0,1);
									  o.Normal = fixed3(0,0,1);

									  // call surface function
									  surf(surfIN, o); outDepth = _ZDepth;
									  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
									  fixed4 c = 0;
									  float3 worldN;
									  worldN.x = dot(_unity_tbn_0, o.Normal);
									  worldN.y = dot(_unity_tbn_1, o.Normal);
									  worldN.z = dot(_unity_tbn_2, o.Normal);
									  worldN = normalize(worldN);
									  o.Normal = worldN;

									  // Setup lighting environment
									  UnityGI gi;
									  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
									  gi.indirect.diffuse = 0;
									  gi.indirect.specular = 0;
									  gi.light.color = _LightColor0.rgb;
									  gi.light.dir = lightDir;
									  gi.light.color *= atten;
									  c += LightingLambert(o, gi);
									  c.a = 0.0;
									  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
									  UNITY_OPAQUE_ALPHA(c.a);
									  return c;
									}


									#endif


									ENDCG

									}

							  // ---- deferred lighting base geometry pass:
							  Pass {
								  Name "PREPASS"
								  Tags { "LightMode" = "PrePassBase" }

						  CGPROGRAM
										// compile directives
										#pragma vertex vert_surf
										#pragma fragment frag_surf
										#pragma target 3.0
										#pragma multi_compile_instancing
										#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2

										#include "HLSLSupport.cginc"
										#define UNITY_INSTANCED_LOD_FADE
										#define UNITY_INSTANCED_SH
										#define UNITY_INSTANCED_LIGHTMAPSTS
										#include "UnityShaderVariables.cginc"
										#include "UnityShaderUtilities.cginc"
										// -------- variant for: <when no other keywords are defined>
										#if !defined(INSTANCING_ON)
										// Surface shader code generated based on:
										// vertex modifier: 'vert'
										// writes to per-pixel normal: YES
										// writes to emission: no
										// writes to occlusion: no
										// needs world space reflection vector: no
										// needs world space normal vector: no
										// needs screen space position: no
										// needs world space position: YES
										// needs view direction: no
										// needs world space view direction: no
										// needs world space position for lighting: YES
										// needs world space view direction for lighting: no
										// needs world space view direction for lightmaps: no
										// needs vertex color: no
										// needs VFACE: no
										// needs SV_IsFrontFace: no
										// passes tangent-to-world matrix to pixel shader: YES
										// reads from normal: no
										// 0 texcoords actually used
										#include "UnityCG.cginc"
										#include "Lighting.cginc"

										#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
										#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
										#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

										// Original surface shader snippet:
										#line 33 ""
										#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
										#endif
										/* UNITY: Original start of shader */
												// Physically based Standard lighting model, and enable shadows on all light types
												////#pragma surface surf Standard fullforwardshadows
												////#pragma surface surf Lambert vertex:vert
												//#pragma surface surf Lambert vertex:vert

												// Use shader model 3.0 target, to get nicer looking lighting
												//#pragma target 3.0

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
												// //#pragma instancing_options assumeuniformscaling
												UNITY_INSTANCING_BUFFER_START(Props)
													// put more per-instance properties here
												UNITY_INSTANCING_BUFFER_END(Props)

												void vert(inout appdata_full v, out Input o) {
													UNITY_INITIALIZE_OUTPUT(Input, o);
													o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
													o.wNormal = UnityObjectToWorldNormal(v.normal);
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


												// vertex-to-fragment interpolation data
												struct v2f_surf {
												  UNITY_POSITION(pos);
												  float4 tSpace0 : TEXCOORD0;
												  float4 tSpace1 : TEXCOORD1;
												  float4 tSpace2 : TEXCOORD2;
												  float3 custompack0 : TEXCOORD3; // wTangent
												  float3 custompack1 : TEXCOORD4; // wNormal
												  UNITY_VERTEX_INPUT_INSTANCE_ID
												  UNITY_VERTEX_OUTPUT_STEREO
												};

												// vertex shader
												v2f_surf vert_surf(appdata_full v) {
												  UNITY_SETUP_INSTANCE_ID(v);
												  v2f_surf o;
												  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
												  UNITY_TRANSFER_INSTANCE_ID(v,o);
												  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
												  Input customInputData;
												  vert(v, customInputData);
												  o.custompack0.xyz = customInputData.wTangent;
												  o.custompack1.xyz = customInputData.wNormal;
												  o.pos = UnityObjectToClipPos(v.vertex);
												  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
												  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
												  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
												  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
												  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
												  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
												  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
												  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
												  return o;
												}

												// fragment shader
												fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
												  UNITY_SETUP_INSTANCE_ID(IN);
												// prepare and unpack data
												Input surfIN;
												#ifdef FOG_COMBINED_WITH_TSPACE
												  UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
												#elif defined (FOG_COMBINED_WITH_WORLD_POS)
												  UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
												#else
												  UNITY_EXTRACT_FOG(IN);
												#endif
												#ifdef FOG_COMBINED_WITH_TSPACE
												  UNITY_RECONSTRUCT_TBN(IN);
												#else
												  UNITY_EXTRACT_TBN(IN);
												#endif
												UNITY_INITIALIZE_OUTPUT(Input,surfIN);
												surfIN.uv_MainTex.x = 1.0;
												surfIN.worldPos.x = 1.0;
												surfIN.viewDir.x = 1.0;
												surfIN.wTangent.x = 1.0;
												surfIN.wNormal.x = 1.0;
												surfIN.wTangent = IN.custompack0.xyz;
												surfIN.wNormal = IN.custompack1.xyz;
												float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
												#ifndef USING_DIRECTIONAL_LIGHT
												  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
												#else
												  fixed3 lightDir = _WorldSpaceLightPos0.xyz;
												#endif
												surfIN.worldPos = worldPos;
												#ifdef UNITY_COMPILER_HLSL
												SurfaceOutput o = (SurfaceOutput)0;
												#else
												SurfaceOutput o;
												#endif
												o.Albedo = 0.0;
												o.Emission = 0.0;
												o.Specular = 0.0;
												o.Alpha = 0.0;
												o.Gloss = 0.0;
												fixed3 normalWorldVertex = fixed3(0,0,1);
												o.Normal = fixed3(0,0,1);

												// call surface function
												surf(surfIN, o); outDepth = _ZDepth;
												float3 worldN;
												worldN.x = dot(_unity_tbn_0, o.Normal);
												worldN.y = dot(_unity_tbn_1, o.Normal);
												worldN.z = dot(_unity_tbn_2, o.Normal);
												worldN = normalize(worldN);
												o.Normal = worldN;

												// output normal and specular
												fixed4 res;
												res.rgb = o.Normal * 0.5 + 0.5;
												res.a = o.Specular;
												return res;
											  }


											  #endif

													// -------- variant for: INSTANCING_ON 
													#if defined(INSTANCING_ON)
													// Surface shader code generated based on:
													// vertex modifier: 'vert'
													// writes to per-pixel normal: YES
													// writes to emission: no
													// writes to occlusion: no
													// needs world space reflection vector: no
													// needs world space normal vector: no
													// needs screen space position: no
													// needs world space position: YES
													// needs view direction: no
													// needs world space view direction: no
													// needs world space position for lighting: YES
													// needs world space view direction for lighting: no
													// needs world space view direction for lightmaps: no
													// needs vertex color: no
													// needs VFACE: no
													// needs SV_IsFrontFace: no
													// passes tangent-to-world matrix to pixel shader: YES
													// reads from normal: no
													// 0 texcoords actually used
													#include "UnityCG.cginc"
													#include "Lighting.cginc"

													#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
													#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
													#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

													// Original surface shader snippet:
													#line 33 ""
													#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
													#endif
													/* UNITY: Original start of shader */
															// Physically based Standard lighting model, and enable shadows on all light types
															////#pragma surface surf Standard fullforwardshadows
															////#pragma surface surf Lambert vertex:vert
															//#pragma surface surf Lambert vertex:vert

															// Use shader model 3.0 target, to get nicer looking lighting
															//#pragma target 3.0

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
															// //#pragma instancing_options assumeuniformscaling
															UNITY_INSTANCING_BUFFER_START(Props)
																// put more per-instance properties here
															UNITY_INSTANCING_BUFFER_END(Props)

															void vert(inout appdata_full v, out Input o) {
																UNITY_INITIALIZE_OUTPUT(Input, o);
																o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
																o.wNormal = UnityObjectToWorldNormal(v.normal);
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


															// vertex-to-fragment interpolation data
															struct v2f_surf {
															  UNITY_POSITION(pos);
															  float4 tSpace0 : TEXCOORD0;
															  float4 tSpace1 : TEXCOORD1;
															  float4 tSpace2 : TEXCOORD2;
															  float3 custompack0 : TEXCOORD3; // wTangent
															  float3 custompack1 : TEXCOORD4; // wNormal
															  UNITY_VERTEX_INPUT_INSTANCE_ID
															  UNITY_VERTEX_OUTPUT_STEREO
															};

															// vertex shader
															v2f_surf vert_surf(appdata_full v) {
															  UNITY_SETUP_INSTANCE_ID(v);
															  v2f_surf o;
															  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
															  UNITY_TRANSFER_INSTANCE_ID(v,o);
															  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
															  Input customInputData;
															  vert(v, customInputData);
															  o.custompack0.xyz = customInputData.wTangent;
															  o.custompack1.xyz = customInputData.wNormal;
															  o.pos = UnityObjectToClipPos(v.vertex);
															  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
															  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
															  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
															  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
															  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
															  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
															  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
															  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
															  return o;
															}

															// fragment shader
															fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
															  UNITY_SETUP_INSTANCE_ID(IN);
															// prepare and unpack data
															Input surfIN;
															#ifdef FOG_COMBINED_WITH_TSPACE
															  UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
															#elif defined (FOG_COMBINED_WITH_WORLD_POS)
															  UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
															#else
															  UNITY_EXTRACT_FOG(IN);
															#endif
															#ifdef FOG_COMBINED_WITH_TSPACE
															  UNITY_RECONSTRUCT_TBN(IN);
															#else
															  UNITY_EXTRACT_TBN(IN);
															#endif
															UNITY_INITIALIZE_OUTPUT(Input,surfIN);
															surfIN.uv_MainTex.x = 1.0;
															surfIN.worldPos.x = 1.0;
															surfIN.viewDir.x = 1.0;
															surfIN.wTangent.x = 1.0;
															surfIN.wNormal.x = 1.0;
															surfIN.wTangent = IN.custompack0.xyz;
															surfIN.wNormal = IN.custompack1.xyz;
															float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
															#ifndef USING_DIRECTIONAL_LIGHT
															  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
															#else
															  fixed3 lightDir = _WorldSpaceLightPos0.xyz;
															#endif
															surfIN.worldPos = worldPos;
															#ifdef UNITY_COMPILER_HLSL
															SurfaceOutput o = (SurfaceOutput)0;
															#else
															SurfaceOutput o;
															#endif
															o.Albedo = 0.0;
															o.Emission = 0.0;
															o.Specular = 0.0;
															o.Alpha = 0.0;
															o.Gloss = 0.0;
															fixed3 normalWorldVertex = fixed3(0,0,1);
															o.Normal = fixed3(0,0,1);

															// call surface function
															surf(surfIN, o); outDepth = _ZDepth;
															float3 worldN;
															worldN.x = dot(_unity_tbn_0, o.Normal);
															worldN.y = dot(_unity_tbn_1, o.Normal);
															worldN.z = dot(_unity_tbn_2, o.Normal);
															worldN = normalize(worldN);
															o.Normal = worldN;

															// output normal and specular
															fixed4 res;
															res.rgb = o.Normal * 0.5 + 0.5;
															res.a = o.Specular;
															return res;
														  }


														  #endif


														  ENDCG

														  }

										// ---- deferred lighting final pass:
										Pass {
											Name "PREPASS"
											Tags { "LightMode" = "PrePassFinal" }
											ZWrite Off

									CGPROGRAM
															  // compile directives
															  #pragma vertex vert_surf
															  #pragma fragment frag_surf
															  #pragma target 3.0
															  #pragma multi_compile_instancing
															  #pragma multi_compile_fog
															  #pragma multi_compile_prepassfinal
															  #include "HLSLSupport.cginc"
															  #define UNITY_INSTANCED_LOD_FADE
															  #define UNITY_INSTANCED_SH
															  #define UNITY_INSTANCED_LIGHTMAPSTS
															  #include "UnityShaderVariables.cginc"
															  #include "UnityShaderUtilities.cginc"
															  // -------- variant for: <when no other keywords are defined>
															  #if !defined(INSTANCING_ON)
															  // Surface shader code generated based on:
															  // vertex modifier: 'vert'
															  // writes to per-pixel normal: YES
															  // writes to emission: no
															  // writes to occlusion: no
															  // needs world space reflection vector: no
															  // needs world space normal vector: no
															  // needs screen space position: no
															  // needs world space position: YES
															  // needs view direction: no
															  // needs world space view direction: no
															  // needs world space position for lighting: YES
															  // needs world space view direction for lighting: no
															  // needs world space view direction for lightmaps: no
															  // needs vertex color: no
															  // needs VFACE: no
															  // needs SV_IsFrontFace: no
															  // passes tangent-to-world matrix to pixel shader: YES
															  // reads from normal: no
															  // 0 texcoords actually used
															  #include "UnityCG.cginc"
															  #include "Lighting.cginc"

															  #define INTERNAL_DATA
															  #define WorldReflectionVector(data,normal) data.worldRefl
															  #define WorldNormalVector(data,normal) normal

															  // Original surface shader snippet:
															  #line 33 ""
															  #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
															  #endif
															  /* UNITY: Original start of shader */
																	  // Physically based Standard lighting model, and enable shadows on all light types
																	  ////#pragma surface surf Standard fullforwardshadows
																	  ////#pragma surface surf Lambert vertex:vert
																	  //#pragma surface surf Lambert vertex:vert

																	  // Use shader model 3.0 target, to get nicer looking lighting
																	  //#pragma target 3.0

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
																	  // //#pragma instancing_options assumeuniformscaling
																	  UNITY_INSTANCING_BUFFER_START(Props)
																		  // put more per-instance properties here
																	  UNITY_INSTANCING_BUFFER_END(Props)

																	  void vert(inout appdata_full v, out Input o) {
																		  UNITY_INITIALIZE_OUTPUT(Input, o);
																		  o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
																		  o.wNormal = UnityObjectToWorldNormal(v.normal);
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


																	  // vertex-to-fragment interpolation data
																	  struct v2f_surf {
																		UNITY_POSITION(pos);
																		float3 worldPos : TEXCOORD0;
																		float3 custompack0 : TEXCOORD1; // wTangent
																		float3 custompack1 : TEXCOORD2; // wNormal
																		float4 screen : TEXCOORD3;
																		float4 lmap : TEXCOORD4;
																	  #ifndef LIGHTMAP_ON
																		float3 vlight : TEXCOORD5;
																	  #else
																	  #ifdef DIRLIGHTMAP_OFF
																		float4 lmapFadePos : TEXCOORD5;
																	  #endif
																	  #endif
																		UNITY_FOG_COORDS(6)
																		#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
																		float3 tSpace0 : TEXCOORD7;
																		float3 tSpace1 : TEXCOORD8;
																		float3 tSpace2 : TEXCOORD9;
																		#endif
																		UNITY_VERTEX_INPUT_INSTANCE_ID
																		UNITY_VERTEX_OUTPUT_STEREO
																	  };

																	  // vertex shader
																	  v2f_surf vert_surf(appdata_full v) {
																		UNITY_SETUP_INSTANCE_ID(v);
																		v2f_surf o;
																		UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
																		UNITY_TRANSFER_INSTANCE_ID(v,o);
																		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
																		Input customInputData;
																		vert(v, customInputData);
																		o.custompack0.xyz = customInputData.wTangent;
																		o.custompack1.xyz = customInputData.wNormal;
																		o.pos = UnityObjectToClipPos(v.vertex);
																		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
																		float3 worldNormal = UnityObjectToWorldNormal(v.normal);
																		#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
																		fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
																		fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
																		fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
																		#endif
																		#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
																		o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
																		o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
																		o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
																		#endif
																		o.worldPos.xyz = worldPos;
																		o.screen = ComputeScreenPos(o.pos);
																	  #ifdef DYNAMICLIGHTMAP_ON
																		o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
																	  #else
																		o.lmap.zw = 0;
																	  #endif
																	  #ifdef LIGHTMAP_ON
																		o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
																		#ifdef DIRLIGHTMAP_OFF
																		  o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
																		  o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
																		#endif
																	  #else
																		o.lmap.xy = 0;
																		float3 worldN = UnityObjectToWorldNormal(v.normal);
																		o.vlight = ShadeSH9(float4(worldN,1.0));
																	  #endif
																		UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
																		return o;
																	  }
																	  sampler2D _LightBuffer;
																	  sampler2D _CameraNormalsTexture;
																	  #ifdef LIGHTMAP_ON
																	  float4 unity_LightmapFade;
																	  #endif
																	  fixed4 unity_Ambient;

																	  // fragment shader
																	  fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
																		UNITY_SETUP_INSTANCE_ID(IN);
																	  // prepare and unpack data
																	  Input surfIN;
																	  #ifdef FOG_COMBINED_WITH_TSPACE
																		UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
																	  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
																		UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
																	  #else
																		UNITY_EXTRACT_FOG(IN);
																	  #endif
																	  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
																	  surfIN.uv_MainTex.x = 1.0;
																	  surfIN.worldPos.x = 1.0;
																	  surfIN.viewDir.x = 1.0;
																	  surfIN.wTangent.x = 1.0;
																	  surfIN.wNormal.x = 1.0;
																	  surfIN.wTangent = IN.custompack0.xyz;
																	  surfIN.wNormal = IN.custompack1.xyz;
																	  float3 worldPos = IN.worldPos.xyz;
																	  #ifndef USING_DIRECTIONAL_LIGHT
																		fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
																	  #else
																		fixed3 lightDir = _WorldSpaceLightPos0.xyz;
																	  #endif
																	  surfIN.worldPos = worldPos;
																	  #ifdef UNITY_COMPILER_HLSL
																	  SurfaceOutput o = (SurfaceOutput)0;
																	  #else
																	  SurfaceOutput o;
																	  #endif
																	  o.Albedo = 0.0;
																	  o.Emission = 0.0;
																	  o.Specular = 0.0;
																	  o.Alpha = 0.0;
																	  o.Gloss = 0.0;
																	  fixed3 normalWorldVertex = fixed3(0,0,1);

																	  // call surface function
																	  surf(surfIN, o); outDepth = _ZDepth;
																	  half4 light = tex2Dproj(_LightBuffer, UNITY_PROJ_COORD(IN.screen));
																	#if defined (SHADER_API_MOBILE)
																	  light = max(light, half4(0.001, 0.001, 0.001, 0.001));
																	#endif
																	#ifndef UNITY_HDR_ON
																	  light = -log2(light);
																	#endif
																	  #ifdef LIGHTMAP_ON
																		#ifdef DIRLIGHTMAP_OFF
																	  // single lightmap
																	  fixed4 lmtex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy);
																	  fixed3 lm = DecodeLightmap(lmtex);
																	  light.rgb += lm;
																	#elif DIRLIGHTMAP_COMBINED
																	  half4 nspec = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(IN.screen));
																	  half3 normal = nspec.rgb * 2 - 1;
																	  o.Normal = normalize(normal);
																	  // directional lightmaps
																	  fixed4 lmtex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy);
																	  fixed4 lmIndTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, IN.lmap.xy);
																	  half4 lm = half4(DecodeDirectionalLightmap(DecodeLightmap(lmtex), lmIndTex, o.Normal), 0);
																	  light += lm;
																	#endif // DIRLIGHTMAP_OFF
																  #else
																	light.rgb += IN.vlight;
																  #endif // LIGHTMAP_ON

																  #ifdef DYNAMICLIGHTMAP_ON
																  fixed4 dynlmtex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, IN.lmap.zw);
																  light.rgb += DecodeRealtimeLightmap(dynlmtex);
																  #endif

																  fixed4 c = LightingLambert_PrePass(o, light);
																  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
																  UNITY_OPAQUE_ALPHA(c.a);
																  return c;
																}


																#endif

																		  // -------- variant for: INSTANCING_ON 
																		  #if defined(INSTANCING_ON)
																		  // Surface shader code generated based on:
																		  // vertex modifier: 'vert'
																		  // writes to per-pixel normal: YES
																		  // writes to emission: no
																		  // writes to occlusion: no
																		  // needs world space reflection vector: no
																		  // needs world space normal vector: no
																		  // needs screen space position: no
																		  // needs world space position: YES
																		  // needs view direction: no
																		  // needs world space view direction: no
																		  // needs world space position for lighting: YES
																		  // needs world space view direction for lighting: no
																		  // needs world space view direction for lightmaps: no
																		  // needs vertex color: no
																		  // needs VFACE: no
																		  // needs SV_IsFrontFace: no
																		  // passes tangent-to-world matrix to pixel shader: YES
																		  // reads from normal: no
																		  // 0 texcoords actually used
																		  #include "UnityCG.cginc"
																		  #include "Lighting.cginc"

																		  #define INTERNAL_DATA
																		  #define WorldReflectionVector(data,normal) data.worldRefl
																		  #define WorldNormalVector(data,normal) normal

																		  // Original surface shader snippet:
																		  #line 33 ""
																		  #ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
																		  #endif
																		  /* UNITY: Original start of shader */
																				  // Physically based Standard lighting model, and enable shadows on all light types
																				  ////#pragma surface surf Standard fullforwardshadows
																				  ////#pragma surface surf Lambert vertex:vert
																				  //#pragma surface surf Lambert vertex:vert

																				  // Use shader model 3.0 target, to get nicer looking lighting
																				  //#pragma target 3.0

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
																				  // //#pragma instancing_options assumeuniformscaling
																				  UNITY_INSTANCING_BUFFER_START(Props)
																					  // put more per-instance properties here
																				  UNITY_INSTANCING_BUFFER_END(Props)

																				  void vert(inout appdata_full v, out Input o) {
																					  UNITY_INITIALIZE_OUTPUT(Input, o);
																					  o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
																					  o.wNormal = UnityObjectToWorldNormal(v.normal);
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


																				  // vertex-to-fragment interpolation data
																				  struct v2f_surf {
																					UNITY_POSITION(pos);
																					float3 worldPos : TEXCOORD0;
																					float3 custompack0 : TEXCOORD1; // wTangent
																					float3 custompack1 : TEXCOORD2; // wNormal
																					float4 screen : TEXCOORD3;
																					float4 lmap : TEXCOORD4;
																				  #ifndef LIGHTMAP_ON
																					float3 vlight : TEXCOORD5;
																				  #else
																				  #ifdef DIRLIGHTMAP_OFF
																					float4 lmapFadePos : TEXCOORD5;
																				  #endif
																				  #endif
																					UNITY_FOG_COORDS(6)
																					#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
																					float3 tSpace0 : TEXCOORD7;
																					float3 tSpace1 : TEXCOORD8;
																					float3 tSpace2 : TEXCOORD9;
																					#endif
																					UNITY_VERTEX_INPUT_INSTANCE_ID
																					UNITY_VERTEX_OUTPUT_STEREO
																				  };

																				  // vertex shader
																				  v2f_surf vert_surf(appdata_full v) {
																					UNITY_SETUP_INSTANCE_ID(v);
																					v2f_surf o;
																					UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
																					UNITY_TRANSFER_INSTANCE_ID(v,o);
																					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
																					Input customInputData;
																					vert(v, customInputData);
																					o.custompack0.xyz = customInputData.wTangent;
																					o.custompack1.xyz = customInputData.wNormal;
																					o.pos = UnityObjectToClipPos(v.vertex);
																					float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
																					float3 worldNormal = UnityObjectToWorldNormal(v.normal);
																					#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED)
																					fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
																					fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
																					fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
																					#endif
																					#if defined(LIGHTMAP_ON) && defined(DIRLIGHTMAP_COMBINED) && !defined(UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS)
																					o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
																					o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
																					o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
																					#endif
																					o.worldPos.xyz = worldPos;
																					o.screen = ComputeScreenPos(o.pos);
																				  #ifdef DYNAMICLIGHTMAP_ON
																					o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
																				  #else
																					o.lmap.zw = 0;
																				  #endif
																				  #ifdef LIGHTMAP_ON
																					o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
																					#ifdef DIRLIGHTMAP_OFF
																					  o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
																					  o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
																					#endif
																				  #else
																					o.lmap.xy = 0;
																					float3 worldN = UnityObjectToWorldNormal(v.normal);
																					o.vlight = ShadeSH9(float4(worldN,1.0));
																				  #endif
																					UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
																					return o;
																				  }
																				  sampler2D _LightBuffer;
																				  sampler2D _CameraNormalsTexture;
																				  #ifdef LIGHTMAP_ON
																				  float4 unity_LightmapFade;
																				  #endif
																				  fixed4 unity_Ambient;

																				  // fragment shader
																				  fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
																					UNITY_SETUP_INSTANCE_ID(IN);
																				  // prepare and unpack data
																				  Input surfIN;
																				  #ifdef FOG_COMBINED_WITH_TSPACE
																					UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
																				  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
																					UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
																				  #else
																					UNITY_EXTRACT_FOG(IN);
																				  #endif
																				  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
																				  surfIN.uv_MainTex.x = 1.0;
																				  surfIN.worldPos.x = 1.0;
																				  surfIN.viewDir.x = 1.0;
																				  surfIN.wTangent.x = 1.0;
																				  surfIN.wNormal.x = 1.0;
																				  surfIN.wTangent = IN.custompack0.xyz;
																				  surfIN.wNormal = IN.custompack1.xyz;
																				  float3 worldPos = IN.worldPos.xyz;
																				  #ifndef USING_DIRECTIONAL_LIGHT
																					fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
																				  #else
																					fixed3 lightDir = _WorldSpaceLightPos0.xyz;
																				  #endif
																				  surfIN.worldPos = worldPos;
																				  #ifdef UNITY_COMPILER_HLSL
																				  SurfaceOutput o = (SurfaceOutput)0;
																				  #else
																				  SurfaceOutput o;
																				  #endif
																				  o.Albedo = 0.0;
																				  o.Emission = 0.0;
																				  o.Specular = 0.0;
																				  o.Alpha = 0.0;
																				  o.Gloss = 0.0;
																				  fixed3 normalWorldVertex = fixed3(0,0,1);

																				  // call surface function
																				  surf(surfIN, o); outDepth = _ZDepth;
																				  half4 light = tex2Dproj(_LightBuffer, UNITY_PROJ_COORD(IN.screen));
																				#if defined (SHADER_API_MOBILE)
																				  light = max(light, half4(0.001, 0.001, 0.001, 0.001));
																				#endif
																				#ifndef UNITY_HDR_ON
																				  light = -log2(light);
																				#endif
																				  #ifdef LIGHTMAP_ON
																					#ifdef DIRLIGHTMAP_OFF
																				  // single lightmap
																				  fixed4 lmtex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy);
																				  fixed3 lm = DecodeLightmap(lmtex);
																				  light.rgb += lm;
																				#elif DIRLIGHTMAP_COMBINED
																				  half4 nspec = tex2Dproj(_CameraNormalsTexture, UNITY_PROJ_COORD(IN.screen));
																				  half3 normal = nspec.rgb * 2 - 1;
																				  o.Normal = normalize(normal);
																				  // directional lightmaps
																				  fixed4 lmtex = UNITY_SAMPLE_TEX2D(unity_Lightmap, IN.lmap.xy);
																				  fixed4 lmIndTex = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, IN.lmap.xy);
																				  half4 lm = half4(DecodeDirectionalLightmap(DecodeLightmap(lmtex), lmIndTex, o.Normal), 0);
																				  light += lm;
																				#endif // DIRLIGHTMAP_OFF
																			  #else
																				light.rgb += IN.vlight;
																			  #endif // LIGHTMAP_ON

																			  #ifdef DYNAMICLIGHTMAP_ON
																			  fixed4 dynlmtex = UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, IN.lmap.zw);
																			  light.rgb += DecodeRealtimeLightmap(dynlmtex);
																			  #endif

																			  fixed4 c = LightingLambert_PrePass(o, light);
																			  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
																			  UNITY_OPAQUE_ALPHA(c.a);
																			  return c;
																			}


																			#endif


																			ENDCG

																			}

															  // ---- deferred shading pass:
															  Pass {
																  Name "DEFERRED"
																  Tags { "LightMode" = "Deferred" }

														  CGPROGRAM
																				// compile directives
																				#pragma vertex vert_surf
																				#pragma fragment frag_surf
																				#pragma target 3.0
																				#pragma multi_compile_instancing
																				#pragma exclude_renderers nomrt
																				#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
																				#pragma multi_compile_prepassfinal
																				#include "HLSLSupport.cginc"
																				#define UNITY_INSTANCED_LOD_FADE
																				#define UNITY_INSTANCED_SH
																				#define UNITY_INSTANCED_LIGHTMAPSTS
																				#include "UnityShaderVariables.cginc"
																				#include "UnityShaderUtilities.cginc"
																				// -------- variant for: <when no other keywords are defined>
																				#if !defined(INSTANCING_ON)
																				// Surface shader code generated based on:
																				// vertex modifier: 'vert'
																				// writes to per-pixel normal: YES
																				// writes to emission: no
																				// writes to occlusion: no
																				// needs world space reflection vector: no
																				// needs world space normal vector: no
																				// needs screen space position: no
																				// needs world space position: YES
																				// needs view direction: no
																				// needs world space view direction: no
																				// needs world space position for lighting: YES
																				// needs world space view direction for lighting: no
																				// needs world space view direction for lightmaps: no
																				// needs vertex color: no
																				// needs VFACE: no
																				// needs SV_IsFrontFace: no
																				// passes tangent-to-world matrix to pixel shader: YES
																				// reads from normal: no
																				// 0 texcoords actually used
																				#include "UnityCG.cginc"
																				#include "Lighting.cginc"

																				#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
																				#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
																				#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

																				// Original surface shader snippet:
																				#line 33 ""
																				#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
																				#endif
																				/* UNITY: Original start of shader */
																						// Physically based Standard lighting model, and enable shadows on all light types
																						////#pragma surface surf Standard fullforwardshadows
																						////#pragma surface surf Lambert vertex:vert
																						//#pragma surface surf Lambert vertex:vert

																						// Use shader model 3.0 target, to get nicer looking lighting
																						//#pragma target 3.0

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
																						// //#pragma instancing_options assumeuniformscaling
																						UNITY_INSTANCING_BUFFER_START(Props)
																							// put more per-instance properties here
																						UNITY_INSTANCING_BUFFER_END(Props)

																						void vert(inout appdata_full v, out Input o) {
																							UNITY_INITIALIZE_OUTPUT(Input, o);
																							o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
																							o.wNormal = UnityObjectToWorldNormal(v.normal);
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


																						// vertex-to-fragment interpolation data
																						struct v2f_surf {
																						  UNITY_POSITION(pos);
																						  float4 tSpace0 : TEXCOORD0;
																						  float4 tSpace1 : TEXCOORD1;
																						  float4 tSpace2 : TEXCOORD2;
																						  float3 custompack0 : TEXCOORD3; // wTangent
																						  float3 custompack1 : TEXCOORD4; // wNormal
																						  float4 lmap : TEXCOORD5;
																						#ifndef LIGHTMAP_ON
																						  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
																							half3 sh : TEXCOORD6; // SH
																						  #endif
																						#else
																						  #ifdef DIRLIGHTMAP_OFF
																							float4 lmapFadePos : TEXCOORD6;
																						  #endif
																						#endif
																						  UNITY_VERTEX_INPUT_INSTANCE_ID
																						  UNITY_VERTEX_OUTPUT_STEREO
																						};

																						// vertex shader
																						v2f_surf vert_surf(appdata_full v) {
																						  UNITY_SETUP_INSTANCE_ID(v);
																						  v2f_surf o;
																						  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
																						  UNITY_TRANSFER_INSTANCE_ID(v,o);
																						  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
																						  Input customInputData;
																						  vert(v, customInputData);
																						  o.custompack0.xyz = customInputData.wTangent;
																						  o.custompack1.xyz = customInputData.wNormal;
																						  o.pos = UnityObjectToClipPos(v.vertex);
																						  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
																						  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
																						  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
																						  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
																						  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
																						  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
																						  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
																						  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
																						#ifdef DYNAMICLIGHTMAP_ON
																						  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
																						#else
																						  o.lmap.zw = 0;
																						#endif
																						#ifdef LIGHTMAP_ON
																						  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
																						  #ifdef DIRLIGHTMAP_OFF
																							o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
																							o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
																						  #endif
																						#else
																						  o.lmap.xy = 0;
																							#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
																							  o.sh = 0;
																							  o.sh = ShadeSHPerVertex(worldNormal, o.sh);
																							#endif
																						#endif
																						  return o;
																						}
																						#ifdef LIGHTMAP_ON
																						float4 unity_LightmapFade;
																						#endif
																						fixed4 unity_Ambient;

																						// fragment shader
																						void frag_surf(v2f_surf IN,
																							out half4 outGBuffer0 : SV_Target0,
																							out half4 outGBuffer1 : SV_Target1,
																							out half4 outGBuffer2 : SV_Target2,
																							out half4 outEmission : SV_Target3, out float outDepth : SV_Depth
																						#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
																							, out half4 outShadowMask : SV_Target4
																						#endif
																						) {
																						  UNITY_SETUP_INSTANCE_ID(IN);
																						  // prepare and unpack data
																						  Input surfIN;
																						  #ifdef FOG_COMBINED_WITH_TSPACE
																							UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
																						  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
																							UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
																						  #else
																							UNITY_EXTRACT_FOG(IN);
																						  #endif
																						  #ifdef FOG_COMBINED_WITH_TSPACE
																							UNITY_RECONSTRUCT_TBN(IN);
																						  #else
																							UNITY_EXTRACT_TBN(IN);
																						  #endif
																						  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
																						  surfIN.uv_MainTex.x = 1.0;
																						  surfIN.worldPos.x = 1.0;
																						  surfIN.viewDir.x = 1.0;
																						  surfIN.wTangent.x = 1.0;
																						  surfIN.wNormal.x = 1.0;
																						  surfIN.wTangent = IN.custompack0.xyz;
																						  surfIN.wNormal = IN.custompack1.xyz;
																						  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
																						  #ifndef USING_DIRECTIONAL_LIGHT
																							fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
																						  #else
																							fixed3 lightDir = _WorldSpaceLightPos0.xyz;
																						  #endif
																						  surfIN.worldPos = worldPos;
																						  #ifdef UNITY_COMPILER_HLSL
																						  SurfaceOutput o = (SurfaceOutput)0;
																						  #else
																						  SurfaceOutput o;
																						  #endif
																						  o.Albedo = 0.0;
																						  o.Emission = 0.0;
																						  o.Specular = 0.0;
																						  o.Alpha = 0.0;
																						  o.Gloss = 0.0;
																						  fixed3 normalWorldVertex = fixed3(0,0,1);
																						  o.Normal = fixed3(0,0,1);

																						  // call surface function
																						  surf(surfIN, o); outDepth = _ZDepth;
																						fixed3 originalNormal = o.Normal;
																						  float3 worldN;
																						  worldN.x = dot(_unity_tbn_0, o.Normal);
																						  worldN.y = dot(_unity_tbn_1, o.Normal);
																						  worldN.z = dot(_unity_tbn_2, o.Normal);
																						  worldN = normalize(worldN);
																						  o.Normal = worldN;
																						  half atten = 1;

																						  // Setup lighting environment
																						  UnityGI gi;
																						  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
																						  gi.indirect.diffuse = 0;
																						  gi.indirect.specular = 0;
																						  gi.light.color = 0;
																						  gi.light.dir = half3(0,1,0);
																						  // Call GI (lightmaps/SH/reflections) lighting function
																						  UnityGIInput giInput;
																						  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
																						  giInput.light = gi.light;
																						  giInput.worldPos = worldPos;
																						  giInput.atten = atten;
																						  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
																							giInput.lightmapUV = IN.lmap;
																						  #else
																							giInput.lightmapUV = 0.0;
																						  #endif
																						  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
																							giInput.ambient = IN.sh;
																						  #else
																							giInput.ambient.rgb = 0.0;
																						  #endif
																						  giInput.probeHDR[0] = unity_SpecCube0_HDR;
																						  giInput.probeHDR[1] = unity_SpecCube1_HDR;
																						  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
																							giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
																						  #endif
																						  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
																							giInput.boxMax[0] = unity_SpecCube0_BoxMax;
																							giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
																							giInput.boxMax[1] = unity_SpecCube1_BoxMax;
																							giInput.boxMin[1] = unity_SpecCube1_BoxMin;
																							giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
																						  #endif
																						  LightingLambert_GI(o, giInput, gi);

																						  // call lighting function to output g-buffer
																						  outEmission = LightingLambert_Deferred(o, gi, outGBuffer0, outGBuffer1, outGBuffer2);
																						  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
																							outShadowMask = UnityGetRawBakedOcclusions(IN.lmap.xy, worldPos);
																						  #endif
																						  #ifndef UNITY_HDR_ON
																						  outEmission.rgb = exp2(-outEmission.rgb);
																						  #endif
																						}


																						#endif

																						// -------- variant for: INSTANCING_ON 
																						#if defined(INSTANCING_ON)
																						// Surface shader code generated based on:
																						// vertex modifier: 'vert'
																						// writes to per-pixel normal: YES
																						// writes to emission: no
																						// writes to occlusion: no
																						// needs world space reflection vector: no
																						// needs world space normal vector: no
																						// needs screen space position: no
																						// needs world space position: YES
																						// needs view direction: no
																						// needs world space view direction: no
																						// needs world space position for lighting: YES
																						// needs world space view direction for lighting: no
																						// needs world space view direction for lightmaps: no
																						// needs vertex color: no
																						// needs VFACE: no
																						// needs SV_IsFrontFace: no
																						// passes tangent-to-world matrix to pixel shader: YES
																						// reads from normal: no
																						// 0 texcoords actually used
																						#include "UnityCG.cginc"
																						#include "Lighting.cginc"

																						#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
																						#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
																						#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

																						// Original surface shader snippet:
																						#line 33 ""
																						#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
																						#endif
																						/* UNITY: Original start of shader */
																								// Physically based Standard lighting model, and enable shadows on all light types
																								////#pragma surface surf Standard fullforwardshadows
																								////#pragma surface surf Lambert vertex:vert
																								//#pragma surface surf Lambert vertex:vert

																								// Use shader model 3.0 target, to get nicer looking lighting
																								//#pragma target 3.0

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
																								// //#pragma instancing_options assumeuniformscaling
																								UNITY_INSTANCING_BUFFER_START(Props)
																									// put more per-instance properties here
																								UNITY_INSTANCING_BUFFER_END(Props)

																								void vert(inout appdata_full v, out Input o) {
																									UNITY_INITIALIZE_OUTPUT(Input, o);
																									o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
																									o.wNormal = UnityObjectToWorldNormal(v.normal);
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


																								// vertex-to-fragment interpolation data
																								struct v2f_surf {
																								  UNITY_POSITION(pos);
																								  float4 tSpace0 : TEXCOORD0;
																								  float4 tSpace1 : TEXCOORD1;
																								  float4 tSpace2 : TEXCOORD2;
																								  float3 custompack0 : TEXCOORD3; // wTangent
																								  float3 custompack1 : TEXCOORD4; // wNormal
																								  float4 lmap : TEXCOORD5;
																								#ifndef LIGHTMAP_ON
																								  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
																									half3 sh : TEXCOORD6; // SH
																								  #endif
																								#else
																								  #ifdef DIRLIGHTMAP_OFF
																									float4 lmapFadePos : TEXCOORD6;
																								  #endif
																								#endif
																								  UNITY_VERTEX_INPUT_INSTANCE_ID
																								  UNITY_VERTEX_OUTPUT_STEREO
																								};

																								// vertex shader
																								v2f_surf vert_surf(appdata_full v) {
																								  UNITY_SETUP_INSTANCE_ID(v);
																								  v2f_surf o;
																								  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
																								  UNITY_TRANSFER_INSTANCE_ID(v,o);
																								  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
																								  Input customInputData;
																								  vert(v, customInputData);
																								  o.custompack0.xyz = customInputData.wTangent;
																								  o.custompack1.xyz = customInputData.wNormal;
																								  o.pos = UnityObjectToClipPos(v.vertex);
																								  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
																								  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
																								  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
																								  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
																								  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
																								  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
																								  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
																								  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
																								#ifdef DYNAMICLIGHTMAP_ON
																								  o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
																								#else
																								  o.lmap.zw = 0;
																								#endif
																								#ifdef LIGHTMAP_ON
																								  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
																								  #ifdef DIRLIGHTMAP_OFF
																									o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
																									o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
																								  #endif
																								#else
																								  o.lmap.xy = 0;
																									#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
																									  o.sh = 0;
																									  o.sh = ShadeSHPerVertex(worldNormal, o.sh);
																									#endif
																								#endif
																								  return o;
																								}
																								#ifdef LIGHTMAP_ON
																								float4 unity_LightmapFade;
																								#endif
																								fixed4 unity_Ambient;

																								// fragment shader
																								void frag_surf(v2f_surf IN,
																									out half4 outGBuffer0 : SV_Target0,
																									out half4 outGBuffer1 : SV_Target1,
																									out half4 outGBuffer2 : SV_Target2,
																									out half4 outEmission : SV_Target3, out float outDepth : SV_Depth
																								#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
																									, out half4 outShadowMask : SV_Target4
																								#endif
																								) {
																								  UNITY_SETUP_INSTANCE_ID(IN);
																								  // prepare and unpack data
																								  Input surfIN;
																								  #ifdef FOG_COMBINED_WITH_TSPACE
																									UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
																								  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
																									UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
																								  #else
																									UNITY_EXTRACT_FOG(IN);
																								  #endif
																								  #ifdef FOG_COMBINED_WITH_TSPACE
																									UNITY_RECONSTRUCT_TBN(IN);
																								  #else
																									UNITY_EXTRACT_TBN(IN);
																								  #endif
																								  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
																								  surfIN.uv_MainTex.x = 1.0;
																								  surfIN.worldPos.x = 1.0;
																								  surfIN.viewDir.x = 1.0;
																								  surfIN.wTangent.x = 1.0;
																								  surfIN.wNormal.x = 1.0;
																								  surfIN.wTangent = IN.custompack0.xyz;
																								  surfIN.wNormal = IN.custompack1.xyz;
																								  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
																								  #ifndef USING_DIRECTIONAL_LIGHT
																									fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
																								  #else
																									fixed3 lightDir = _WorldSpaceLightPos0.xyz;
																								  #endif
																								  surfIN.worldPos = worldPos;
																								  #ifdef UNITY_COMPILER_HLSL
																								  SurfaceOutput o = (SurfaceOutput)0;
																								  #else
																								  SurfaceOutput o;
																								  #endif
																								  o.Albedo = 0.0;
																								  o.Emission = 0.0;
																								  o.Specular = 0.0;
																								  o.Alpha = 0.0;
																								  o.Gloss = 0.0;
																								  fixed3 normalWorldVertex = fixed3(0,0,1);
																								  o.Normal = fixed3(0,0,1);

																								  // call surface function
																								  surf(surfIN, o); outDepth = _ZDepth;
																								fixed3 originalNormal = o.Normal;
																								  float3 worldN;
																								  worldN.x = dot(_unity_tbn_0, o.Normal);
																								  worldN.y = dot(_unity_tbn_1, o.Normal);
																								  worldN.z = dot(_unity_tbn_2, o.Normal);
																								  worldN = normalize(worldN);
																								  o.Normal = worldN;
																								  half atten = 1;

																								  // Setup lighting environment
																								  UnityGI gi;
																								  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
																								  gi.indirect.diffuse = 0;
																								  gi.indirect.specular = 0;
																								  gi.light.color = 0;
																								  gi.light.dir = half3(0,1,0);
																								  // Call GI (lightmaps/SH/reflections) lighting function
																								  UnityGIInput giInput;
																								  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
																								  giInput.light = gi.light;
																								  giInput.worldPos = worldPos;
																								  giInput.atten = atten;
																								  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
																									giInput.lightmapUV = IN.lmap;
																								  #else
																									giInput.lightmapUV = 0.0;
																								  #endif
																								  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
																									giInput.ambient = IN.sh;
																								  #else
																									giInput.ambient.rgb = 0.0;
																								  #endif
																								  giInput.probeHDR[0] = unity_SpecCube0_HDR;
																								  giInput.probeHDR[1] = unity_SpecCube1_HDR;
																								  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
																									giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
																								  #endif
																								  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
																									giInput.boxMax[0] = unity_SpecCube0_BoxMax;
																									giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
																									giInput.boxMax[1] = unity_SpecCube1_BoxMax;
																									giInput.boxMin[1] = unity_SpecCube1_BoxMin;
																									giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
																								  #endif
																								  LightingLambert_GI(o, giInput, gi);

																								  // call lighting function to output g-buffer
																								  outEmission = LightingLambert_Deferred(o, gi, outGBuffer0, outGBuffer1, outGBuffer2);
																								  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
																									outShadowMask = UnityGetRawBakedOcclusions(IN.lmap.xy, worldPos);
																								  #endif
																								  #ifndef UNITY_HDR_ON
																								  outEmission.rgb = exp2(-outEmission.rgb);
																								  #endif
																								}


																								#endif


																								ENDCG

																								}

																				// ---- meta information extraction pass:
																				Pass {
																					Name "Meta"
																					Tags { "LightMode" = "Meta" }
																					Cull Off

																			CGPROGRAM
																									// compile directives
																									#pragma vertex vert_surf
																									#pragma fragment frag_surf
																									#pragma target 3.0
																									#pragma multi_compile_instancing
																									#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
																									#pragma shader_feature EDITOR_VISUALIZATION

																									#include "HLSLSupport.cginc"
																									#define UNITY_INSTANCED_LOD_FADE
																									#define UNITY_INSTANCED_SH
																									#define UNITY_INSTANCED_LIGHTMAPSTS
																									#include "UnityShaderVariables.cginc"
																									#include "UnityShaderUtilities.cginc"
																									// -------- variant for: <when no other keywords are defined>
																									#if !defined(INSTANCING_ON)
																									// Surface shader code generated based on:
																									// vertex modifier: 'vert'
																									// writes to per-pixel normal: YES
																									// writes to emission: no
																									// writes to occlusion: no
																									// needs world space reflection vector: no
																									// needs world space normal vector: no
																									// needs screen space position: no
																									// needs world space position: YES
																									// needs view direction: no
																									// needs world space view direction: no
																									// needs world space position for lighting: YES
																									// needs world space view direction for lighting: no
																									// needs world space view direction for lightmaps: no
																									// needs vertex color: no
																									// needs VFACE: no
																									// needs SV_IsFrontFace: no
																									// passes tangent-to-world matrix to pixel shader: YES
																									// reads from normal: no
																									// 0 texcoords actually used
																									#include "UnityCG.cginc"
																									#include "Lighting.cginc"

																									#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
																									#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
																									#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

																									// Original surface shader snippet:
																									#line 33 ""
																									#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
																									#endif
																									/* UNITY: Original start of shader */
																											// Physically based Standard lighting model, and enable shadows on all light types
																											////#pragma surface surf Standard fullforwardshadows
																											////#pragma surface surf Lambert vertex:vert
																											//#pragma surface surf Lambert vertex:vert

																											// Use shader model 3.0 target, to get nicer looking lighting
																											//#pragma target 3.0

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
																											// //#pragma instancing_options assumeuniformscaling
																											UNITY_INSTANCING_BUFFER_START(Props)
																												// put more per-instance properties here
																											UNITY_INSTANCING_BUFFER_END(Props)

																											void vert(inout appdata_full v, out Input o) {
																												UNITY_INITIALIZE_OUTPUT(Input, o);
																												o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
																												o.wNormal = UnityObjectToWorldNormal(v.normal);
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

																									#include "UnityMetaPass.cginc"

																											// vertex-to-fragment interpolation data
																											struct v2f_surf {
																											  UNITY_POSITION(pos);
																											  float4 tSpace0 : TEXCOORD0;
																											  float4 tSpace1 : TEXCOORD1;
																											  float4 tSpace2 : TEXCOORD2;
																											  float3 custompack0 : TEXCOORD3; // wTangent
																											  float3 custompack1 : TEXCOORD4; // wNormal
																											#ifdef EDITOR_VISUALIZATION
																											  float2 vizUV : TEXCOORD5;
																											  float4 lightCoord : TEXCOORD6;
																											#endif
																											  UNITY_VERTEX_INPUT_INSTANCE_ID
																											  UNITY_VERTEX_OUTPUT_STEREO
																											};

																											// vertex shader
																											v2f_surf vert_surf(appdata_full v) {
																											  UNITY_SETUP_INSTANCE_ID(v);
																											  v2f_surf o;
																											  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
																											  UNITY_TRANSFER_INSTANCE_ID(v,o);
																											  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
																											  Input customInputData;
																											  vert(v, customInputData);
																											  o.custompack0.xyz = customInputData.wTangent;
																											  o.custompack1.xyz = customInputData.wNormal;
																											  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
																											#ifdef EDITOR_VISUALIZATION
																											  o.vizUV = 0;
																											  o.lightCoord = 0;
																											  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
																												o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
																											  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
																											  {
																												o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
																												o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
																											  }
																											#endif
																											  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
																											  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
																											  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
																											  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
																											  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
																											  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
																											  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
																											  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
																											  return o;
																											}

																											// fragment shader
																											fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
																											  UNITY_SETUP_INSTANCE_ID(IN);
																											// prepare and unpack data
																											Input surfIN;
																											#ifdef FOG_COMBINED_WITH_TSPACE
																											  UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
																											#elif defined (FOG_COMBINED_WITH_WORLD_POS)
																											  UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
																											#else
																											  UNITY_EXTRACT_FOG(IN);
																											#endif
																											#ifdef FOG_COMBINED_WITH_TSPACE
																											  UNITY_RECONSTRUCT_TBN(IN);
																											#else
																											  UNITY_EXTRACT_TBN(IN);
																											#endif
																											UNITY_INITIALIZE_OUTPUT(Input,surfIN);
																											surfIN.uv_MainTex.x = 1.0;
																											surfIN.worldPos.x = 1.0;
																											surfIN.viewDir.x = 1.0;
																											surfIN.wTangent.x = 1.0;
																											surfIN.wNormal.x = 1.0;
																											surfIN.wTangent = IN.custompack0.xyz;
																											surfIN.wNormal = IN.custompack1.xyz;
																											float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
																											#ifndef USING_DIRECTIONAL_LIGHT
																											  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
																											#else
																											  fixed3 lightDir = _WorldSpaceLightPos0.xyz;
																											#endif
																											surfIN.worldPos = worldPos;
																											#ifdef UNITY_COMPILER_HLSL
																											SurfaceOutput o = (SurfaceOutput)0;
																											#else
																											SurfaceOutput o;
																											#endif
																											o.Albedo = 0.0;
																											o.Emission = 0.0;
																											o.Specular = 0.0;
																											o.Alpha = 0.0;
																											o.Gloss = 0.0;
																											fixed3 normalWorldVertex = fixed3(0,0,1);

																											// call surface function
																											surf(surfIN, o); outDepth = _ZDepth;
																											UnityMetaInput metaIN;
																											UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
																											metaIN.Albedo = o.Albedo;
																											metaIN.Emission = o.Emission;
																											metaIN.SpecularColor = o.Specular;
																										  #ifdef EDITOR_VISUALIZATION
																											metaIN.VizUV = IN.vizUV;
																											metaIN.LightCoord = IN.lightCoord;
																										  #endif
																											return UnityMetaFragment(metaIN);
																										  }


																										  #endif

																												// -------- variant for: INSTANCING_ON 
																												#if defined(INSTANCING_ON)
																												// Surface shader code generated based on:
																												// vertex modifier: 'vert'
																												// writes to per-pixel normal: YES
																												// writes to emission: no
																												// writes to occlusion: no
																												// needs world space reflection vector: no
																												// needs world space normal vector: no
																												// needs screen space position: no
																												// needs world space position: YES
																												// needs view direction: no
																												// needs world space view direction: no
																												// needs world space position for lighting: YES
																												// needs world space view direction for lighting: no
																												// needs world space view direction for lightmaps: no
																												// needs vertex color: no
																												// needs VFACE: no
																												// needs SV_IsFrontFace: no
																												// passes tangent-to-world matrix to pixel shader: YES
																												// reads from normal: no
																												// 0 texcoords actually used
																												#include "UnityCG.cginc"
																												#include "Lighting.cginc"

																												#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
																												#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
																												#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

																												// Original surface shader snippet:
																												#line 33 ""
																												#ifdef DUMMY_PREPROCESSOR_TO_WORK_AROUND_HLSL_COMPILER_LINE_HANDLING
																												#endif
																												/* UNITY: Original start of shader */
																														// Physically based Standard lighting model, and enable shadows on all light types
																														////#pragma surface surf Standard fullforwardshadows
																														////#pragma surface surf Lambert vertex:vert
																														//#pragma surface surf Lambert vertex:vert

																														// Use shader model 3.0 target, to get nicer looking lighting
																														//#pragma target 3.0

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
																														// //#pragma instancing_options assumeuniformscaling
																														UNITY_INSTANCING_BUFFER_START(Props)
																															// put more per-instance properties here
																														UNITY_INSTANCING_BUFFER_END(Props)

																														void vert(inout appdata_full v, out Input o) {
																															UNITY_INITIALIZE_OUTPUT(Input, o);
																															o.wTangent = UnityObjectToWorldDir(v.tangent.xyz);
																															o.wNormal = UnityObjectToWorldNormal(v.normal);
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

																												#include "UnityMetaPass.cginc"

																														// vertex-to-fragment interpolation data
																														struct v2f_surf {
																														  UNITY_POSITION(pos);
																														  float4 tSpace0 : TEXCOORD0;
																														  float4 tSpace1 : TEXCOORD1;
																														  float4 tSpace2 : TEXCOORD2;
																														  float3 custompack0 : TEXCOORD3; // wTangent
																														  float3 custompack1 : TEXCOORD4; // wNormal
																														#ifdef EDITOR_VISUALIZATION
																														  float2 vizUV : TEXCOORD5;
																														  float4 lightCoord : TEXCOORD6;
																														#endif
																														  UNITY_VERTEX_INPUT_INSTANCE_ID
																														  UNITY_VERTEX_OUTPUT_STEREO
																														};

																														// vertex shader
																														v2f_surf vert_surf(appdata_full v) {
																														  UNITY_SETUP_INSTANCE_ID(v);
																														  v2f_surf o;
																														  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
																														  UNITY_TRANSFER_INSTANCE_ID(v,o);
																														  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
																														  Input customInputData;
																														  vert(v, customInputData);
																														  o.custompack0.xyz = customInputData.wTangent;
																														  o.custompack1.xyz = customInputData.wNormal;
																														  o.pos = UnityMetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST, unity_DynamicLightmapST);
																														#ifdef EDITOR_VISUALIZATION
																														  o.vizUV = 0;
																														  o.lightCoord = 0;
																														  if (unity_VisualizationMode == EDITORVIZ_TEXTURE)
																															o.vizUV = UnityMetaVizUV(unity_EditorViz_UVIndex, v.texcoord.xy, v.texcoord1.xy, v.texcoord2.xy, unity_EditorViz_Texture_ST);
																														  else if (unity_VisualizationMode == EDITORVIZ_SHOWLIGHTMASK)
																														  {
																															o.vizUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
																															o.lightCoord = mul(unity_EditorViz_WorldToLight, mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)));
																														  }
																														#endif
																														  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
																														  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
																														  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
																														  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
																														  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
																														  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
																														  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
																														  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
																														  return o;
																														}

																														// fragment shader
																														fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
																														  UNITY_SETUP_INSTANCE_ID(IN);
																														// prepare and unpack data
																														Input surfIN;
																														#ifdef FOG_COMBINED_WITH_TSPACE
																														  UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
																														#elif defined (FOG_COMBINED_WITH_WORLD_POS)
																														  UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
																														#else
																														  UNITY_EXTRACT_FOG(IN);
																														#endif
																														#ifdef FOG_COMBINED_WITH_TSPACE
																														  UNITY_RECONSTRUCT_TBN(IN);
																														#else
																														  UNITY_EXTRACT_TBN(IN);
																														#endif
																														UNITY_INITIALIZE_OUTPUT(Input,surfIN);
																														surfIN.uv_MainTex.x = 1.0;
																														surfIN.worldPos.x = 1.0;
																														surfIN.viewDir.x = 1.0;
																														surfIN.wTangent.x = 1.0;
																														surfIN.wNormal.x = 1.0;
																														surfIN.wTangent = IN.custompack0.xyz;
																														surfIN.wNormal = IN.custompack1.xyz;
																														float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
																														#ifndef USING_DIRECTIONAL_LIGHT
																														  fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
																														#else
																														  fixed3 lightDir = _WorldSpaceLightPos0.xyz;
																														#endif
																														surfIN.worldPos = worldPos;
																														#ifdef UNITY_COMPILER_HLSL
																														SurfaceOutput o = (SurfaceOutput)0;
																														#else
																														SurfaceOutput o;
																														#endif
																														o.Albedo = 0.0;
																														o.Emission = 0.0;
																														o.Specular = 0.0;
																														o.Alpha = 0.0;
																														o.Gloss = 0.0;
																														fixed3 normalWorldVertex = fixed3(0,0,1);

																														// call surface function
																														surf(surfIN, o); outDepth = _ZDepth;
																														UnityMetaInput metaIN;
																														UNITY_INITIALIZE_OUTPUT(UnityMetaInput, metaIN);
																														metaIN.Albedo = o.Albedo;
																														metaIN.Emission = o.Emission;
																														metaIN.SpecularColor = o.Specular;
																													  #ifdef EDITOR_VISUALIZATION
																														metaIN.VizUV = IN.vizUV;
																														metaIN.LightCoord = IN.lightCoord;
																													  #endif
																														return UnityMetaFragment(metaIN);
																													  }


																													  #endif


																													  ENDCG

																													  }

																									// ---- end of surface shader generated code

																								#LINE 140

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