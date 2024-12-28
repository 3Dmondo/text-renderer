namespace FontParser.Tables.Glyph;

[Flags]
internal enum GlyphFlag : byte
{
  None = 0,
  OnCurve = 1,
  XShort = 2,
  YShort = 4,
  Repeat = 8,
  InstructionX = 16,
  InstructionY = 32
}
