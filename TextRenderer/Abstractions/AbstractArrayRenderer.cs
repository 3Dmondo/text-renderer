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
    float[] vertices,
    int stride,
    params (int size, int offset)[] vertexAttribPointerArgs)
  {
    NumberOfVertices = vertices.Length / stride;

    VertexBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
    GL.BufferData(
      BufferTarget.ArrayBuffer,
      vertices.Length * sizeof(float),
      vertices,
      BufferUsageHint.StaticDraw);

    VertexArrayObject = GL.GenVertexArray();
    GL.BindVertexArray(VertexArrayObject);
    for (int i = 0; i < vertexAttribPointerArgs.Length; i++)
    {
      var args = vertexAttribPointerArgs[i];
      GL.VertexAttribPointer(
        i,
        args.size,
        VertexAttribPointerType.Float,
        false,
        stride * sizeof(float),
        args.offset * sizeof(float));
      GL.EnableVertexAttribArray(i);
    }

    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    GL.BindVertexArray(0);
  }

  public virtual void Render(Matrix4 model, Matrix4 projection, PrimitiveType primitiveType)
  {
    Shader.Use();
    GL.UniformMatrix4(0, false, ref model);
    GL.UniformMatrix4(1, false, ref projection);
    GL.BindVertexArray(VertexArrayObject);
    GL.DrawArrays(primitiveType, 0, NumberOfVertices);
    GL.BindVertexArray(0);
    GL.UseProgram(0);
  }

}
