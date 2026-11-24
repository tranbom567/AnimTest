#ifndef TOON_EDGES_INCLUDED
#define TOON_EDGES_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/NormalBuffer.hlsl"

float3 ToonNormal(int2 p)
{
    NormalData nd;
    DecodeFromNormalBuffer(uint2(p), nd);
    return nd.normalWS;
}

float ToonDepth(int2 p)
{
    return LinearEyeDepth(LoadCameraDepth(uint2(p)), _ZBufferParams);
}
#endif

void ToonEdges_float(float2 UV, float Thickness, float DepthThreshold,
                     float NormalThreshold, out float Edge)
{
#ifdef SHADERGRAPH_PREVIEW
    Edge = 0.0;
#else
    int2 p = int2(UV * _ScreenSize.xy);
    int  t = (int)max(Thickness, 1.0);

    int2 a = p + int2(-t,  t);
    int2 b = p + int2( t, -t);
    int2 c = p + int2( t,  t);
    int2 d = p + int2(-t, -t);

    float dA = ToonDepth(a), dB = ToonDepth(b);
    float dC = ToonDepth(c), dD = ToonDepth(d);

    float depthDiff = abs(dA - dB) + abs(dC - dD);
    float depthRef  = min(min(dA, dB), min(dC, dD));
    float depthEdge = smoothstep(DepthThreshold, DepthThreshold * 2.0,
                                 depthDiff / max(depthRef, 0.001));

    float3 nA = ToonNormal(a), nB = ToonNormal(b);
    float3 nC = ToonNormal(c), nD = ToonNormal(d);
    float normalDiff = (1.0 - dot(nA, nB)) + (1.0 - dot(nC, nD));
    float normalEdge = smoothstep(NormalThreshold, NormalThreshold * 2.0, normalDiff);

    Edge = saturate(max(depthEdge, normalEdge));
#endif
}

#endif