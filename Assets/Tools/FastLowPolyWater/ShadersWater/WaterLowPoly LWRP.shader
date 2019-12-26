// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FastWaterLowPoly/WaterLWRP"
{
    Properties
    {
		_Colorwater("Color water", Color) = (0,0,0,0)
		_ColorDepth("Color Depth", Color) = (0,0,0,0)
		_ColorFoam("Color Foam", Color) = (0,0,0,0)
		_Depth("Depth", Float) = 0
		_OpasityIntensity("OpasityIntensity", Range( 0 , 1)) = 1
		_SizeFoam("SizeFoam", Float) = 2
		_Gloss("Gloss", Range( 0 , 1)) = 0.9
		_Specular("Specular", Range( 0 , 1)) = 0.2
		_Noise("Noise", 2D) = "white" {}
		_Speed("Speed", Float) = 0
		_NoiseIntensity("NoiseIntensity", Float) = 0
		_ScaleNoise("ScaleNoise", Float) = 0
    }


    SubShader
    {
		
        Tags { "RenderPipeline"="LightweightPipeline" "RenderType"="Transparent" "Queue"="Transparent" }

		Cull Back
		HLSLINCLUDE
		#pragma target 3.0
		ENDHLSL
		
        Pass
        {
			
        	Tags { "LightMode"="LightweightForward" }

        	Name "Base"
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
            
        	HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            

        	// -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
            
        	// -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
        	#pragma fragment frag

        	#define ASE_SRP_VERSION 41000
        	#define REQUIRE_DEPTH_TEXTURE 1
        	#define _SPECULAR_SETUP 1


        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
        	#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"

			sampler2D _Noise;
			float _ScaleNoise;
			float _Speed;
			float _NoiseIntensity;
			uniform float4 _CameraDepthTexture_TexelSize;
			float _SizeFoam;
			float4 _Colorwater;
			float4 _ColorDepth;
			float _Depth;
			float4 _ColorFoam;
			float _Specular;
			float _Gloss;
			float _OpasityIntensity;

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
                float4 ase_tangent : TANGENT;
                float4 texcoord1 : TEXCOORD1;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct GraphVertexOutput
            {
                float4 clipPos                : SV_POSITION;
                float4 lightmapUVOrVertexSH	  : TEXCOORD0;
        		half4 fogFactorAndVertexLight : TEXCOORD1; // x: fogFactor, yzw: vertex light
            	float4 shadowCoord            : TEXCOORD2;
				float4 tSpace0					: TEXCOORD3;
				float4 tSpace1					: TEXCOORD4;
				float4 tSpace2					: TEXCOORD5;
				float4 ase_texcoord7 : TEXCOORD7;
				float4 ase_texcoord8 : TEXCOORD8;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            	UNITY_VERTEX_OUTPUT_STEREO
            };

			
            GraphVertexOutput vert (GraphVertexInput v)
        	{
        		GraphVertexOutput o = (GraphVertexOutput)0;
                UNITY_SETUP_INSTANCE_ID(v);
            	UNITY_TRANSFER_INSTANCE_ID(v, o);
        		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float mulTime43 = _Time.y * _Speed;
				float4 appendResult51 = (float4(0.0 , ( tex2Dlod( _Noise, float4( ( ( (ase_worldPos).xz * _ScaleNoise ) + mulTime43 ), 0, 0.0) ).r * _NoiseIntensity ) , 0.0 , 0.0));
				
				float4 temp_output_63_0 = ( float4( v.vertex.xyz , 0.0 ) + appendResult51 );
				float3 vertexPos13 = temp_output_63_0.xyz;
				float4 ase_clipPos13 = TransformObjectToHClip((vertexPos13).xyz);
				float4 screenPos13 = ComputeScreenPos(ase_clipPos13);
				o.ase_texcoord7 = screenPos13;
				float3 vertexPos28 = temp_output_63_0.xyz;
				float4 ase_clipPos28 = TransformObjectToHClip((vertexPos28).xyz);
				float4 screenPos28 = ComputeScreenPos(ase_clipPos28);
				o.ase_texcoord8 = screenPos28;
				
				float3 vertexValue = appendResult51.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal =  v.ase_normal ;

        		// Vertex shader outputs defined by graph
                float3 lwWNormal = TransformObjectToWorldNormal(v.ase_normal);
				float3 lwWorldPos = TransformObjectToWorld(v.vertex.xyz);
				float3 lwWTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				float3 lwWBinormal = normalize(cross(lwWNormal, lwWTangent) * v.ase_tangent.w);
				o.tSpace0 = float4(lwWTangent.x, lwWBinormal.x, lwWNormal.x, lwWorldPos.x);
				o.tSpace1 = float4(lwWTangent.y, lwWBinormal.y, lwWNormal.y, lwWorldPos.y);
				o.tSpace2 = float4(lwWTangent.z, lwWBinormal.z, lwWNormal.z, lwWorldPos.z);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                
         		// We either sample GI from lightmap or SH.
        	    // Lightmap UV and vertex SH coefficients use the same interpolator ("float2 lightmapUV" for lightmap or "half3 vertexSH" for SH)
                // see DECLARE_LIGHTMAP_OR_SH macro.
        	    // The following funcions initialize the correct variable with correct data
        	    OUTPUT_LIGHTMAP_UV(v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy);
        	    OUTPUT_SH(lwWNormal, o.lightmapUVOrVertexSH.xyz);

        	    half3 vertexLight = VertexLighting(vertexInput.positionWS, lwWNormal);
        	    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);
        	    o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
        	    o.clipPos = vertexInput.positionCS;

        	#ifdef _MAIN_LIGHT_SHADOWS
        		o.shadowCoord = GetShadowCoord(vertexInput);
        	#endif
        		return o;
        	}

        	half4 frag (GraphVertexOutput IN ) : SV_Target
            {
            	UNITY_SETUP_INSTANCE_ID(IN);

        		float3 WorldSpaceNormal = normalize(float3(IN.tSpace0.z,IN.tSpace1.z,IN.tSpace2.z));
				float3 WorldSpaceTangent = float3(IN.tSpace0.x,IN.tSpace1.x,IN.tSpace2.x);
				float3 WorldSpaceBiTangent = float3(IN.tSpace0.y,IN.tSpace1.y,IN.tSpace2.y);
				float3 WorldSpacePosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldSpaceViewDirection = SafeNormalize( _WorldSpaceCameraPos.xyz  - WorldSpacePosition );
    
				float4 screenPos13 = IN.ase_texcoord7;
				float4 ase_screenPosNorm13 = screenPos13 / screenPos13.w;
				ase_screenPosNorm13.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm13.z : ase_screenPosNorm13.z * 0.5 + 0.5;
				float screenDepth13 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos13.xy/screenPos13.w  ),_ZBufferParams);
				float distanceDepth13 = abs( ( screenDepth13 - LinearEyeDepth( ase_screenPosNorm13.z,_ZBufferParams ) ) / ( _SizeFoam ) );
				float lerpResult17 = lerp( distanceDepth13 , ( distanceDepth13 * ( 1.0 - WorldSpaceNormal.y ) ) , 0.998);
				float4 screenPos28 = IN.ase_texcoord8;
				float4 ase_screenPosNorm28 = screenPos28 / screenPos28.w;
				ase_screenPosNorm28.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm28.z : ase_screenPosNorm28.z * 0.5 + 0.5;
				float screenDepth28 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos28.xy/screenPos28.w  ),_ZBufferParams);
				float distanceDepth28 = abs( ( screenDepth28 - LinearEyeDepth( ase_screenPosNorm28.z,_ZBufferParams ) ) / ( _Depth ) );
				float clampResult32 = clamp( distanceDepth28 , 0.0 , 1.0 );
				float4 lerpResult30 = lerp( _Colorwater , _ColorDepth , clampResult32);
				float4 ifLocalVar7 = 0;
				if( lerpResult17 <= _SizeFoam )
				ifLocalVar7 = _ColorFoam;
				else
				ifLocalVar7 = lerpResult30;
				
				float3 temp_cast_1 = (_Specular).xxx;
				
				float4 ifLocalVar35 = 0;
				if( lerpResult17 <= _SizeFoam )
				ifLocalVar35 = _ColorFoam;
				float lerpResult33 = lerp( 1.0 , clampResult32 , _OpasityIntensity);
				float4 clampResult39 = clamp( ( ifLocalVar35 + lerpResult33 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
				
				
		        float3 Albedo = ifLocalVar7.rgb;
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = temp_cast_1;
				float Metallic = 0;
				float Smoothness = _Gloss;
				float Occlusion = 1;
				float Alpha = clampResult39.r;
				float AlphaClipThreshold = 0;

        		InputData inputData;
        		inputData.positionWS = WorldSpacePosition;

        #ifdef _NORMALMAP
        	    inputData.normalWS = normalize(TransformTangentToWorld(Normal, half3x3(WorldSpaceTangent, WorldSpaceBiTangent, WorldSpaceNormal)));
        #else
            #if !SHADER_HINT_NICE_QUALITY
                inputData.normalWS = WorldSpaceNormal;
            #else
        	    inputData.normalWS = normalize(WorldSpaceNormal);
            #endif
        #endif

        #if !SHADER_HINT_NICE_QUALITY
        	    // viewDirection should be normalized here, but we avoid doing it as it's close enough and we save some ALU.
        	    inputData.viewDirectionWS = WorldSpaceViewDirection;
        #else
        	    inputData.viewDirectionWS = normalize(WorldSpaceViewDirection);
        #endif

        	    inputData.shadowCoord = IN.shadowCoord;

        	    inputData.fogCoord = IN.fogFactorAndVertexLight.x;
        	    inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
        	    inputData.bakedGI = SAMPLE_GI(IN.lightmapUVOrVertexSH.xy, IN.lightmapUVOrVertexSH.xyz, inputData.normalWS);

        		half4 color = LightweightFragmentPBR(
        			inputData, 
        			Albedo, 
        			Metallic, 
        			Specular, 
        			Smoothness, 
        			Occlusion, 
        			Emission, 
        			Alpha);

			#ifdef TERRAIN_SPLAT_ADDPASS
				color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
			#else
				color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
			#endif

        #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif

		#if ASE_LW_FINAL_COLOR_ALPHA_MULTIPLY
				color.rgb *= color.a;
		#endif
        		return color;
            }

        	ENDHLSL
        }

		
        Pass
        {
			
        	Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #define ASE_SRP_VERSION 41000
            #define REQUIRE_DEPTH_TEXTURE 1


            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
                float3 ase_normal : NORMAL;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			sampler2D _Noise;
			float _ScaleNoise;
			float _Speed;
			float _NoiseIntensity;
			uniform float4 _CameraDepthTexture_TexelSize;
			float _SizeFoam;
			float4 _ColorFoam;
			float _Depth;
			float _OpasityIntensity;

        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                float4 ase_texcoord7 : TEXCOORD7;
                float4 ase_texcoord8 : TEXCOORD8;
                float4 ase_texcoord9 : TEXCOORD9;
                UNITY_VERTEX_INPUT_INSTANCE_ID
        	};

			
            // x: global clip space bias, y: normal world space bias
            float4 _ShadowBias;
            float3 _LightDirection;

            VertexOutput ShadowPassVertex(GraphVertexInput v)
        	{
        	    VertexOutput o;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float mulTime43 = _Time.y * _Speed;
				float4 appendResult51 = (float4(0.0 , ( tex2Dlod( _Noise, float4( ( ( (ase_worldPos).xz * _ScaleNoise ) + mulTime43 ), 0, 0.0) ).r * _NoiseIntensity ) , 0.0 , 0.0));
				
				float4 temp_output_63_0 = ( float4( v.vertex.xyz , 0.0 ) + appendResult51 );
				float3 vertexPos13 = temp_output_63_0.xyz;
				float4 ase_clipPos13 = TransformObjectToHClip((vertexPos13).xyz);
				float4 screenPos13 = ComputeScreenPos(ase_clipPos13);
				o.ase_texcoord7 = screenPos13;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord8.xyz = ase_worldNormal;
				float3 vertexPos28 = temp_output_63_0.xyz;
				float4 ase_clipPos28 = TransformObjectToHClip((vertexPos28).xyz);
				float4 screenPos28 = ComputeScreenPos(ase_clipPos28);
				o.ase_texcoord9 = screenPos28;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord8.w = 0;
				float3 vertexValue = appendResult51.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;

        	    float3 positionWS = TransformObjectToWorld(v.vertex.xyz);
                float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

                float invNdotL = 1.0 - saturate(dot(_LightDirection, normalWS));
                float scale = invNdotL * _ShadowBias.y;

                // normal bias is negative since we want to apply an inset normal offset
                positionWS = _LightDirection * _ShadowBias.xxx + positionWS;
				positionWS = normalWS * scale.xxx + positionWS;
                float4 clipPos = TransformWorldToHClip(positionWS);

                // _ShadowBias.x sign depens on if platform has reversed z buffer
                //clipPos.z += _ShadowBias.x;

        	#if UNITY_REVERSED_Z
        	    clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
        	#else
        	    clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
        	#endif
                o.clipPos = clipPos;

        	    return o;
        	}

            half4 ShadowPassFragment(VertexOutput IN) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);

               float4 screenPos13 = IN.ase_texcoord7;
               float4 ase_screenPosNorm13 = screenPos13 / screenPos13.w;
               ase_screenPosNorm13.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm13.z : ase_screenPosNorm13.z * 0.5 + 0.5;
               float screenDepth13 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos13.xy/screenPos13.w  ),_ZBufferParams);
               float distanceDepth13 = abs( ( screenDepth13 - LinearEyeDepth( ase_screenPosNorm13.z,_ZBufferParams ) ) / ( _SizeFoam ) );
               float3 ase_worldNormal = IN.ase_texcoord8.xyz;
               float lerpResult17 = lerp( distanceDepth13 , ( distanceDepth13 * ( 1.0 - ase_worldNormal.y ) ) , 0.998);
               float4 ifLocalVar35 = 0;
               if( lerpResult17 <= _SizeFoam )
               ifLocalVar35 = _ColorFoam;
               float4 screenPos28 = IN.ase_texcoord9;
               float4 ase_screenPosNorm28 = screenPos28 / screenPos28.w;
               ase_screenPosNorm28.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm28.z : ase_screenPosNorm28.z * 0.5 + 0.5;
               float screenDepth28 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos28.xy/screenPos28.w  ),_ZBufferParams);
               float distanceDepth28 = abs( ( screenDepth28 - LinearEyeDepth( ase_screenPosNorm28.z,_ZBufferParams ) ) / ( _Depth ) );
               float clampResult32 = clamp( distanceDepth28 , 0.0 , 1.0 );
               float lerpResult33 = lerp( 1.0 , clampResult32 , _OpasityIntensity);
               float4 clampResult39 = clamp( ( ifLocalVar35 + lerpResult33 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
               

				float Alpha = clampResult39.r;
				float AlphaClipThreshold = AlphaClipThreshold;

         #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif
                return 0;
            }

            ENDHLSL
        }

		
        Pass
        {
			
        	Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }

            ZWrite On
			ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            #define ASE_SRP_VERSION 41000
            #define REQUIRE_DEPTH_TEXTURE 1


            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			sampler2D _Noise;
			float _ScaleNoise;
			float _Speed;
			float _NoiseIntensity;
			uniform float4 _CameraDepthTexture_TexelSize;
			float _SizeFoam;
			float4 _ColorFoam;
			float _Depth;
			float _OpasityIntensity;

            struct GraphVertexInput
            {
                float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                float4 ase_texcoord : TEXCOORD0;
                float4 ase_texcoord1 : TEXCOORD1;
                float4 ase_texcoord2 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
        	};

			           

            VertexOutput vert(GraphVertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float mulTime43 = _Time.y * _Speed;
				float4 appendResult51 = (float4(0.0 , ( tex2Dlod( _Noise, float4( ( ( (ase_worldPos).xz * _ScaleNoise ) + mulTime43 ), 0, 0.0) ).r * _NoiseIntensity ) , 0.0 , 0.0));
				
				float4 temp_output_63_0 = ( float4( v.vertex.xyz , 0.0 ) + appendResult51 );
				float3 vertexPos13 = temp_output_63_0.xyz;
				float4 ase_clipPos13 = TransformObjectToHClip((vertexPos13).xyz);
				float4 screenPos13 = ComputeScreenPos(ase_clipPos13);
				o.ase_texcoord = screenPos13;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord1.xyz = ase_worldNormal;
				float3 vertexPos28 = temp_output_63_0.xyz;
				float4 ase_clipPos28 = TransformObjectToHClip((vertexPos28).xyz);
				float4 screenPos28 = ComputeScreenPos(ase_clipPos28);
				o.ase_texcoord2 = screenPos28;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;
				float3 vertexValue = appendResult51.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;

        	    o.clipPos = TransformObjectToHClip(v.vertex.xyz);
        	    return o;
            }

            half4 frag(VertexOutput IN) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);

				float4 screenPos13 = IN.ase_texcoord;
				float4 ase_screenPosNorm13 = screenPos13 / screenPos13.w;
				ase_screenPosNorm13.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm13.z : ase_screenPosNorm13.z * 0.5 + 0.5;
				float screenDepth13 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos13.xy/screenPos13.w  ),_ZBufferParams);
				float distanceDepth13 = abs( ( screenDepth13 - LinearEyeDepth( ase_screenPosNorm13.z,_ZBufferParams ) ) / ( _SizeFoam ) );
				float3 ase_worldNormal = IN.ase_texcoord1.xyz;
				float lerpResult17 = lerp( distanceDepth13 , ( distanceDepth13 * ( 1.0 - ase_worldNormal.y ) ) , 0.998);
				float4 ifLocalVar35 = 0;
				if( lerpResult17 <= _SizeFoam )
				ifLocalVar35 = _ColorFoam;
				float4 screenPos28 = IN.ase_texcoord2;
				float4 ase_screenPosNorm28 = screenPos28 / screenPos28.w;
				ase_screenPosNorm28.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm28.z : ase_screenPosNorm28.z * 0.5 + 0.5;
				float screenDepth28 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos28.xy/screenPos28.w  ),_ZBufferParams);
				float distanceDepth28 = abs( ( screenDepth28 - LinearEyeDepth( ase_screenPosNorm28.z,_ZBufferParams ) ) / ( _Depth ) );
				float clampResult32 = clamp( distanceDepth28 , 0.0 , 1.0 );
				float lerpResult33 = lerp( 1.0 , clampResult32 , _OpasityIntensity);
				float4 clampResult39 = clamp( ( ifLocalVar35 + lerpResult33 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
				

				float Alpha = clampResult39.r;
				float AlphaClipThreshold = AlphaClipThreshold;

         #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif
                return 0;
            }
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
		
        Pass
        {
			
        	Name "Meta"
            Tags { "LightMode"="Meta" }

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x

            #pragma vertex vert
            #pragma fragment frag

            #define ASE_SRP_VERSION 41000
            #define REQUIRE_DEPTH_TEXTURE 1


			uniform float4 _MainTex_ST;
			
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/MetaInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/ShaderGraphFunctions.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			sampler2D _Noise;
			float _ScaleNoise;
			float _Speed;
			float _NoiseIntensity;
			uniform float4 _CameraDepthTexture_TexelSize;
			float _SizeFoam;
			float4 _Colorwater;
			float4 _ColorDepth;
			float _Depth;
			float4 _ColorFoam;
			float _OpasityIntensity;

            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature EDITOR_VISUALIZATION


            struct GraphVertexInput
            {
                float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

        	struct VertexOutput
        	{
        	    float4 clipPos      : SV_POSITION;
                float4 ase_texcoord : TEXCOORD0;
                float4 ase_texcoord1 : TEXCOORD1;
                float4 ase_texcoord2 : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
        	};

			
            VertexOutput vert(GraphVertexInput v)
            {
                VertexOutput o = (VertexOutput)0;
        	    UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				float3 ase_worldPos = mul(GetObjectToWorldMatrix(), v.vertex).xyz;
				float mulTime43 = _Time.y * _Speed;
				float4 appendResult51 = (float4(0.0 , ( tex2Dlod( _Noise, float4( ( ( (ase_worldPos).xz * _ScaleNoise ) + mulTime43 ), 0, 0.0) ).r * _NoiseIntensity ) , 0.0 , 0.0));
				
				float4 temp_output_63_0 = ( float4( v.vertex.xyz , 0.0 ) + appendResult51 );
				float3 vertexPos13 = temp_output_63_0.xyz;
				float4 ase_clipPos13 = TransformObjectToHClip((vertexPos13).xyz);
				float4 screenPos13 = ComputeScreenPos(ase_clipPos13);
				o.ase_texcoord = screenPos13;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord1.xyz = ase_worldNormal;
				float3 vertexPos28 = temp_output_63_0.xyz;
				float4 ase_clipPos28 = TransformObjectToHClip((vertexPos28).xyz);
				float4 screenPos28 = ComputeScreenPos(ase_clipPos28);
				o.ase_texcoord2 = screenPos28;
				
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.w = 0;

				float3 vertexValue = appendResult51.xyz;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal =  v.ase_normal ;
				
                o.clipPos = MetaVertexPosition(v.vertex, v.texcoord1.xy, v.texcoord2.xy, unity_LightmapST);
        	    return o;
            }

            half4 frag(VertexOutput IN) : SV_TARGET
            {
                UNITY_SETUP_INSTANCE_ID(IN);

           		float4 screenPos13 = IN.ase_texcoord;
           		float4 ase_screenPosNorm13 = screenPos13 / screenPos13.w;
           		ase_screenPosNorm13.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm13.z : ase_screenPosNorm13.z * 0.5 + 0.5;
           		float screenDepth13 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos13.xy/screenPos13.w  ),_ZBufferParams);
           		float distanceDepth13 = abs( ( screenDepth13 - LinearEyeDepth( ase_screenPosNorm13.z,_ZBufferParams ) ) / ( _SizeFoam ) );
           		float3 ase_worldNormal = IN.ase_texcoord1.xyz;
           		float lerpResult17 = lerp( distanceDepth13 , ( distanceDepth13 * ( 1.0 - ase_worldNormal.y ) ) , 0.998);
           		float4 screenPos28 = IN.ase_texcoord2;
           		float4 ase_screenPosNorm28 = screenPos28 / screenPos28.w;
           		ase_screenPosNorm28.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm28.z : ase_screenPosNorm28.z * 0.5 + 0.5;
           		float screenDepth28 = LinearEyeDepth(SHADERGRAPH_SAMPLE_SCENE_DEPTH( screenPos28.xy/screenPos28.w  ),_ZBufferParams);
           		float distanceDepth28 = abs( ( screenDepth28 - LinearEyeDepth( ase_screenPosNorm28.z,_ZBufferParams ) ) / ( _Depth ) );
           		float clampResult32 = clamp( distanceDepth28 , 0.0 , 1.0 );
           		float4 lerpResult30 = lerp( _Colorwater , _ColorDepth , clampResult32);
           		float4 ifLocalVar7 = 0;
           		if( lerpResult17 <= _SizeFoam )
           		ifLocalVar7 = _ColorFoam;
           		else
           		ifLocalVar7 = lerpResult30;
           		
           		float4 ifLocalVar35 = 0;
           		if( lerpResult17 <= _SizeFoam )
           		ifLocalVar35 = _ColorFoam;
           		float lerpResult33 = lerp( 1.0 , clampResult32 , _OpasityIntensity);
           		float4 clampResult39 = clamp( ( ifLocalVar35 + lerpResult33 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
           		
				
		        float3 Albedo = ifLocalVar7.rgb;
				float3 Emission = 0;
				float Alpha = clampResult39.r;
				float AlphaClipThreshold = 0;

         #if _AlphaClip
        		clip(Alpha - AlphaClipThreshold);
        #endif

                MetaInput metaInput = (MetaInput)0;
                metaInput.Albedo = Albedo;
                metaInput.Emission = Emission;
                
                return MetaFragment(metaInput);
            }
            ENDHLSL
        }
		
    }
    FallBack "Hidden/InternalErrorShader"
	CustomEditor "ASEMaterialInspector"
	
	
}
/*ASEBEGIN
Version=16300
7;7;1352;692;-1431.148;-588.0409;1.893302;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;45;694.0103,1361.929;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;54;1274.605,1268.694;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;44;1022.704,1499.557;Float;False;Property;_Speed;Speed;9;0;Create;True;0;0;False;0;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;52;1161.892,1382.505;Float;False;Property;_ScaleNoise;ScaleNoise;11;0;Create;True;0;0;False;0;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;1538.261,1285.97;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;43;1200.704,1487.557;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;42;1328.067,1034.249;Float;True;Property;_Noise;Noise;8;0;Create;True;0;0;False;0;None;eaa1c5562af66424092dc5b1f37208e9;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleAddOpNode;56;1765.838,1256.832;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;53;2025.855,1166.695;Float;True;Property;_TextureSample0;Texture Sample 0;13;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;50;2246.665,1406.527;Float;False;Property;_NoiseIntensity;NoiseIntensity;10;0;Create;True;0;0;False;0;0;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;2460.665,1388.527;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;2739.304,1503.169;Float;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;62;2930.888,1177.632;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;51;3076.747,1396.918;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.WorldNormalVector;1;-174.4834,301.1824;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;63;3341.888,1343.632;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-338.7028,135.0336;Float;False;Property;_SizeFoam;SizeFoam;5;0;Create;True;0;0;False;0;2;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;27;311.731,378.2962;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1452.036,952.2343;Float;False;Property;_Depth;Depth;3;0;Create;True;0;0;False;0;0;1.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;13;216.2884,90.14276;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;638.5895,246.5706;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;28;-1211.037,929.2343;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;177.0331,599.5805;Float;False;Constant;_Lerp_;Lerp_;0;0;Create;True;0;0;False;0;0.998;0.998;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;17;885.3664,375.2695;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;306.5998,787.0221;Float;False;Property;_ColorFoam;Color Foam;2;0;Create;True;0;0;False;0;0,0,0,0;1,0.9931185,0.86,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;34;-800.91,1145.026;Float;False;Property;_OpasityIntensity;OpasityIntensity;4;0;Create;True;0;0;False;0;1;0.489;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;32;-780.2148,940.8075;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;31;-575.4737,369.9068;Float;False;Property;_ColorDepth;Color Depth;1;0;Create;True;0;0;False;0;0,0,0,0;0.06077648,0.1231123,0.1607843,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;35;1102.634,646.0739;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;11;-653.0463,617.0334;Float;False;Property;_Colorwater;Color water;0;0;Create;True;0;0;False;0;0,0,0,0;0.1078,0.3022537,0.35,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;33;662.0513,1087.102;Float;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;30;-79.04843,474.3805;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;1413.607,903.7444;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;7;1724.745,415.7606;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;23;2644.094,907.2694;Float;False;Property;_Gloss;Gloss;6;0;Create;True;0;0;False;0;0.9;0.852;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;39;2816.778,1091.218;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;24;2637.187,812.5731;Float;False;Property;_Specular;Specular;7;0;Create;True;0;0;False;0;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;58;3449.993,754.686;Float;False;True;2;Float;ASEMaterialInspector;0;2;FastWaterLowPoly/WaterLWRP;1976390536c6c564abb90fe41f6ee334;True;Base;0;0;Base;11;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;0;True;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;False;False;False;True;True;True;True;True;0;False;-1;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=LightweightForward;False;0;;0;0;Standard;2;Vertex Position,InvertActionOnDeselection;1;Receive Shadows;1;1;_FinalColorxAlpha;0;4;True;True;True;True;False;11;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;9;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT3;0,0,0;False;10;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;59;3449.993,754.686;Float;False;False;2;Float;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;ShadowCaster;0;1;ShadowCaster;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;0;;0;0;Standard;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;61;3449.993,754.686;Float;False;False;2;Float;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;Meta;0;3;Meta;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;True;2;False;-1;False;False;False;False;False;True;1;LightMode=Meta;False;0;;0;0;Standard;0;6;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;60;3449.993,754.686;Float;False;False;2;Float;ASEMaterialInspector;0;1;Hidden/Templates/LightWeightSRPPBR;1976390536c6c564abb90fe41f6ee334;True;DepthOnly;0;2;DepthOnly;0;False;False;False;True;0;False;-1;False;False;False;False;False;True;3;RenderPipeline=LightweightPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;2;0;False;False;False;False;True;False;False;False;False;0;False;-1;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;0;;0;0;Standard;0;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;0
WireConnection;54;0;45;0
WireConnection;55;0;54;0
WireConnection;55;1;52;0
WireConnection;43;0;44;0
WireConnection;56;0;55;0
WireConnection;56;1;43;0
WireConnection;53;0;42;0
WireConnection;53;1;56;0
WireConnection;48;0;53;1
WireConnection;48;1;50;0
WireConnection;51;0;57;0
WireConnection;51;1;48;0
WireConnection;51;2;57;0
WireConnection;63;0;62;0
WireConnection;63;1;51;0
WireConnection;27;0;1;2
WireConnection;13;1;63;0
WireConnection;13;0;8;0
WireConnection;18;0;13;0
WireConnection;18;1;27;0
WireConnection;28;1;63;0
WireConnection;28;0;29;0
WireConnection;17;0;13;0
WireConnection;17;1;18;0
WireConnection;17;2;6;0
WireConnection;32;0;28;0
WireConnection;35;0;17;0
WireConnection;35;1;8;0
WireConnection;35;3;12;0
WireConnection;35;4;12;0
WireConnection;33;1;32;0
WireConnection;33;2;34;0
WireConnection;30;0;11;0
WireConnection;30;1;31;0
WireConnection;30;2;32;0
WireConnection;38;0;35;0
WireConnection;38;1;33;0
WireConnection;7;0;17;0
WireConnection;7;1;8;0
WireConnection;7;2;30;0
WireConnection;7;3;12;0
WireConnection;7;4;12;0
WireConnection;39;0;38;0
WireConnection;58;0;7;0
WireConnection;58;9;24;0
WireConnection;58;4;23;0
WireConnection;58;6;39;0
WireConnection;58;8;51;0
ASEEND*/
//CHKSM=377A536BF7985997028165471D45AE947F4656D5