#version 460
out vec4 outputColor;
in vec2 texCoord; 

void main()
{
  vec2 p = texCoord;
  
  //float value = p.y*p.y-p.x;

  //if(-value < 0) discard;

  vec2 px = dFdx(p);
  vec2 py = dFdy(p);
  
  float fx = (2 * p.x) * px.x - px.y;
  float fy = (2 * p.x) * py.x - py.y;
  
  float sd = (p.x * p.x - p.y) / sqrt(fx * fx + fy * fy);
  
  float alpha = 0.5 - sd;
  
  if (alpha < 0) discard;

  outputColor = vec4(1.0, 0.0, 0.0, 1.0);
}