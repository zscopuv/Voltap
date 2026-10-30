using System.Diagnostics;
using System.Runtime.InteropServices;
using Voltap.Models;
namespace Voltap.Core;
public sealed class ClickerEngine : IDisposable
{
 private sealed class Runtime { public required ClickerProfile Profile; public bool Running,Previous; public double Next; public long Count; }
 private readonly object gate=new(); private readonly List<Runtime> items=[]; private readonly Thread worker; private volatile bool disposed; private readonly bool timerResolutionSet;
 [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);
 [DllImport("winmm.dll")] private static extern uint timeBeginPeriod(uint period);
 [DllImport("winmm.dll")] private static extern uint timeEndPeriod(uint period);
 public ClickerEngine(IEnumerable<ClickerProfile> profiles) { foreach(var p in profiles)items.Add(new Runtime{Profile=p.Copy()}); timerResolutionSet=timeBeginPeriod(1)==0; worker=new Thread(Loop){IsBackground=true,Name="Voltap scheduler"}; worker.Start(); }
 public IReadOnlyList<ClickerSnapshot> Snapshots() { lock(gate)return items.Select(x=>new ClickerSnapshot(x.Profile.Copy(),x.Running,x.Count)).ToList(); }
 public void AddOrUpdate(ClickerProfile p) { lock(gate) {var x=items.FirstOrDefault(i=>i.Profile.Id==p.Id);if(x==null)items.Add(new Runtime{Profile=p.Copy()});else{x.Profile=p.Copy();x.Next=0;}} }
 public void Remove(Guid id) { lock(gate)items.RemoveAll(x=>x.Profile.Id==id); }
 public void SetRunning(Guid id,bool running) { lock(gate){var x=items.FirstOrDefault(i=>i.Profile.Id==id);if(x!=null){x.Running=running;x.Next=0;}} }
 public void StopAll() { lock(gate)foreach(var x in items)x.Running=false; }
 private void Loop() {
  while(!disposed) {
   double now=Stopwatch.GetTimestamp()/(double)Stopwatch.Frequency;
   lock(gate) foreach(var x in items) {
    bool down=(GetAsyncKeyState((int)x.Profile.StartKey)&0x8000)!=0;
    if(down&&!x.Previous){if(x.Profile.Mode==ActivationMode.Toggle)x.Running=!x.Running;else x.Running=true;x.Next=0;}
    else if(!down&&x.Previous&&x.Profile.Mode==ActivationMode.Hold)x.Running=false;
    x.Previous=down;if(!x.Running||now<x.Next)continue;
    try{InputSender.Send(x.Profile);x.Count++;}catch{x.Running=false;continue;}
    int cps=x.Profile.RandomizerEnabled?Random.Shared.Next(Math.Clamp(x.Profile.RandomMin,1,250),Math.Clamp(Math.Max(x.Profile.RandomMin,x.Profile.RandomMax),1,250)+1):Math.Clamp(x.Profile.Cps,1,250);
    x.Next=now+1.0/cps;
   }
   Thread.Sleep(1);
  }
 }
 public void Dispose(){disposed=true;StopAll();if(worker.IsAlive)worker.Join(300);if(timerResolutionSet)timeEndPeriod(1);}
}