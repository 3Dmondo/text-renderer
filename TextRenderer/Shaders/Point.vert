#version 460
layout (location = 0) in vec2 position;
layout (location = 1) in float onCurve;
layout (location = 0) uniform mat4 model;
layout (location = 1) uniform mat4 projection;
out vec4 fragColor;
void main()
{
  gl_Position = projection * model * vec4(position, 0, 1.0);
  gl_PointSize = 5.0;
  float red = onCurve == 0 ? 1.0 : 0.0; 
  float blue = onCurve > 0 ? 1.0 : 0;
  fragColor = vec4(red,0.0,blue,1.0);
}