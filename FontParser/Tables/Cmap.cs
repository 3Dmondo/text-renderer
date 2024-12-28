namespace FontParser.Tables;

internal readonly record struct GlyphMap(uint Index, uint Unicode);

internal readonly record struct CmapIndex(
  ushort Version,
  ushort NumberSubtables);

internal readonly record struct Cmap(
  ushort PlatformId,
  ushort PlatformSpecificId,
  uint Offset);

internal readonly record struct CmapFormat4(
  ushort Format,
  ushort Length,
  ushort Language,
  ushort SegCountX2,
  ushort SearchRange,
  ushort EntrySelector,
  ushort RangeShift);

internal readonly record struct CmapFormat12(
  ushort Format,
  ushort Reserved,
  uint Length,
  uint Language,
  uint NGroups);
