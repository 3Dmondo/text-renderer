namespace FontParser.Tests;

[SetUpFixture]
public class Setup
{
  private const string FontName = "PARCHM.TTF";
  public static readonly string FontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
  public static readonly string FontPath = Path.Combine(FontsPath, FontName);

  [OneTimeSetUp]
  public void BeforeAnyTest()
  {
    Assert.That(Directory.Exists(FontsPath), Is.True, $"The path {FontsPath} does not exist");
    Assert.That(File.Exists(FontPath), Is.True, $"The file {FontPath} does not exist");
  }
}
