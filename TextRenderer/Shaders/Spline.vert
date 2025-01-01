#version 460
layout (location = 0) in vec2 position;
layout (location = 0) uniform mat4 model;
layout (location = 1) uniform mat4 projection;

out vec2 texCoord; 

void main()
{
  gl_Position = projection * model * vec4(position,0.0, 1.0);

  int vertexInTriangle = gl_VertexID % 3; 
  
  if (vertexInTriangle == 0)
      texCoord = vec2(0.0, 0.0); 
  else if (vertexInTriangle == 1)
      texCoord = vec2(0.5, 0.0); 
  else
      texCoord = vec2(1.0, 1.0); 
}