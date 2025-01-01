using FontParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace TextRenderer;

internal class SplineRenderer
{
  private static Lazy<Shader> SplineShader =
    new Lazy<Shader>(() => new Shader("Spline"));
  private float[] SplineVertices;
  private int SplineVertexBufferObject;
  private int SplineVertexArrayObject;

  public SplineRenderer(GlyphData glyphData)
  {
    SplineVertices = glyphData.Contours.SelectMany(GetTriangles).ToArray();

    SplineVertexArrayObject = GL.GenVertexArray();
    GL.BindVertexArray(SplineVertexArrayObject);
    SplineVertexBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, SplineVertexBufferObject);

    GL.BufferData(
      BufferTarget.ArrayBuffer,
      SplineVertices.Length * sizeof(float),
      SplineVertices,
      BufferUsageHint.StaticDraw);

    GL.VertexAttribPointer(
      0,
      2,
      VertexAttribPointerType.Float,
      false,
      2 * sizeof(float),
      0);
    GL.EnableVertexAttribArray(0);

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

    SplineShader.Value.Use();
    GL.UniformMatrix4(0, false, ref model);
    GL.UniformMatrix4(1, false, ref projection);

    GL.BindVertexArray(SplineVertexArrayObject);

    //GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

    GL.DrawArrays(PrimitiveType.Triangles, 0, SplineVertices.Length / 2);

    //GL.PolygonMode(TriangleFace.FrontAndBack,PolygonMode.Fill);

    GL.BindVertexArray(0);
    GL.UseProgram(0);
  }

  private static IEnumerable<float> GetTriangles(Point[] c)
  {
    for (int i = 0; i < c.Length - 1; i += 2)
    {
      Debug.Assert(c[i].OnCurve); //TODO: handle glyphs starting off curve
      yield return c[i].X;
      yield return c[i].Y;
      yield return c[i + 1].X;
      yield return c[i + 1].Y;
      yield return c[(i + 2) % c.Length].X;
      yield return c[(i + 2) % c.Length].Y;
    }
  }
}
