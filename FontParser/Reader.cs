using System.Buffers.Binary;
using System.Runtime.InteropServices;
namespace FontParser;

internal class Reader : IDisposable
{
  private static readonly bool IsLittleEndian = BitConverter.IsLittleEndian;
  private readonly Stream Stream;
  private readonly BinaryReader BinaryReader;
  private bool disposedValue;

  public Reader(string fontFilePath)
  {
    Stream = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    BinaryReader = new BinaryReader(Stream);
  }

  public long Position => Stream.Position;

  public void Seek(int position) => Seek((long)position);

  public void Seek(uint position) => Seek((long)position);

  public void Seek(long position) => Stream.Seek(position, SeekOrigin.Begin);

  public void ReadByte(Span<byte> buffer) => Stream.Read(buffer);

  public byte ReadByte() => BinaryReader.ReadByte();

  public void ReadUInt16(Span<ushort> span)
  {
    var byteSpan = MemoryMarshal.AsBytes(span);
    Stream.Read(byteSpan);
    if (IsLittleEndian)
      BinaryPrimitives.ReverseEndianness(span, span);
  }

  public ushort ReadUInt16()
  {
    var value = BinaryReader.ReadUInt16();
    if (IsLittleEndian)
      value = BinaryPrimitives.ReverseEndianness(value);
    return value;
  }

  public uint ReadUInt32()
  {
    var value = BinaryReader.ReadUInt32();
    if (IsLittleEndian)
      value = BinaryPrimitives.ReverseEndianness(value);
    return value;
  }

  public ulong ReadUint64()
  {
    var value = BinaryReader.ReadUInt64();
    if (IsLittleEndian)
      value = BinaryPrimitives.ReverseEndianness(value);
    return value;
  }

  internal string ReadString(int count)
  {
    Span<char> span = stackalloc char[count];
    for (int i = 0; i < count; i++)
      span[i] = (char)BinaryReader.ReadByte();
    return span.ToString();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!disposedValue)
    {
      if (disposing)
      {
        BinaryReader.Dispose();
        Stream.Dispose();
      }
      disposedValue = true;
    }
  }

  void IDisposable.Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

}
