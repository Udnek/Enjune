layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexPos;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec4 color;
out vec2 texPos;

void main() {
    texPos = aTexPos;
    mat4 pvm = uProjection * uView * uModel;
    gl_Position = pvm * vec4(aPos, 1.0);
}