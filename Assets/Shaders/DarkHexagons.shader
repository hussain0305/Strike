Shader "Abyss/DarkHexagons"
{
    Properties
    {
        _MainTex ("iChannel0", 2D) = "white" {}
        _SecondTex ("iChannel1", 2D) = "white" {}
        _ThirdTex ("iChannel2", 2D) = "white" {}
        _FourthTex ("iChannel3", 2D) = "white" {}
        _Mouse ("Mouse", Vector) = (0.5, 0.5, 0.5, 0.5)
        [ToggleUI] _GammaCorrect ("Gamma Correction", Float) = 1
        _Resolution ("Resolution (Change if AA is bad)", Range(1, 1024)) = 1
        _Scale ("Pattern Scale", Float) = 1
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            // Built-in properties
            sampler2D _MainTex;   float4 _MainTex_TexelSize;
            sampler2D _SecondTex; float4 _SecondTex_TexelSize;
            sampler2D _ThirdTex;  float4 _ThirdTex_TexelSize;
            sampler2D _FourthTex; float4 _FourthTex_TexelSize;
            float4 _Mouse;
            float _GammaCorrect;
            float _Resolution;
            float _Scale;

            // GLSL Compatability macros
            #define glsl_mod(x,y) (((x)-(y)*floor((x)/(y))))
            #define texelFetch(ch, uv, lod) tex2Dlod(ch, float4((uv).xy * ch##_TexelSize.xy + ch##_TexelSize.xy * 0.5, 0, lod))
            #define textureLod(ch, uv, lod) tex2Dlod(ch, float4(uv, 0, lod))
            #define iResolution float3(_Resolution, _Resolution, _Resolution)
            #define iFrame (floor(_Time.y / 60))
            #define iChannelTime float4(_Time.y, _Time.y, _Time.y, _Time.y)
            #define iDate float4(2020, 6, 18, 30)
            #define iSampleRate (44100)
            #define iChannelResolution float4x4(                      \
                _MainTex_TexelSize.z,   _MainTex_TexelSize.w,   0, 0, \
                _SecondTex_TexelSize.z, _SecondTex_TexelSize.w, 0, 0, \
                _ThirdTex_TexelSize.z,  _ThirdTex_TexelSize.w,  0, 0, \
                _FourthTex_TexelSize.z, _FourthTex_TexelSize.w, 0, 0)

            // Global access to uv data
            static v2f vertex_output;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv =  v.uv;
                return o;
            }

#define SHAPE 0
#define FAR 50.
            float2x2 r2(in float a)
            {
                float c = cos(a), s = sin(a);
                return transpose(float2x2(c, -s, s, c));
            }

            float hash21(float2 p)
            {
                float n = dot(p, float2(7.163, 157.247));
                return frac(sin(n)*43758.547);
            }

            float hash31(float3 p)
            {
                float n = dot(p, float3(13.163, 157.247, 7.951));
                return frac(sin(n)*43758.547);
            }

            float smax(float a, float b, float k)
            {
                float f = max(0., 1.-abs(b-a)/k);
                return max(a, b)+k*0.25*f*f;
            }

            float3 tex3D(sampler2D t, in float3 p, in float3 n)
            {
                n = max(abs(n)-0.2, 0.001);
                n /= length(n);
                float3 tx = tex2D(t, p.yz).xyz;
                float3 ty = tex2D(t, p.zx).xyz;
                float3 tz = tex2D(t, p.xy).xyz;
                return tx*tx*n.x+ty*ty*n.y+tz*tz*n.z;
            }

            float noise3D(in float3 p)
            {
                const float3 s = float3(113, 157, 1);
                float3 ip = floor(p);
                float4 h = float4(0., s.yz, s.y+s.z)+dot(ip, s);
                p -= ip;
                p = p*p*(3.-2.*p);
                h = lerp(frac(sin(h)*43758.547), frac(sin(h+s.x)*43758.547), p.x);
                h.xy = lerp(h.xz, h.yw, p.y);
                float n = lerp(h.x, h.y, p.z);
                return n;
            }

            float fbm(in float3 p)
            {
                return 0.5333*noise3D(p)+0.2667*noise3D(p*2.02)+0.1333*noise3D(p*4.03)+0.0667*noise3D(p*8.03);
            }

            float2 path(in float z)
            {
                return float2(sin(z*0.15)*2.4, 0);
            }

            static const float2 s = float2(0.866025, 1);
            float hex(in float2 p)
            {
                p = abs(p);
                return max(p.x*0.866025+p.y*0.5, p.y);
            }

            float hexPylon(float2 p2, float pz, float r, float ht)
            {
                float3 p = float3(p2.x, pz, p2.y);
                float3 b = float3(r, ht, r);
#if SHAPE == 0
                p.xz = abs(p.xz);
                p.xz = float2(p.x*0.866025+p.z*0.5, p.z);
                return length(max(abs(p)-b+0.015, 0.))-0.015;
#elif SHAPE == 1
                p.xz = abs(p.xz);
                p2 = p.xz*0.8660254+p.zx*0.5;
                p.xz = float2(max(p2.x, p2.y), max(p.z, p.x));
                return length(max(abs(p)-b+0.015, 0.))-0.015;
#else
                p.xy = abs(float2(length(p.xz), p.y))-b.xy+0.015;
                return min(max(p.x, p.y), 0.)+length(max(p.xy, 0.))-0.015;
#endif
            }

            static float4 litID;
            static float svLitID;
            float objDist(float2 p, float pH, float r, float ht, inout float id, float dir)
            {
                const float s = 1./16.;
                float h1 = hexPylon(p, pH, r, ht);
#ifdef ADD_DETAIL_GROOVE
                h1 = max(h1, -hexPylon(p, pH+ht, r-0.06, s/4.));
#endif
#ifdef ADD_DETAIL_BOLT
                h1 = min(h1, hexPylon(p, pH, 0.1, ht+s/4.));
#endif
                float h2 = hexPylon(p, pH+ht-s, r+0.01, s/3.);
                id = h1<h2 ? 0. : 1.;
                return min(h1, h2);
            }

            float hexHeight(float2 p)
            {
                return dot(sin(p*2.-cos(p.yx*1.4)), ((float2)0.25))+0.5;
            }

            float4 getHex(float2 p, float pH)
            {
                float4 hC = floor(float4(p, p-float2(0, 0.5))/s.xyxy)+float4(0, 0, 0, 0.5);
                float4 hC2 = floor(float4(p-float2(0.5, 0.25), p-float2(0.5, 0.75))/s.xyxy)+float4(0.5, 0.25, 0.5, 0.75);
                float4 h = float4(p-(hC.xy+0.5)*s, p-(hC.zw+0.5)*s);
                float4 h2 = float4(p-(hC2.xy+0.5)*s, p-(hC2.zw+0.5)*s);
                float4 ht = float4(hexHeight(hC.xy), hexHeight(hC.zw), hexHeight(hC2.xy), hexHeight(hC2.zw));
                ht = floor(ht*4.99)/4./2.+0.02;
                const float r = 0.25;
                float4 obj = float4(objDist(h.xy, pH, r, ht.x, litID.x, 1.), objDist(h.zw, pH, r, ht.y, litID.y, -1.), objDist(h2.xy, pH, r, ht.z, litID.z, -1.), objDist(h2.zw, pH, r, ht.w, litID.w, 1.));
                h = obj.x<obj.y ? float4(h.xy, hC.xy) : float4(h.zw, hC.zw);
                h2 = obj.z<obj.w ? float4(h2.xy, hC2.xy) : float4(h2.zw, hC2.zw);
                float2 oH = obj.x<obj.y ? float2(obj.x, litID.x) : float2(obj.y, litID.y);
                float2 oH2 = obj.z<obj.w ? float2(obj.z, litID.z) : float2(obj.w, litID.w);
                return oH.x<oH2.x ? float4(oH, h.zw) : float4(oH2, h2.zw);
            }

            static float2 v2Rnd, svV2Rnd;
            static float gLitID;
            float heightMap(in float3 p)
            {
                const float sc = 1.;
                float4 h = getHex(p.xz*sc, -p.y*sc);
                v2Rnd = h.zw;
                gLitID = h.y;
                return h.x/sc;
            }

            float map(float3 p)
            {
                float c = heightMap(p);
                return c*0.7;
            }

            static float3 glow;
            float getRndID(float2 p)
            {
#ifdef ANIMATE_LIGHTS
                float rnd = hash21(p);
                return smoothstep(0.5, 0.875, sin(rnd*6.283+_Time.y));
#else
                return hash21(p)-0.75;
#endif
            }

            float trace(float3 ro, float3 rd)
            {
                float t = hash31(ro+rd)*0.25, d, ad;
                glow = ((float3)0);
                for (int i = 0;i<80; i++)
                {
                    d = map(ro+rd*t);
                    ad = abs(d);
                    if (ad<0.001*(t*0.125+1.)||t>FAR)
                        break;
                        
                    const float gd = 0.1;
                    float rnd = getRndID(v2Rnd);
                    if (rnd>0.&&gLitID==1.&&ad<gd)
                    {
                        float gl = 0.2*(gd-ad)/gd/(1.+ad*ad/gd/gd*8.);
                        glow += gl;
                    }
                    
                    t += d;
                }
                return min(t, FAR);
            }

            float softShadow(float3 ro, float3 lp, float k)
            {
                const int maxIterationsShad = 32;
                float3 rd = lp-ro;
                float shade = 1.;
                float dist = 0.01;
                float end = max(length(rd), 0.001);
                float stepDist = end/float(maxIterationsShad);
                rd /= end;
                for (int i = 0;i<maxIterationsShad; i++)
                {
                    float h = map(ro+rd*dist);
                    shade = min(shade, smoothstep(0., 1., k*h/dist));
                    dist += clamp(h, 0.02, 0.25);
                    if (h<0.||dist>end)
                        break;
                        
                }
                return min(max(shade, 0.)+0.05, 1.);
            }

            float3 getNormal(in float3 p)
            {
                const float2 e = float2(0.0025, 0);
                return normalize(float3(map(p+e.xyy)-map(p-e.xyy), map(p+e.yxy)-map(p-e.yxy), map(p+e.yyx)-map(p-e.yyx)));
            }

            float calcAO(in float3 p, in float3 n)
            {
                float sca = 4., occ = 0.;
                for (int i = 1;i<6; i++)
                {
                    float hr = float(i)*0.125/5.;
                    float dd = map(p+hr*n);
                    occ += (hr-dd)*sca;
                    sca *= 0.75;
                }
                return clamp(1.-occ, 0., 1.);
            }

            float3 texBump(sampler2D tx, in float3 p, in float3 n, float bf)
            {
                const float2 e = float2(0.001, 0);
                float3x3 m = transpose(float3x3(tex3D(tx, p-e.xyy, n), tex3D(tx, p-e.yxy, n), tex3D(tx, p-e.yyx, n)));
                float3 g = mul(float3(0.299, 0.587, 0.114),m);
                g = (g-dot(tex3D(tx, p, n), float3(0.299, 0.587, 0.114)))/e.x;
                g -= n*dot(n, g);
                return normalize(n+g*bf);
            }

            float3 envMap(float3 p)
            {
                p *= 3.;
                float n3D2 = noise3D(p*3.);
                float c = noise3D(p)*0.57+noise3D(p*2.)*0.28+noise3D(p*4.)*0.15;
                c = smoothstep(0.25, 1., c);
                p = float3(c, c*c, c*c*c);
                return lerp(p, p.zyx, n3D2*0.25+0.75);
            }

            float3 getObjectColor(float3 p, float3 n)
            {
                float sz0 = 1./2.;
                float3 txP = p;
                float3 col = tex3D(_MainTex, txP*sz0, n);
                col = smoothstep(-0., 0.5, col);
                col = lerp(col, ((float3)1)*dot(col, float3(0.299, 0.587, 0.114)), 0.5);
                col /= 16.;
                float rnd = getRndID(svV2Rnd);
                float oGlow = 0.;
                if (rnd>0.&&svLitID==1.)
                {
                    float ht = hexHeight(svV2Rnd);
                    ht = floor(ht*4.99)/4./2.+0.02;
                    const float s = 1./4./2.*0.5;
                    oGlow = lerp(1., 0., clamp(abs(p.y-(ht-s))/s*3.*1., 0., 1.));
                    oGlow = smoothstep(0., 1., oGlow*1.);
                }
                
                glow = lerp(glow, ((float3)oGlow), 0.75);
                glow = pow(float3(1.5, 1, 1)*glow, float3(1, 3, 6));
                glow = lerp(glow, glow.xzy, dot(sin(p*4.-cos(p.yzx*4.)), ((float3)0.166))+0.5);
                glow = lerp(glow, glow.zyx, dot(cos(p*2.-sin(p.yzx*2.)), ((float3)0.166))+0.5);
#ifdef GREEN_GLOW
                glow = glow.yxz;
#endif
                return col;
            }

            float3 doColor(in float3 sp, in float3 rd, in float3 sn, in float3 lp, in float t)
            {
                float3 sceneCol = ((float3)0);
                if (t<FAR)
                {
                    float sz0 = 1./1.;
                    float3 txP = sp;
                    sn = texBump(_MainTex, txP*sz0, sn, 0.005);
                    float sh = softShadow(sp, lp, 12.);
                    float ao = calcAO(sp, sn);
                    sh = min(sh+ao*0.3, 1.);
                    float3 ld = lp-sp;
                    float lDist = max(length(ld), 0.001);
                    ld /= lDist;
                    float atten = 1.5/(1.+lDist*0.1+lDist*lDist*0.02);
                    float diff = max(dot(sn, ld), 0.);
                    float spec = pow(max(dot(reflect(-ld, sn), -rd), 0.), 32.);
                    float fres = clamp(1.+dot(rd, sn), 0., 1.);
                    float3 objCol = getObjectColor(sp, sn);
                    sceneCol = objCol*(diff+float3(1, 0.6, 0.3)*spec*4.+0.5*ao+float3(0.3, 0.5, 1)*fres*fres*2.);
                    sceneCol += pow(sceneCol, ((float3)1.))*envMap(reflect(rd, sn))*4.;
                    sceneCol *= atten*sh*ao;
                    sceneCol += (objCol*6.+1.)*glow;
                }
                
                return sceneCol;
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                float2 uv = (fragCoord-iResolution.xy*0.5)/iResolution.y;
                uv /= _Scale;
                float3 lk = float3(0, 1.25, _Time.y*1.);
                float3 ro = lk+float3(0, 0.175, -0.25);
                float3 lp = ro+float3(0, 1, 4);
                lk.xy += path(lk.z);
                ro.xy += path(ro.z);
                lp.xy += path(lp.z);
                float FOV = 3.14159/3.;
                float3 forward = normalize(lk-ro);
                float3 right = normalize(float3(forward.z, 0., -forward.x));
                float3 up = cross(forward, right);
                float3 rd = normalize(forward+FOV*uv.x*right+FOV*uv.y*up);
                float3 sceneColor, passColor, sn, sSn;
                float t = trace(ro, rd);
                svV2Rnd = v2Rnd;
                svLitID = gLitID;
                float fog = smoothstep(0., FAR-1., t);
                ro += rd*t;
                sn = getNormal(ro);
                passColor = doColor(ro, rd, sn, lp, t);
                sceneColor = passColor;
                sceneColor = lerp(sceneColor, ((float3)0), fog);
                uv = fragCoord/iResolution.xy;
                sceneColor = min(sceneColor, 1.)*pow(16.*uv.x*uv.y*(1.-uv.x)*(1.-uv.y), 0.0625);
                fragColor = float4(sqrt(clamp(sceneColor, 0., 1.)), 1.);
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            ENDCG
        }
    }
}

