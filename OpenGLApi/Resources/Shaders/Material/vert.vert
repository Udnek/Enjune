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
layout (location = 1) in vec2 aTexPos;
layout (location = 2) in vec3 aNorm;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 texPos;
out vec3 nonNormNormal;
out vec3 fragPos;
out vec4 fragPosInLightSpace[5];

void main() {
    texPos = aTexPos;
    nonNormNormal = (uModel * vec4(aNorm, 0)).xyz; // zero cuase it is direction
    fragPos = (uModel * vec4(aPos, 1)).xyz; 
    for (int i = 0; i < lightsLength; i++) {
        SpotLight light = spotLights[i];
        fragPosInLightSpace[i] = light.projection * light.view * vec4(fragPos, 1);
    }
    
    gl_Position = uProjection * uView * vec4(fragPos, 1);
}