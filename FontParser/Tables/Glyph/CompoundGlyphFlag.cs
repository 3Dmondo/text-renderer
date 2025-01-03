﻿namespace FontParser.Tables.Glyph;

[Flags]
internal enum CompoundGlyphFlag : ushort
{
  None = 0,
  ARG_1_AND_2_ARE_WORDS = 1,
  ARGS_ARE_XY_VALUES = 2,
  ROUND_XY_TO_GRID = 4,
  WE_HAVE_A_SCALE = 8,
  Obsolete = 16,
  MORE_COMPONENTS = 32,
  WE_HAVE_AN_X_AND_Y_SCALE = 64,
  WE_HAVE_A_TWO_BY_TWO = 128,
  WE_HAVE_INSTRUCTIONS = 256,
  USE_MY_METRICS = 512,
  OVERLAP_COMPOUND = 1024
}
