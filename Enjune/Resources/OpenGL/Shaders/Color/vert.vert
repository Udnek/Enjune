#version 430 core

in vec3 inPosition;
in vec4 inColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec4 color;

void main() {
    color = inColor;
    mat4 pvm = projection * view * model;
    gl_Position = pvm * vec4(inPosition, 1.0);
}