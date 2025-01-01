using FontParser;
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
    var start = c.Select((p, i) => (p, i)).First(pi => pi.p.OnCurve).i;
    for (int i = start; i < c.Length + start - 1; i += 2)
    {
      yield return c[i % c.Length].X;
      yield return c[i % c.Length].Y;
      yield return c[(i + 1) % c.Length].X;
      yield return c[(i + 1) % c.Length].Y;
      yield return c[(i + 2) % c.Length].X;
      yield return c[(i + 2) % c.Length].Y;
    }
  }
}
