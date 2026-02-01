Shader "Custom/RotatingUVSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        [PerRendererData] _Color ("Tint", Color) = (1,1,1,1)
        _Rotation ("Rotation", Float) = 0
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Blend One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            float4 _Color;
            float _Rotation;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                
                // Center UVs at origin, rotate, then move back
                float2 centeredUV = v.uv - 0.5;
                
                float sinRot = sin(_Rotation);
                float cosRot = cos(_Rotation);
                
                float2 rotatedUV;
                rotatedUV.x = centeredUV.x * cosRot - centeredUV.y * sinRot;
                rotatedUV.y = centeredUV.x * sinRot + centeredUV.y * cosRot;
                
                o.uv = rotatedUV + 0.5;
                // Vertex color from SpriteRenderer * material _Color
                o.color = v.color * _Color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 col = texColor * i.color;
                // Premultiply alpha for correct blending
                col.rgb *= col.a;
                return col;
            }
            ENDHLSL
        }
    }
}
