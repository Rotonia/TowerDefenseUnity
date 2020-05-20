Shader "Custom/Die"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _BlendTex ("Alpha Blended (RGBA)", 2D) = "white" {}
        _BlendColor ("Color", Color) = (1,1,1,1)
        _BlendCutout ("Alpha Blended (RGBA) 2", 2D) = "white" {}
        _BlendBumpMap ("Bumpmap", 2D) = "bump" {}
        _DissolveMap ("Dissolve Map", 2D) = "white" {}
		_DissolveAmount ("DissolveAmount", Range(0,1)) = 0
		_DissolveColor ("DissolveColor", Color) = (1,1,1,1)
		_DissolveEmission ("DissolveEmission", Range(0,1)) = 1
		_DissolveWidth ("DissolveWidth", Range(0,0.1)) = 0.05
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
        sampler2D _BlendTex;
        sampler2D _BlendCutout;
        sampler2D _BlendBumpMap;
        sampler2D _BumpMap;
		sampler2D _DissolveMap;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BlendTex;
            float2 uv_BlendCutout;
            float2 uv_BlendBumpMap;
             float2 uv_BumpMap;
			float2 uv_DissolveMap;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _BlendColor;
        half _DissolveAmount;
		half _DissolveEmission;
		half _DissolveWidth;
		fixed4 _DissolveColor;

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
            fixed4 c2 = tex2D (_BlendTex, IN.uv_BlendTex) * _BlendColor;
            fixed4 blend = tex2D(_BlendCutout, IN.uv_BlendCutout);   
            
            fixed4 mask = tex2D (_DissolveMap, IN.uv_DissolveMap);

			if(mask.r < _DissolveAmount)
				discard;
            
            c = lerp(c,c2, blend.a);  
            
            fixed4 n = lerp( tex2D (_BumpMap, IN.uv_BumpMap) , tex2D (_BlendBumpMap, IN.uv_BlendBumpMap), blend.a);
           
            o.Albedo = c.rgb;
            
            if(mask.r < _DissolveAmount + _DissolveWidth) {
				o.Albedo = _DissolveColor;
				o.Emission = _DissolveColor * _DissolveEmission;
			}
            
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Normal = UnpackNormal (n);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
