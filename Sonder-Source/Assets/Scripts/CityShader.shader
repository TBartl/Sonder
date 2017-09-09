// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/CityShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _TintCancel ("TintCancel", Color) = (1,1,1,1)
        _TintCancelVal ("TintCancelVal", Range(0,1)) = 0.5
        _Min ("Min", Range(-1,1)) = 0.5
        _Constant ("Constant", Range(-1, 1)) = 0.5
        _Scale ("Scale", Range(-1, 1)) = 0.5
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
        //Cull Off //uncomment if we need both sides
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		
		#include "UnityCG.cginc"

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
            float3 vertexColor; // Vertex color stored here by vert() method
		};
		
		struct v2f {
           float4 pos : SV_POSITION;
           fixed4 color : COLOR;
           
         };
         
		half _Glossiness;
        half _Metallic;
        half _TintCancelVal;
        half _Min;
        half _Constant;
        half _Scale;
        fixed4 _Color;
        fixed4 _TintCancel;
        
        
        void vert (inout appdata_full v, out Input o)
         {
         	 v2f o2;
             o2.pos = UnityObjectToClipPos (v.vertex);
             
             UNITY_INITIALIZE_OUTPUT(Input,o);
             o.vertexColor = v.color * max(_Min,_Constant - _Scale * o2.pos.z); // Save the Vertex Color in the Input for the surf() method
         }

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			//o.Albedo = c.rgb;
            o.Albedo = c.rgb * (IN.vertexColor * _TintCancelVal + _TintCancel); // Combine normal color with the vertex color
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}

