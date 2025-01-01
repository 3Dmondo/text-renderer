namespace TextRenderer;

internal static class ResourceReader
{
  public static string Read(string name)
  {
    var assembly = typeof(ResourceReader).Assembly;
    using var stream = assembly.GetManifestResourceStream(name);
    using var reader = new StreamReader(stream!);
    return reader.ReadToEnd();
  }
}
