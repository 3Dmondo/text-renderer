using FontParser;
using TextRenderer.Abstractions;

namespace TextRenderer;

internal class QuadRenderer : AbstractArrayRenderer
{
  private static Lazy<Shader> QuadShader =
  new Lazy<Shader>(() => new Shader("Quad"));
  protected override Shader Shader => QuadShader.Value;

  public QuadRenderer(GlyphData glyphData)
  {
    var vertices = new float[] 
    {
      glyphData.MinX, glyphData.MinY,
      glyphData.MinX, glyphData.MaxY,
      glyphData.MaxX, glyphData.MaxY,

      glyphData.MinX, glyphData.MinY,
      glyphData.MaxX, glyphData.MaxY,
      glyphData.MaxX, glyphData.MinY
    };
    InitBuffers(vertices, 2, [(2, 0)]);
  }
}
