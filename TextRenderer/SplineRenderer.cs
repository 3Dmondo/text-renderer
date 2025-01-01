using FontParser;
using System.Diagnostics;
using TextRenderer.Abstractions;

namespace TextRenderer;

internal class SplineRenderer : AbstractArrayRenderer
{
  private static Lazy<Shader> SplineShader =
    new Lazy<Shader>(() => new Shader("Spline"));
  protected override Shader Shader => SplineShader.Value;

  public SplineRenderer(GlyphData glyphData)
  {
    var vertices = glyphData.Contours.SelectMany(GetTriangles).ToArray();
    InitBuffers(vertices, 2, [(2, 0)]);
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
