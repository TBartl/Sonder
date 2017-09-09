// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "ToonLit/Toon-VertexLit-View" {
	Properties {
		_ColoringTex ("Color Sheet", 2D) = "white" {}
		_RampTex("Ramp Texture", 2D) = "white" {}
		_ViewInfluence("View Influence", Range(0.0, 3.0)) = .5
		_BakedInfluence("Baked Influence", Range(0.0, 3.0)) = .5
	}
	SubShader {
		//Tags { "RenderType"="Opaque" }
		//LOD 200
		Cull Off
		Pass {
		
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			//#pragma surface surf Standard fullforwardshadows
			#pragma vertex vert
			#pragma fragment frag
      		#include "UnityCG.cginc"

			// Use shader model 3.0 target, to get nicer looking lighting
			//#pragma target 3.0

			sampler2D _ColoringTex;
			float4 _ColoringTex_ST;
			sampler2D _RampTex;
			float _ViewInfluence;
			float _BakedInfluence;

			struct vertInput {
				float4 pos : POSITION;
				float4 normal : NORMAL;
				float4 color : COLOR;
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
				//float2 uv_ColoringTex;
			};
			
			struct vertOutput{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
				float3 viewT : TEXCOORD1; //you don't need these semantics except for XBox360
				float4 normal : NORMAL;
				float2 texcoord : TEXCOORD0;
			};
			
			vertOutput vert(vertInput input) {
				vertOutput o;
				o.pos = UnityObjectToClipPos(input.pos);
				//o.color.xyz = input.normal * 0.5 + 0.5;
          		//o.color.w = 1.0;
				o.color = input.color;
				o.viewT = normalize(ObjSpaceViewDir(input.vertex));
				o.normal = normalize(input.normal); //Redundant?
				o.texcoord = TRANSFORM_TEX(input.texcoord, _ColoringTex);
				return o;
			}
			
			
			half4 frag(vertOutput output) : COLOR {
				
				
				//half d = dot (s.Normal, lightDir)*0.5 + 0.5;
				//half d = output.color.r * output.color.r;
				half d =  clamp(dot(output.normal, output.viewT) * _ViewInfluence + output.color.r * _BakedInfluence, 0, 1);
				half3 ramp = tex2D (_RampTex, float2(d,d)).rgb;
				half4 c;
				c.rgb = ramp * tex2D(_ColoringTex, output.texcoord);
				c.a = 1;
				
				
				return c;
				
			
				//return output.color; 
				
				
			
				//half4 c;
				//c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten);
			}
						
			ENDCG
		}
	} 
	FallBack "Diffuse"
}
