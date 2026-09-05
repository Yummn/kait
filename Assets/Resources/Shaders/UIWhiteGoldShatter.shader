Shader "Kait/UI White Gold Shatter"
{
    Properties { [PerRendererData] _MainTex("Shatter Atlas", 2D) = "black" {} }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="False" }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            sampler2D _MainTex;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; o.color=v.color; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);
                float3 authored=c.rgb;
                #ifndef UNITY_COLORSPACE_GAMMA
                authored=LinearToGammaSpace(authored);
                #endif
                // Black-matte material: preserve the approved PNG without baking edits.
                float coverage=smoothstep(0.025,0.12,max(authored.r,max(authored.g,authored.b)));
                c.a*=coverage*i.color.a;
                c.rgb*=i.color.rgb;
                return c;
            }
            ENDCG
        }
    }
}
