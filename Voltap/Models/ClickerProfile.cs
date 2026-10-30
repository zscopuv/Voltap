using System.Windows.Forms;
namespace Voltap.Models;
public enum ClickOutput { LeftMouse, RightMouse, MiddleMouse, Keystroke }
public enum ActivationMode { Toggle, Hold }
public sealed class ClickerProfile
{
 public Guid Id { get; set; } = Guid.NewGuid();
 public string Name { get; set; } = "Clicker";
 public ClickOutput Output { get; set; } = ClickOutput.LeftMouse;
 public ActivationMode Mode { get; set; } = ActivationMode.Toggle;
 public Keys StartKey { get; set; } = Keys.F6;
 public Keys OutputKey { get; set; } = Keys.Space;
 public int Cps { get; set; } = 10;
 public bool RandomizerEnabled { get; set; }
 public int RandomMin { get; set; } = 5;
 public int RandomMax { get; set; } = 15;
 public ClickerProfile Copy() => new() { Id=Id, Name=Name, Output=Output, Mode=Mode, StartKey=StartKey, OutputKey=OutputKey, Cps=Cps, RandomizerEnabled=RandomizerEnabled, RandomMin=RandomMin, RandomMax=RandomMax };
}
public sealed record ClickerSnapshot(ClickerProfile Profile, bool IsRunning, long TotalClicks);