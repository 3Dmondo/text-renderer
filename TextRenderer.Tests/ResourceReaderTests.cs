namespace TextRenderer.Tests;

public class ResourceReaderTests
{
  [TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.Shaders))]
  public void Read(string name)
  {
    Assert.Multiple(() =>
    {
      foreach (var extension in TestCaseSources.ShaderExtensions)
      {
        var shaderName = name + extension;
        var value = ResourceReader.Read(shaderName);
        Assert.That(value, Is.Not.Null, shaderName);
        Assert.That(value, Is.Not.Empty, shaderName);
      }
    });
  }
}