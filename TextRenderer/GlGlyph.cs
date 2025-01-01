using FontParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TextRenderer;

internal class GlGlyph
{
  private PointRenderer PointRenderer;
  private TriangleFanRenderer[] TriangleFanRenderers;
  private SplineRenderer SplineRenderer;
  private QuadRenderer QuadRenderer;

  public GlGlyph(GlyphData glyphData)
  {
    PointRenderer = new PointRenderer(glyphData);
    TriangleFanRenderers = glyphData
      .Contours
      .Select(c => new TriangleFanRenderer(
         glyphData.MinX,
         glyphData.MinY,
         c))
      .ToArray();
    SplineRenderer = new SplineRenderer(glyphData);
    QuadRenderer = new QuadRenderer(glyphData);
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

    ConfigureSetencilTest();
    foreach (var triangleFanRenderer in TriangleFanRenderers)
      triangleFanRenderer.Render(model, projection, PrimitiveType.TriangleFan);
    SplineRenderer.Render(model, projection, PrimitiveType.Triangles);

    ConfigureStencilRendering();
    QuadRenderer.Render(model, projection, PrimitiveType.Triangles);

    GL.Disable(EnableCap.StencilTest);

    //PointRenderer.Render(model, projection, PrimitiveType.Points);
  }

  private static void ConfigureStencilRendering()
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
