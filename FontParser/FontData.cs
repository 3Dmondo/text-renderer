using System.Diagnostics;

namespace FontParser;

public class FontData
{
  private Dictionary<uint, GlyphData> Glyphs;
  private GlyphData MissingChar;
  public int UnitsPerEm { get; }
  public FontData(
    int unitsPerEm, 
    Dictionary<uint, GlyphData> glyphData,
    GlyphData missingChar)
  {
    UnitsPerEm = unitsPerEm;
    Glyphs = glyphData;
    MissingChar = missingChar;
  }
  public GlyphData this[uint unicode]
  {
    get
    {
      if (Glyphs.TryGetValue(unicode, out GlyphData? glyphData))
        return glyphData;
      return MissingChar;
    }
  }
}

[DebuggerDisplay("GlyphIndex = {GlyphIndex}")]
public class GlyphData
{
  public required int GlyphIndex { get; init; }
  public required Point[][] Contours { get; init; }
  public required int MinX { get; init; }
  public required int MinY { get; init; }
  public required int MaxX { get; init; }
  public required int MaxY { get; init; }
  public int AdvanceWidth { get; init; }
  public int LeftSideBearing { get; init; }
}

public readonly record struct Point(float X, float Y, bool OnCurve);