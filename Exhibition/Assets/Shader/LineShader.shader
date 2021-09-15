Shader "Custom/LineShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
	   Tags {
			//渲染队列-通常这个索引用来渲染透明度混合的物体
			"Queue" = "Transparent"
			//Projector为投影器，这样设置将会使该物体忽略任何投影类型的材质或贴图的影响
			"IgnoreProjector" = "True"
			//渲染透明物体时使用
			"RenderType" = "Transparent"
			//预览-平面
			"PreviewType" = "Plane"
		}
		//关闭光照 剔除关闭(正背面全部显示) 深度测试开启 深度写入关闭
		//深度测试为当这个物体比深度缓冲中的像素靠近摄像机时显示，否则不显示
		Lighting Off
		Cull Off
		ZTest Always
		ZWrite Off
		//以这个物体的a值为标准，设置颜色缓冲区中的颜色为1-这个物体的a值
		Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
