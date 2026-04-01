#version 330 core

in vec3 position;
in vec4 color;
in vec2 texcoord;
in int texLayer;

out vec4 vertexColor;
out vec2 textureCoord;
flat out int textureLayer; 

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main() {
    vertexColor = color;
    textureCoord = texcoord;
    textureLayer = texLayer;
    mat4 mvp = projection * view * model;
    gl_Position = mvp * vec4(position, 1.0);
}