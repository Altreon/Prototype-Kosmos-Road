// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "FastWaterLowPoly/WaterStandardRender"
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
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float4 screenPosition13;
			float3 worldNormal;
			float4 screenPos;
		};

		uniform sampler2D _Noise;
		uniform float _ScaleNoise;
		uniform float _Speed;
		uniform float _NoiseIntensity;
		uniform sampler2D _CameraDepthTexture;
		uniform float _SizeFoam;
		uniform float4 _Colorwater;
		uniform float4 _ColorDepth;
		uniform float _Depth;
		uniform float4 _ColorFoam;
		uniform float _Specular;
		uniform float _Gloss;
		uniform float _OpasityIntensity;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float mulTime43 = _Time.y * _Speed;
			float4 appendResult51 = (float4(0.0 , ( tex2Dlod( _Noise, float4( ( ( (ase_worldPos).xz * _ScaleNoise ) + mulTime43 ), 0, 0.0) ).r * _NoiseIntensity ) , 0.0 , 0.0));
			v.vertex.xyz += appendResult51.xyz;
			float3 ase_vertex3Pos = v.vertex.xyz;
			float3 vertexPos13 = ase_vertex3Pos;
			float4 ase_screenPos13 = ComputeScreenPos( UnityObjectToClipPos( vertexPos13 ) );
			o.screenPosition13 = ase_screenPos13;
		}

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float4 ase_screenPos13 = i.screenPosition13;
			float4 ase_screenPosNorm13 = ase_screenPos13 / ase_screenPos13.w;
			ase_screenPosNorm13.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm13.z : ase_screenPosNorm13.z * 0.5 + 0.5;
			float screenDepth13 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos13 ))));
			float distanceDepth13 = abs( ( screenDepth13 - LinearEyeDepth( ase_screenPosNorm13.z ) ) / ( _SizeFoam ) );
			float3 ase_worldNormal = i.worldNormal;
			float lerpResult17 = lerp( distanceDepth13 , ( distanceDepth13 * ( 1.0 - ase_worldNormal.y ) ) , 0.998);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth28 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			float distanceDepth28 = abs( ( screenDepth28 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depth ) );
			float clampResult32 = clamp( distanceDepth28 , 0.0 , 1.0 );
			float4 lerpResult30 = lerp( _Colorwater , _ColorDepth , clampResult32);
			float4 ifLocalVar7 = 0;
			if( lerpResult17 <= _SizeFoam )
				ifLocalVar7 = _ColorFoam;
			else
				ifLocalVar7 = lerpResult30;
			o.Albedo = ifLocalVar7.rgb;
			float3 temp_cast_1 = (_Specular).xxx;
			o.Specular = temp_cast_1;
			o.Smoothness = _Gloss;
			float4 ifLocalVar35 = 0;
			if( lerpResult17 <= _SizeFoam )
				ifLocalVar35 = _ColorFoam;
			float lerpResult33 = lerp( 1.0 , clampResult32 , _OpasityIntensity);
			float4 clampResult39 = clamp( ( ifLocalVar35 + lerpResult33 ) , float4( 0,0,0,0 ) , float4( 1,0,0,0 ) );
			o.Alpha = clampResult39.r;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardSpecular alpha:fade keepalpha fullforwardshadows exclude_path:deferred vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float4 screenPos : TEXCOORD3;
				float3 worldNormal : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldNormal = worldNormal;
				o.customPack1.xyzw = customInputData.screenPosition13;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.screenPosition13 = IN.customPack1.xyzw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = IN.worldNormal;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputStandardSpecular o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardSpecular, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16200
7;7;1352;692;2279.467;448.2882;3.590209;True;True
Node;AmplifyShaderEditor.RangedFloatNode;8;-174.4654,299.2709;Float;False;Property;_SizeFoam;SizeFoam;5;0;Create;True;0;0;False;0;2;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;45;694.0103,1361.929;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldNormalVector;1;-724,-335;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.PosVertexDataNode;58;-610.3984,140.198;Float;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;44;1022.704,1499.557;Float;False;Property;_Speed;Speed;9;0;Create;True;0;0;False;0;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;13;216.2884,90.14276;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;27;311.731,378.2962;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1468.085,307.0621;Float;False;Property;_Depth;Depth;3;0;Create;True;0;0;False;0;0;1.9;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;54;1274.605,1268.694;Float;False;True;False;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;52;1161.892,1382.505;Float;False;Property;_ScaleNoise;ScaleNoise;11;0;Create;True;0;0;False;0;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;28;-1227.086,284.0621;Float;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;177.0331,599.5805;Float;False;Constant;_Lerp_;Lerp_;0;0;Create;True;0;0;False;0;0.998;0.998;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;43;1200.704,1487.557;Float;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;638.5895,246.5706;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;1538.261,1285.97;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;42;1328.067,1034.249;Float;True;Property;_Noise;Noise;8;0;Create;True;0;0;False;0;None;eaa1c5562af66424092dc5b1f37208e9;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SimpleAddOpNode;56;1765.838,1256.832;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ClampOpNode;32;-780.2148,940.8075;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;17;885.3664,375.2695;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-800.91,1145.026;Float;False;Property;_OpasityIntensity;OpasityIntensity;4;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-366.7387,743.674;Float;False;Property;_ColorFoam;Color Foam;2;0;Create;True;0;0;False;0;0,0,0,0;1,0.9931185,0.86,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;11;-653.0463,617.0334;Float;False;Property;_Colorwater;Color water;0;0;Create;True;0;0;False;0;0,0,0,0;0.1056504,0.5212209,0.559,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;53;2025.855,1166.695;Float;True;Property;_TextureSample0;Texture Sample 0;13;0;Create;True;0;0;False;0;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ConditionalIfNode;35;1102.634,646.0739;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;31;-575.4737,369.9068;Float;False;Property;_ColorDepth;Color Depth;1;0;Create;True;0;0;False;0;0,0,0,0;0.06382974,0.1607839,0.04705847,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;33;662.0513,1087.102;Float;False;3;0;FLOAT;1;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;1727.652,1415.214;Float;False;Property;_NoiseIntensity;NoiseIntensity;10;0;Create;True;0;0;False;0;0;0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;38;1413.607,903.7444;Float;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;1941.652,1397.214;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;2220.292,1511.856;Float;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;30;-79.04843,474.3805;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;51;2416.419,1418.029;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;24;1580.093,440.4498;Float;False;Property;_Specular;Specular;7;0;Create;True;0;0;False;0;0.2;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;1587,535.1461;Float;False;Property;_Gloss;Gloss;6;0;Create;True;0;0;False;0;0.9;0.852;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ConditionalIfNode;7;1218.864,416.6523;Float;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;39;1635.872,901.3589;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;3013.5,607.0168;Float;False;True;2;Float;ASEMaterialInspector;0;0;StandardSpecular;FastWaterLowPoly/WaterStandardRender;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;True;0;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;13;1;58;0
WireConnection;13;0;8;0
WireConnection;27;0;1;2
WireConnection;54;0;45;0
WireConnection;28;0;29;0
WireConnection;43;0;44;0
WireConnection;18;0;13;0
WireConnection;18;1;27;0
WireConnection;55;0;54;0
WireConnection;55;1;52;0
WireConnection;56;0;55;0
WireConnection;56;1;43;0
WireConnection;32;0;28;0
WireConnection;17;0;13;0
WireConnection;17;1;18;0
WireConnection;17;2;6;0
WireConnection;53;0;42;0
WireConnection;53;1;56;0
WireConnection;35;0;17;0
WireConnection;35;1;8;0
WireConnection;35;3;12;0
WireConnection;35;4;12;0
WireConnection;33;1;32;0
WireConnection;33;2;34;0
WireConnection;38;0;35;0
WireConnection;38;1;33;0
WireConnection;48;0;53;1
WireConnection;48;1;50;0
WireConnection;30;0;11;0
WireConnection;30;1;31;0
WireConnection;30;2;32;0
WireConnection;51;0;57;0
WireConnection;51;1;48;0
WireConnection;51;2;57;0
WireConnection;7;0;17;0
WireConnection;7;1;8;0
WireConnection;7;2;30;0
WireConnection;7;3;12;0
WireConnection;7;4;12;0
WireConnection;39;0;38;0
WireConnection;0;0;7;0
WireConnection;0;3;24;0
WireConnection;0;4;23;0
WireConnection;0;9;39;0
WireConnection;0;11;51;0
ASEEND*/
//CHKSM=1597BB88CDA180924DECB90F7F53D413396F10F9