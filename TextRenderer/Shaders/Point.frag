#version 460
out vec4 outputColor;
in vec4 fragColor;
void main()
{
  vec2 C = 2.0 * (gl_PointCoord - vec2(0.5, 0.5));
  float mag = dot(C,C);
  if (mag > 1.0) discard;
  outputColor = fragColor;
}