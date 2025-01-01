#version 460
layout (location = 0) in vec2 position;
layout (location = 0) uniform mat4 model;
layout (location = 1) uniform mat4 projection;


void main()
{
  gl_Position = projection * model * vec4(position, 0.0, 1.0);
}