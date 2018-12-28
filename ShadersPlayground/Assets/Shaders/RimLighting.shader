Shader "Custom/RimLighting" {
    
    Properties {
        _Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
        _SpecColor ("Specular color", Color) = (1.0, 1.0, 1.0, 1.0)
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
            
            uniform float4 _Color;
            uniform float4 _SpecColor;
            uniform float _Shininess;
            uniform float4 _RimColor;
            uniform float _RimPower;
            
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
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            };
            
            float4 frag(vertexOutput output): COLOR {
                float3 normalDirection = output.normalDir;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - output.posWorld.xyz);
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float attenuation = 1.0;
                
                float satDotNormalAndLight = saturate(dot(normalDirection, lightDirection));
                float3 diffuseReflection = attenuation * _LightColor0.xyz * satDotNormalAndLight;
                float3 specularReflection = attenuation * _SpecColor.rgb * satDotNormalAndLight * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
                
                // rim lighting 
                float rim = 1 - saturate(dot(viewDirection, normalDirection));
                float3 rimLighting = attenuation * _LightColor0.xyz * _RimColor * saturate(dot(normalDirection, lightDirection)) * pow(rim, _RimPower);
                float3 lightFinal = rimLighting + diffuseReflection + specularReflection + UNITY_LIGHTMODEL_AMBIENT.xyz; 
                return float4(lightFinal * _Color.xyz, 1.0);
            };
            
            ENDCG
        }
    }
    
    // TODO fallback commented out during development
	// Fallback "Diffuse"

}