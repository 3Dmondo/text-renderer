using FontParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace TextRenderer;

internal class TriangleRenderer
{
  private static Lazy<Shader> SplineShader =
    new Lazy<Shader>(() => new Shader("Spline"));

  private static Lazy<Shader> QuadShader =
    new Lazy<Shader>(() => new Shader("Quad"));

  private float[] SplineVertices;
  private int SplineVertexBufferObject;
  private int SplineVertexArrayObject;

  private float[] QuadVertices;
  private int QuadVertexBufferObject;
  private int QuadVertexArrayObject;

  private TriangleFanRenderer TriangleFanRenderer;


  public TriangleRenderer(GlyphData glyphData)
  {
    TriangleFanRenderer = new TriangleFanRenderer(
      glyphData.MinX,
      glyphData.MinY,
      glyphData.Contours);
    InitializeSpline(glyphData);
    InitializeQuad(glyphData);
  }

  private void InitializeQuad(GlyphData glyphData)
  {
    QuadVertices = [
      glyphData.MinX, glyphData.MinY,
      glyphData.MinX, glyphData.MaxY,
      glyphData.MaxX, glyphData.MaxY,

      glyphData.MinX, glyphData.MinY,
      glyphData.MaxX, glyphData.MaxY,
      glyphData.MaxX, glyphData.MinY];

    QuadVertexArrayObject = GL.GenVertexArray();
    GL.BindVertexArray(QuadVertexArrayObject);
    QuadVertexBufferObject = GL.GenBuffer();
    GL.BindBuffer(BufferTarget.ArrayBuffer, QuadVertexBufferObject);

    GL.BufferData(
      BufferTarget.ArrayBuffer,
      QuadVertices.Length * sizeof(float),
      QuadVertices,
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

  private void InitializeSpline(GlyphData glyphData)
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
    ConfigureSetencilTest();

    TriangleFanRenderer.OnRender(position, scaleFactor, windowSize);
    RenderSpline(position, scaleFactor, windowSize);

    ConfigureStencilTestForRendering();
    //GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);
    RenderQuad(position, scaleFactor, windowSize);
    //GL.PolygonMode(TriangleFace.FrontAndBack,PolygonMode.Fill);

    GL.Disable(EnableCap.StencilTest);
  }

  private static void ConfigureStencilTestForRendering()
  {
    GL.ColorMask(true, true, true, true);
    GL.DepthMask(true);
    GL.StencilFunc(StencilFunction.Notequal, 0, 0xFF);
    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
  }

  private static void ConfigureSetencilTest()
  {
    GL.Enable(EnableCap.StencilTest);
    GL.ClearStencil(0);
    GL.Clear(ClearBufferMask.StencilBufferBit);
    GL.ColorMask(false, false, false, false);
    GL.DepthMask(false);
    GL.StencilFunc(StencilFunction.Always, 0x1, 0xFF);
    GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Invert);
  }

  private void RenderQuad(Vector2 position, float scaleFactor, Vector2i windowSize)
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

    QuadShader.Value.Use();
    GL.UniformMatrix4(0, false, ref model);
    GL.UniformMatrix4(1, false, ref projection);

    GL.BindVertexArray(QuadVertexArrayObject);

    GL.DrawArrays(PrimitiveType.Triangles, 0, QuadVertices.Length / 2);

    GL.BindVertexArray(0);
    GL.UseProgram(0);
  }

  private void RenderSpline(Vector2 position, float scaleFactor, Vector2i windowSize)
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
      Debug.Assert(c[i].OnCurve);
      yield return c[i].X;
      yield return c[i].Y;
      yield return c[i + 1].X;
      yield return c[i + 1].Y;
      yield return c[(i + 2) % c.Length].X;
      yield return c[(i + 2) % c.Length].Y;
    }
  }

  private static IEnumerable<float> GetOnCurve(Point[] c)
  {
    foreach (Point p in c)
      if (p.OnCurve)
      {
        yield return p.X;
        yield return p.Y;
      }
    var first = c[0];
    yield return first.X;
    yield return first.Y;
  }

}
