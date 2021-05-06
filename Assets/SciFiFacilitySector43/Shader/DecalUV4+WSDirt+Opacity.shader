
Shader "Triplebrick/DecalDirtOpacity"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		_MainColor("Main Color", Color) = (1,1,1,0)
		_TintColor("Tint Color", Color) = (1,1,1,0)
		_Metallness("Metallness", Range( 0 , 1)) = 0
		_DetailColor("Detail Color", Color) = (0.5,0.5,0.5,0)
		_DetailMetallness("Detail Metallness", Range( 0 , 1)) = 0
		_DetailSmoothness("Detail Smoothness", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 2)) = 1
		_DirtStrength("Dirt Strength", Range( 0 , 1)) = 0
		_DirtScale("Dirt Scale", Range( 0.1 , 5)) = 1
		[NoScaleOffset]_NormalDecal("Normal Decal", 2D) = "bump" {}
		[NoScaleOffset]_DirtRTintGSmoothnessA("Dirt(R) Tint(G) Smoothness(A)", 2D) = "white" {}
		[NoScaleOffset]_DetailMaskROpacityGAOA("Detail Mask(R)+Opacity(G)+AO(A)", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_TEXTURE_PARAMS(textureName) textureName

		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _NormalDecal;
		uniform float4 _MainColor;
		uniform float4 _TintColor;
		uniform sampler2D _DirtRTintGSmoothnessA;
		uniform float _DirtScale;
		uniform float4 _DetailColor;
		uniform sampler2D _DetailMaskROpacityGAOA;
		uniform float _DirtStrength;
		uniform float _Metallness;
		uniform float _DetailMetallness;
		uniform float _Smoothness;
		uniform float _DetailSmoothness;
		uniform float _Cutoff = 0.5;


		inline float4 TriplanarSamplingSF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalDecal6 = i.uv_texcoord;
			float3 tex2DNode6 = UnpackNormal( tex2D( _NormalDecal, uv_NormalDecal6 ) );
			o.Normal = tex2DNode6;
			float2 appendResult13 = (float2(_DirtScale , _DirtScale));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar1 = TriplanarSamplingSF( _DirtRTintGSmoothnessA, ase_worldPos, ase_worldNormal, 1.0, appendResult13, 1.0, 0 );
			float4 lerpResult15 = lerp( _MainColor , _TintColor , triplanar1.y);
			float2 uv_DetailMaskROpacityGAOA20 = i.uv_texcoord;
			float4 tex2DNode20 = tex2D( _DetailMaskROpacityGAOA, uv_DetailMaskROpacityGAOA20 );
			float4 lerpResult29 = lerp( lerpResult15 , _DetailColor , tex2DNode20.r);
			float4 lerpResult9 = lerp( lerpResult29 , ( lerpResult15 * triplanar1.x ) , _DirtStrength);
			o.Albedo = lerpResult9.rgb;
			float lerpResult26 = lerp( _Metallness , _DetailMetallness , tex2DNode20.r);
			o.Metallic = lerpResult26;
			float lerpResult18 = lerp( 1.0 , triplanar1.x , _DirtStrength);
			float clampResult40 = clamp( ( ( lerpResult18 * triplanar1.w ) * _Smoothness ) , 0.0 , 0.98 );
			float lerpResult30 = lerp( clampResult40 , _DetailSmoothness , tex2DNode20.r);
			o.Smoothness = lerpResult30;
			o.Occlusion = tex2DNode20.a;
			o.Alpha = 0.5;
			clip( tex2DNode20.g - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
