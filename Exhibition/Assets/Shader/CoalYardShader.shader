Shader "Custom/CoalYardShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Level("Level",int) = 5
		_Height("Height",float) = 2.0
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.1
		_Emission("Emission", Color) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
			float3 worldPos;
			float3 worldNormal;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		fixed4 _Emission;
		int _Level;
		half _Height;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {            
			fixed interval = 1.0 / _Level;

			fixed2 uv = fixed2(0,0);

			for (int i = 0; i < _Level;i++) {
				if (IN.worldPos.y > i * _Height && IN.worldPos.y <=(i+1)*_Height){
					uv = fixed2(interval * (i + 0.5), IN.uv_MainTex.y);
					break;
				}
			}

			fixed4 c = tex2D(_MainTex, uv) * _Color;

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
			o.Emission = _Emission.rgb * _Emission.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
