namespace FontParser.Tables;

internal readonly record struct Maxp(
  double Version,
  ushort NumGlyphs,
  ushort MaxPoints,
  ushort MaxContours,
  ushort MaxComponentPoints,
  ushort MaxComponentContours,
  ushort MaxZones,
  ushort MaxTwilightPoints,
  ushort MaxStorage,
  ushort MaxFunctionDefs,
  ushort MaxInstructionDefs,
  ushort MaxStackElements,
  ushort MaxSizeOfInstructions,
  ushort MaxComponentElements,
  ushort MaxComponentDepth);
