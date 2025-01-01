using FontParser;
using TextRenderer.Abstractions;

namespace TextRenderer;

internal class TriangleFanRenderer : AbstractArrayRenderer
{
  private static Lazy<Shader> TriangleFanShader =
    new Lazy<Shader>(() => new Shader("TriangleFan"));
  protected override Shader Shader => TriangleFanShader.Value;

  public TriangleFanRenderer(float minX, float minY, Point[] contour)
  {
      var vertices = new float[] { minX, minY }
        .Concat(GetOnCurve(contour))
        .ToArray();
    InitBuffers(vertices, 2, [(2, 0)]);
  }

  private static IEnumerable<float> GetOnCurve(Point[] c)
  {
    foreach (Point p in c)
      if (p.OnCurve)
      {
        yield return p.X;
        yield return p.Y;
      }
    var first = c.First(p => p.OnCurve);
    yield return first.X;
    yield return first.Y;
  }
}
