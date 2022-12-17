Shader "Custom/TestShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
// #pragma exclude_renderers d3d11 gles
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int maxColorCount = 8;
        int baseColorCount;
        float baseStartHeights[maxColorCount];
        float3 shaderColors[maxColorCount];
        float terrainMinHeight;
        float terrainMaxHeight;

        struct Input
        {
            float3 worldPos;
        };

        float inverseLerp(float a, float b, float value)
        {
            //Clamp between 0 and 1
            return saturate((value-a)/(b-a));
        }
        

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(terrainMinHeight, terrainMaxHeight, IN.worldPos.y);
            for(int i = 0; i<= baseColorCount; ++i)
            {
                float drawStrength = saturate(sign(heightPercent - baseStartHeights[i]));
                o.Albedo = o.Albedo * (1-drawStrength + shaderColors[i] * drawStrength);
                
            }
            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
