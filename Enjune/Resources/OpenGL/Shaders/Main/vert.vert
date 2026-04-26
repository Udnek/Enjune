#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 texcoord;
layout (location = 2) in vec3 inNorm;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 textureCoord;
out vec3 nonNormNormal;
out vec3 fragPos;

void main() {
    textureCoord = texcoord;
    nonNormNormal = inNorm;
    fragPos = vec3(model * vec4(position, 1.0));
    mat4 pvm = projection * view * model;
    gl_Position = pvm * vec4(position, 1.0);
}