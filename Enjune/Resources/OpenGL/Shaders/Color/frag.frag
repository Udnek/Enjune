#version 430 core

in vec4 color;

uniform vec4 uGlobalColor;

out vec4 fragColor;

void main() {
    vec4 col = color * uGlobalColor;
    if (col.a < 0.1) discard;
    fragColor = col;
}