Shader "Custom/TextureBasic" {
	Properties {
	    _Color ("Color tint", Color) = (1.0, 1.0, 1.0, 1.0)
	    _MainTex ("Diffuse texture", 2D) = "white" {}
	    _SpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
	    _Shininess ("Shininess", Float) = 10
	    _RimColor ("Rim color", Color) = (1.0, 1.0, 1.0, 1.0)
	    _RimPower ("Rim power", Range(0.1, 10.0)) = 3.0 
	}
	
	SubShader {
	    Pass {
	        Tags { "LightMode" = "ForwardBase" }
	        
	        CGPROGRAM
	        #pragma vertex vert
	        #pragma fragment frag
	        
	        // user variables
	        uniform float4 _Color;
	        uniform sampler2D _MainTex;
	        uniform float4 _MainTex_ST;
	        uniform float4 _SpecColor;
	        uniform float4 _RimColor;
	        uniform float _Shininess;
	        uniform float _RimPower;
	        
	        // unity defined variables
	        uniform float4 _LightColor0;
	        
	        struct vertexInput {
	            float4 vertex: POSITION;
	            float3 normal: NORMAL;
	            float4 texcoord: TEXCOORD0;
	        };
	        
	        struct vertexOutput {
	            float4 pos: SV_POSITION;
	            float4 tex: TEXCOORD0;
	            float4 posWorld: TEXCOORD1;
	            float3 normalDir: TEXCOORD2;
	        };
	        
	        vertexOutput vert(vertexInput v) {
	            vertexOutput o;
	            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
	            o.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
	            o.pos = UnityObjectToClipPos(v.vertex);
	            o.tex = v.texcoord;
	            return o;
	        };
	        
	        float4 frag(vertexOutput output): COLOR {
	            float3 normalDirection = output.normalDir;
	            float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - output.posWorld.xyz);
	            float3 lightDirection;
	            float attenuation;
	            if(_WorldSpaceLightPos0.w == 0.0) { // directional light
	                attenuation = 1.0;
	                lightDirection = normalize(_WorldSpaceLightPos0.xyz);
	            } else {
	                float3 fragmentToLightSource = _WorldSpaceLightPos0.xyz - output.posWorld.xyz;
	                float distance = length(fragmentToLightSource);
	                attenuation = 1.0 / distance;
	                lightDirection = normalize(fragmentToLightSource);
	            }
	            
	            float satDotNormalAndLight = saturate(dot(normalDirection, lightDirection));
	            float3 diffuseReflection = attenuation * _LightColor0.rgb * satDotNormalAndLight;
	            float3 specularReflection = diffuseReflection * _SpecColor.rgb * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
	            
	            float rim = 1 - saturate(dot(normalDirection, viewDirection));
	            float3 rimLighting = satDotNormalAndLight * _RimColor.rgb * _LightColor0.rgb * pow(rim, _RimPower);
	            
	            float3 lightFinal = diffuseReflection + specularReflection + rimLighting + UNITY_LIGHTMODEL_AMBIENT.rgb;
	            
	            // texture maps
	            float4 tex = tex2D(_MainTex, output.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);
	            
	            return float4(tex.xyz * lightFinal * _Color.xyz, 1.0);
	        };
	        
	        ENDCG
	    }
	}
	
	// TODO fallback commented out during development
	// Fallback "Diffuse"
}
