using FontParser.Tables;

namespace FontParser.Tests;

[TestFixture]
public class ParserTests
{
  private readonly string[] RequiredTables =
    ["cmap",
     "glyf",
     "head",
     "hhea",
     "hmtx",
     "loca",
     "maxp",
     "name",
     "post"];

  private static IEnumerable<string> FontFileNames { get; } = Directory
      .GetFiles(Setup.FontsPath)
      .Where(f => f.EndsWith(".ttf", StringComparison.InvariantCultureIgnoreCase))
      .Select(f => Path.GetFileName(f));

  private static object[] FailingFonts =
  {
    new object[] { "corbell.ttf", 748 }
  };

  private static string FilePath(string fileName) => Path.Combine(Setup.FontsPath, fileName);

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseFont(string fileName)
  {
    var fontData = Parser.ParseFont(FilePath(fileName));
    Assert.IsNotNull(fontData);
  }

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseTableDirectory(string fileName)
  {
    using var reader = new Reader(FilePath(fileName));
    var offsetSubTable = Parser.ParseOffsetSubTable(reader);
    var tableLocations = Parser
      .ParseTableDirectory(reader, offsetSubTable.NumTables)
      .ToDictionary(x => x.Tag, x => x.Offset + x.Length);
    Assert.That(tableLocations.Keys, Is.SupersetOf(RequiredTables));
    var fileSize = new FileInfo(FilePath(fileName)).Length;
    var maxIndex = tableLocations.Values.Max();
    Assert.That(maxIndex, Is.LessThanOrEqualTo(fileSize));
  }

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseHead(string fileName)
  {
    var offset = GetTableOffset("head", fileName);
    using var reader = new Reader(FilePath(fileName));
    var head = Parser.ParseHead(reader, offset);
    Assert.That(head, Is.Not.EqualTo(default(Head)));
  }

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseMaxp(string fileName)
  {
    var offset = GetTableOffset("maxp", fileName);
    using var reader = new Reader(FilePath(fileName));
    var maxp = Parser.ParseMaxp(reader, offset);
    Assert.That(maxp, Is.Not.EqualTo(default(Maxp)));
  }

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseCmap(string fileName)
  {
    var offset = GetTableOffset("cmap", fileName);
    using var reader = new Reader(FilePath(fileName));
    var gliphMaps = Parser.ParseCmap(reader, offset).ToArray();
    Assert.That(gliphMaps, Is.Not.Empty);
  }

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseHhea(string fileName)
  {
    var offset = GetTableOffset("hhea", fileName);
    using var reader = new Reader(FilePath(fileName));
    var hhea = Parser.ParseHhea(reader, offset);
    Assert.That(hhea, Is.Not.EqualTo(default(Hhea)));
  }

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseHmtx(string fileName)
  {
    var offset = GetTableOffset("maxp", fileName);
    using var reader = new Reader(FilePath(fileName));
    var maxp = Parser.ParseMaxp(reader, offset);

    offset = GetTableOffset("hhea", fileName);
    var hhea = Parser.ParseHhea(reader, offset);

    offset = GetTableOffset("hmtx", fileName);
    var hmtx = Parser.ParseHmtx(
      reader,
      offset,
      hhea.NumOfLongHorMetrics,
      maxp.NumGlyphs)
      .ToArray();

    Assert.That(hmtx.Length, Is.EqualTo(maxp.NumGlyphs));
  }

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseLoca(string fileName)
  {
    var offset = GetTableOffset("maxp", fileName);
    using var reader = new Reader(FilePath(fileName));
    var maxp = Parser.ParseMaxp(reader, offset);

    offset = GetTableOffset("head", fileName);
    var head = Parser.ParseHead(reader, offset);

    offset = GetTableOffset("loca", fileName);
    var loca = Parser.ParseLoca(
      reader,
      offset,
      head.IndexToLocFormat,
      maxp.NumGlyphs,
      GetTableOffset("glyf", fileName))
      .ToArray();

    Assert.That(loca.Length, Is.EqualTo(maxp.NumGlyphs));
    Assert.That(loca, Is.Ordered.Ascending);
  }

  [TestCaseSource(nameof(FontFileNames))]
  public void ParseGlyf(string fileName)
  {
    var offset = GetTableOffset("maxp", fileName);
    using var reader = new Reader(FilePath(fileName));
    var maxp = Parser.ParseMaxp(reader, offset);

    offset = GetTableOffset("head", fileName);
    var head = Parser.ParseHead(reader, offset);

    offset = GetTableOffset("loca", fileName);
    var loca = Parser.ParseLoca(
      reader,
      offset,
      head.IndexToLocFormat,
      maxp.NumGlyphs,
      GetTableOffset("glyf", fileName))
      .ToArray();

    var glyphs = Parser.ParseGlyf(reader, loca).ToArray();

    Assert.Multiple(() =>
    {
      for (int i = 0; i < glyphs.Length; i++)
      {
        var glyph = glyphs[i];
        Assert.That(glyph, Is.Not.Null);
        Assert.That(glyph.YCoordinates.Length, Is.EqualTo(glyph.XCoordinates.Length));
        Assert.That(glyph.YCoordinates.Length, Is.EqualTo(glyph.OnCurve.Length));
        if (glyph.YCoordinates.Length > 0)
        {
          Assert.That(glyph.XCoordinates.Max(), Is.AtMost(glyph.Header.XMax), $"x coordinate greater than Max at index {glyph.index}");
          Assert.That(glyph.XCoordinates.Min(), Is.AtLeast(glyph.Header.XMin), $"x coordinate smaller than Min at index {glyph.index}");
          Assert.That(glyph.YCoordinates.Max(), Is.AtMost(glyph.Header.YMax), $"y coordinate greater than Max at index {glyph.index}");
          Assert.That(glyph.YCoordinates.Min(), Is.AtLeast(glyph.Header.YMin), $"y coordinate smaller than Min at index {glyph.index}");
        }
      }
    });
  }

  [TestCaseSource(nameof(FailingFonts))]
  public void ParseGlyph(string fileName, int index)
  {
    var offset = GetTableOffset("maxp", fileName);
    using var reader = new Reader(FilePath(fileName));
    var maxp = Parser.ParseMaxp(reader, offset);

    offset = GetTableOffset("head", fileName);
    var head = Parser.ParseHead(reader, offset);

    offset = GetTableOffset("loca", fileName);
    var loca = Parser.ParseLoca(
      reader,
      offset,
      head.IndexToLocFormat,
      maxp.NumGlyphs,
      GetTableOffset("glyf", fileName))
      .ToArray();

    var glyph = Parser.ParseGlyph(reader, loca, index);

    Assert.That(glyph, Is.Not.Null);
    Assert.That(glyph.YCoordinates.Length, Is.EqualTo(glyph.XCoordinates.Length));

    var points = glyph.XCoordinates
      .Zip(glyph.YCoordinates, glyph.OnCurve)
      .Select((p, i) => (i, x: p.First, y: p.Second, onCurve: p.Third))
      .Where(x => x.onCurve);

    foreach (var p in points)
    {
      Assert.That(p.x, Is.AtMost(glyph.Header.XMax).Within(1), $"x coordinate greater than Max of point {p.i}");
      Assert.That(p.x, Is.AtLeast(glyph.Header.XMin).Within(1), $"x coordinate smaller than Min of point {p.i}");
      Assert.That(p.y, Is.AtMost(glyph.Header.YMax).Within(1), $"y coordinate greater than Max of point {p.i}");
      Assert.That(p.y, Is.AtLeast(glyph.Header.YMin).Within(1), $"y coordinate smaller than Min of point {p.i}");
    }
  }

  private static uint GetTableOffset(string tag, string fileName)
  {
    using var reader = new Reader(FilePath(fileName));
    var offsetSubTable = Parser.ParseOffsetSubTable(reader);
    return Parser
      .ParseTableDirectory(reader, offsetSubTable.NumTables)
      .Where(x => x.Tag == tag)
      .Select(x => x.Offset)
      .First();
  }
}
