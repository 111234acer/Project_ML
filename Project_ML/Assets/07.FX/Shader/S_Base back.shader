// Made with Amplify Shader Editor v1.9.9.4
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "S_Base front"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Main_Tex( "Main_Tex", 2D ) = "white" {}
		_Main_Speed( "Main_Speed", Vector ) = ( 0, 0, 0, 0 )
		_Sub_Tex( "Sub_Tex", 2D ) = "white" {}
		_Sub_Speed( "Sub_Speed", Vector ) = ( 0, 0, 0, 0 )
		_Intensity( "Intensity", Float ) = 1
		_Dissolve_Tex( "Dissolve_Tex", 2D ) = "white" {}
		_Alpha( "Alpha", Float ) = 1
		[Toggle( _USE_DISSOLVE_ON )] _Use_Dissolve( "Use_Dissolve", Float ) = 0
		[Toggle( _USE_FRESNEL_ON )] _Use_Fresnel( "Use_Fresnel", Float ) = 0
		_Fresnel_Power( "Fresnel_Power", Float ) = 5
		_Add_Fresnel_Power( "Add_Fresnel_Power", Float ) = 5
		[Toggle( _USE_WAVE_ON )] _Use_Wave( "Use_Wave", Float ) = 0
		_Distortion_Texture( "Distortion_Texture", 2D ) = "white" {}
		_DistortionSpeedXYIntensityZ( "Distortion Speed X,Y/Intensity Z", Vector ) = ( 0, 0, 0, 0 )
		_DistortionRange_X( "DistortionRange_X", Range( 0, 1 ) ) = 1
		_DistortionRange_Y( "DistortionRange_Y", Range( 0, 1 ) ) = 1
		_DistortionRange_Z( "DistortionRange_Z", Range( 0, 1 ) ) = 1
		_DistortionRange_W( "DistortionRange_W", Range( 0, 1 ) ) = 1
		_DistortionRange_Smooth( "DistortionRange_Smooth", Range( 0.01, 1 ) ) = 0.4322
		_SubColor_X( "SubColor_X", Range( 0, 1 ) ) = 0
		_SubColor_Y( "SubColor_Y", Range( 0, 1 ) ) = 1
		_SubColor_Z( "SubColor_Z", Range( 0, 1 ) ) = 1
		_SubColor_W( "SubColor_W", Range( 0, 1 ) ) = 1
		_SubColor_Smooth( "SubColor_Smooth", Range( 0.01, 1 ) ) = 0.4322
		[HDR] _SubColor( "SubColor", Color ) = ( 1, 0, 0, 0 )
		[Toggle( _USE_DISSOLVE_DISTORTION_ON )] _Use_Dissolve_Distortion( "Use_Dissolve_Distortion", Float ) = 0
		_Dissolve_Edge_Range( "Dissolve_Edge_Range", Float ) = 0.2
		[HDR] _Edge_Color( "Edge_Color", Color ) = ( 1, 0, 0, 0 )
		[Toggle( _USE_DISSOLVE_EDGE_ON )] _Use_Dissolve_Edge( "Use_Dissolve_Edge", Float ) = 0
		[HDR] _Add_Fresnel_Color( "Add_Fresnel_Color", Color ) = ( 1, 0, 0, 0 )
		[Toggle( _USE_ADD_FRESNEL_ON )] _Use_Add_Fresnel( "Use_Add_Fresnel", Float ) = 0
		_Depth( "Depth", Float ) = 0

	}


	Category
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Back
			Lighting Off
			ZWrite Off
			ZTest LEqual
			
			Pass {

				CGPROGRAM
				#define ASE_VERSION 19904

				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif

				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.5
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#include "UnityShaderVariables.cginc"
				#define ASE_NEEDS_TEXTURE_COORDINATES0
				#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0
				#define ASE_NEEDS_TEXTURE_COORDINATES2
				#define ASE_NEEDS_FRAG_COLOR
				#pragma shader_feature_local _USE_ADD_FRESNEL_ON
				#pragma shader_feature_local _USE_DISSOLVE_EDGE_ON
				#pragma shader_feature_local _USE_WAVE_ON
				#pragma shader_feature_local _USE_DISSOLVE_DISTORTION_ON
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
					float4 ase_texcoord6 : TEXCOORD6;
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
				uniform sampler2D _Main_Tex;
				uniform float2 _Main_Speed;
				uniform float4 _Main_Tex_ST;
				uniform sampler2D _Distortion_Texture;
				uniform float4 _DistortionSpeedXYIntensityZ;
				uniform float4 _Distortion_Texture_ST;
				uniform float _DistortionRange_X;
				uniform float _DistortionRange_Smooth;
				uniform float _DistortionRange_Y;
				uniform float _DistortionRange_Z;
				uniform float _DistortionRange_W;
				uniform sampler2D _Dissolve_Tex;
				uniform float4 _Dissolve_Tex_ST;
				uniform float _Dissolve_Edge_Range;
				uniform float4 _Edge_Color;
				uniform float4 _SubColor;
				uniform float _SubColor_X;
				uniform float _SubColor_Smooth;
				uniform float _SubColor_Y;
				uniform float _SubColor_Z;
				uniform float _SubColor_W;
				uniform float _Intensity;
				uniform sampler2D _Sub_Tex;
				uniform float2 _Sub_Speed;
				uniform float4 _Sub_Tex_ST;
				uniform float _Add_Fresnel_Power;
				uniform float4 _Add_Fresnel_Color;
				uniform float _Fresnel_Power;
				uniform float _Alpha;
				uniform float4 _CameraDepthTexture_TexelSize;
				uniform float _Depth;


				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					float3 ase_positionWS = mul( unity_ObjectToWorld, float4( ( v.vertex ).xyz, 1 ) ).xyz;
					o.ase_texcoord4.xyz = ase_positionWS;
					float3 ase_normalWS = UnityObjectToWorldNormal( v.ase_normal );
					o.ase_texcoord5.xyz = ase_normalWS;
					float4 ase_positionCS = UnityObjectToClipPos( v.vertex );
					float4 screenPos = ComputeScreenPos( ase_positionCS );
					o.ase_texcoord6 = screenPos;
					
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

				fixed4 frag ( v2f i , uint ase_vface : SV_IsFrontFace ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float2 uv_Main_Tex = i.texcoord.xy * _Main_Tex_ST.xy + _Main_Tex_ST.zw;
					float4 appendResult40 = (float4(_DistortionSpeedXYIntensityZ.x , _DistortionSpeedXYIntensityZ.y , 0.0 , 0.0));
					float2 uv_Distortion_Texture = i.texcoord.xy * _Distortion_Texture_ST.xy + _Distortion_Texture_ST.zw;
					float2 panner39 = ( 1.0 * _Time.y * appendResult40.xy + uv_Distortion_Texture);
					float temp_output_53_0 = ( _DistortionRange_X * ( 1.0 + _DistortionRange_Smooth ) );
					float2 texCoord56 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult52 = smoothstep( temp_output_53_0 , ( temp_output_53_0 - _DistortionRange_Smooth ) , texCoord56.x);
					float temp_output_59_0 = ( ( 1.0 - _DistortionRange_Y ) * ( 1.0 + _DistortionRange_Smooth ) );
					float smoothstepResult61 = smoothstep( temp_output_59_0 , ( temp_output_59_0 - _DistortionRange_Smooth ) , texCoord56.x);
					float temp_output_66_0 = ( _DistortionRange_Z * ( 1.0 + _DistortionRange_Smooth ) );
					float smoothstepResult68 = smoothstep( temp_output_66_0 , ( temp_output_66_0 - _DistortionRange_Smooth ) , texCoord56.y);
					float temp_output_73_0 = ( ( 1.0 - _DistortionRange_W ) * ( 1.0 + _DistortionRange_Smooth ) );
					float smoothstepResult75 = smoothstep( temp_output_73_0 , ( temp_output_73_0 - _DistortionRange_Smooth ) , texCoord56.y);
					float4 temp_output_32_0 = ( tex2D( _Distortion_Texture, panner39 ) * _DistortionSpeedXYIntensityZ.z * ( smoothstepResult52 * ( 1.0 - smoothstepResult61 ) * smoothstepResult68 * ( 1.0 - smoothstepResult75 ) ) );
					float4 temp_output_31_0 = ( float4( uv_Main_Tex, 0.0 , 0.0 ) + temp_output_32_0 );
					float4 texCoord15 = i.ase_texcoord3;
					texCoord15.xy = i.ase_texcoord3.xy * float2( 1,1 ) + float2( 0,0 );
					float4 appendResult33 = (float4(0.0 , texCoord15.y , 0.0 , 0.0));
					#ifdef _USE_WAVE_ON
					float4 staticSwitch29 = ( temp_output_31_0 + appendResult33 );
					#else
					float4 staticSwitch29 = temp_output_31_0;
					#endif
					float2 panner10 = ( 1.0 * _Time.y * _Main_Speed + staticSwitch29.rg);
					float4 tex2DNode2 = tex2D( _Main_Tex, panner10 );
					float2 uv_Dissolve_Tex = i.texcoord.xy * _Dissolve_Tex_ST.xy + _Dissolve_Tex_ST.zw;
					#ifdef _USE_DISSOLVE_DISTORTION_ON
					float4 staticSwitch120 = ( temp_output_32_0 + float4( uv_Dissolve_Tex, 0.0 , 0.0 ) );
					#else
					float4 staticSwitch120 = float4( uv_Dissolve_Tex, 0.0 , 0.0 );
					#endif
					float4 tex2DNode13 = tex2D( _Dissolve_Tex, staticSwitch120.rg );
					float4 temp_cast_7 = (( texCoord15.x + _Dissolve_Edge_Range )).xxxx;
					float4 temp_output_123_0 = step( tex2DNode13 , temp_cast_7 );
					float4 temp_cast_8 = (texCoord15.x).xxxx;
					float4 temp_output_14_0 = step( tex2DNode13 , temp_cast_8 );
					float4 temp_output_126_0 = ( temp_output_123_0 - temp_output_14_0 );
					float4 lerpResult130 = lerp( tex2DNode2 , ( temp_output_126_0 * _Edge_Color ) , temp_output_126_0);
					#ifdef _USE_DISSOLVE_EDGE_ON
					float4 staticSwitch131 = lerpResult130;
					#else
					float4 staticSwitch131 = tex2DNode2;
					#endif
					float temp_output_103_0 = ( _SubColor_X * ( 1.0 + _SubColor_Smooth ) );
					float2 texCoord84 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult110 = smoothstep( temp_output_103_0 , ( temp_output_103_0 - _SubColor_Smooth ) , texCoord84.x);
					float temp_output_88_0 = ( ( 1.0 - _SubColor_Y ) * ( 1.0 + _SubColor_Smooth ) );
					float smoothstepResult109 = smoothstep( temp_output_88_0 , ( temp_output_88_0 - _SubColor_Smooth ) , texCoord84.x);
					float temp_output_91_0 = ( _SubColor_Z * ( 1.0 + _SubColor_Smooth ) );
					float smoothstepResult111 = smoothstep( temp_output_91_0 , ( temp_output_91_0 - _SubColor_Smooth ) , texCoord84.y);
					float temp_output_94_0 = ( ( 1.0 - _SubColor_W ) * ( 1.0 + _SubColor_Smooth ) );
					float smoothstepResult108 = smoothstep( temp_output_94_0 , ( temp_output_94_0 - _SubColor_Smooth ) , texCoord84.y);
					float temp_output_114_0 = ( smoothstepResult110 * ( 1.0 - smoothstepResult109 ) * smoothstepResult111 * ( 1.0 - smoothstepResult108 ) );
					float4 lerpResult83 = lerp( staticSwitch131 , _SubColor , temp_output_114_0);
					float2 uv_Sub_Tex = i.texcoord.xy * _Sub_Tex_ST.xy + _Sub_Tex_ST.zw;
					float2 panner45 = ( 1.0 * _Time.y * _Sub_Speed + uv_Sub_Tex);
					float4 tex2DNode42 = tex2D( _Sub_Tex, panner45 );
					float4 temp_output_3_0 = ( lerpResult83 * _Intensity * i.color * tex2DNode42 );
					float3 ase_positionWS = i.ase_texcoord4.xyz;
					float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_positionWS );
					float3 ase_viewDirWS = normalize( ase_viewVectorWS );
					float3 ase_normalWS = i.ase_texcoord5.xyz;
					float2 _Vector0 = float2(0,1);
					float4 appendResult138 = (float4(_Add_Fresnel_Power , 0.0 , 0.0 , 0.0));
					float fresnelNdotV134 = dot( ase_normalWS, ase_viewDirWS );
					float fresnelNode134 = ( _Vector0.x + _Vector0.y * pow( 1.0 - fresnelNdotV134, appendResult138.x ) );
					#ifdef _USE_ADD_FRESNEL_ON
					float4 staticSwitch142 = ( ( fresnelNode134 * ( ase_vface > 0 ? +1 : -1 ) * _Add_Fresnel_Color ) + temp_output_3_0 );
					#else
					float4 staticSwitch142 = temp_output_3_0;
					#endif
					float2 _Fresnel = float2(0,1);
					float4 appendResult24 = (float4(_Fresnel_Power , 0.0 , 0.0 , 0.0));
					float fresnelNdotV22 = dot( ase_normalWS, ase_viewDirWS );
					float fresnelNode22 = ( _Fresnel.x + _Fresnel.y * pow( 1.0 - fresnelNdotV22, appendResult24.x ) );
					#ifdef _USE_FRESNEL_ON
					float staticSwitch19 = ( tex2DNode2.a * ( fresnelNode22 * ( ase_vface > 0 ? +1 : -1 ) ) );
					#else
					float staticSwitch19 = tex2DNode2.a;
					#endif
					float4 temp_cast_11 = (_Alpha).xxxx;
					#ifdef _USE_DISSOLVE_ON
					float4 staticSwitch18 = temp_output_123_0;
					#else
					float4 staticSwitch18 = temp_cast_11;
					#endif
					float4 screenPos = i.ase_texcoord6;
					float4 ase_positionSSNorm = screenPos / screenPos.w;
					ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
					float screenDepth143 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_positionSSNorm.xy ));
					float distanceDepth143 = abs( ( screenDepth143 - LinearEyeDepth( ase_positionSSNorm.z ) ) / ( _Depth ) );
					float4 appendResult7 = (float4((staticSwitch142).rgb , ( ( staticSwitch19 * i.color.a * tex2DNode2 * staticSwitch18 * tex2DNode42 * tex2DNode2.a ) * saturate( distanceDepth143 ) ).r));
					

					fixed4 col = appendResult7;
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG
			}
		}
	}
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19904
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;71;-3969.371,1542.453;Inherit;False;Constant;_Float1;Float 1;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;57;-4114.593,961.9839;Inherit;False;Constant;_Float2;Float 2;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;62;-4263.294,848.8046;Inherit;False;Property;_DistortionRange_Y;DistortionRange_Y;16;0;Create;True;0;0;0;False;0;False;1;0.5506806;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;76;-4320.597,1432.664;Inherit;False;Property;_DistortionRange_W;DistortionRange_W;18;0;Create;True;0;0;0;False;0;False;1;0.407048;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;50;-3895.625,647.4623;Inherit;False;Constant;_1;1;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;58;-3862.616,967.5557;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;64;-4013.83,1245.396;Inherit;False;Constant;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;72;-3848.47,1569.753;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;78;-3961.445,850.0979;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;80;-3954.586,1457.713;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;54;-4088.998,728.7097;Inherit;False;Property;_DistortionRange_Smooth;DistortionRange_Smooth;19;0;Create;True;0;0;0;False;0;False;0.4322;0.01;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;56;-3910.611,330.8834;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;51;-3774.724,674.7624;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;59;-3752.116,875.2555;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;65;-3892.929,1272.696;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;73;-3737.97,1477.453;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;69;-4235.084,1142.472;Inherit;False;Property;_DistortionRange_Z;DistortionRange_Z;17;0;Create;True;0;0;0;False;0;False;1;0.407048;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;60;-3627.316,992.2555;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;74;-3613.17,1594.452;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;48;-4049.322,556.1341;Inherit;False;Property;_DistortionRange_X;DistortionRange_X;15;0;Create;True;0;0;0;False;0;False;1;0.4928903;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;41;-3547.569,127.9409;Inherit;False;Property;_DistortionSpeedXYIntensityZ;Distortion Speed X,Y/Intensity Z;14;0;Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;40;-3204.378,125.6059;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;38;-3257.622,8.00091;Inherit;False;0;35;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;66;-3782.429,1180.396;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;53;-3664.224,582.4622;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;55;-3539.424,699.4621;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;67;-3657.629,1297.395;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;75;-3478.823,1453.156;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;61;-3492.969,850.9588;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;12;-2196.969,757.297;Inherit;False;0;13;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;39;-3031.378,49.6059;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;15;-2818.886,879.0212;Inherit;False;2;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;68;-3477.281,1147.099;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;81;-3179.586,1456.713;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;79;-3215.819,872.0059;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;52;-3405.077,558.1655;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;119;-1928.989,613.6919;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT2;0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;36;-2452.809,654.3171;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;47;-2556.412,202.168;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;82;-2950.726,592.9125;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;35;-2841.485,14.17371;Inherit;True;Property;_Distortion_Texture;Distortion_Texture;13;0;Create;True;0;0;0;False;0;False;-1;7ec54ec8b5cb4c04ea0b1f8318277994;7ec54ec8b5cb4c04ea0b1f8318277994;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;125;-2041.56,1165.724;Inherit;False;Property;_Dissolve_Edge_Range;Dissolve_Edge_Range;27;0;Create;True;0;0;0;False;0;False;0.2;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;120;-1813.055,691.5476;Inherit;False;Property;_Use_Dissolve_Distortion;Use_Dissolve_Distortion;26;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;33;-2365.817,310.0287;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;92;-2106.745,-483.6988;Inherit;False;Constant;_Float5;Float 5;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;95;-2251.967,-1064.168;Inherit;False;Constant;_Float6;Float 6;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;100;-2400.667,-1177.347;Inherit;False;Property;_SubColor_Y;SubColor_Y;21;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;102;-2457.971,-593.4878;Inherit;False;Property;_SubColor_W;SubColor_W;23;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;46;-2490.667,-202.8161;Inherit;False;0;2;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;32;-2508.812,-0.1024451;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;13;-1564.61,810.6838;Inherit;True;Property;_Dissolve_Tex;Dissolve_Tex;5;0;Create;True;0;0;0;False;0;False;-1;a3221f6b02fdc8946863b98163f67c78;a3221f6b02fdc8946863b98163f67c78;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;122;-1747.56,1055.724;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;31;-2241.504,-91.75932;Inherit;True;2;2;0;FLOAT2;0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;30;-1930.344,44.02869;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;85;-2032.999,-1378.69;Inherit;False;Constant;_Float3;Float 3;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;87;-1999.99,-1058.596;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;89;-2151.204,-780.7558;Inherit;False;Constant;_Float4;Float 4;0;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;93;-1985.843,-456.3988;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;96;-2098.819,-1176.054;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;97;-2091.96,-568.4387;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;99;-2188.672,-1293.542;Inherit;False;Property;_SubColor_Smooth;SubColor_Smooth;24;0;Create;True;0;0;0;False;0;False;0.4322;0.4322;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;14;-1212.11,847.1838;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;123;-1255.81,1172.974;Inherit;True;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;26;-926.733,1603.128;Inherit;False;Property;_Fresnel_Power;Fresnel_Power;9;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;11;-1721.451,133.174;Inherit;False;Property;_Main_Speed;Main_Speed;1;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;29;-1841.185,-73.07632;Inherit;False;Property;_Use_Wave;Use_Wave;11;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;84;-2047.985,-1695.269;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;86;-1912.097,-1351.389;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;88;-1889.489,-1150.896;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;90;-2030.303,-753.4557;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;94;-1875.343,-548.6987;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;105;-1764.689,-1033.896;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;107;-1750.543,-431.6998;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;98;-2198.223,-1466.609;Inherit;False;Property;_SubColor_X;SubColor_X;20;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;101;-2372.458,-883.6797;Inherit;False;Property;_SubColor_Z;SubColor_Z;22;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;126;-928.5601,1091.724;Inherit;True;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;25;-648.5352,1853.61;Inherit;False;True;True;True;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;133;-487.677,-920.5865;Inherit;False;Property;_Add_Fresnel_Power;Add_Fresnel_Power;10;0;Create;True;0;0;0;False;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;128;-915.4867,1304.976;Inherit;False;Property;_Edge_Color;Edge_Color;28;1;[HDR];Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.PannerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;10;-1476.451,-72.82605;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;91;-1919.802,-845.7558;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;103;-1801.597,-1443.69;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;104;-1676.797,-1326.69;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;106;-1795.002,-728.7568;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;108;-1616.196,-572.9957;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;109;-1630.342,-1175.193;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;127;-524.6708,993.15;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;37;-576,1550.741;Inherit;False;Constant;_Fresnel;Fresnel;8;0;Create;True;0;0;0;False;0;False;0,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;24;-440.5352,1853.61;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;139;-224.4792,-930.1046;Inherit;False;True;True;True;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;110;-1542.45,-1467.986;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;111;-1614.654,-879.0528;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;112;-1316.959,-569.4387;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;113;-1353.192,-1154.146;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;44;-1673.987,476.7573;Inherit;False;Property;_Sub_Speed;Sub_Speed;3;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;43;-1737.448,356.6993;Inherit;False;0;42;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;2;-1242.539,-88.8043;Inherit;True;Property;_Main_Tex;Main_Tex;0;0;Create;True;0;0;0;False;0;False;-1;0a9ab7326c31a444b9cac666f32d1457;0a9ab7326c31a444b9cac666f32d1457;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;130;-688.6705,-91.8473;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TwoSidedSign, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;27;-272,1742.741;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;137;-151.944,-1232.974;Inherit;False;Constant;_Vector0;Vector 0;8;0;Create;True;0;0;0;False;0;False;0,1;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;138;-16.47923,-930.1046;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.FresnelNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;22;-400,1534.741;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;114;-1091.551,-1433.239;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;117;-1029.713,-1140.011;Inherit;False;Property;_SubColor;SubColor;25;1;[HDR];Create;True;0;0;0;False;0;False;1,0,0,0;1,1,1,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.PannerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;45;-1447.487,413.3567;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;28;-80,1534.741;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;131;-593.6409,-229.2473;Inherit;False;Property;_Use_Dissolve_Edge;Use_Dissolve_Edge;29;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;141;104.7036,-830.7795;Inherit;False;Property;_Add_Fresnel_Color;Add_Fresnel_Color;30;1;[HDR];Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;True;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.FresnelNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;134;24.05595,-1248.974;Inherit;True;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TwoSidedSign, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;136;152.056,-1040.974;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;4;-1131.236,-154.9243;Inherit;False;Property;_Intensity;Intensity;4;0;Create;True;0;0;0;False;0;False;1;55;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;42;-1264.183,384.3096;Inherit;True;Property;_Sub_Tex;Sub_Tex;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;20;-239.1987,815.259;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;5;-1145.929,163.2917;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;135;344.056,-1248.974;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;144;207.9054,579.0218;Inherit;False;Property;_Depth;Depth;32;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;17;-1151.401,633.9953;Inherit;False;Property;_Alpha;Alpha;6;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;83;-812.3653,-680.4587;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;18;-403.7419,510.1094;Inherit;False;Property;_Use_Dissolve;Use_Dissolve;7;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;19;-70.1279,723.6796;Inherit;False;Property;_Use_Fresnel;Use_Fresnel;8;0;Create;True;0;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;3;-238.1671,-173.1147;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;143;389.9275,534.3437;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;140;308.7519,-613.7827;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;8;-50.87292,134.2168;Inherit;True;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;145;655.9054,530.0218;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;142;528.907,-954.9749;Inherit;False;Property;_Use_Add_Fresnel;Use_Add_Fresnel;31;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;6;414.7122,-153.9403;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;146;596.235,118.9984;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;34;-2719.185,261.9237;Inherit;False;Property;_Distortion_Power;Distortion_Power;12;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;116;-692.0948,-1386.583;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;121;-2176.497,1255.759;Inherit;False;Constant;_Float7;Float 7;26;0;Create;True;0;0;0;False;0;False;0.2786707;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;7;612.7122,-148.6403;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;16;-954.3065,737.4573;Inherit;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1;852.6667,-148.5028;Float;False;True;-1;2;AmplifyShaderEditor.MaterialInspector;0;11;S_Base front;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;2;5;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;True;True;0;False;;True;True;True;True;True;False;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;58;0;57;0
WireConnection;58;1;54;0
WireConnection;72;0;71;0
WireConnection;72;1;54;0
WireConnection;78;0;62;0
WireConnection;80;0;76;0
WireConnection;51;0;50;0
WireConnection;51;1;54;0
WireConnection;59;0;78;0
WireConnection;59;1;58;0
WireConnection;65;0;64;0
WireConnection;65;1;54;0
WireConnection;73;0;80;0
WireConnection;73;1;72;0
WireConnection;60;0;59;0
WireConnection;60;1;54;0
WireConnection;74;0;73;0
WireConnection;74;1;54;0
WireConnection;40;0;41;1
WireConnection;40;1;41;2
WireConnection;66;0;69;0
WireConnection;66;1;65;0
WireConnection;53;0;48;0
WireConnection;53;1;51;0
WireConnection;55;0;53;0
WireConnection;55;1;54;0
WireConnection;67;0;66;0
WireConnection;67;1;54;0
WireConnection;75;0;56;2
WireConnection;75;1;73;0
WireConnection;75;2;74;0
WireConnection;61;0;56;1
WireConnection;61;1;59;0
WireConnection;61;2;60;0
WireConnection;39;0;38;0
WireConnection;39;2;40;0
WireConnection;68;0;56;2
WireConnection;68;1;66;0
WireConnection;68;2;67;0
WireConnection;81;0;75;0
WireConnection;79;0;61;0
WireConnection;52;0;56;1
WireConnection;52;1;53;0
WireConnection;52;2;55;0
WireConnection;119;0;32;0
WireConnection;119;1;12;0
WireConnection;36;0;15;2
WireConnection;47;0;41;3
WireConnection;82;0;52;0
WireConnection;82;1;79;0
WireConnection;82;2;68;0
WireConnection;82;3;81;0
WireConnection;35;1;39;0
WireConnection;120;1;12;0
WireConnection;120;0;119;0
WireConnection;33;1;36;0
WireConnection;32;0;35;0
WireConnection;32;1;47;0
WireConnection;32;2;82;0
WireConnection;13;1;120;0
WireConnection;122;0;15;1
WireConnection;122;1;125;0
WireConnection;31;0;46;0
WireConnection;31;1;32;0
WireConnection;30;0;31;0
WireConnection;30;1;33;0
WireConnection;87;0;95;0
WireConnection;87;1;99;0
WireConnection;93;0;92;0
WireConnection;93;1;99;0
WireConnection;96;0;100;0
WireConnection;97;0;102;0
WireConnection;14;0;13;0
WireConnection;14;1;15;1
WireConnection;123;0;13;0
WireConnection;123;1;122;0
WireConnection;29;1;31;0
WireConnection;29;0;30;0
WireConnection;86;0;85;0
WireConnection;86;1;99;0
WireConnection;88;0;96;0
WireConnection;88;1;87;0
WireConnection;90;0;89;0
WireConnection;90;1;99;0
WireConnection;94;0;97;0
WireConnection;94;1;93;0
WireConnection;105;0;88;0
WireConnection;105;1;99;0
WireConnection;107;0;94;0
WireConnection;107;1;99;0
WireConnection;126;0;123;0
WireConnection;126;1;14;0
WireConnection;25;0;26;0
WireConnection;10;0;29;0
WireConnection;10;2;11;0
WireConnection;91;0;101;0
WireConnection;91;1;90;0
WireConnection;103;0;98;0
WireConnection;103;1;86;0
WireConnection;104;0;103;0
WireConnection;104;1;99;0
WireConnection;106;0;91;0
WireConnection;106;1;99;0
WireConnection;108;0;84;2
WireConnection;108;1;94;0
WireConnection;108;2;107;0
WireConnection;109;0;84;1
WireConnection;109;1;88;0
WireConnection;109;2;105;0
WireConnection;127;0;126;0
WireConnection;127;1;128;0
WireConnection;24;0;25;0
WireConnection;139;0;133;0
WireConnection;110;0;84;1
WireConnection;110;1;103;0
WireConnection;110;2;104;0
WireConnection;111;0;84;2
WireConnection;111;1;91;0
WireConnection;111;2;106;0
WireConnection;112;0;108;0
WireConnection;113;0;109;0
WireConnection;2;1;10;0
WireConnection;130;0;2;0
WireConnection;130;1;127;0
WireConnection;130;2;126;0
WireConnection;138;0;139;0
WireConnection;22;1;37;1
WireConnection;22;2;37;2
WireConnection;22;3;24;0
WireConnection;114;0;110;0
WireConnection;114;1;113;0
WireConnection;114;2;111;0
WireConnection;114;3;112;0
WireConnection;45;0;43;0
WireConnection;45;2;44;0
WireConnection;28;0;22;0
WireConnection;28;1;27;0
WireConnection;131;1;2;0
WireConnection;131;0;130;0
WireConnection;134;1;137;1
WireConnection;134;2;137;2
WireConnection;134;3;138;0
WireConnection;42;1;45;0
WireConnection;20;0;2;4
WireConnection;20;1;28;0
WireConnection;135;0;134;0
WireConnection;135;1;136;0
WireConnection;135;2;141;0
WireConnection;83;0;131;0
WireConnection;83;1;117;0
WireConnection;83;2;114;0
WireConnection;18;1;17;0
WireConnection;18;0;123;0
WireConnection;19;1;2;4
WireConnection;19;0;20;0
WireConnection;3;0;83;0
WireConnection;3;1;4;0
WireConnection;3;2;5;0
WireConnection;3;3;42;0
WireConnection;143;0;144;0
WireConnection;140;0;135;0
WireConnection;140;1;3;0
WireConnection;8;0;19;0
WireConnection;8;1;5;4
WireConnection;8;2;2;0
WireConnection;8;3;18;0
WireConnection;8;4;42;0
WireConnection;8;5;2;4
WireConnection;145;0;143;0
WireConnection;142;1;3;0
WireConnection;142;0;140;0
WireConnection;6;0;142;0
WireConnection;146;0;8;0
WireConnection;146;1;145;0
WireConnection;116;0;114;0
WireConnection;116;1;117;0
WireConnection;7;0;6;0
WireConnection;7;3;146;0
WireConnection;16;0;17;0
WireConnection;16;1;14;0
WireConnection;1;0;7;0
ASEEND*/
//CHKSM=41D80E4D3BD9F0871757BADC1BA235F695363CD4