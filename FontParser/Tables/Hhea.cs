namespace FontParser.Tables;

internal readonly record struct Hhea(
  double Version,
  short Ascent,
  short Descent,
  short LineGap,
  ushort AdvanceWidthMax,
  short MinLeftSideBearing,
  short MinRightSideBearing,
  short XMaxExtent,
  short CaretSlopeRise,
  short CaretSlopeRun,
  short CaretOffset,
  short Reserved0,
  short Reserved1,
  short Reserved2,
  short Reserved3,
  short MetricDataFormat,
  ushort NumOfLongHorMetrics);
