using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TextRenderer.Abstractions;

internal abstract class AbstractArrayRenderer
{
  private int VertexBufferObject;
  private int VertexArrayObject;
  private int NumberOfVertices;
  protected abstract Shader Shader { get; }

  protected void InitBuffers(
    float[] Vertices,
    int stride,
    params VertexAttribPointerArgs[] vertexAttribPointerArgs)
  {
    NumberOfVertices = Vertices.Length / stride;
    VertexBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
    GL.BufferData(
      BufferTarget.ArrayBuffer,
      Vertices.Length * sizeof(float),
      Vertices,
      BufferUsageHint.StaticDraw);
    VertexArrayObject = GL.GenVertexArray();
    GL.BindVertexArray(VertexArrayObject);
    for (int i = 0; i < vertexAttribPointerArgs.Length; i++)
    {
      var args = vertexAttribPointerArgs[i];
      GL.VertexAttribPointer(
        args.Index,
        args.Size,
        VertexAttribPointerType.Float,
        false,
        stride * sizeof(float),
        args.Offset);
      GL.EnableVertexAttribArray(i);
    }
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    GL.BindVertexArray(0);
  }

  public void Render(Matrix4 projectionModel, PrimitiveType primitiveType)
  {
    Shader.Use();
    GL.UniformMatrix4(0, false, ref projectionModel);
    GL.BindVertexArray(VertexArrayObject);
    GL.DrawArrays(PrimitiveType.Triangles, 0, NumberOfVertices);
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    GL.BindVertexArray(0);
  }

}
