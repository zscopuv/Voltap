using System.Diagnostics;
using Voltap.Core;
using Voltap.Models;
using Voltap.Services;
namespace Voltap.UI;
public sealed class MainForm : Form
{
 private readonly List<ClickerProfile> profiles;
 private readonly ClickerEngine engine;
 private readonly DataGridView grid=new();
 private readonly Label totalLabel=new(),activeLabel=new(),inputLabel=new();
 private readonly System.Windows.Forms.Timer timer=new(){Interval=180};
 private bool closing;
 public MainForm()
 {
  Text="Voltap";StartPosition=FormStartPosition.CenterScreen;MinimumSize=new Size(900,590);Size=new Size(1120,720);
  BackColor=Color.FromArgb(15,19,28);ForeColor=Color.FromArgb(232,237,246);Font=new Font("Segoe UI",9.5f);
  profiles=ConfigStore.Load();if(profiles.Count==0)profiles.Add(new ClickerProfile());engine=new ClickerEngine(profiles);
  BuildUi();RefreshGrid();timer.Tick+=(_,_)=>RefreshGrid();timer.Start();
  FormClosing+=(_,_)=>{closing=true;timer.Stop();engine.StopAll();ConfigStore.Save(profiles);engine.Dispose();};
 }
 private void BuildUi()
 {
  var root=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=3,Padding=new Padding(24)};
  root.RowStyles.Add(new RowStyle(SizeType.Absolute,88));root.RowStyles.Add(new RowStyle(SizeType.Absolute,104));root.RowStyles.Add(new RowStyle(SizeType.Percent,100));Controls.Add(root);
  var header=new Panel{Dock=DockStyle.Fill};
  header.Controls.Add(new Label{Text="VOLTAP",Font=new Font("Segoe UI Semibold",21,FontStyle.Bold),ForeColor=Color.White,AutoSize=true,Location=new Point(0,2)});
  header.Controls.Add(new Label{Text="LIGHTWEIGHT INPUT AUTOMATION  •  BUILT BY ZSCOPUV",Font=new Font("Segoe UI",8.5f),ForeColor=Color.FromArgb(129,145,168),AutoSize=true,Location=new Point(2,43)});
  var github=new LinkLabel{Text="GitHub ↗",AutoSize=true,LinkColor=Color.FromArgb(116,165,255),Location=new Point(890,18),Anchor=AnchorStyles.Top|AnchorStyles.Right};
  github.Click+=(_,_)=>OpenUrl("https://github.com/zscopuv/Voltap");
  var license=new LinkLabel{Text="License ↗",AutoSize=true,LinkColor=Color.FromArgb(116,165,255),Location=new Point(970,18),Anchor=AnchorStyles.Top|AnchorStyles.Right};
  license.Click+=(_,_)=>OpenUrl("https://raw.githubusercontent.com/zscopuv/Voltap/refs/heads/main/LICENSE");
  header.Controls.AddRange([github,license]);root.Controls.Add(header,0,0);
  var stats=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=3,Padding=new Padding(0,4,0,14)};
  for(int i=0;i<3;i++)stats.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.333f));
  stats.Controls.Add(Stat("CLICKERS",totalLabel,"Configured instances"),0,0);
  stats.Controls.Add(Stat("RUNNING",activeLabel,"Active right now"),1,0);
  stats.Controls.Add(Stat("TOTAL INPUTS",inputLabel,"Session count"),2,0);root.Controls.Add(stats,0,1);
  var panel=new Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(21,27,38),Padding=new Padding(18)};
  panel.Paint+=(_,e)=>ControlPaint.DrawBorder(e.Graphics,panel.ClientRectangle,Color.FromArgb(39,49,66),ButtonBorderStyle.Solid);root.Controls.Add(panel,0,2);
  var bar=new FlowLayoutPanel{Dock=DockStyle.Top,Height=48,WrapContents=false};
  var add=Btn("+  Add clicker",Color.FromArgb(48,119,255),128);
  var edit=Btn("Edit",Color.FromArgb(42,52,70),82);var remove=Btn("Delete",Color.FromArgb(42,52,70),82);
  var toggle=Btn("Start / Stop",Color.FromArgb(24,145,105),112);var stop=Btn("Stop all",Color.FromArgb(130,55,66),88);
  add.Click+=(_,_)=>AddProfile();edit.Click+=(_,_)=>EditProfile();remove.Click+=(_,_)=>DeleteProfile();
  toggle.Click+=(_,_)=>ToggleSelected();stop.Click+=(_,_)=>engine.StopAll();bar.Controls.AddRange([add,edit,remove,toggle,stop]);panel.Controls.Add(bar);
  grid.Dock=DockStyle.Fill;grid.Top=56;grid.BackgroundColor=Color.FromArgb(21,27,38);grid.BorderStyle=BorderStyle.None;grid.GridColor=Color.FromArgb(39,49,66);
  grid.EnableHeadersVisualStyles=false;grid.ColumnHeadersDefaultCellStyle=new DataGridViewCellStyle{BackColor=Color.FromArgb(27,35,49),ForeColor=Color.FromArgb(153,168,190),Font=new Font("Segoe UI Semibold",8.5f,FontStyle.Bold),SelectionBackColor=Color.FromArgb(27,35,49),Padding=new Padding(8)};
  grid.DefaultCellStyle=new DataGridViewCellStyle{BackColor=Color.FromArgb(21,27,38),ForeColor=Color.FromArgb(226,233,244),SelectionBackColor=Color.FromArgb(38,57,87),SelectionForeColor=Color.White,Padding=new Padding(8),Font=new Font("Segoe UI",9.5f)};
  grid.RowTemplate.Height=44;grid.ColumnHeadersHeight=42;grid.RowHeadersVisible=false;grid.AllowUserToAddRows=false;grid.AllowUserToDeleteRows=false;grid.AllowUserToResizeRows=false;grid.MultiSelect=false;grid.ReadOnly=true;grid.SelectionMode=DataGridViewSelectionMode.FullRowSelect;grid.AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill;
  grid.Columns.Add("name","NAME");grid.Columns.Add("output","OUTPUT");grid.Columns.Add("mode","MODE");grid.Columns.Add("hotkey","START KEY");grid.Columns.Add("rate","RATE");grid.Columns.Add("status","STATUS");grid.Columns.Add("inputs","INPUTS");grid.Columns["inputs"]!.FillWeight=65;grid.DoubleClick+=(_,_)=>EditProfile();panel.Controls.Add(grid);
  var footer=new Label{Text="Hotkeys work globally while Voltap is running  •  250 CPS maximum per instance  •  Settings save automatically",Dock=DockStyle.Bottom,Height=26,ForeColor=Color.FromArgb(125,140,161),TextAlign=ContentAlignment.MiddleLeft};
  panel.Controls.Add(footer);footer.BringToFront();bar.BringToFront();
 }
 private static Panel Stat(string title,Label value,string hint)
 {
  var p=new Panel{Dock=DockStyle.Fill,BackColor=Color.FromArgb(21,27,38),Margin=new Padding(0,0,12,0)};
  p.Paint+=(_,e)=>ControlPaint.DrawBorder(e.Graphics,p.ClientRectangle,Color.FromArgb(39,49,66),ButtonBorderStyle.Solid);
  p.Controls.Add(new Label{Text=title,AutoSize=true,ForeColor=Color.FromArgb(134,150,173),Font=new Font("Segoe UI Semibold",8,FontStyle.Bold),Location=new Point(14,9)});
  value.Text="0";value.AutoSize=true;value.ForeColor=Color.White;value.Font=new Font("Segoe UI Semibold",19,FontStyle.Bold);value.Location=new Point(13,27);p.Controls.Add(value);
  p.Controls.Add(new Label{Text=hint,AutoSize=true,ForeColor=Color.FromArgb(118,132,153),Font=new Font("Segoe UI",8),Location=new Point(15,63)});return p;
 }
 private static Button Btn(string text,Color color,int width)=>new(){Text=text,Width=width,Height=36,BackColor=color,ForeColor=Color.White,FlatStyle=FlatStyle.Flat,FlatAppearance={BorderSize=0},Cursor=Cursors.Hand,Margin=new Padding(0,0,9,0),Font=new Font("Segoe UI Semibold",9,FontStyle.Bold)};
 private Guid? SelectedId()=>grid.CurrentRow?.Tag is Guid id?id:null;
 private ClickerProfile? SelectedProfile(){var id=SelectedId();return id==null?null:profiles.FirstOrDefault(p=>p.Id==id.Value);}
 private void RefreshGrid()
 {
  if(closing||IsDisposed)return;var selected=SelectedId();var snapshots=engine.Snapshots();grid.Rows.Clear();
  foreach(var s in snapshots)
  {
   string output=s.Profile.Output switch{ClickOutput.LeftMouse=>"Left mouse",ClickOutput.RightMouse=>"Right mouse",ClickOutput.MiddleMouse=>"Middle mouse",_=>$"Key: {s.Profile.OutputKey}"};
   string rate=s.Profile.RandomizerEnabled?$"{s.Profile.RandomMin}–{s.Profile.RandomMax} CPS":$"{s.Profile.Cps} CPS";
   int row=grid.Rows.Add(s.Profile.Name,output,s.Profile.Mode,s.Profile.StartKey,rate,s.IsRunning?"●  RUNNING":"○  STOPPED",s.TotalClicks.ToString("N0"));
   grid.Rows[row].Tag=s.Profile.Id;grid.Rows[row].Cells[5].Style.ForeColor=s.IsRunning?Color.FromArgb(67,209,151):Color.FromArgb(143,154,172);if(selected==s.Profile.Id)grid.Rows[row].Selected=true;
  }
  totalLabel.Text=snapshots.Count.ToString();activeLabel.Text=snapshots.Count(x=>x.IsRunning).ToString();inputLabel.Text=snapshots.Sum(x=>x.TotalClicks).ToString("N0");
 }
 private void AddProfile(){using var d=new ProfileEditor();if(d.ShowDialog(this)!=DialogResult.OK)return;profiles.Add(d.Result);engine.AddOrUpdate(d.Result);Save();RefreshGrid();}
 private void EditProfile(){var p=SelectedProfile();if(p==null)return;using var d=new ProfileEditor(p);if(d.ShowDialog(this)!=DialogResult.OK)return;int i=profiles.FindIndex(x=>x.Id==p.Id);if(i>=0)profiles[i]=d.Result;engine.AddOrUpdate(d.Result);Save();RefreshGrid();}
 private void DeleteProfile(){var p=SelectedProfile();if(p==null)return;engine.Remove(p.Id);profiles.RemoveAll(x=>x.Id==p.Id);Save();RefreshGrid();}
 private void ToggleSelected(){var id=SelectedId();if(id==null)return;var s=engine.Snapshots().FirstOrDefault(x=>x.Profile.Id==id);if(s!=null)engine.SetRunning(id.Value,!s.IsRunning);RefreshGrid();}
 private void Save()=>ConfigStore.Save(profiles);
 private static void OpenUrl(string url){try{Process.Start(new ProcessStartInfo(url){UseShellExecute=true});}catch{}}
}