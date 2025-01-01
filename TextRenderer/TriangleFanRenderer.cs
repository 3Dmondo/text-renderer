using FontParser;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace TextRenderer;

internal class TriangleFanRenderer
{
  private static Lazy<Shader> TriangleFanShader =
    new Lazy<Shader>(() => new Shader("Triangle"));

  private float[][] TriangleFanVertices;
  private int[] TriangleFanVertexBufferObject;
  private int[] TriangleFanVertexArrayObject;

  public TriangleFanRenderer(float minX, float minY, Point[][] contours)
  {
    TriangleFanVertices = new float[contours.Length][];
    TriangleFanVertexBufferObject = new int[contours.Length];
    TriangleFanVertexArrayObject = new int[contours.Length];

    for (int i = 0; i < contours.Length; i++)
    {
      TriangleFanVertices[i] = new float[] { minX, minY }
        .Concat(GetOnCurve(contours[i]))
        .ToArray();

      TriangleFanVertexArrayObject[i] = GL.GenVertexArray();
      GL.BindVertexArray(TriangleFanVertexArrayObject[i]);
      TriangleFanVertexBufferObject[i] = GL.GenBuffer();
      GL.BindBuffer(BufferTarget.ArrayBuffer, TriangleFanVertexBufferObject[i]);
      GL.BufferData(
        BufferTarget.ArrayBuffer,
        TriangleFanVertices[i].Length * sizeof(float),
        TriangleFanVertices[i],
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

    TriangleFanShader.Value.Use();
    GL.UniformMatrix4(0, false, ref model);
    GL.UniformMatrix4(1, false, ref projection);
    for (int i = 0; i < TriangleFanVertices.Length; i++)
    {
      GL.BindVertexArray(TriangleFanVertexArrayObject[i]);
      GL.DrawArrays(PrimitiveType.TriangleFan, 0, TriangleFanVertices[i].Length / 2);

    }

    GL.BindVertexArray(0);
    GL.UseProgram(0);

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
