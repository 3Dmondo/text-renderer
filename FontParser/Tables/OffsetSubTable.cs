namespace FontParser.Tables;

internal readonly record struct OffsetSubTable(
  uint ScalerType,
  ushort NumTables,
  ushort SearchRange,
  ushort EntrySelector,
  ushort RangeShift);
