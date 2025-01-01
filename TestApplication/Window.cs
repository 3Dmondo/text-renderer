using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace TestApplication;

internal class Window : GameWindow
{
  public Window() : base(
    new GameWindowSettings
    {
      UpdateFrequency =0
    }, 
    new NativeWindowSettings
    {
      ClientSize = new Vector2i(800, 600),
      Title = "TestApplication",
      Flags = ContextFlags.ForwardCompatible,
    })
  { }

  protected override void OnLoad()
  {
    base.OnLoad();
    GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
  }
  protected override void OnRenderFrame(FrameEventArgs args)
  {
    GL.Clear(ClearBufferMask.ColorBufferBit);
    base.OnRenderFrame(args);
    SwapBuffers();
  }

  protected override void OnResize(ResizeEventArgs e)
  {
    base.OnResize(e);
    GL.Viewport(0, 0, Size.X, Size.Y);
  }

}
