namespace FontParser.Tables;

internal readonly record struct TableDirectory(
  string Tag,
  uint CheckSum,
  uint Offset,
  uint Length);
