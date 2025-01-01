#version 460
out vec4 outputColor;
in vec2 texCoord; 

void main()
{
  vec2 p = texCoord;
  float value = p.x*p.x-p.y;
  if(value > 0) discard;
  outputColor = vec4(1.0,1.0,1.0,1.0);
}