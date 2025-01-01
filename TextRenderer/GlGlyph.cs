using FontParser;
using OpenTK.Mathematics;

namespace TextRenderer;

internal class GlGlyph
{
  private PointRenderer PointRenderer;
  private TriangleRenderer TriangleRenderer;

  public GlGlyph(GlyphData glyphData)
  {
    PointRenderer = new PointRenderer(glyphData);
    TriangleRenderer = new TriangleRenderer(glyphData);
  }

  public void OnRender(Vector2 position, float scaleFactor, Vector2i windowSize)
  {
    //PointRenderer.OnRender(position, scaleFactor, windowSize);
    TriangleRenderer.OnRender(position, scaleFactor, windowSize);
  }
}
