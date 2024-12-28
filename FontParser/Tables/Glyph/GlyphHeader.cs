namespace FontParser.Tables.Glyph;

internal readonly record struct GlyphHeader(
  short NumberOfContours,
  short XMin,
  short YMin,
  short XMax,
  short YMax);