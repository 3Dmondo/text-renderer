using FontParser;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TextRenderer;

public class SingleCharRenderer
{
  private const string FontName = "arial.ttf"; //"PARCHM.TTF";

  public static readonly string FontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
  public static readonly string FontPath = Path.Combine(FontsPath, FontName);
  private FontData FontData { get; set; }
  private uint Character { get; set; } = 'P';
  private bool FontChanged { get; set; } = true;
  private bool CharacterChanged { get; set; } = true;
  private GlGlyph? GlGlyph { get; set; }
  private GameWindow Window { get; }
  private float Scale { get; set; } = 0.1f;
  private Vector2 Position { get; set; }
  private Vector2 MousePosition { get; set; }

  public SingleCharRenderer(GameWindow window)
  {
    window.RenderFrame += OnRenderFrame;
    window.TextInput += OnTextInput;
    window.FileDrop += OnFileDrop;
    window.UpdateFrame += OnUpdateFrame;
    window.MouseWheel += OnMouseWheel;
    window.MouseMove += OnMouseMove;
    FontData = Parser.ParseFont(FontPath);
    Window = window;
    Position = Window.Size / 2;
  }

  private void OnMouseMove(MouseMoveEventArgs args)
  {
    MousePosition = args.Position;
    if (Window.IsMouseButtonDown(OpenTK.Windowing.GraphicsLibraryFramework.MouseButton.Left))
      Position += new Vector2(args.Delta.X, -args.DeltaY);
  }

  private void OnMouseWheel(MouseWheelEventArgs args)
  {
    Scale *= 1.0f + args.OffsetY * 0.1f;
  }

  private void OnUpdateFrame(FrameEventArgs args)
  {
    if (FontChanged || CharacterChanged)
    {
      GlGlyph = new GlGlyph(FontData[Character]);
      FontChanged = false;
      CharacterChanged = false;
    }
  }

  int frameCount = 0;
  private void OnRenderFrame(FrameEventArgs frameEventArgs)
  {
    GlGlyph?.OnRender(Position, Scale, Window.Size);
    var fps = 1.0 / frameEventArgs.Time;
    if (++frameCount % 1000 == 0)
    {
      frameCount = 0;
      Console.WriteLine($"{fps:0.0}");
    }
  }

  private void OnTextInput(TextInputEventArgs textInputEventArgs)
  {
    Character = (uint)textInputEventArgs.Unicode;
    CharacterChanged = true;
  }

  public void OnFileDrop(FileDropEventArgs fileDropEventArgs)
  {
    FontData = Parser.ParseFont(fileDropEventArgs.FileNames.First());
    FontChanged = true;
  }
}
