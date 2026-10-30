using System.Text.Json;
using System.Text.Json.Serialization;
using Voltap.Models;
namespace Voltap.Services;
public static class ConfigStore
{
 private static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Voltap");
 private static readonly string FilePath = Path.Combine(Folder, "config.json");
 private static readonly JsonSerializerOptions Options = new() { WriteIndented=true, Converters={new JsonStringEnumConverter()} };
 public static List<ClickerProfile> Load()
 {
  try { if (!File.Exists(FilePath)) return []; var list=JsonSerializer.Deserialize<List<ClickerProfile>>(File.ReadAllText(FilePath),Options) ?? []; foreach(var p in list) Normalize(p); return list; }
  catch { return []; }
 }
 public static void Save(IEnumerable<ClickerProfile> profiles)
 {
  Directory.CreateDirectory(Folder); var tmp=FilePath+".tmp"; File.WriteAllText(tmp,JsonSerializer.Serialize(profiles,Options)); File.Move(tmp,FilePath,true);
 }
 private static void Normalize(ClickerProfile p)
 {
  p.Name=string.IsNullOrWhiteSpace(p.Name)?"Clicker":p.Name.Trim(); p.Cps=Math.Clamp(p.Cps,1,250);
  p.RandomMin=Math.Clamp(p.RandomMin,1,250); p.RandomMax=Math.Clamp(p.RandomMax,p.RandomMin,250);
  if(p.StartKey==System.Windows.Forms.Keys.None)p.StartKey=System.Windows.Forms.Keys.F6;
 }
}