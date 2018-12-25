Shader "Custom/Specular" {
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
                float4 col: COLOR;
            };
            
            vertexOutput vert(vertexInput v) {
                vertexOutput o;
                
                // vectors
                float3 normalDirection = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                float3 viewDirection = normalize(float3(float4(_WorldSpaceCameraPos.xyz, 1.0) - mul(v.vertex, unity_ObjectToWorld).xyz));
                float attenuation = 1.0;
                
                // lighting
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float maxDotNormalLight = max(0.0, dot(normalDirection, lightDirection));
                float3 diffuseReflection = attenuation * _LightColor0.xyz * maxDotNormalLight;
                float3 specularReflection = attenuation * _SpecColor.rgb * maxDotNormalLight * pow(max(0.0,dot( reflect( -lightDirection, normalDirection), viewDirection)), _Shininess);
                float3 lightFinal = diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz;
                
                o.col = float4(lightFinal * _Color.rgb, 1.0);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            };
            
            float4 frag(vertexOutput output) : COLOR {
                return output.col;
            };
            
            ENDCG    
        }
    }

	// TODO fallback commented out during development
	// Fallback "Diffuse"
}
