using FontParser;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TextRenderer;

public class CharRenderer
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

  public CharRenderer(GameWindow window)
  {
    window.RenderFrame += OnRenderFrame;
    window.TextInput += OnTextInput;
    window.FileDrop += OnFileDrop;
    window.UpdateFrame += OnUpdateFrame;
    window.MouseWheel += OnMouseWeel;
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

  private void OnMouseWeel(MouseWheelEventArgs args)
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

  private void OnRenderFrame(FrameEventArgs frameEventArgs)
  {
    GlGlyph?.OnRender(Position, Scale, Window.Size);
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
