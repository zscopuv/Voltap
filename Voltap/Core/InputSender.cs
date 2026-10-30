using System.Runtime.InteropServices;
using Voltap.Models;
namespace Voltap.Core;
internal static class InputSender
{
 private const uint MouseInput=0, KeyboardInput=1, KeyUp=2;
 [StructLayout(LayoutKind.Sequential)] private struct INPUT { public uint Type; public InputUnion Data; }
 [StructLayout(LayoutKind.Explicit)] private struct InputUnion { [FieldOffset(0)] public MOUSEINPUT Mouse; [FieldOffset(0)] public KEYBDINPUT Keyboard; }
 [StructLayout(LayoutKind.Sequential)] private struct MOUSEINPUT { public int Dx,Dy; public uint MouseData,Flags,Time; public UIntPtr ExtraInfo; }
 [StructLayout(LayoutKind.Sequential)] private struct KEYBDINPUT { public ushort VirtualKey,Scan; public uint Flags,Time; public UIntPtr ExtraInfo; }
 [DllImport("user32.dll",SetLastError=true)] private static extern uint SendInput(uint count,INPUT[] inputs,int size);
 public static void Send(ClickerProfile p)
 {
  INPUT[] a;
  if(p.Output==ClickOutput.Keystroke) { ushort k=(ushort)p.OutputKey; a=[new INPUT{Type=KeyboardInput,Data=new InputUnion{Keyboard=new KEYBDINPUT{VirtualKey=k}}},new INPUT{Type=KeyboardInput,Data=new InputUnion{Keyboard=new KEYBDINPUT{VirtualKey=k,Flags=KeyUp}}}]; }
  else { var (d,u)=p.Output switch { ClickOutput.RightMouse=>(8u,16u),ClickOutput.MiddleMouse=>(32u,64u),_=>(2u,4u) }; a=[new INPUT{Type=MouseInput,Data=new InputUnion{Mouse=new MOUSEINPUT{Flags=d}}},new INPUT{Type=MouseInput,Data=new InputUnion{Mouse=new MOUSEINPUT{Flags=u}}}]; }
  if(SendInput((uint)a.Length,a,Marshal.SizeOf<INPUT>())!=a.Length) throw new InvalidOperationException("Windows could not send the requested input.");
 }
}