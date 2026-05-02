#version 430 core

in vec3 aPos;
in vec4 aColor;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec4 color;

void main() {
    color = aColor;
    mat4 pvm = uProjection * uView * uModel;
    gl_Position = pvm * vec4(aPos, 1.0);
}