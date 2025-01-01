using FontParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Diagnostics;

namespace TextRenderer;

internal class TriangleRenderer
{
  private static Lazy<Shader> QuadShader =
    new Lazy<Shader>(() => new Shader("Quad"));

  private float[] QuadVertices;
  private int QuadVertexBufferObject;
  private int QuadVertexArrayObject;

  private TriangleFanRenderer TriangleFanRenderer;
  private SplineRenderer SplineRenderer2;


  public TriangleRenderer(GlyphData glyphData)
  {
    TriangleFanRenderer = new TriangleFanRenderer(
      glyphData.MinX,
      glyphData.MinY,
      glyphData.Contours);

    SplineRenderer2 = new SplineRenderer(glyphData);

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

  public void OnRender(Vector2 position, float scaleFactor, Vector2i windowSize)
  {
    //GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Line);

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

    ConfigureSetencilTest();

    TriangleFanRenderer.OnRender(position, scaleFactor, windowSize);
    SplineRenderer2.Render(model, projection, PrimitiveType.Triangles);

    ConfigureStencilTestForRendering();
    RenderQuad(position, scaleFactor, windowSize);

    GL.Disable(EnableCap.StencilTest);

    //GL.PolygonMode(TriangleFace.FrontAndBack,PolygonMode.Fill);
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

}
