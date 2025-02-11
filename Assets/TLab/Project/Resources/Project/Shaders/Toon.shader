Shader "TLab/Project/Lit/Toon"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        [HDR] _EdgeColor("Edge Color", Color) = (1,1,1,1)
        _Width("Width", Range(0, 0.1)) = .025
        _ZOffset("Z Offset", Range(-0.5, 0.5)) = .2
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend One OneMinusSrcAlpha
        ZWrite On
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            uniform float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex.xyz);
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half nl = max(0, dot(i.normal, _WorldSpaceLightPos0.xyz));
                half nt = nl;

                half t0 = nl < 1e-3;
                nt = 0.3 * t0 + nt * (1.0 - t0);
                half t1 = nt < 0.3;
                nt = 0.5 * t1 + nt * (1.0 - t1);
                half t2 = nt > 0.9;
                nt = 1.0 * t2 + nt * (1.0 - t2);
                half t3 = t0 || t1 || t2;
                nt = 1.0 * (1. - t3) + nt * t3;

                _Color = _Color * (1. - t2) + half4(1, 1, 1, 1) * t2;
                _Color.rgb *= nt;

                half4 col = _Color;
                return col;
            }
            ENDCG
        }

        GrabPass { }

        Pass
        {
            Name "OUTLINE"

            Cull Front

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform float4 _EdgeColor;
            uniform float _Width;
            uniform float _ZOffset;
            sampler2D _GrabTexture;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float3 normal : NORMAL;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float3 normal   : TEXCOORD1;
                float4 grabPos  : TEXCOORD2;
            };

            v2f vert(appdata v)
            {
                v2f o;

                float3 positionWS = mul(unity_ObjectToWorld, v.vertex);
                float3 zOffset = normalize(positionWS - _WorldSpaceCameraPos) * _ZOffset;
                o.pos = UnityWorldToClipPos(positionWS + zOffset);

                float3 norm = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.color.rgb));
                float3 offset = TransformViewToProjection(norm);

                o.pos.xyz += offset.xyz * _Width;
                o.uv = v.uv;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.grabPos = ComputeGrabScreenPos(o.pos);

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 layer0 = tex2Dproj(_GrabTexture, i.grabPos);
                fixed4 effect = _EdgeColor * _EdgeColor.a;
                layer0.a *= layer0.a;
                layer0.a *= (1. - effect.a);
                layer0.rgb *= layer0.a;
                fixed4 col = effect + layer0;
                col.a = 1.0;
                return col;
            }
            ENDCG
        }
    }
}