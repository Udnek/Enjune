#version 430 core

in vec4 color;

uniform vec4 globalColor;

out vec4 fragColor;

void main() {
    vec4 col = color * globalColor;
    if (col.a < 0.1) discard;
    fragColor = col;
}