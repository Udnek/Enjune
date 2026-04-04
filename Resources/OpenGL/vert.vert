#version 430 core

layout (location = 0) in vec3 position;
layout (location = 1) in vec2 texcoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec2 textureCoord;

void main() {
    textureCoord = texcoord;
    mat4 mvp = projection * view * model;
    gl_Position = mvp * vec4(position, 1.0);
}