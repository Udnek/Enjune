#version 430 core

struct Material {
    vec4 color;
    int textureId;
};
struct SpotLight {
    mat4 view;
    mat4 projection;
    vec4 color;
    vec3 pos;
};
struct PerPrimitive {
    vec4 color;
    int matId;
};

layout ( std430, binding = 0) readonly buffer MaterialBuffer {
    Material materials[];
};
layout ( std430, binding = 1) readonly buffer PerPrimitiveBuffer {
    PerPrimitive perPrimitives[];
};
layout ( std430, binding = 2) readonly buffer SpotLightBuffer {
    int lightsLength;
    SpotLight spotLights[];
};