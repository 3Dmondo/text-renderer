using FontParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TextRenderer;

internal class TriangleRenderer
{
  private TriangleFanRenderer TriangleFanRenderer;
  private SplineRenderer SplineRenderer;
  private QuadRenderer QuadRenderer;

  public TriangleRenderer(GlyphData glyphData)
  {
    TriangleFanRenderer = new TriangleFanRenderer(
      glyphData.MinX,
      glyphData.MinY,
      glyphData.Contours);
    SplineRenderer = new SplineRenderer(glyphData);
    QuadRenderer = new QuadRenderer(glyphData);
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
    SplineRenderer.Render(model, projection, PrimitiveType.Triangles);

    ConfigureStencilTestForRendering();
    QuadRenderer.Render(model, projection, PrimitiveType.Triangles);

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
}
