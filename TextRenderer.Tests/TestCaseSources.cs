namespace TextRenderer.Tests;

internal static class TestCaseSources
{
  internal const string FragExtension = ".frag";
  internal const string VertExtension = ".vert";

  internal static IEnumerable<string> Shaders { get; } = Directory
    .GetFiles($"..\\..\\..\\..\\{nameof(TextRenderer)}\\Shaders")
    .Select(f => $"{nameof(TextRenderer)}.Shaders.{Path.GetFileNameWithoutExtension(f)}")
    .Distinct();

  internal static IEnumerable<string> ShaderExtensions { get; } = 
    [FragExtension, VertExtension];
}
