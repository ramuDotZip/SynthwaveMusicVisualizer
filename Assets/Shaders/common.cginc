#define GRID_SPEED 0.2
#define SUN_SPEED 1.0
#define LINEXFREQ 2.0
#define LINEYFREQ 2.0
#define LINEXSIZE 0.05
#define LINEYSIZE 0.05
#define SUN_HEIGHT 0.75
#define MOUNTAIN_HEIGHT 0.6
#define MOUNTAIN_WEIGHT 0.8

#define BLOOM_BIAS 20.0
#define BLOOM_MOD 0.0
#define GRADIENT_RED_BIAS 4.0
#define GRADIENT_RED_MOD -1.5
#define GRADIENT_BLUE_BIAS 1.25
#define GRADIENT_BLUE_MOD -0.6
#define SUN_GLOW_BIAS 0.3
#define SUN_GLOW_MOD 0.2

#define DRUM_MIX 0.5

#define DRAW_TRIANGLES true
#define DRAW_FREQVIS true
#define DRAW_CENTER_IMAGE true


#define SIMPLE_SAMPLE(X, Y) ((X).SampleLevel(pointClampSampler, (Y), 0))
#define TEXEL_FETCH(X, Y) ((X).SampleLevel(pointClampSampler, ((Y) + 0.5) / float2(width, height), 0))
SamplerState pointClampSampler;

int width;
int height;

float iTime;


uint hash(uint state) {
    state ^= 2747636419u;
    state *= 2654435769u;
    state ^= state >> 16;
    state *= 2654435769u;
    state ^= state >> 16;
    state *= 2654435769u;
    return state;
}

float rand(int seed) {
    return float(hash(uint(seed))) / 4294967296.0;
}
float2 rand2d(int2 seed) {
    return float2(rand(seed.x + 1280 * seed.y), rand(seed.x + 1280 * seed.y + 921600));
}

float perlin1d(float x) {
    int nearestLow = int(x/128.0);
    float t = x/128.0 - float(nearestLow);
    float lfnoise = (1.0-t) * (rand(nearestLow) * t) + t * (rand(nearestLow+1) * (t-1.0));
    nearestLow = int(x/32.0);
    t = x/32.0 - float(nearestLow);
    float hfnoise = (1.0-t) * (rand(nearestLow+10) * t) + t * (rand(nearestLow+11) * (t-1.0));
    return 0.5 + lfnoise + 0.25*hfnoise;
}

float perlin2d(float2 v) {
    int2 nearestLow = int2(v/4.0);
    float2 t = v/4.0 - float2(nearestLow);
    float noise = (1.0-t.x)*(1.0-t.y) * dot(rand2d(nearestLow), t) + t.x*(1.0-t.y) * dot(rand2d(nearestLow+int2(1,0)), t-float2(1,0));
    noise += (1.0-t.x)*t.y * dot(rand2d(nearestLow+int2(0,1)), t-float2(0,1)) + t.x*t.y * dot(rand2d(nearestLow+int2(1,1)), t-float2(1,1));
    return 0.5+noise;
}


float mod(float a, float b) {
    if (a >= 0) {
        return a % b;
    } else {
        float m = (-a) % b;
        if (m > 0) m = b - m;
        return m;
    }
}
