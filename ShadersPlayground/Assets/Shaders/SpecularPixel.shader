Shader "Custom/SpecularPixel" {
    Properties {
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _SpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _Shininess ("Shininess", Float) = 10
    }
    
    SubShader {
        Tags { "Lightmode" = "ForwardBase" }
         
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            uniform float4 _Color;
            uniform float4 _SpecColor;
            uniform float _Shininess;
            
            // Unity variables
	        uniform float4 _LightColor0;
            
            struct vertexInput {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
            };
            
            struct vertexOutput {
                float4 pos: SV_POSITION;
                float4 posWorld: TEXCOORD0;
                float3 normalDir: TEXCOORD1;
            };
            
            vertexOutput vert(vertexInput v) {
                vertexOutput o;
                o.posWorld = mul(v.vertex, unity_ObjectToWorld);
                o.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            };
            
            float4 frag(vertexOutput output) : COLOR {
                // vectors
                float3 normalDirection = output.normalDir;
                float3 viewDirection = normalize(_WorldSpaceCameraPos - output.posWorld.xyz);
                float attenuation = 1.0;
                
                // lighting
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float maxDotNormalLight = max(0.0, dot(normalDirection, lightDirection));
                float3 diffuseReflection = attenuation * _LightColor0.xyz * maxDotNormalLight;
                float3 specularReflection = attenuation * _SpecColor.rgb * maxDotNormalLight * pow(max(0.0,dot( reflect( -lightDirection, normalDirection), viewDirection)), _Shininess);
                float3 lightFinal = diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;
                
                return float4(lightFinal * _Color.rgb, 1.0);
            };
            
            ENDCG    
        }
    }

	// TODO fallback commented out during development
	// Fallback "Diffuse"
}
