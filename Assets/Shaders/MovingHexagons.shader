Shader "Abyss/MovingHexagons"
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

            static const float PI = 3.14159;
            static const float SCALE = 1.;
            static const float MAX_DIST = 1000.;
            static const float FLOOR_HEIGHT = 0.;
            static const float X_REPEAT_DIST = 0.9*SCALE;
            static const float Z_REPEAT_DIST = 1.05*SCALE;
            static const float PRIM_HEIGHT = 1.;
            static const float HEX_HALF_WIDTH = 0.26*SCALE;
            static const float GEOMETRY_DISPLACEMENT = 1.;
            static float g_time;
            struct AnimationChannels 
            {
                float material_roughness;
                float geometry_width;
                float geometry_scale;
                float geometry_displacement;
                float geometry_smoothness;
                float3 camPos;
                float3 camLookAt;
            };
            static AnimationChannels g_animationChannels;
            static const int MATERIALID_NONE = 0;
            static const int MATERIALID_FLOOR = 1;
            static const int MATERIALID_SKY = 2;
            static const int MATERIALID_PLASTIC = 3;
            static const int MATERIALID_METAL = 4;
            static const int DEBUG_RAYLEN = 0;
            static const int DEBUG_GEODIST = 1;
            static const int DEBUG_NORMAL = 2;
            static const int DEBUG_MATID = 3;
            static float fDEBUG = 0.1;
#define saturate(x) clamp(x, 0., 1.)
            struct Cam 
            {
                float3 R;
                float3 U;
                float3 D;
                float3 o;
                float lens;
                float zoom;
            };
            Cam CAM_lookAt(float3 target, float pitchAngleRad, float dist, float theta);
            Cam CAM_mouseLookAt(float3 at, float dst);
            Cam CAM_animate(float2 uv, float fTime);
            float3 CAM_getRay(Cam cam, float2 uv);
            float3 POST_ProcessFX(float3 c, float2 uv);
            float RAYINTERSEC_plane(float3 o, float3 d, float3 po, float3 pn)
            {
                return dot(po-o, pn)/dot(d, pn);
            }

            struct repeatInfo 
            {
                float3 smpl;
                float3 anchor;
            };
#define normalized_wave(a) (0.5*a+0.5)
            repeatInfo DF_repeatHex(float3 p)
            {
                float xRepeatDist = X_REPEAT_DIST;
                float zRepeatDist = Z_REPEAT_DIST*0.5;
                float latticeX = (frac(p.x/xRepeatDist+0.5)-0.5)*xRepeatDist;
                float latticeY = (frac(p.z/zRepeatDist+0.5)-0.5)*zRepeatDist;
                float2 anchorPosXZ = p.xz-float2(latticeX, latticeY);
                p.x = latticeX;
                p.z = latticeY;
                float period = frac(g_time/30.)*3.;
                float theta = period*2.*PI;
                float overallAmplitude = normalized_wave(-cos(theta));
                float waveAmplitude = g_animationChannels.geometry_displacement*normalized_wave(sin(anchorPosXZ.x+anchorPosXZ.y+theta*4.));
                float primHeight = FLOOR_HEIGHT+overallAmplitude*waveAmplitude;
                repeatInfo outData;
                outData.anchor = float3(anchorPosXZ[0], primHeight, anchorPosXZ[1]);
                outData.smpl = p;
                return outData;
            }

#define zclamp(a) max(a, 0.)
            float DF_RoundedHex(float3 p, float width, float height)
            {
                float smoothRadius = g_animationChannels.geometry_smoothness*0.2;
                width -= smoothRadius*2.;
                p = abs(p);
                float da = p.x*0.866025+p.z*0.5-width;
                float db = p.x*0.866025-p.z*0.5-width;
                float dc = p.z-width;
                float3 d = zclamp(float3(da, db, dc));
                float dw = length(d)-smoothRadius;
                float dh = p.y-height;
                float externalDistance = length(zclamp(float2(dh, dw)))-smoothRadius;
                float internalDistance = max(max(da, dc), dh);
                return min(externalDistance, internalDistance);
            }

            struct DF_out 
            {
                float d;
                int matID;
                float3 objectPos;
            };
            DF_out DF_composition(in float3 pos)
            {
                DF_out oFloor;
                DF_out oHexA;
                DF_out oHexB;
                oHexA.matID = MATERIALID_PLASTIC;
                repeatInfo infoA = DF_repeatHex(pos-((float3)0));
                oHexA.objectPos = infoA.anchor;
                oHexA.d = DF_RoundedHex(infoA.smpl-float3(0, infoA.anchor.y, 0), g_animationChannels.geometry_width*HEX_HALF_WIDTH, PRIM_HEIGHT);
                oHexB.matID = MATERIALID_PLASTIC;
                repeatInfo infoB = DF_repeatHex(pos-float3(X_REPEAT_DIST*0.5, 0, Z_REPEAT_DIST*0.25));
                oHexB.objectPos = infoB.anchor;
                oHexB.d = DF_RoundedHex(infoB.smpl-float3(0, infoB.anchor.y, 0), g_animationChannels.geometry_width*HEX_HALF_WIDTH, PRIM_HEIGHT);
                if (oHexA.d<oHexB.d)
                return oHexA;
                else return oHexB;
            }

            float3 DF_gradient(in float3 p)
            {
                const float d = 0.001;
                float3 grad = float3(DF_composition(p+float3(d, 0, 0)).d-DF_composition(p-float3(d, 0, 0)).d, DF_composition(p+float3(0, d, 0)).d-DF_composition(p-float3(0, d, 0)).d, DF_composition(p+float3(0, 0, d)).d-DF_composition(p-float3(0, 0, d)).d);
                return grad/(2.*d);
            }

#define OVERSTEP_COMPENSATION 1
            float RAYMARCH_isosurface(float3 o, float3 d, float isoSurfaceValue)
            {
                const float tolerance = 0.0001;
                float t = 0.;
                float dist = MAX_DIST;
#if OVERSTEP_COMPENSATION
                for (int i = 0;i<30; i++)
                {
                    dist = DF_composition(o+d*t).d;
                    dist -= isoSurfaceValue;
                    if (abs(dist)<tolerance*100.)
                        break;
                        
                    t += dist;
                }
                t -= Z_REPEAT_DIST/2.;
                for (int i = 0;i<30; i++)
                {
                    dist = DF_composition(o+d*t).d;
                    dist -= isoSurfaceValue;
                    if (abs(dist)<tolerance)
                        break;
                        
                    t += min(dist, Z_REPEAT_DIST/5.);
                }
#else
                for (int i = 0;i<70; i++)
                {
                    dist = DF_composition(o+d*t).d;
                    dist -= isoSurfaceValue;
                    if (abs(dist)<tolerance)
                        break;
                        
                    t += dist;
                }
#endif
                return t;
            }

#define saturate(x) clamp(x, 0., 1.)
            float RAYMARCH_DFSS(float3 o, float3 L, float coneWidth)
            {
                float minAperture = 1.;
                float t = 0.;
                float dist = 10.;
                for (int i = 0;i<7; i++)
                {
                    float3 p = o+L*t;
                    float dist = DF_composition(p).d;
                    dist = min(dist, t);
                    float curAperture = dist/t;
                    minAperture = min(minAperture, curAperture);
                    t += 0.02+min(dist, 0.4);
                }
                return saturate(minAperture/coneWidth);
            }

            float RAYMARCH_DFAO(float3 o, float3 N, float isoSurfaceValue)
            {
                float MaxOcclusion = 0.;
                float TotalOcclusion = 0.;
                const int nSAMPLES = 4;
                float stepSize = 0.11/float(nSAMPLES);
                for (int i = 0;i<nSAMPLES; i++)
                {
                    float t = 0.01+stepSize;
                    stepSize = stepSize*2.;
                    float dist = DF_composition(o+N*t).d-isoSurfaceValue;
                    float occlusion = zclamp(t-dist);
                    TotalOcclusion += occlusion;
                    MaxOcclusion += t;
                }
                return saturate(1.-TotalOcclusion/(MaxOcclusion*1.25));
            }

            struct TraceData 
            {
                float rayLen;
                float3 rayDir;
                float geoDist;
                float3 normal;
                float3 objectPos;
                int matID;
            };
            TraceData new_TraceData()
            {
                TraceData td;
                td.rayLen = 0.;
                td.rayDir = ((float3)0);
                td.geoDist = 0.;
                td.normal = ((float3)0);
                td.objectPos = ((float3)0);
                td.matID = MATERIALID_NONE;
                return td;
            }

            float3 PBR_HDRremap(float3 c)
            {
                float fHDR = smoothstep(2.9, 3., c.x+c.y+c.z);
                return lerp(c, 1.3*float3(4.5, 3.5, 3.), fHDR);
            }

            static const float F_DIELECTRIC_PLASTIC = 1.49;
            static const float F_DIELECTRIC_WATER = 1.33;
            static const float F_DIELECTRIC_DIAMOND = 2.42;
            float3 PBR_Fresnel_Schlick_Dielectric(float3 n, float VdotH)
            {
                float3 F0 = abs((1.-n)/(1.+n));
                return F0+(1.-F0)*pow(1.-VdotH, 5.);
            }

            float3 PBR_ABL_Equation(float3 V, float3 L, float3 N, float roughness, float metallic, float3 ior_n, float3 ior_k)
            {
                roughness = max(roughness, 0.01);
                float3 H = normalize(L+V);
                float NdotH = dot(N, H);
                float NdotL = dot(N, L);
                float VdotH = dot(V, H);
                float NdotV = dot(N, V);
                float PI = 3.14159;
                float alpha2 = roughness*roughness;
                float NoH2 = NdotH*NdotH;
                float den = NoH2*(alpha2-1.)+1.;
                float D = NdotH>0. ? alpha2/(PI*den*den) : 0.;
                float3 F = PBR_Fresnel_Schlick_Dielectric(ior_n, VdotH);
                float Gk = (roughness+1.)*(roughness+1.)/8.;
                float Gl = max(NdotL, 0.)/(NdotL*(1.-Gk)+Gk);
                float Gv = max(NdotV, 0.)/(NdotV*(1.-Gk)+Gk);
                float G = Gl*Gv;
                float softTr = 0.2;
                float3 Rs = D*F*G/(4.*NdotV*NdotL*(1.-softTr)+softTr);
                return Rs;
            }

#define saturate(x) clamp(x, 0., 1.)
            float3 MAT_Plastic(TraceData traceData, float3 cDiff, float3 N, float3 V, float3 L0, float3 L1, float dfao, float dfss0, float dfss1)
            {
                float3 col = ((float3)0);
                float fRoughness = g_animationChannels.material_roughness;
                float3 cAmb = float3(0.26, 0.24, 0.23)*((float3)0.5+0.5*dot(traceData.normal, float3(+0.08, 1, +0.1)))+float3(0.25, 0.25, 0.3)*((float3)0.5+0.5*dot(traceData.normal, float3(-0.28, 1, -0.17)))+float3(0.19, 0.25, 0.3)*((float3)0.5+0.5*dot(traceData.normal, float3(+0.28, 1, -0.27)));
                float3 CL0 = PBR_HDRremap(((float3)1))*PBR_ABL_Equation(V, L0, traceData.normal, fRoughness, 0., ((float3)F_DIELECTRIC_PLASTIC), ((float3)0));
                float3 CL1 = PBR_HDRremap(((float3)1))*PBR_ABL_Equation(V, L1, traceData.normal, fRoughness, 0., ((float3)F_DIELECTRIC_PLASTIC), ((float3)0));
                col = cAmb*dfao;
                col *= saturate(0.3+fRoughness*0.5+0.2*(dfss0+dfss1));
                col += (dfss0+fRoughness*0.25)*CL0;
                col += (dfss1+fRoughness*0.25)*CL1;
                return col*0.75;
            }

            float SAMPLER_trilinear(float3 p)
            {
                const float TEXTURE_RES = 256.;
                p *= TEXTURE_RES;
                float3 pixCoord = floor(p);
                float3 t = p-pixCoord;
                t = (3.-2.*t)*t*t;
                float2 layer_translation = -pixCoord.y*float2(37., 17.)/TEXTURE_RES;
                float2 layer1_layer2 = tex2D(_MainTex, layer_translation+(pixCoord.xz+t.xz+0.5)/TEXTURE_RES).xy;
                return lerp(layer1_layer2.x, layer1_layer2.y, t.y);
            }

            float MAT_remap_angle_probability(float x_01)
            {
                return 1.-cos(x_01*PI/2.);
            }

            float3 MAT_addFog(float travelDist, in float3 color, in float3 p, in float3 c_atmosphere)
            {
                float a = 0.08;
                float NORMALIZATION_TERM = log((1.+a)/a);
                float da = travelDist/50.;
                da = log((da+a)/a)/NORMALIZATION_TERM;
                float3 FinalColor = lerp(color, c_atmosphere, saturate(da));
                return FinalColor;
            }

            float4 MAT_apply(float3 pos, TraceData traceData)
            {
                float3 c_atmosphere = lerp(float3(0.87, 0.94, 1.), float3(0.6, 0.8, 1.), clamp(3.*pos.y/length(pos.xz), 0., 1.));
                if (traceData.matID==MATERIALID_SKY)
                {
                    return float4(c_atmosphere, 1.);
                }
                
                float4 col = ((float4)0);
                float3 N = traceData.normal;
                float3 V = normalize(-traceData.rayDir);
                float3 L0 = normalize(float3(0.5, 1.2, 0.3));
                float3 L1 = normalize(float3(-L0.x, L0.y, -L0.z+0.5));
                float fNoiseAmplitude = 0.4;
                float jitter_01 = SAMPLER_trilinear(pos*10.+g_time*50.);
                float t = MAT_remap_angle_probability(jitter_01)*fNoiseAmplitude;
                float3 Na = float3(mul(N.xz,transpose(float2x2(cos(t), sin(t), -sin(t), cos(t)))), N.y).xzy;
                jitter_01 = SAMPLER_trilinear(5.+pos*9.11);
                t = MAT_remap_angle_probability(jitter_01)*fNoiseAmplitude;
                float3 Nb = float3(mul(N.xz,transpose(float2x2(cos(t), -sin(t), sin(t), cos(t)))), N.y).xzy;
                float dfaoA = RAYMARCH_DFAO(pos, Na, 0.02);
                float dfaoB = RAYMARCH_DFAO(pos, Nb, 0.02);
                float dfaoAveraged = 0.5*(dfaoA+dfaoB);
                float dfss0 = RAYMARCH_DFSS(pos+L0*0.01, L0, 0.2);
                float dfss1 = RAYMARCH_DFSS(pos+L1*0.01, L1, 0.2);
                if (traceData.matID==MATERIALID_PLASTIC)
                {
                    col.rgb = MAT_Plastic(traceData, ((float3)1), N, V, L0, L1, dfaoAveraged, dfss0, dfss1);
                }
                
                col.rgb = MAT_addFog(traceData.rayLen*0.3, col.rgb, pos, c_atmosphere);
                return col;
            }

            float TRACE_zprime(float3 o, float3 d)
            {
                float geometryCeiling = FLOOR_HEIGHT+PRIM_HEIGHT+g_animationChannels.geometry_displacement*GEOMETRY_DISPLACEMENT;
                float t = RAYINTERSEC_plane(o, d, float3(0, geometryCeiling, 0), float3(0, 1, 0));
                return t<0. ? MAX_DIST : t;
                return t;
            }

            TraceData TRACE_geometry(float3 o, float3 d)
            {
                TraceData dfTrace;
                float rayLen = RAYMARCH_isosurface(o, d, 0.);
                float3 dfHitPosition = o+rayLen*d;
                DF_out compInfo = DF_composition(dfHitPosition);
                rayLen += compInfo.d;
                dfHitPosition = o+rayLen*d;
                dfTrace.rayLen = rayLen;
                dfTrace.matID = compInfo.matID;
                dfTrace.objectPos = compInfo.objectPos;
                dfTrace.geoDist = compInfo.d;
                dfTrace.rayDir = d;
                dfTrace.normal = normalize(DF_gradient(dfHitPosition));
                return dfTrace;
            }

            float3 TRACE_debug(TraceData traceData, int elemID)
            {
                if (elemID==DEBUG_RAYLEN)
                    return ((float3)log(traceData.rayLen)*0.2);
                    
                if (elemID==DEBUG_GEODIST)
                    return ((float3)traceData.geoDist);
                    
                if (elemID==DEBUG_NORMAL)
                    return traceData.normal;
                    
                if (elemID==DEBUG_MATID)
                    return traceData.matID==MATERIALID_PLASTIC ? ((float3)1) : float3(traceData.matID==MATERIALID_FLOOR ? 1 : 0, traceData.matID==MATERIALID_METAL ? 1 : 0, traceData.matID==MATERIALID_SKY ? 1 : 0);
                    
                return ((float3)0);
            }

            static const int SPLINE_POINT_COUNT = 8;
            struct SPLINE_CtrlPts 
            {
                float4 p[SPLINE_POINT_COUNT];
            };
            float4 SPLINE_PointArray(int i, SPLINE_CtrlPts ctrlPts)
            {
                if (i==0||i==SPLINE_POINT_COUNT)
                    return ctrlPts.p[0];
                    
                if (i==1||i==SPLINE_POINT_COUNT+1)
                    return ctrlPts.p[1];
                    
                if (i==2||i==SPLINE_POINT_COUNT+2)
                    return ctrlPts.p[2];
                    
                if (i==3)
                    return ctrlPts.p[3];
                    
                if (i==4)
                    return ctrlPts.p[4];
                    
                if (i==5)
                    return ctrlPts.p[5];
                    
                if (i==6)
                    return ctrlPts.p[6];
                    
                if (i==7)
                    return ctrlPts.p[7];
                    
                return ((float4)0);
            }

            float4 SPLINE_catmullRom(float fTime, SPLINE_CtrlPts ctrlPts)
            {
                float t = frac(fTime);
                const float n = float(SPLINE_POINT_COUNT);
                int idxOffset = int(t*n);
                float4 p1 = SPLINE_PointArray(idxOffset, ctrlPts);
                float4 p2 = SPLINE_PointArray(idxOffset+1, ctrlPts);
                float4 p3 = SPLINE_PointArray(idxOffset+2, ctrlPts);
                float4 p4 = SPLINE_PointArray(idxOffset+3, ctrlPts);
                t *= n;
                t = t-float(int(t));
                float4 val = 0.5*((-p1+3.*p2-3.*p3+p4)*t*t*t+(2.*p1-5.*p2+4.*p3-p4)*t*t+(-p1+p3)*t+2.*p2);
                return val;
            }

            void ANIM_main(float fTime)
            {
                float t1 = 0.01*fTime;
                float t2 = 0.01*fTime+0.03;
                SPLINE_CtrlPts cameraPosKeyFrames;
                cameraPosKeyFrames.p[1] = float4(10., 2.7, 5., 1.9);
                cameraPosKeyFrames.p[2] = float4(16., 3.3, 8.5, 1.);
                cameraPosKeyFrames.p[3] = float4(20., 6.8, 5., 2.97);
                cameraPosKeyFrames.p[4] = float4(40., 3.4, 17.5, 0.82);
                cameraPosKeyFrames.p[5] = float4(30., 3.1, 27.5, 1.97);
                cameraPosKeyFrames.p[6] = float4(25., 3.2, 22.5, 1.93);
                cameraPosKeyFrames.p[7] = float4(15., 3., 24.5, 1.95);
                cameraPosKeyFrames.p[0] = float4(5., 2.8, 12.5, 1.2);
                float4 cameraPos = SPLINE_catmullRom(t1, cameraPosKeyFrames);
                float4 cameraDir = normalize(SPLINE_catmullRom(t2, cameraPosKeyFrames)-cameraPos);
                SPLINE_CtrlPts geometryKeyFrames;
                geometryKeyFrames.p[1] = float4(0.07, 1., 0.3, 1.);
                geometryKeyFrames.p[2] = float4(0.09, 0.9, 0.5, 0.9);
                geometryKeyFrames.p[3] = float4(0.08, 1., 0.2, 1.);
                geometryKeyFrames.p[4] = float4(0.15, 0.97, 0.5, 0.99);
                geometryKeyFrames.p[5] = float4(0.09, 0.82, 0.5, 0.82);
                geometryKeyFrames.p[6] = float4(0.11, 0.97, 0.5, 0.99);
                geometryKeyFrames.p[7] = float4(0.05, 0.93, 0.5, 0.93);
                geometryKeyFrames.p[0] = float4(0.12, 0.95, 0.5, 0.98);
                float4 geoPose = SPLINE_catmullRom(t1*25., geometryKeyFrames);
                g_animationChannels.camPos = cameraPos.xyz;
                g_animationChannels.camLookAt = cameraPos.xyz+cameraDir.xyz-float3(0, cameraPos.w, 0);
                g_animationChannels.geometry_smoothness = geoPose[0];
                g_animationChannels.material_roughness = 0.45;
                g_animationChannels.geometry_width = geoPose[1];
                g_animationChannels.geometry_displacement = GEOMETRY_DISPLACEMENT;
            }

            float3 TRACE_main(float3 o, float3 dir, float2 uv)
            {
                float fRemainingAlpha = 1.;
                float zStart = TRACE_zprime(o, dir);
                float3 pt = o+dir*zStart;
                float3 ptGeo = ((float3)0);
                TraceData geometryTraceData;
                if (zStart<MAX_DIST)
                {
                    geometryTraceData = TRACE_geometry(pt, dir);
                    geometryTraceData.rayLen += zStart;
                    ptGeo = o+dir*geometryTraceData.rayLen;
                }
                else 
                {
                    geometryTraceData.rayLen = MAX_DIST;
                    geometryTraceData.matID = MATERIALID_SKY;
                    geometryTraceData.objectPos = pt;
                    geometryTraceData.geoDist = 0.;
                    geometryTraceData.rayDir = dir;
                    ptGeo = pt;
                }
                float4 cFinal = MAT_apply(ptGeo, geometryTraceData);
                return cFinal.rgb;
            }

            float4 frag (v2f __vertex_output) : SV_Target
            {
                vertex_output = __vertex_output;
                float4 fragColor = 0;
                float2 fragCoord = vertex_output.uv * _Resolution;
                g_time = _Time.y+2.6;
                float2 uv = (fragCoord.xy-0.5*iResolution.xy)/iResolution.xx;
                uv /= _Scale;
                float fTime = g_time+2.1;
                ANIM_main(fTime);
                Cam cam = CAM_animate(uv, fTime);
                float3 d = CAM_getRay(cam, uv);
                float3 c = TRACE_main(cam.o, d, uv);
                c = POST_ProcessFX(c, uv);
                fragColor = float4(c, 1.);
                if (_GammaCorrect) fragColor.rgb = pow(fragColor.rgb, 2.2);
                return fragColor;
            }
            float3 POST_ProcessFX(float3 c, float2 uv)
            {
                float lensRadius = 0.65;
                uv /= lensRadius;
                float sin2 = uv.x*uv.x+uv.y*uv.y;
                float cos2 = 1.-min(sin2*sin2, 1.);
                float cos4 = cos2*cos2;
                c *= cos4;
                c = pow(c, ((float3)0.4545));
                return c;
            }

            Cam CAM_animate(float2 uv, float fTime)
            {
                Cam cam;
                cam.o = g_animationChannels.camPos;
                cam.D = normalize(g_animationChannels.camLookAt-cam.o);
                cam.R = normalize(cross(cam.D, float3(0, 1, 0)));
                cam.U = normalize(cross(cam.R, cam.D));
                cam.lens = 1.2+0.3*sin(fTime*0.1);
                cam.zoom = 3.+sin(fTime*0.1)/cam.lens;
                return cam;
            }

            float3 CAM_getRay(Cam cam, float2 uv)
            {
                uv = cam.lens*uv/(cam.lens-length(uv)*length(uv));
                uv *= cam.zoom;
                return normalize(uv.x*cam.R+uv.y*cam.U+cam.D);
            }

            ENDCG
        }
    }
}

