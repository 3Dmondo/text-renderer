namespace FontParser.Extensions;

internal static class IntegralsExtensions
{
  internal readonly static DateTimeOffset TrueTypeEpoch =
    new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc);

  public static float ShortFracToFloat(this ushort value) =>
    ((short)value).ShortFracToFloat();

  public static float ShortFracToFloat(this short value) =>
    value / 16384.0f;

  public static double FixedToDouble(this uint value) =>
    ((int)value).FixedToDouble();

  public static double FixedToDouble(this int value) =>
    value / 65536.0;

  public static DateTimeOffset LongDateTimeToDateTimeOffset(this ulong longDateTime) =>
    ((long)longDateTime).LongDateTimeToDateTimeOffset();

  public static DateTimeOffset LongDateTimeToDateTimeOffset(this long longDateTime) =>
    new DateTimeOffset(
      TrueTypeEpoch.Ticks +
      Math.Max(0, longDateTime) *
      TimeSpan.TicksPerSecond, TimeSpan.Zero);
}
