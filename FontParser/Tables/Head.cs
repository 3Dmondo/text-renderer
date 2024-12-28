namespace FontParser.Tables;

internal readonly record struct Head(
  double Version,
  double FontRevision,
  uint CheckSumAdjustment,
  uint MagicNumber,
  ushort Flags,
  ushort UnitsPerEm,
  DateTimeOffset Created,
  DateTimeOffset Modified,
  short XMin,
  short YMin,
  short XMax,
  short YMax,
  ushort MacStyle,
  ushort LowestRecPPEM,
  short FontDirectionHint,
  short IndexToLocFormat,
  short GlyphDataFormat);
