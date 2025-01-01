using FontParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TextRenderer;

internal class PointRenderer
{
  private static Lazy<Shader> Shader =
    new Lazy<Shader>(() => new Shader("Point"));

  private float[] Vertices;
  private int VertexBufferObject;
  private int VertexArrayObject;

  public PointRenderer(GlyphData glyphData)
  {
    Vertices = glyphData
      .Contours
      .SelectMany(c => c.SelectMany(GetFloats))
      .ToArray();

    VertexArrayObject = GL.GenVertexArray();
    GL.BindVertexArray(VertexArrayObject);
    VertexBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);

    GL.BufferData(
      BufferTarget.ArrayBuffer,
      Vertices.Length * sizeof(float),
      Vertices,
      BufferUsageHint.StaticDraw);

    GL.VertexAttribPointer(
      0,
      2,
      VertexAttribPointerType.Float,
      false,
      3 * sizeof(float),
      0);
    GL.EnableVertexAttribArray(0);

    GL.VertexAttribPointer(
      1,
      1,
      VertexAttribPointerType.Float,
      false,
      3 * sizeof(float),
      2 * sizeof(float));
    GL.EnableVertexAttribArray(1);

    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    GL.BindVertexArray(0);
  }

  public void OnRender(Vector2 position, float scaleFactor, Vector2i windowSize)
  {
    var scale = Matrix4.CreateScale(scaleFactor, scaleFactor, 1.0f);
    var translate = Matrix4.CreateTranslation(position.X, position.Y, 0f);
    var model = scale * translate;

    var projection = Matrix4.CreateOrthographicOffCenter(
      0.0f,
      windowSize.X,
      0.0f,
      windowSize.Y,
      -1.0f,
      1.0f);

    Shader.Value.Use();
    GL.UniformMatrix4(0, false, ref model);
    GL.UniformMatrix4(1, false, ref projection);

    GL.Enable(EnableCap.PointSprite);
    GL.Enable(EnableCap.VertexProgramPointSize);
    GL.BindVertexArray(VertexArrayObject);

    GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Length / 3);

    GL.Disable(EnableCap.PointSprite);
    GL.Disable(EnableCap.VertexProgramPointSize);
    GL.BindVertexArray(0);
    GL.UseProgram(0);
  }

  private static IEnumerable<float> GetFloats(Point p)
  {
    yield return p.X;
    yield return p.Y;
    yield return p.OnCurve ? 1f : 0f;
  }
}
