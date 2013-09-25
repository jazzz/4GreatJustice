Shader "Custom/DepthGradient" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"
			
				struct v2f {
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
					float2 uv2 : TEXCOORD1;
				};
			
			
				v2f vert(appdata_base v) {
					v2f o;
					o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
					o.uv = v.texcoord;
					o.uv2.x = 1-o.uv.x;
					
					o.uv2.y = 1-o.uv.y;
					return o;
				}
			
				sampler2D _MainTex;
				sampler2D _CurrentDepth;
				sampler2D _BackgroundDepth;
			
				float getDepth(float4 depth) {
					return dot(depth, float4(255,256*255,0,0))/(4096*16);
				}
				float4 frag(v2f IN) : COLOR {
					float d =  getDepth(tex2D (_CurrentDepth, IN.uv2));
					float bg = getDepth(tex2D (_BackgroundDepth, IN.uv2));
					float4 main = tex2D(_MainTex, IN.uv);
					float x = d < (bg - 0.01) ? 0 : 1;
					float4 col = float4(d,1-d,0,1);
					float4 inv = float4(float3(1,1,1) - main.xyz, main.w);	
					return lerp(float4(d,1-d,0,1), main, x);
				}
			ENDCG
		}
	}
}