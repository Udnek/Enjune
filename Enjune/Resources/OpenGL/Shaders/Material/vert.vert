#version 430 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexPos;
layout (location = 2) in vec3 aNorm;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 texPos;
out vec3 nonNormNormal;
out vec3 fragPos;

void main() {
    texPos = aTexPos;
    nonNormNormal = (uModel * vec4(aNorm, 1)).xyz;
    fragPos = vec3(uModel * vec4(aPos, 1.0));
    mat4 pvm = uProjection * uView * uModel;
    gl_Position = pvm * vec4(aPos, 1.0);
}