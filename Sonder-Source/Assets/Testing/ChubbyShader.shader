Shader "Custom/ChubbyShader" {
	Properties {
		_ColorTex ("Color Texture", 2D) = "white" {}
		_Chubbiness ("Chubbiness", Range(-1,1)) = 0.005
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Lambert vertex:vert	

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _ColorTex;
		half _Chubbiness;

		struct Input {
			float2 uv_ColorTex;
			float4 vertColor : COLOR;
		};
		
		void vert (inout appdata_full v) {
			v.vertex.xyz += v.normal * _Chubbiness * v.color * v.color  * v.color;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_ColorTex, IN.uv_ColorTex);
			//fixed4 c = fixed4(.5, 1, .2, 1);
			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
