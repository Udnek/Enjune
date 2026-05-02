#version 430 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTexPos;

out vec2 texPos;

void main() {
    texPos = aTexPos;
    gl_Position = vec4(aPos, 0, 1);
}