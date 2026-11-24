#ifndef SCOPE_FUNCTION_INCLUDED
#define SCOPE_FUNCTION_INCLUDED

float SC_SdBox(float2 p, float2 b)
{
    float2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

// coverage from signed distance, anti-aliased
float SC_Cover(float d, float aa)
{
    return 1.0 - smoothstep(-aa, aa, d);
}

// Reticle in centred space p (-1..1 across the lens). Returns 0..1 coverage.
float SC_Reticle(float2 p, float thickness, float gap, float dotSpacing, float dotRadius,
                 float centerDot, float ringRadius, float aa)
{
    float2 ap = abs(p);
    float outer = 0.96;

    // Duplex crosshair: thin near centre, thick towards the edge
    float2 th = lerp(thickness, thickness * 6.0, smoothstep(0.45, 0.6, ap));

    // horizontal / vertical bars
    float len = (outer - gap) * 0.5;
    float mid = (outer + gap) * 0.5;
    float dH = SC_SdBox(float2(ap.x - mid, p.y), float2(len, th.x));
    float dV = SC_SdBox(float2(p.x, ap.y - mid), float2(th.y, len));
    float d = min(dH, dV);

    // mil dots along both axes (1..5 each side)
    float kx = clamp(round(ap.x / dotSpacing), 1.0, 5.0);
    float ky = clamp(round(ap.y / dotSpacing), 1.0, 5.0);
    float dotsH = length(float2(ap.x - kx * dotSpacing, p.y)) - dotRadius;
    float dotsV = length(float2(p.x, ap.y - ky * dotSpacing)) - dotRadius;
    d = min(d, min(dotsH, dotsV));

    // centre dot and optional ring
    d = min(d, length(p) - centerDot);
    if (ringRadius > 0.001)
        d = min(d, abs(length(p) - ringRadius) - thickness);

    return SC_Cover(d, aa);
}

// ---------- Custom Function entry point ----------
// UV            : mesh UV (0..1, lens centred at 0.5)
// ViewDirTS     : View Direction node, Tangent space (for reticle parallax)
// Distortion    : barrel distortion, 0..0.5
// ChromaticAb   : 0..0.02
// VignetteStart : radius where the dark scope tube starts, 0..1
// Output Color  : final lens colour (use for Emission / Unlit Color)
// Output Mask   : lens circle (use as Alpha with Alpha Clipping)
void Scope_float(
    UnityTexture2D SceneRT,
    UnitySamplerState SS,
    float2 UV,
    float3 ViewDirTS,
    float  Distortion,
    float  ChromaticAb,
    float  VignetteStart,
    float  Parallax,
    float  RetThickness,
    float  RetGap,
    float  DotSpacing,
    float  DotRadius,
    float  CenterDot,
    float  RingRadius,
    float4 ReticleColor,
    float3 LensTint,
    out float3 Color,
    out float  Mask)
{
    float2 p = (UV - 0.5) * 2.0;     // -1..1
    float r = length(p);

    // Lens mask (antialiased circle)
    float aa = fwidth(r) * 1.5 + 1e-4;
    Mask = 1.0 - smoothstep(1.0 - aa, 1.0, r);

    // Barrel distortion of the scene image
    float2 sp = p * (1.0 + Distortion * r * r);
    float2 suv = sp * 0.5 + 0.5;

    // Chromatic aberration (radial)
    float2 ca = p * ChromaticAb * r;
    float cr = SceneRT.Sample(SS.samplerstate, suv + ca).r;
    float cg = SceneRT.Sample(SS.samplerstate, suv).g;
    float cb = SceneRT.Sample(SS.samplerstate, suv - ca).b;
    float3 scene = float3(cr, cg, cb) * LensTint;

    // Scope tube vignette
    float vig = 1.0 - smoothstep(VignetteStart, 1.0, r);
    vig = vig * vig;
    scene *= vig;

    // Reticle with parallax: shifts when the eye moves off-axis
    float2 shift = ViewDirTS.xy / max(abs(ViewDirTS.z), 0.2) * Parallax;
    float2 rp = p - shift;
    float raa = fwidth(rp.x) + fwidth(rp.y) + 1e-4;
    float ret = SC_Reticle(rp, RetThickness, RetGap, DotSpacing, DotRadius, CenterDot, RingRadius, raa);
    ret *= ReticleColor.a;

    Color = lerp(scene, ReticleColor.rgb, ret) * Mask;
}

void Scope_half(
    UnityTexture2D SceneRT, UnitySamplerState SS, half2 UV, half3 ViewDirTS,
    half Distortion, half ChromaticAb, half VignetteStart, half Parallax,
    half RetThickness, half RetGap, half DotSpacing, half DotRadius, half CenterDot,
    half RingRadius, half4 ReticleColor, half3 LensTint,
    out half3 Color, out half Mask)
{
    float3 c; float m;
    Scope_float(SceneRT, SS, UV, ViewDirTS, Distortion, ChromaticAb, VignetteStart, Parallax,
                RetThickness, RetGap, DotSpacing, DotRadius, CenterDot, RingRadius,
                ReticleColor, LensTint, c, m);
    Color = c; Mask = m;
}

#endif
