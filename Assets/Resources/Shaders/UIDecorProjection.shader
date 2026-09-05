Shader "Hidden/Kait/Decor Projection"
{
    Properties { _MainTex("Silhouette",2D)="white" {} _Strength("Strength",Float)=1 _Softness("Softness",Float)=.007 }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            sampler2D _MainTex; float _Strength, _Softness;
            v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.uv=v.uv; return o; }
            float coverage(float2 uv) { return tex2D(_MainTex,uv).a * step(0,uv.x)*step(uv.x,1)*step(0,uv.y)*step(uv.y,1); }
            fixed4 frag(v2f i):SV_Target
            {
                float2 s=float2(_Softness,0);
                float a=coverage(i.uv)*.4;
                a+=(coverage(i.uv+s)+coverage(i.uv-s)+coverage(i.uv+s.yx)+coverage(i.uv-s.yx))*.15;
                return fixed4(0,0,0,a*_Strength);
            }
            ENDCG
        }
    }
}
