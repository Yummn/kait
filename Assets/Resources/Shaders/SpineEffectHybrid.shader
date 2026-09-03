// One Spine effect instance is shared by both visual halves. The shader only
// changes its sampling style across the global diagonal, so animation timing
// and hit placement can never diverge between the two art directions.
Shader "Spine/SkeletonGraphic Hybrid Effect"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        [Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput ("Straight Alpha Texture", Int) = 0
        [Toggle(_CANVAS_GROUP_COMPATIBLE)] _CanvasGroupCompatible ("CanvasGroup Compatible", Int) = 0
        _Color ("Tint", Color) = (1,1,1,1)
        _SplitBottom ("Bottom Split", Range(0,1)) = 0.447
        _SplitTop ("Top Split", Range(0,1)) = 0.563
        _PixelSize ("Pixel Sample Size", Range(1,8)) = 3

        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.StencilOp)] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Fog { Mode Off }
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Normal"

            CGPROGRAM
            #pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
            #pragma shader_feature _ _CANVAS_GROUP_COMPATIBLE
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct VertexInput
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                half2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float _SplitBottom;
            float _SplitTop;
            float _PixelSize;

            VertexOutput vert(VertexInput input)
            {
                VertexOutput output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.worldPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.screenPosition = ComputeScreenPos(output.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * float4(_Color.rgb * _Color.a, _Color.a);
                return output;
            }

            fixed4 frag(VertexOutput input) : SV_Target
            {
                float2 screenUv = input.screenPosition.xy / max(input.screenPosition.w, 0.0001);
                float splitX = lerp(_SplitBottom, _SplitTop, saturate(screenUv.y));
                float2 uv = input.texcoord;
                if (screenUv.x < splitX)
                {
                    float2 pixelStep = max(_MainTex_TexelSize.xy * max(_PixelSize, 1), float2(0.00001, 0.00001));
                    uv = (floor(uv / pixelStep) + 0.5) * pixelStep;
                }

                half4 texColor = tex2D(_MainTex, uv);
                #if defined(_STRAIGHT_ALPHA_INPUT)
                texColor.rgb *= texColor.a;
                #endif

                half4 color = (texColor + _TextureSampleAdd) * input.color;
                if (screenUv.x < splitX)
                    color.rgb = floor(color.rgb * 12.0 + 0.5) / 12.0;

                #ifdef _CANVAS_GROUP_COMPATIBLE
                color.rgb *= input.color.a;
                #endif

                color *= UnityGet2DClipping(input.worldPosition.xy, _ClipRect);
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
