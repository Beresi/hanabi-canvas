Shader "HanabiCanvas/PixelGrid"
{
    Properties
    {
        _MainTex ("Canvas Texture", 2D) = "black" {}
        _BackgroundColor ("Background Color", Color) = (0.125, 0.125, 0.125, 1)
        _GridColor ("Grid Line Color", Color) = (0.3, 0.3, 0.3, 0.5)
        _GridLineWidth ("Grid Line Width", Range(0.001, 0.1)) = 0.02
        _GridSize ("Grid Size (XY)", Vector) = (32, 32, 0, 0)
        _HoverCell ("Hover Cell (XY, -1 = none)", Vector) = (-1, -1, 0, 0)
        _HoverColor ("Hover Color", Color) = (1, 1, 1, 0.3)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "PixelGrid"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BackgroundColor;
                float4 _GridColor;
                float _GridLineWidth;
                float4 _GridSize;
                float4 _HoverCell;
                float4 _HoverColor;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                // Sample canvas texture (point-filtered)
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // Blend: use background for empty cells (alpha == 0), texture color for filled
                half4 color = lerp(_BackgroundColor, half4(texColor.rgb, 1.0), texColor.a);
                color.a = 1.0;

                // Grid lines
                float2 gridUV = uv * _GridSize.xy;
                float2 gridFrac = frac(gridUV);
                float lineX = step(gridFrac.x, _GridLineWidth) + step(1.0 - _GridLineWidth, gridFrac.x);
                float lineY = step(gridFrac.y, _GridLineWidth) + step(1.0 - _GridLineWidth, gridFrac.y);
                float gridLine = saturate(lineX + lineY);
                color.rgb = lerp(color.rgb, _GridColor.rgb, gridLine * _GridColor.a);

                // Hover highlight
                float2 cellIndex = floor(gridUV);
                float isHoverActive = step(0.0, _HoverCell.x);
                float matchX = 1.0 - step(0.5, abs(cellIndex.x - _HoverCell.x));
                float matchY = 1.0 - step(0.5, abs(cellIndex.y - _HoverCell.y));
                float isHovered = isHoverActive * matchX * matchY;
                color.rgb = lerp(color.rgb, _HoverColor.rgb, isHovered * _HoverColor.a);

                return color;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
