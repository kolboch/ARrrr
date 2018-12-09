Shader "Custom/ItemGlowColor" {
	Properties {
		_ColorTint("Color tint", Color) = (1, 1, 1, 1)
		_RimColor("Rim Color", Color) = (1, 1, 1, 1)
		_RimPower("Rim Strength", Range(1.0, 6.0)) = 3.0

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input {
			float4 color: Color;
			float3 viewDirection;
		};

		float4 _ColorTint;
		float4 _RimColor;
		float _RimPower;

		void surf (Input IN, inout SurfaceOutput o) {
			o.Albedo = _ColorTint;
			o.Emission = _RimColor * 0.5;
			// half rim = 1.0 - saturate(dot(normalize(IN.viewDirection), o.Normal));
			// o.Emission = _RimColor.rgb * pow(rim, _RimPower);
		}
		ENDCG
	}
	FallBack "Diffuse"
}

	


