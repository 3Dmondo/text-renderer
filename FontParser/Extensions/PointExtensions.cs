namespace FontParser.Extensions;

internal static class PointExtensions
{
  public static IEnumerable<Point> AddImpliedPoints(this IEnumerable<Point> points) =>
    points
    .Zip(points.Skip(1))
    .SelectMany(AddMiddleIfNeeded)
    .Concat(AddMiddleIfNeeded((points.Last(), points.First())));

  private static IEnumerable<Point> AddMiddleIfNeeded((Point First, Point Second) pair)
  {
    yield return pair.First;
    if (!(pair.First.OnCurve ^ pair.Second.OnCurve))
      yield return new Point(
        (pair.First.X + pair.Second.X) / 2,
        (pair.First.Y + pair.Second.Y) / 2,
        !pair.First.OnCurve);
  }
}

