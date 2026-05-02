#version 430 core

struct SpotLight {
    mat4 view;
    mat4 projection;
    vec4 color;
    vec3 pos;
};
layout ( std430, binding = 2) readonly buffer SpotLightBuffer {
    int lightsLength;
    SpotLight spotLights[];
};

layout (location = 0) in vec3 aPos;

uniform mat4 uModel;
uniform int uLightId;

void main() {
    SpotLight light = spotLights[uLightId];
    mat4 pvm = light.projection * light.view * uModel;
    gl_Position = pvm * vec4(aPos, 1.0);
}