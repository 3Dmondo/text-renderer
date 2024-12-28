using FontParser.Extensions;

namespace FontParser.Tests;

[TestFixture]
public class IntegralsExtensionsTests
{
  [Test, Sequential]
  public void ShortFracToFloat(
    [Values(0x7fff, 0x8000, 0x4000, 0xc000)] int value,
    [Values(1.99993896f, -2.0f, 1.0f, -1.0f)] float expected)
  {
    var result = ((ushort)value).ShortFracToFloat();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test, Sequential]
  public void FixedToDouble(
  [Values(0x00018000u)] uint value,
  [Values(1.5)] double expected)
  {
    var result = value.FixedToDouble();
    Assert.That(result, Is.EqualTo(expected));
  }

  [Test]
  public void LongDateTimeToDateTimeOffset()
  {
    long longDateTime = 3817709820;
    var result = longDateTime.LongDateTimeToDateTimeOffset();
    DateTimeOffset expected = new DateTime(2024, 12, 22, 10, 57, 0, DateTimeKind.Utc);
    Assert.That(result, Is.EqualTo(expected));
  }
}
