using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using EasySave.Core.Controller;
using EasySave.Core.Models;
using System.Linq;

namespace EasySave.Core.Services
{
    public class BackupServer
    {
        public const int Port = 11000;
        public bool IsRunning { get; private set; }
        
        public event Action<string>? OnLog;
        public event Action<ModelJob>? OnRunJobRequested;
        public event Action? OnRunAllRequested;

        private TcpListener? _listener;
        private SauvegardeController? _controller;

        public void Start(SauvegardeController controller)
        {
            if (IsRunning) return;
            _controller = controller;
            IsRunning = true;
            Task.Run(Listen);
            Log($"Serveur HTTP démarré sur le port {Port}");
        }

        public void Stop()
        {
            IsRunning = false;
            _listener?.Stop();
            Log("Serveur arrêté.");
        }

        private void Listen()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
            }
            catch (Exception ex)
            {
                Log($"Erreur : impossible de démarrer ({ex.Message}). Port déjà utilisé ?");
                IsRunning = false;
                return;
            }
            while (IsRunning)
            {
                try { Handle(_listener.AcceptTcpClient()); }
                catch { break; }
            }
        }

        private void Handle(TcpClient tcp)
        {
            try
            {
                var stream = tcp.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);

                string? requestLine = reader.ReadLine();
                if (requestLine == null) return;

                string? header;
                while (!string.IsNullOrEmpty(header = reader.ReadLine())) { }

                string pathFull = "/";
                var parts = requestLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2) pathFull = parts[1];
                string path = pathFull.ToLower();

                var writer = new StreamWriter(stream, new UTF8Encoding(false)) { AutoFlush = true };

                writer.WriteLine("HTTP/1.1 200 OK");
                writer.WriteLine("Access-Control-Allow-Origin: *");
                writer.WriteLine("Connection: close");
                
                if (path.StartsWith("/run-job?name="))
                {
                    string jobName = Uri.UnescapeDataString(pathFull.Substring(14));
                    var job = _controller?.myJobs.FirstOrDefault(j => j.Name == jobName);
                    if (job != null)
                    {
                        OnRunJobRequested?.Invoke(job);
                    }
                    writer.WriteLine("Content-Type: application/json; charset=utf-8");
                    writer.WriteLine();
                    writer.WriteLine("{\"status\":\"ok\"}");
                }
                else if (path == "/run")
                {
                    OnRunAllRequested?.Invoke();
                    writer.WriteLine("Content-Type: application/json; charset=utf-8");
                    writer.WriteLine();
                    writer.WriteLine("{\"status\":\"started_all\"}");
                }
                else if (path == "/pause")
                {
                    if (_controller != null) _controller.IsPausedRequested = true;
                    writer.WriteLine("Content-Type: application/json; charset=utf-8");
                    writer.WriteLine();
                    writer.WriteLine("{\"status\":\"paused\"}");
                }
                else if (path == "/status")
                {
                    writer.WriteLine("Content-Type: application/json; charset=utf-8");
                    writer.WriteLine();
                    writer.WriteLine(File.Exists("state.json") ? File.ReadAllText("state.json") : "{}");
                }
                else if (path == "/logs")
                {
                    string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
                    string logContent = "Aucun log pour aujourd'hui.";
                    string dateFmt = DateTime.Now.ToString("yyyy-MM-dd");
                    string jsonPath = Path.Combine(logFolder, dateFmt + ".json");
                    string xmlPath = Path.Combine(logFolder, dateFmt + ".xml");

                    if (File.Exists(jsonPath)) logContent = File.ReadAllText(jsonPath);
                    else if (File.Exists(xmlPath)) logContent = File.ReadAllText(xmlPath);

                    string htmlLogs = $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8""><title>EasySave Logs</title>
<style>
body{{font-family:Consolas, monospace;background:#111;color:#10B981;padding:20px;}}
h2{{color:#eee; text-align:center; font-family:Arial; margin-bottom:30px;}}
pre{{background:#222; padding:20px; border-radius:8px; border:1px solid #333; overflow-x:auto; white-space:pre-wrap;}}
a{{color:#4af;text-decoration:none; font-family:Arial; display:block; text-align:center; margin-top:20px;}} 
a:hover{{text-decoration:underline;}}
</style>
<script>
const dictLogs = {{
  'fr': {{ title: ""Journaux de bord (Logs du jour)"", back: ""Retour au moniteur"", noLog: ""Aucun log pour aujourd'hui."" }},
  'en': {{ title: ""Daily Logs"", back: ""Back to monitor"", noLog: ""No logs for today."" }}
}};

function changeLangLogs(l) {{
    document.getElementById('logTitle').innerText = dictLogs[l].title;
    document.getElementById('backBtn').innerText = dictLogs[l].back;
    let preElt = document.getElementById('logContent');
    if (preElt.innerText === dictLogs['fr'].noLog || preElt.innerText === dictLogs['en'].noLog) {{
        preElt.innerText = dictLogs[l].noLog;
    }}
}}
</script>
</head><body>
<select onchange=""changeLangLogs(this.value)"" style=""position:absolute; top:15px; right:15px; background:#222; color:#eee; border:1px solid #444; padding:5px; border-radius:4px;"">
    <option value=""fr"">Français</option>
    <option value=""en"">English</option>
</select>
<h2 id=""logTitle"">Journaux de bord (Logs du jour)</h2>
<pre id=""logContent"">{logContent}</pre>
<a href=""/"" id=""backBtn"">Retour au moniteur</a>
</body></html>";

                    writer.WriteLine("Content-Type: text/html; charset=utf-8");
                    writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(htmlLogs)}");
                    writer.WriteLine();
                    writer.Write(htmlLogs);
                }
                else
                {
                    string jobsHtml = "";
                    if (_controller != null)
                    {
                        foreach (var j in _controller.myJobs)
                        {
                            jobsHtml += $@"
                            <div class='job'>
                                <div style='text-align:left;max-width:300px;overflow:hidden;text-overflow:ellipsis;white-space:nowrap;'><b>{j.Name}</b><br><small style='color:#888'>{j.Source}</small></div>
                                <button id='btn_{j.Name}' class='btn btn-play' onclick='actionJob(""{j.Name}"")'>Play</button>
                            </div>";
                        }
                    }

                    string html = $@"<!DOCTYPE html>
<html><head><meta charset=""utf-8""><title>EasySave Monitor</title>
<style>
body{{font-family:Arial;background:#111;color:#eee;text-align:center;padding:40px}}
.blocks{{display:flex;gap:4px;justify-content:center;margin:10px auto;width:400px}}
.block{{flex:1;height:12px;background:#333;border-radius:2px;}}
.block.on{{background:#10B981;}}
.job{{display:flex;justify-content:space-between;align-items:center;background:#222;padding:15px;margin:10px auto;width:400px;border-radius:8px;}}
.btn{{padding:8px 15px;border:none;border-radius:4px;cursor:pointer;font-weight:bold;color:#111;width:100px;}}
.btn-play{{background:#10B981;}}
.btn-pause{{background:#EAB308;}}
a{{color:#4af;text-decoration:none;}} a:hover{{text-decoration:underline;}}
</style>
<script>
const dict = {{
  'fr': {{ title: ""EasySave Server"", ready: ""Prêt"", runAll: ""Tout lancer"", viewLogs: ""Voir les logs du jour"", play: ""Play"", pause: ""Pause"", resume: ""Reprendre"" }},
  'en': {{ title: ""EasySave Server"", ready: ""Ready"", runAll: ""Run All"", viewLogs: ""View Today's Logs"", play: ""Play"", pause: ""Pause"", resume: ""Resume"" }}
}};
let curLang = 'fr';

function changeLang(l) {{
    curLang = l;
    document.getElementById('title').innerText = dict[l].title;
    document.getElementById('runAllBtn').innerText = dict[l].runAll;
    document.getElementById('viewLogsBtn').innerText = dict[l].viewLogs;
    
    let prog = document.getElementById('progText');
    if(prog.innerText === dict[l === 'fr' ? 'en' : 'fr'].ready) {{
        prog.innerText = dict[l].ready;
    }}
}}

function actionJob(name) {{
    let btn = document.getElementById('btn_' + name);
    if (btn.innerText === dict[curLang].pause) {{
        fetch('/pause');
    }} else {{
        fetch('/run-job?name=' + encodeURIComponent(name));
    }}
}}

setInterval(() => {{
  fetch('/status').then(r => r.json()).then(data => {{
    document.querySelectorAll('.btn').forEach(b => {{
        if(b.innerText === dict['fr'].pause || b.innerText === dict['en'].pause) {{ 
            b.innerText = dict[curLang].play; 
            b.className = 'btn btn-play'; 
        }}
    }});

    if(data.State && data.State !== 'INACTIF') {{
       document.getElementById('progText').innerText = data.Name + ' (' + data.State + ')';
       let blocksOn = Math.floor(data.Progression / 10);
       for(let i=1; i<=10; i++) {{
           document.getElementById('block'+i).className = (i <= blocksOn || (data.Progression > 0 && i === 1)) ? 'block on' : 'block';
       }}
       
       let activeBtn = document.getElementById('btn_' + data.Name);
       if (activeBtn) {{
           if (data.State === 'PAUSE') {{
               activeBtn.innerText = dict[curLang].resume;
               activeBtn.className = 'btn btn-play';
           }} else {{
               activeBtn.innerText = dict[curLang].pause;
               activeBtn.className = 'btn btn-pause';
           }}
       }}
    }} else {{
       document.getElementById('progText').innerText = dict[curLang].ready;
       for(let i=1; i<=10; i++) document.getElementById('block'+i).className = 'block';
    }}
  }}).catch(() => {{}});
}}, 1000);
</script>
</head><body>
<select onchange=""changeLang(this.value)"" style=""position:absolute; top:15px; right:15px; background:#222; color:#eee; border:1px solid #444; padding:5px; border-radius:4px;"">
    <option value=""fr"">Français</option>
    <option value=""en"">English</option>
</select>
<h2 id=""title"">EasySave Server</h2>

<div class=""blocks"">
    <div class=""block"" id=""block1""></div><div class=""block"" id=""block2""></div><div class=""block"" id=""block3""></div><div class=""block"" id=""block4""></div><div class=""block"" id=""block5""></div><div class=""block"" id=""block6""></div><div class=""block"" id=""block7""></div><div class=""block"" id=""block8""></div><div class=""block"" id=""block9""></div><div class=""block"" id=""block10""></div>
</div>
<div id=""progText"" style=""margin-bottom:20px;color:#aaa;"">Prêt</div>

<div style=""margin:30px 0"">
    {jobsHtml}
</div>

<a href=""#"" id=""runAllBtn"" onclick=""fetch('/run')"">Tout lancer</a> | 
<a href=""/logs"" id=""viewLogsBtn"" target=""_blank"">Voir les logs du jour</a>
</body></html>";
                    writer.WriteLine("Content-Type: text/html; charset=utf-8");
                    writer.WriteLine($"Content-Length: {Encoding.UTF8.GetByteCount(html)}");
                    writer.WriteLine();
                    writer.Write(html);
                }
            }
            catch (Exception ex) { Log($"Erreur: {ex.Message}"); }
            finally { tcp.Close(); }
        }

        private void Log(string msg) => OnLog?.Invoke(msg);
    }
}