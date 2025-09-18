// Made with Amplify Shader Editor v1.9.1.9
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "YUBI"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Intencity("Intencity", Float) = 1
		_TextureSample0("Main_Tex", 2D) = "white" {}
		_MainTex_SpeedXY("MainTex_Speed X,Y", Vector) = (0,0,0,0)
		_Sub_Tex("Sub_Tex", 2D) = "white" {}
		_Sub_Speed("Sub_Speed", Vector) = (0,0,0,0)
		_Alpha("Alpha", Float) = 1
		[Toggle(_USE_DISSOLVE_ON)] _Use_Dissolve("Use_Dissolve", Float) = 0
		_Dissolve("Dissolve", 2D) = "white" {}
		[Toggle(_USE_FRESNEL_ON)] _Use_Fresnel("Use_Fresnel", Float) = 0
		_Fresnel_Power("Fresnel_Power", Float) = 5
		[Toggle(_USE_WAVE_ON)] _Use_Wave("Use_Wave", Float) = 0
		_Distortion_Texture("Distortion_Texture", 2D) = "white" {}
		_DistortionSpeedXYPowerZ("Distortion Speed X,Y/Power Z", Vector) = (0,0,0,0)
		_DistortionRange_Smooth("DistortionRange_Smooth", Range( 0.01 , 1)) = 0.01
		_DistortionRange_X("DistortionRange_X", Range( 0 , 1)) = 0.4928903
		_DistortionRange_Y("DistortionRange_Y", Range( 0 , 1)) = 0.5506806
		_DistortionRange_W("DistortionRange_W", Range( 0 , 1)) = 0.407048
		_DistortionRange_Z("DistortionRange_Z", Range( 0 , 1)) = 0.407048
		[Toggle(_USE_COLORRANGE_ON)] _Use_ColorRange("Use_ColorRange", Float) = 0
		_SubColorRange_Smooth("SubColorRange_Smooth", Range( 0.01 , 1)) = 0.01
		_SubColorRange_X("SubColorRange_X", Range( 0 , 1)) = 0
		_SubColorRange_Y("SubColorRange_Y", Range( 0 , 1)) = 1
		_SubColorRange_Z("SubColorRange_Z", Range( 0 , 1)) = 0
		_SubColorRange_W("SubColorRange_W", Range( 0 , 1)) = 1
		[HDR]_SubColor("SubColor", Color) = (1,1,1,0)

	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_FRAG_COLOR
				#pragma shader_feature_local _USE_COLORRANGE_ON
				#pragma shader_feature_local _USE_WAVE_ON
				#pragma shader_feature_local _USE_FRESNEL_ON
				#pragma shader_feature_local _USE_DISSOLVE_ON


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					float4 ase_texcoord2 : TEXCOORD2;
					float3 ase_normal : NORMAL;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					float4 ase_texcoord3 : TEXCOORD3;
					float4 ase_texcoord4 : TEXCOORD4;
					float4 ase_texcoord5 : TEXCOORD5;
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				uniform sampler2D _TextureSample0;
				uniform float2 _MainTex_SpeedXY;
				uniform float4 _TextureSample0_ST;
				uniform sampler2D _Distortion_Texture;
				uniform float3 _DistortionSpeedXYPowerZ;
				uniform float4 _Distortion_Texture_ST;
				uniform float _DistortionRange_X;
				uniform float _DistortionRange_Smooth;
				uniform float _DistortionRange_Y;
				uniform float _DistortionRange_Z;
				uniform float _DistortionRange_W;
				uniform float4 _SubColor;
				uniform float _SubColorRange_X;
				uniform float _SubColorRange_Smooth;
				uniform float _SubColorRange_Y;
				uniform float _SubColorRange_Z;
				uniform float _SubColorRange_W;
				uniform float _Intencity;
				uniform sampler2D _Sub_Tex;
				uniform float2 _Sub_Speed;
				uniform float _Fresnel_Power;
				uniform float _Alpha;
				uniform sampler2D _Dissolve;
				uniform float4 _Dissolve_ST;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_worldPos = mul(unity_ObjectToWorld, float4( (v.vertex).xyz, 1 )).xyz;
					o.ase_texcoord4.xyz = ase_worldPos;
					float3 ase_worldNormal = UnityObjectToWorldNormal(v.ase_normal);
					o.ase_texcoord5.xyz = ase_worldNormal;
					
					o.ase_texcoord3 = v.ase_texcoord2;
					
					//setting value to unused interpolator channels and avoid initialization warnings
					o.ase_texcoord4.w = 0;
					o.ase_texcoord5.w = 0;

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i , bool ase_vface : SV_IsFrontFace ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 uv_TextureSample0 = i.texcoord.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
					float4 appendResult55 = (float4(_DistortionSpeedXYPowerZ.x , _DistortionSpeedXYPowerZ.y , 0.0 , 0.0));
					float2 uv_Distortion_Texture = i.texcoord.xy * _Distortion_Texture_ST.xy + _Distortion_Texture_ST.zw;
					float2 panner54 = ( 1.0 * _Time.y * appendResult55.xy + uv_Distortion_Texture);
					float temp_output_63_0 = ( _DistortionRange_X * ( 1.0 + _DistortionRange_Smooth ) );
					float2 texCoord67 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult61 = smoothstep( temp_output_63_0 , ( temp_output_63_0 - _DistortionRange_Smooth ) , texCoord67.x);
					float temp_output_71_0 = ( _DistortionRange_Y * ( 1.0 + _DistortionRange_Smooth ) );
					float smoothstepResult73 = smoothstep( temp_output_71_0 , ( temp_output_71_0 - _DistortionRange_Smooth ) , texCoord67.x);
					float temp_output_77_0 = ( _DistortionRange_Z * ( 1.0 + _DistortionRange_Smooth ) );
					float smoothstepResult79 = smoothstep( temp_output_77_0 , ( temp_output_77_0 - _DistortionRange_Smooth ) , texCoord67.y);
					float temp_output_83_0 = ( _DistortionRange_W * ( 1.0 + _DistortionRange_Smooth ) );
					float smoothstepResult85 = smoothstep( temp_output_83_0 , ( temp_output_83_0 - _DistortionRange_Smooth ) , texCoord67.y);
					float4 temp_output_39_0 = ( float4( uv_TextureSample0, 0.0 , 0.0 ) + ( tex2D( _Distortion_Texture, panner54 ) * _DistortionSpeedXYPowerZ.z * ( smoothstepResult61 * ( 1.0 - smoothstepResult73 ) * smoothstepResult79 * ( 1.0 - smoothstepResult85 ) ) ) );
					float4 texCoord25 = i.ase_texcoord3;
					texCoord25.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
					float4 appendResult35 = (float4(0.0 , texCoord25.y , 0.0 , 0.0));
					#ifdef _USE_WAVE_ON
					float4 staticSwitch37 = ( temp_output_39_0 + appendResult35 );
					#else
					float4 staticSwitch37 = temp_output_39_0;
					#endif
					float2 panner19 = ( 1.0 * _Time.y * _MainTex_SpeedXY + staticSwitch37.rg);
					float4 tex2DNode11 = tex2D( _TextureSample0, panner19 );
					float temp_output_117_0 = ( _SubColorRange_X * ( 1.0 + _SubColorRange_Smooth ) );
					float2 texCoord107 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult121 = smoothstep( temp_output_117_0 , ( temp_output_117_0 - _SubColorRange_Smooth ) , texCoord107.x);
					float temp_output_110_0 = ( _SubColorRange_Y * ( 1.0 + _SubColorRange_Smooth ) );
					float smoothstepResult112 = smoothstep( temp_output_110_0 , ( temp_output_110_0 - _SubColorRange_Smooth ) , texCoord107.x);
					float temp_output_95_0 = ( _SubColorRange_Z * ( 1.0 + _SubColorRange_Smooth ) );
					float smoothstepResult97 = smoothstep( temp_output_95_0 , ( temp_output_95_0 - _SubColorRange_Smooth ) , texCoord107.y);
					float temp_output_105_0 = ( _SubColorRange_W * ( 1.0 + _SubColorRange_Smooth ) );
					float smoothstepResult100 = smoothstep( temp_output_105_0 , ( temp_output_105_0 - _SubColorRange_Smooth ) , texCoord107.y);
					float4 lerpResult124 = lerp( tex2DNode11 , _SubColor , ( smoothstepResult121 * ( 1.0 - smoothstepResult112 ) * smoothstepResult97 * ( 1.0 - smoothstepResult100 ) ));
					#ifdef _USE_COLORRANGE_ON
					float4 staticSwitch126 = lerpResult124;
					#else
					float4 staticSwitch126 = tex2DNode11;
					#endif
					float2 texCoord57 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float2 panner58 = ( 1.0 * _Time.y * _Sub_Speed + texCoord57);
					float4 tex2DNode56 = tex2D( _Sub_Tex, panner58 );
					float3 ase_worldPos = i.ase_texcoord4.xyz;
					float3 ase_worldViewDir = UnityWorldSpaceViewDir(ase_worldPos);
					ase_worldViewDir = normalize(ase_worldViewDir);
					float3 ase_worldNormal = i.ase_texcoord5.xyz;
					float3 _Fresnel = float3(0,1,5);
					float4 appendResult51 = (float4(_Fresnel_Power , 0.0 , 0.0 , 0.0));
					float fresnelNdotV43 = dot( ase_worldNormal, ase_worldViewDir );
					float fresnelNode43 = ( _Fresnel.x + _Fresnel.y * pow( 1.0 - fresnelNdotV43, appendResult51.x ) );
					#ifdef _USE_FRESNEL_ON
					float staticSwitch44 = ( tex2DNode11.a * ( fresnelNode43 * ase_vface ) );
					#else
					float staticSwitch44 = tex2DNode11.a;
					#endif
					float4 temp_cast_6 = (_Alpha).xxxx;
					float2 uv_Dissolve = i.texcoord.xy * _Dissolve_ST.xy + _Dissolve_ST.zw;
					float4 temp_cast_7 = (texCoord25.x).xxxx;
					#ifdef _USE_DISSOLVE_ON
					float4 staticSwitch26 = ( _Alpha * step( tex2D( _Dissolve, uv_Dissolve ) , temp_cast_7 ) );
					#else
					float4 staticSwitch26 = temp_cast_6;
					#endif
					float4 appendResult15 = (float4((( staticSwitch126 * _Intencity * i.color * tex2DNode56 )).rgb , ( staticSwitch44 * i.color.a * tex2DNode11 * staticSwitch26 * tex2DNode56 ).r));
					

					fixed4 col = appendResult15;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19109
Node;AmplifyShaderEditor.CommentaryNode;125;-3628.17,-1627.168;Inherit;False;1646.438;1372.084;Color Range;28;94;95;96;97;98;100;101;102;104;105;106;107;109;110;111;112;113;115;116;117;118;119;120;121;108;114;99;103;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;93;-1478.742,1007.226;Inherit;False;1149.667;475.2246;Fresnel;7;43;47;48;46;50;49;51;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;92;-3617.705,-164.2798;Inherit;False;1035.907;385.3333;Distorsion;5;38;53;54;52;55;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;90;-4615.862,405.1236;Inherit;False;1684.946;1364.031;Custom Mask;28;76;77;78;79;80;81;85;84;86;87;82;83;89;70;71;72;73;88;75;74;67;68;61;64;65;66;63;62;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;15;-118,-104.5;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;10;187,-111;Float;False;True;-1;2;ASEMaterialInspector;0;11;YUBI;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;2;5;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;False;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.PannerNode;19;-1446,-35.5;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-792.6565,753.8203;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;16;-413,-111.5;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;39;-2191.045,-196.2701;Inherit;True;2;2;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;37;-1802.803,-73.53873;Inherit;False;Property;_Use_Wave;Use_Wave;10;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;26;-588.7753,540.9591;Inherit;False;Property;_Use_Dissolve;Use_Dissolve;6;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;45;-261.402,606.256;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;11;-1220,-110.5;Inherit;True;Property;_TextureSample0;Main_Tex;1;0;Create;False;0;0;0;False;0;False;-1;100aa7736c329ad419747f812682f5eb;52bc7256ee1e1b74c8bd617492018ba2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-383,223.5;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;34;-1940.263,41.58364;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;44;-92.63533,570.2561;Inherit;False;Property;_Use_Fresnel;Use_Fresnel;8;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-1075.137,550.8225;Inherit;False;Property;_Alpha;Alpha;5;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;57;-1649.909,305.5764;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;58;-1395.242,324.2431;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;59;-1586.576,420.9098;Inherit;False;Property;_Sub_Speed;Sub_Speed;4;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;56;-1082.576,289.5764;Inherit;True;Property;_Sub_Tex;Sub_Tex;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-2445.677,-201.5299;Inherit;False;0;11;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;41;-2448.125,-40.93069;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode;35;-2126.067,702.4473;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.VertexColorNode;14;-1098.748,94.36995;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;13;-1089.333,-183.8334;Inherit;False;Property;_Intencity;Intencity;0;0;Create;True;0;0;0;False;0;False;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-2775.437,442.4331;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;38;-2901.797,-47.90916;Inherit;True;Property;_Distortion_Texture;Distortion_Texture;11;0;Create;True;0;0;0;False;0;False;-1;7ec54ec8b5cb4c04ea0b1f8318277994;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;54;-3108.372,-19.61309;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector3Node;52;-3567.705,37.05347;Inherit;False;Property;_DistortionSpeedXYPowerZ;Distortion Speed X,Y/Power Z;12;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;55;-3272.371,1.053538;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;49;-1428.742,1307.45;Inherit;False;Property;_Fresnel_Power;Fresnel_Power;9;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;46;-1358.075,1095.782;Inherit;False;Constant;_Fresnel;Fresnel;10;0;Create;True;0;0;0;False;0;False;0,1,5;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;50;-1255.409,1314.45;Inherit;False;True;True;True;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;51;-1063.742,1311.45;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TwoSidedSign;47;-789.4079,1369.783;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;43;-888.1826,1131.892;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;-565.0748,1131.449;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;25;-2152.499,913.5175;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;22;-1249.583,667.9012;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;20;-1746.998,49.40104;Inherit;False;Property;_MainTex_SpeedXY;MainTex_Speed X,Y;2;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleSubtractOpNode;76;-3370.583,1141.154;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;-3527.917,1089.154;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;78;-3628.583,1239.821;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;79;-3185.25,1037.821;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;-3977.146,1266.589;Inherit;False;Constant;_Float1;Float 1;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;81;-4093.208,1128.034;Inherit;False;Property;_DistortionRange_Z;DistortionRange_Z;17;0;Create;True;0;0;0;False;0;False;0.407048;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;85;-3333.98,1439.853;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;84;-3831.447,1636.155;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-3977.447,1602.155;Inherit;False;Constant;_Float2;Float 2;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-4018.115,1507.488;Inherit;False;Property;_DistortionRange_W;DistortionRange_W;16;0;Create;True;0;0;0;False;0;False;0.407048;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;82;-3573.447,1537.488;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-3730.782,1485.488;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;89;-3134.848,1438.391;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;67;-4527.355,942.6246;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;70;-3656.906,856.2084;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-3814.24,804.2084;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;72;-3914.906,954.8751;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;73;-3471.573,752.8751;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;88;-3176.732,756.6028;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;75;-4101.564,826.2084;Inherit;False;Property;_DistortionRange_Y;DistortionRange_Y;15;0;Create;True;0;0;0;False;0;False;0.5506806;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-4060.901,920.8751;Inherit;False;Constant;_Float0;Float 0;18;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;62;-3578.122,510.1355;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-3735.455,458.136;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;66;-3836.122,608.802;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;65;-3982.121,574.8022;Inherit;False;Constant;_1;1;13;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;-4022.786,480.1357;Inherit;False;Property;_DistortionRange_X;DistortionRange_X;14;0;Create;True;0;0;0;False;0;False;0.4928903;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;61;-3428.586,447.0702;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-4565.862,634.5274;Inherit;False;Property;_DistortionRange_Smooth;DistortionRange_Smooth;13;0;Create;True;0;0;0;False;0;False;0.01;0.5;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;94;-2421.399,-883.084;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;95;-2578.733,-935.084;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;97;-2236.066,-986.4167;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;98;-3027.961,-757.6492;Inherit;False;Constant;_Float3;Float 3;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;100;-2384.796,-584.3851;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;101;-2882.263,-388.0831;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-3028.262,-422.0831;Inherit;False;Constant;_Float4;Float 4;13;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;104;-2624.263,-486.7501;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;105;-2781.598,-538.7501;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;106;-2185.664,-585.8472;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;107;-3578.17,-1081.613;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;109;-2707.722,-1168.029;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;110;-2865.056,-1220.029;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;111;-2965.722,-1069.363;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;112;-2522.389,-1271.363;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;113;-2227.548,-1267.635;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;115;-3111.716,-1103.363;Inherit;False;Constant;_Float5;Float 5;18;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;116;-2628.938,-1514.102;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;117;-2786.271,-1566.102;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;118;-2886.938,-1415.436;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;119;-3032.937,-1449.435;Inherit;False;Constant;_Float6;Float 6;13;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;-3073.602,-1544.102;Inherit;False;Property;_SubColorRange_X;SubColorRange_X;20;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;121;-2479.402,-1577.168;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;108;-3541.344,-1372.377;Inherit;False;Property;_SubColorRange_Smooth;SubColorRange_Smooth;19;0;Create;True;0;0;0;False;0;False;0.01;0.5;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;99;-3144.023,-896.2041;Inherit;False;Property;_SubColorRange_Z;SubColorRange_Z;22;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;96;-2740.305,-779.0032;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;114;-3152.379,-1198.029;Inherit;False;Property;_SubColorRange_Y;SubColorRange_Y;21;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-3087.204,-555.9997;Inherit;False;Property;_SubColorRange_W;SubColorRange_W;23;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;123;-1561.371,-873.4596;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;53;-3413.039,-114.2798;Inherit;False;0;38;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-641.3738,-109.2603;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;122;-1643.67,-587.1178;Inherit;False;Property;_SubColor;SubColor;24;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;124;-918.9926,-530.8853;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;126;-893.938,-183.907;Inherit;False;Property;_Use_ColorRange;Use_ColorRange;18;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-1906.036,661.4594;Inherit;False;0;23;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;23;-1649.127,640.2891;Inherit;True;Property;_Dissolve;Dissolve;7;0;Create;True;0;0;0;False;0;False;-1;None;7860c88eb780bc64c88114ba3fc90e09;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
WireConnection;15;0;16;0
WireConnection;15;3;17;0
WireConnection;10;0;15;0
WireConnection;19;0;37;0
WireConnection;19;2;20;0
WireConnection;28;0;21;0
WireConnection;28;1;22;0
WireConnection;16;0;12;0
WireConnection;39;0;18;0
WireConnection;39;1;41;0
WireConnection;37;1;39;0
WireConnection;37;0;34;0
WireConnection;26;1;21;0
WireConnection;26;0;28;0
WireConnection;45;0;11;4
WireConnection;45;1;48;0
WireConnection;11;1;19;0
WireConnection;17;0;44;0
WireConnection;17;1;14;4
WireConnection;17;2;11;0
WireConnection;17;3;26;0
WireConnection;17;4;56;0
WireConnection;34;0;39;0
WireConnection;34;1;35;0
WireConnection;44;1;11;4
WireConnection;44;0;45;0
WireConnection;58;0;57;0
WireConnection;58;2;59;0
WireConnection;56;1;58;0
WireConnection;41;0;38;0
WireConnection;41;1;52;3
WireConnection;41;2;60;0
WireConnection;35;1;25;2
WireConnection;60;0;61;0
WireConnection;60;1;88;0
WireConnection;60;2;79;0
WireConnection;60;3;89;0
WireConnection;38;1;54;0
WireConnection;54;0;53;0
WireConnection;54;2;55;0
WireConnection;55;0;52;1
WireConnection;55;1;52;2
WireConnection;50;0;49;0
WireConnection;51;0;50;0
WireConnection;43;1;46;1
WireConnection;43;2;46;2
WireConnection;43;3;51;0
WireConnection;48;0;43;0
WireConnection;48;1;47;0
WireConnection;22;0;23;0
WireConnection;22;1;25;1
WireConnection;76;0;77;0
WireConnection;76;1;68;0
WireConnection;77;0;81;0
WireConnection;77;1;78;0
WireConnection;78;0;80;0
WireConnection;78;1;68;0
WireConnection;79;0;67;2
WireConnection;79;1;77;0
WireConnection;79;2;76;0
WireConnection;85;0;67;2
WireConnection;85;1;83;0
WireConnection;85;2;82;0
WireConnection;84;0;86;0
WireConnection;84;1;68;0
WireConnection;82;0;83;0
WireConnection;82;1;68;0
WireConnection;83;0;87;0
WireConnection;83;1;84;0
WireConnection;89;0;85;0
WireConnection;70;0;71;0
WireConnection;70;1;68;0
WireConnection;71;0;75;0
WireConnection;71;1;72;0
WireConnection;72;0;74;0
WireConnection;72;1;68;0
WireConnection;73;0;67;1
WireConnection;73;1;71;0
WireConnection;73;2;70;0
WireConnection;88;0;73;0
WireConnection;62;0;63;0
WireConnection;62;1;68;0
WireConnection;63;0;64;0
WireConnection;63;1;66;0
WireConnection;66;0;65;0
WireConnection;66;1;68;0
WireConnection;61;0;67;1
WireConnection;61;1;63;0
WireConnection;61;2;62;0
WireConnection;94;0;95;0
WireConnection;94;1;108;0
WireConnection;95;0;99;0
WireConnection;95;1;96;0
WireConnection;97;0;107;2
WireConnection;97;1;95;0
WireConnection;97;2;94;0
WireConnection;100;0;107;2
WireConnection;100;1;105;0
WireConnection;100;2;104;0
WireConnection;101;0;102;0
WireConnection;101;1;108;0
WireConnection;104;0;105;0
WireConnection;104;1;108;0
WireConnection;105;0;103;0
WireConnection;105;1;101;0
WireConnection;106;0;100;0
WireConnection;109;0;110;0
WireConnection;109;1;108;0
WireConnection;110;0;114;0
WireConnection;110;1;111;0
WireConnection;111;0;115;0
WireConnection;111;1;108;0
WireConnection;112;0;107;1
WireConnection;112;1;110;0
WireConnection;112;2;109;0
WireConnection;113;0;112;0
WireConnection;116;0;117;0
WireConnection;116;1;108;0
WireConnection;117;0;120;0
WireConnection;117;1;118;0
WireConnection;118;0;119;0
WireConnection;118;1;108;0
WireConnection;121;0;107;1
WireConnection;121;1;117;0
WireConnection;121;2;116;0
WireConnection;96;0;98;0
WireConnection;96;1;108;0
WireConnection;123;0;121;0
WireConnection;123;1;113;0
WireConnection;123;2;97;0
WireConnection;123;3;106;0
WireConnection;12;0;126;0
WireConnection;12;1;13;0
WireConnection;12;2;14;0
WireConnection;12;3;56;0
WireConnection;124;0;11;0
WireConnection;124;1;122;0
WireConnection;124;2;123;0
WireConnection;126;1;11;0
WireConnection;126;0;124;0
WireConnection;23;1;29;0
ASEEND*/
//CHKSM=2FDC00106A5DA91C820FCAFA6869A5300CE20AC1