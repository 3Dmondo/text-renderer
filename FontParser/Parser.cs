using System.Runtime.CompilerServices;
using FontParser.Extensions;
using FontParser.Tables;
using FontParser.Tables.Glyph;
namespace FontParser;

public class Parser
{
  public static FontData ParseFont(string fontPath)
  {
    using var reader = new Reader(fontPath);
    var offsetSubTable = ParseOffsetSubTable(reader);
    var tableLocations = ParseTableDirectory(reader, offsetSubTable.NumTables)
      .ToDictionary(x => x.Tag, x => x.Offset);
    var head = ParseHead(reader, tableLocations["head"]);
    var maxp = ParseMaxp(reader, tableLocations["maxp"]);
    var glyphMap = ParseCmap(reader, tableLocations["cmap"])
      .ToArray();
    var hhea = ParseHhea(reader, tableLocations["hhea"]);
    var hmtx = ParseHmtx(
        reader,
        tableLocations["hmtx"],
        hhea.NumOfLongHorMetrics,
        maxp.NumGlyphs)
      .ToArray();
    var loca = ParseLoca(
        reader,
        tableLocations["loca"],
        head.IndexToLocFormat,
        maxp.NumGlyphs,
        tableLocations["glyf"])
      .ToArray();

    var glyphDatas = ParseGlyf(reader, loca)
      .Zip(hmtx)
      .Select(pair =>
      {
        var points = pair.First.XCoordinates
          .Zip(pair.First.YCoordinates, pair.First.OnCurve)
          .Select(x => new Point(x.First, x.Second, x.Third))
          .ToArray();
        var contours = new Point[pair.First.EndPtsOfContours.Length][];
        var firstIndex = 0;
        for (int i = 0; i < pair.First.EndPtsOfContours.Length; i++)
        {
          var endPointOfContour = pair.First.EndPtsOfContours[i];
          var lastIndex = endPointOfContour + 1;
          contours[i] = points[firstIndex..lastIndex].AddImpliedPoints().ToArray();
          firstIndex = lastIndex;
        }
        return new GlyphData
        {
          GlyphIndex = pair.First.index,
          AdvanceWidth = pair.Second.AdvanceWidth,
          LeftSideBearing = pair.Second.LeftSideBearing,
          MinX = pair.First.Header.XMin,
          MinY = pair.First.Header.YMin,
          MaxX = pair.First.Header.XMax,
          MaxY = pair.First.Header.YMax,
          Contours = contours
        };
      })
      .ToArray();

    var glyphData = glyphMap
      .DistinctBy(c => c.Unicode)
      .Where(c => c.Index < glyphDatas.Length)
      .ToDictionary(
        x => x.Unicode,
        x => glyphDatas[x.Index]);

    return new FontData(head.UnitsPerEm, glyphData, glyphDatas[0]);
  }

  internal static OffsetSubTable ParseOffsetSubTable(Reader reader)
  {
    return new OffsetSubTable(
      reader.ReadUInt32(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16());
  }

  internal static IEnumerable<TableDirectory> ParseTableDirectory(
    Reader reader,
    int numTables)
  {
    reader.Seek(Unsafe.SizeOf<OffsetSubTable>());
    while (numTables-- > 0)
      yield return new TableDirectory(
        reader.ReadString(4),
        reader.ReadUInt32(),
        reader.ReadUInt32(),
        reader.ReadUInt32());
  }

  internal static Head ParseHead(Reader reader, uint offset)
  {
    reader.Seek(offset);
    return new Head(
      reader.ReadUInt32().FixedToDouble(),
      reader.ReadUInt32().FixedToDouble(),
      reader.ReadUInt32(),
      reader.ReadUInt32(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUint64().LongDateTimeToDateTimeOffset(),
      reader.ReadUint64().LongDateTimeToDateTimeOffset(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16());
  }

  internal static Maxp ParseMaxp(Reader reader, uint offset)
  {
    reader.Seek(offset);
    return new Maxp(
      reader.ReadUInt32().FixedToDouble(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16());
  }

  internal static IEnumerable<GlyphMap> ParseCmap(Reader reader, uint offset)
  {
    reader.Seek(offset);
    var cmapIndex = new CmapIndex(
      reader.ReadUInt16(),
      reader.ReadUInt16());
    var numberOfSubTables = cmapIndex.NumberSubtables;

    var cmaps = ParseCmaps(reader, numberOfSubTables);

    var (cmapOffset, _) = cmaps.Aggregate(
     (cmapOffset: 0u, unicodeVersion: (short)-1),
     (accumulator, c) =>
     {
       switch (c.PlatformId)
       {
         case 0:
           if (c.PlatformSpecificId is 0 or 1 or 3 or 4 && c.PlatformSpecificId >= accumulator.unicodeVersion)
           {
             accumulator.unicodeVersion = (short)c.PlatformSpecificId;
             accumulator.cmapOffset = c.Offset;
           }
           return accumulator;
         case 3:
           if (accumulator.unicodeVersion == -1 && c.PlatformSpecificId is 0 or 1 or 10)
             accumulator.cmapOffset = c.Offset;
           return accumulator;
         default:
           return accumulator;
       }
     });

    if (cmapOffset == 0)
      throw new NotSupportedException("Font does not contain supported character map type");

    reader.Seek(offset + cmapOffset);
    var format = reader.ReadUInt16();

    switch (format)
    {
      case 4:
        return ParseCmapFormat4(reader);
      case 12:
        return ParseCmapFormat12(reader);
      default:
        throw new NotSupportedException($"Cmap format {format} is not supported");
    }
  }

  private static IEnumerable<Cmap> ParseCmaps(Reader reader, uint count)
  {
    while (count-- > 0)
      yield return new Cmap(
        reader.ReadUInt16(),
        reader.ReadUInt16(),
        reader.ReadUInt32());
  }

  private static IEnumerable<GlyphMap> ParseCmapFormat4(Reader reader)
  {
    var cmapFormat4 = new CmapFormat4(
      4,
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16(),
      reader.ReadUInt16());

    var segCount = cmapFormat4.SegCountX2 / 2;

    var endCodes = new ushort[segCount];
    reader.ReadUInt16(endCodes);

    _ = reader.ReadUInt16();

    var startCodes = new ushort[segCount];
    reader.ReadUInt16(startCodes);

    var idDeltas = new ushort[segCount];
    reader.ReadUInt16(idDeltas);

    var idRangeOffsetPosition = reader.Position;
    var idRangeOffsets = new ushort[segCount];
    reader.ReadUInt16(idRangeOffsets);

    for (int i = 0; i < segCount; i++, idRangeOffsetPosition += sizeof(ushort))
    {
      var endCode = endCodes[i];
      if (endCode == 0xFFFF) break;
      var startCode = startCodes[i];
      for (var currCode = startCode; currCode <= endCode; currCode++)
      {
        var idRangeOffset = idRangeOffsets[i];
        if (idRangeOffset == 0)
        {
          var gliphIndex = (currCode + idDeltas[i]) % 65536;
          yield return new GlyphMap((uint)gliphIndex, currCode);
        }
        else
        {
          var rangeOffsetLocation = idRangeOffset + idRangeOffsetPosition;
          var glyphIndexArrayLocation = 2 * (currCode - startCode) + rangeOffsetLocation;
          reader.Seek(glyphIndexArrayLocation);
          var gliphIndex = reader.ReadUInt16();
          if (gliphIndex != 0)
            gliphIndex = (ushort)((gliphIndex + idDeltas[i]) % 65536);
          yield return new GlyphMap(gliphIndex, currCode);
        }
      }
    }
  }

  private static IEnumerable<GlyphMap> ParseCmapFormat12(Reader reader)
  {
    var cmapFormat12 = new CmapFormat12(
      12,
      reader.ReadUInt16(),
      reader.ReadUInt32(),
      reader.ReadUInt32(),
      reader.ReadUInt32());
    var nGroups = cmapFormat12.NGroups;

    while (nGroups-- > 0)
    {
      var startCharCode = reader.ReadUInt32();
      var endCharCode = reader.ReadUInt32();
      var startGliphIndex = reader.ReadUInt32();
      while (startCharCode <= endCharCode)
        yield return new GlyphMap(startCharCode++, startGliphIndex++);
    }
  }

  internal static Hhea ParseHhea(Reader reader, uint offset)
  {
    reader.Seek(offset);
    return new Hhea(
      reader.ReadUInt32().FixedToDouble(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      reader.ReadUInt16());
  }

  internal static IEnumerable<LongHorMetrics> ParseHmtx(
    Reader reader,
    uint offset,
    ushort numOfLongHorMetrics,
    ushort numGlyphs)
  {
    reader.Seek(offset);
    ushort lastAdvanceWidth = 0;
    int glyphIndex = 0;
    for (; glyphIndex < numOfLongHorMetrics; glyphIndex++)
    {
      lastAdvanceWidth = reader.ReadUInt16();
      yield return new LongHorMetrics(
        lastAdvanceWidth,
        (short)reader.ReadUInt16());
    }
    for (; glyphIndex < numGlyphs; glyphIndex++)
      yield return new LongHorMetrics(
        lastAdvanceWidth,
        (short)reader.ReadUInt16());
  }

  internal static IEnumerable<uint> ParseLoca(
    Reader reader,
    uint offset,
    short indexToLocFormat,
    ushort numGlyphs,
    uint glyfOffset)
  {
    Func<Reader, uint> readFunction = indexToLocFormat == 0
      ? r => 2u * r.ReadUInt16()
      : r => r.ReadUInt32();
    reader.Seek(offset);
    while (numGlyphs-- > 0)
      yield return readFunction(reader) + glyfOffset;
  }

  internal static IEnumerable<Glyph> ParseGlyf(Reader reader, IList<uint> glyphOffsets)
  {
    for (int i = 0; i < glyphOffsets.Count; i++)
      yield return ParseGlyph(reader, glyphOffsets, i);
  }

  internal static Glyph ParseGlyph(Reader reader, IList<uint> glyphOffsets, int glyphIndex)
  {
    reader.Seek(glyphOffsets[glyphIndex]);
    var glyphHeader = new GlyphHeader(
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16(),
      (short)reader.ReadUInt16());

    if (glyphHeader.NumberOfContours >= 0)
      return ParseSimpleGlyph(reader, glyphHeader, glyphIndex);
    else
      return ParseCompoundGlyph(reader, glyphHeader, glyphOffsets, glyphIndex);
  }

  private static Glyph ParseSimpleGlyph(Reader reader, GlyphHeader glyphHeader, int glyphIndex)
  {
    Span<ushort> endPtsOfContours = stackalloc ushort[glyphHeader.NumberOfContours];
    reader.ReadUInt16(endPtsOfContours);

    var instructionLength = reader.ReadUInt16();
    Span<byte> instructions = stackalloc byte[instructionLength];
    reader.ReadByte(instructions);

    var numPoints = 0;
    for (int i = 0; i < glyphHeader.NumberOfContours; i++)
      numPoints = Math.Max(numPoints, endPtsOfContours[i] + 1);

    Span<GlyphFlag> flags = stackalloc GlyphFlag[numPoints];

    for (int i = 0; i < numPoints; i++)
    {
      var flag = (GlyphFlag)reader.ReadByte();
      flags[i] = flag;
      if ((flag & GlyphFlag.Repeat) == GlyphFlag.Repeat)
      {
        var count = reader.ReadByte();
        for (int j = 0; j < count; j++)
          flags[++i] = flag;
      }
    }

    Span<int> XCoordinates = stackalloc int[numPoints];
    ReadCoordinates(reader, XCoordinates, flags, GlyphFlag.XShort, GlyphFlag.InstructionX);

    Span<int> YCoordinates = stackalloc int[numPoints];
    ReadCoordinates(reader, YCoordinates, flags, GlyphFlag.YShort, GlyphFlag.InstructionY);

    Span<bool> OnCurve = stackalloc bool[numPoints];
    for (int i = 0; i < numPoints; i++)
      OnCurve[i] = flags[i].HasFlag(GlyphFlag.OnCurve);

    return new Glyph
    {
      index = glyphIndex,
      Header = glyphHeader,
      XCoordinates = XCoordinates.ToArray(),
      YCoordinates = YCoordinates.ToArray(),
      OnCurve = OnCurve.ToArray(),
      EndPtsOfContours = endPtsOfContours.ToArray()
    };
  }

  private static void ReadCoordinates(
    Reader reader,
    Span<int> coordinates,
    Span<GlyphFlag> flags,
    GlyphFlag isShort,
    GlyphFlag instruction)
  {
    int value = 0;
    for (int i = 0; i < coordinates.Length; i++)
    {
      var flag = flags[i];
      if (flag.HasFlag(isShort))
      {
        int offset = reader.ReadByte();
        value += flag.HasFlag(instruction) ? offset : -offset;
      }
      else if (!flag.HasFlag(instruction))
        value += (short)reader.ReadUInt16();
      coordinates[i] = value;
    }
  }

  private static Glyph ParseCompoundGlyph(
    Reader reader,
    GlyphHeader glyphHeader,
    IList<uint> glyphOffsets,
    int glyphIndex)
  {
    bool moreComponents = true;
    var xCoordinates = new List<int>();
    var yCoordinates = new List<int>();
    var onCurve = new List<bool>();
    var endPtsOfContours = new List<ushort>();

    while (moreComponents)
    {
      var flag = (CompoundGlyphFlag)reader.ReadUInt16();
      if (!flag.HasFlag(CompoundGlyphFlag.ARGS_ARE_XY_VALUES))
        throw new NotImplementedException("Compound glyphs referencing points is not implemented");

      var componentGlyphIndex = reader.ReadUInt16();

      var readWords = flag.HasFlag(CompoundGlyphFlag.ARG_1_AND_2_ARE_WORDS);
      int arg1 = readWords
        ? (short)reader.ReadUInt16()
        : (sbyte)reader.ReadByte();
      int arg2 = readWords
        ? (short)reader.ReadUInt16()
        : (sbyte)reader.ReadByte();

      double a = 1.0;
      double b = 0.0;
      double c = 0.0;
      double d = 1.0;
      double e = arg1;
      double f = arg2;

      if (flag.HasFlag(CompoundGlyphFlag.WE_HAVE_A_SCALE))
      {
        a = reader.ReadUInt16().ShortFracToFloat();
        d = a;
      }
      else if (flag.HasFlag(CompoundGlyphFlag.WE_HAVE_AN_X_AND_Y_SCALE))
      {
        a = reader.ReadUInt16().ShortFracToFloat();
        d = reader.ReadUInt16().ShortFracToFloat();
      }
      else if (flag.HasFlag(CompoundGlyphFlag.WE_HAVE_A_TWO_BY_TWO))
      {
        a = reader.ReadUInt16().ShortFracToFloat();
        b = reader.ReadUInt16().ShortFracToFloat();
        c = reader.ReadUInt16().ShortFracToFloat();
        d = reader.ReadUInt16().ShortFracToFloat();
      }

      var position = reader.Position;

      var component = ParseGlyph(reader, glyphOffsets, componentGlyphIndex);

      ushort currentLength = (ushort)onCurve.Count;

      for (int i = 0; i < component.OnCurve.Length; i++)
      {
        var x = (int)(a * component.XCoordinates[i] + c * component.YCoordinates[i] + e);
        xCoordinates.Add(x);
        var y = (int)(b * component.XCoordinates[i] + d * component.YCoordinates[i] + f);
        yCoordinates.Add(y);
        onCurve.Add(component.OnCurve[i]);
      }

      endPtsOfContours.AddRange(component.EndPtsOfContours.Select(x => (ushort)(x + currentLength)));

      reader.Seek(position);
      moreComponents = flag.HasFlag(CompoundGlyphFlag.MORE_COMPONENTS);
    }

    return new Glyph
    {
      index = glyphIndex,
      Header = glyphHeader,
      XCoordinates = xCoordinates.ToArray(),
      YCoordinates = yCoordinates.ToArray(),
      OnCurve = onCurve.ToArray(),
      EndPtsOfContours = endPtsOfContours.ToArray()
    };
  }
}
