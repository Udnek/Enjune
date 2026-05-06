
layout (location = 0) in vec3 aPos;

uniform mat4 uModel;
uniform int uLightId;

void main() {
    SpotLight light = spotLights[uLightId];
    mat4 pvm = light.projection * light.view * uModel;
    gl_Position = pvm * vec4(aPos, 1.0);
}