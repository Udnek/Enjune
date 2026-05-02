#version 430 core

in vec2 pixelPosition;

void main() {
    gl_Position = vec4(pixelPosition, 0.0, 1.0);
}