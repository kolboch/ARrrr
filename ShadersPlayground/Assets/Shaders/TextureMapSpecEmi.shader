Shader "Custom/TextMapSpecEmis" {
	Properties {
	    _Color ("Color tint", Color) = (1.0, 1.0, 1.0, 1.0)
	    _MainTex ("Diffuse Texture, gloss(A)", 2D) = "white" {}
	    _BumpMap ("Normal Texture", 2D) = "bump" {}
	    _EmitMap ("Emission texture", 2D) = "black" {}
	    _BumpDepth ("Bump depth", Range(-2.0, 2.0)) = 1.0
	    _SpecColor ("Specular Color", Color) = (1.0, 1.0, 1.0, 1.0)
	    _Shininess ("Shininess", Float) = 10
	    _RimColor ("Rim color", Color) = (1.0, 1.0, 1.0, 1.0)
	    _RimPower ("Rim power", Range(0.1, 10.0)) = 3.0
	    _EmitStrength ("Emission strength", Range(0.0, 2.0)) = 0 
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
	        uniform sampler2D _BumpMap;
	        uniform float4 _BumpMap_ST;
	        uniform float _BumpDepth;
	        uniform sampler2D _EmitMap;
	        uniform float4 _EmitMap_ST;
	        uniform float _EmitStrength;
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
	            float4 tangent: TANGENT;
	        };
	        
	        struct vertexOutput {
	            float4 pos: SV_POSITION;
	            float4 tex: TEXCOORD0;
	            float4 posWorld: TEXCOORD1;
	            float3 normalWorld: TEXCOORD2;
	            float3 tangentWorld: TEXCOORD3;
	            float3 binormalWorld: TEXCOORD4;
	        };
	        
	        vertexOutput vert(vertexInput v) {
	            vertexOutput o;
	            
	            o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
	            o.tangentWorld = normalize(mul(unity_ObjectToWorld, v.tangent).xyz);
	            o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w);
	            
	            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
	            o.pos = UnityObjectToClipPos(v.vertex);
	            o.tex = v.texcoord;
	            return o;
	        };
	        
	        float4 frag(vertexOutput output): COLOR {
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
	            
	            // texture maps
	            float4 tex = tex2D(_MainTex, output.tex.xy * _MainTex_ST.xy + _MainTex_ST.zw);
	            float4 texMap = tex2D(_BumpMap, output.tex.xy * _BumpMap_ST.xy + _BumpMap_ST.zw);
	            float4 texEmission = tex2D(_EmitMap, output.tex.xy * _EmitMap_ST.xy + _EmitMap_ST.zw);
	            
	            //unpack normal function
	            float3 localCoords = float3(2.0 * texMap.ga - float2(1.0, 1.0), 0.0); // ga is were data is, rb is empty for benefits of compression
	            localCoords.z = _BumpDepth;
	            
	            // normal transpose matrix
	            float3x3 localToWorldTranspose = float3x3(
	                output.tangentWorld,
	                output.binormalWorld,
	                output.normalWorld
	            );
	            
	            // calculate normal direction
	            float3 normalDirection = normalize(mul(localCoords, localToWorldTranspose));
	            
	            // lighting
	            float satDotNormalAndLight = saturate(dot(normalDirection, lightDirection));
	            float3 diffuseReflection = attenuation * _LightColor0.rgb * satDotNormalAndLight;
	            float3 specularReflection = diffuseReflection * _SpecColor.rgb * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
	            
	            float rim = 1 - saturate(dot(normalDirection, viewDirection));
	            float3 rimLighting = satDotNormalAndLight * _RimColor.rgb * _LightColor0.rgb * pow(rim, _RimPower);
	            
	            float3 lightFinal = diffuseReflection + (specularReflection * tex.a) + rimLighting + UNITY_LIGHTMODEL_AMBIENT.rgb + texEmission.xyz * _EmitStrength;
	            
	            return float4(tex.xyz * lightFinal * _Color.xyz, 1.0);
	        };
	        
	        ENDCG
	    }
	    
	    Pass {
	        Tags { "LightMode" = "ForwardAdd" }
	        Blend One One
	        CGPROGRAM
	        #pragma vertex vert
	        #pragma fragment frag
	        
	        // user variables
	        uniform float4 _Color;
	        uniform sampler2D _MainTex;
	        uniform float4 _MainTex_ST;
	        uniform sampler2D _BumpMap;
	        uniform float4 _BumpMap_ST;
	        uniform float _BumpDepth;
	        uniform sampler2D _EmitMap;
	        uniform float4 _EmitMap_ST;
	        uniform float _EmitStrength;
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
	            float4 tangent: TANGENT;
	        };
	        
	        struct vertexOutput {
	            float4 pos: SV_POSITION;
	            float4 tex: TEXCOORD0;
	            float4 posWorld: TEXCOORD1;
	            float3 normalWorld: TEXCOORD2;
	            float3 tangentWorld: TEXCOORD3;
	            float3 binormalWorld: TEXCOORD4;
	        };
	        
	        vertexOutput vert(vertexInput v) {
	            vertexOutput o;
	            
	            o.normalWorld = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject).xyz);
	            o.tangentWorld = normalize(mul(unity_ObjectToWorld, v.tangent).xyz);
	            o.binormalWorld = normalize(cross(o.normalWorld, o.tangentWorld) * v.tangent.w);
	            
	            o.posWorld = mul(unity_ObjectToWorld, v.vertex);
	            o.pos = UnityObjectToClipPos(v.vertex);
	            o.tex = v.texcoord;
	            return o;
	        };
	        
	        float4 frag(vertexOutput output): COLOR {
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
	            
	            // texture maps
	            float4 texMap = tex2D(_BumpMap, output.tex.xy * _BumpMap_ST.xy + _BumpMap_ST.zw);
	            float4 texEmission = tex2D(_EmitMap, output.tex.xy * _EmitMap_ST.xy + _EmitMap_ST.zw);
	            
	            //unpack normal function
	            float3 localCoords = float3(2.0 * texMap.ga - float2(1.0, 1.0), 0.0); // ga is were data is, rb is empty for benefits of compression
	            localCoords.z = _BumpDepth;
	            
	            // normal transpose matrix
	            float3x3 localToWorldTranspose = float3x3(
	                output.tangentWorld,
	                output.binormalWorld,
	                output.normalWorld
	            );
	            
	            // calculate normal direction
	            float3 normalDirection = normalize(mul(localCoords, localToWorldTranspose));
	            
	            // lighting
	            float satDotNormalAndLight = saturate(dot(normalDirection, lightDirection));
	            float3 diffuseReflection = attenuation * _LightColor0.rgb * satDotNormalAndLight;
	            float3 specularReflection = diffuseReflection * _SpecColor.rgb * pow(saturate(dot(reflect(-lightDirection, normalDirection), viewDirection)), _Shininess);
	            
	            float rim = 1 - saturate(dot(normalDirection, viewDirection));
	            float3 rimLighting = satDotNormalAndLight * _RimColor.rgb * _LightColor0.rgb * pow(rim, _RimPower);
	            
	            float3 lightFinal = diffuseReflection + (specularReflection * output.tex.a) + rimLighting;
	            
	            return float4(lightFinal * _Color.xyz, 1.0);
	        };
	        
	        ENDCG
	    }
	}
	
	// TODO fallback commented out during development
	// Fallback "Diffuse"
}
