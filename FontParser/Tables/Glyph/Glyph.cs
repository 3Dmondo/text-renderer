namespace FontParser.Tables.Glyph;

internal class Glyph
{
  public required GlyphHeader Header { get; init; }
  public required int[] XCoordinates { get; init; }
  public required int[] YCoordinates { get; init; }
  public required bool[] OnCurve { get; init; }
  public required ushort[] EndPtsOfContours { get; init; }
}
