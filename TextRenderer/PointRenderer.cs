using FontParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using TextRenderer.Abstractions;

namespace TextRenderer;

internal class PointRenderer : AbstractArrayRenderer
{
  private static Lazy<Shader> PointShader =
    new Lazy<Shader>(() => new Shader("Point"));
  protected override Shader Shader => PointShader.Value;

  public PointRenderer(GlyphData glyphData)
  {
    var vertices = glyphData
      .Contours
      .SelectMany(c => c.SelectMany(GetFloats))
      .ToArray();

    InitBuffers(vertices, 3, [(2,0),(1,2)]);
  }

  public override void Render(Matrix4 model, Matrix4 projection, PrimitiveType primitiveType)
  {
    GL.Enable(EnableCap.PointSprite);
    GL.Enable(EnableCap.VertexProgramPointSize);
    base.Render(model, projection, primitiveType);
    GL.Disable(EnableCap.PointSprite);
    GL.Disable(EnableCap.VertexProgramPointSize);
  }

  private static IEnumerable<float> GetFloats(Point p)
  {
    yield return p.X;
    yield return p.Y;
    yield return p.OnCurve ? 1f : 0f;
  }
}
