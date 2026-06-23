using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IEMS.WPF.Services;

/// <summary>
/// A tiny single-purpose web server (raw TCP, so it needs no admin/URL reservation) that lets a
/// phone on the same network upload one student photo. It serves a small mobile page on GET and
/// receives the raw image bytes on POST, then raises <see cref="PhotoReceived"/>.
/// </summary>
public sealed class PhoneUploadServer : IDisposable
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;

    public string Token { get; } = Guid.NewGuid().ToString("N").Substring(0, 8);
    public string Url { get; private set; } = string.Empty;
    public string StudentName { get; }

    /// <summary>Raised (on a background thread) when a photo has been received.</summary>
    public event Action<byte[]>? PhotoReceived;

    public PhoneUploadServer(string studentName) => StudentName = studentName;

    /// <summary>Starts listening and returns the URL the phone should open. Throws if no LAN IP.</summary>
    public string Start()
    {
        var ip = GetLocalIPv4() ?? throw new InvalidOperationException(
            "Could not find this PC's Wi-Fi/LAN address. Make sure the PC is connected to the same network as the phone.");

        int port = FindFreePort();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Url = $"http://{ip}:{port}/u/{Token}";

        _cts = new CancellationTokenSource();
        _ = Task.Run(() => AcceptLoopAsync(_cts.Token));
        return Url;
    }

    private async Task AcceptLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested && _listener != null)
        {
            TcpClient client;
            try { client = await _listener.AcceptTcpClientAsync(ct); }
            catch { break; }
            _ = Task.Run(() => HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                // ----- Read headers (until blank line) -----
                var headerBytes = new MemoryStream();
                var buf = new byte[1];
                int matched = 0; // counts \r\n\r\n
                while (matched < 4)
                {
                    int n = await stream.ReadAsync(buf, 0, 1);
                    if (n == 0) return;
                    headerBytes.WriteByte(buf[0]);
                    char c = (char)buf[0];
                    if ((matched == 0 && c == '\r') || (matched == 1 && c == '\n') ||
                        (matched == 2 && c == '\r') || (matched == 3 && c == '\n')) matched++;
                    else matched = (c == '\r') ? 1 : 0;
                    if (headerBytes.Length > 16384) return; // guard
                }

                var header = Encoding.ASCII.GetString(headerBytes.ToArray());
                var firstLine = header.Split('\n')[0].Trim();
                var parts = firstLine.Split(' ');
                if (parts.Length < 2) { await WriteAsync(stream, "400 Bad Request", "text/plain", "Bad request"); return; }
                string method = parts[0], path = parts[1];

                if (!path.StartsWith($"/u/{Token}"))
                {
                    await WriteAsync(stream, "404 Not Found", "text/plain", "Not found");
                    return;
                }

                if (method == "GET")
                {
                    await WriteAsync(stream, "200 OK", "text/html; charset=utf-8", BuildPage());
                    return;
                }

                if (method == "POST")
                {
                    int contentLength = ParseContentLength(header);
                    if (contentLength <= 0 || contentLength > 12 * 1024 * 1024)
                    {
                        await WriteAsync(stream, "400 Bad Request", "text/plain", "Invalid upload");
                        return;
                    }

                    var body = new byte[contentLength];
                    int read = 0;
                    while (read < contentLength)
                    {
                        int n = await stream.ReadAsync(body, read, contentLength - read);
                        if (n == 0) break;
                        read += n;
                    }
                    if (read == contentLength)
                    {
                        PhotoReceived?.Invoke(body);
                        await WriteAsync(stream, "200 OK", "text/plain", "OK");
                    }
                    else
                    {
                        await WriteAsync(stream, "400 Bad Request", "text/plain", "Incomplete upload");
                    }
                    return;
                }

                await WriteAsync(stream, "405 Method Not Allowed", "text/plain", "Method not allowed");
            }
        }
        catch { /* ignore per-connection errors */ }
    }

    private static int ParseContentLength(string header)
    {
        foreach (var line in header.Split('\n'))
        {
            var l = line.Trim();
            if (l.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase)
                && int.TryParse(l.Substring("Content-Length:".Length).Trim(), out var len))
                return len;
        }
        return -1;
    }

    private static async Task WriteAsync(NetworkStream stream, string status, string contentType, string body)
    {
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var head = $"HTTP/1.1 {status}\r\nContent-Type: {contentType}\r\nContent-Length: {bodyBytes.Length}\r\nConnection: close\r\nCache-Control: no-store\r\n\r\n";
        var headBytes = Encoding.ASCII.GetBytes(head);
        await stream.WriteAsync(headBytes, 0, headBytes.Length);
        await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
        await stream.FlushAsync();
    }

    private string BuildPage()
    {
        var safeName = System.Net.WebUtility.HtmlEncode(StudentName);
        return $@"<!doctype html><html><head><meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'>
<title>Upload Photo</title>
<style>
 body{{font-family:-apple-system,Segoe UI,Roboto,sans-serif;background:#f2f4f8;margin:0;padding:24px;color:#13235b}}
 .card{{max-width:420px;margin:0 auto;background:#fff;border-radius:14px;padding:22px;box-shadow:0 4px 16px rgba(0,0,0,.1)}}
 h2{{margin:0 0 4px}} .sub{{color:#667;font-size:14px;margin-bottom:18px}}
 input[type=file]{{display:block;width:100%;margin:14px 0;font-size:16px}}
 button{{width:100%;padding:14px;font-size:17px;border:0;border-radius:10px;background:#0070D0;color:#fff;font-weight:600}}
 button:disabled{{background:#9bbbe0}}
 #s{{margin-top:14px;font-weight:600;text-align:center}}
 img#pv{{max-width:100%;border-radius:10px;margin-top:12px;display:none}}
</style></head><body><div class='card'>
<h2>Student Photo</h2>
<div class='sub'>For <b>{safeName}</b></div>
<input type='file' accept='image/*' id='f' onchange='prev()'>
<img id='pv'>
<button id='b' onclick='up()'>Upload to school PC</button>
<p id='s'></p>
</div>
<script>
function prev(){{var f=document.getElementById('f').files[0];if(!f)return;var img=document.getElementById('pv');img.src=URL.createObjectURL(f);img.style.display='block';}}
async function up(){{
 var f=document.getElementById('f').files[0];
 if(!f){{alert('Please choose or take a photo first.');return;}}
 var b=document.getElementById('b'); b.disabled=true;
 document.getElementById('s').innerText='Uploading…';
 try{{
  var buf=await f.arrayBuffer();
  var r=await fetch(location.pathname+'/photo',{{method:'POST',headers:{{'Content-Type':'application/octet-stream'}},body:buf}});
  document.getElementById('s').innerText = r.ok ? '✓ Sent! You can close this page.' : 'Upload failed, please try again.';
  if(!r.ok) b.disabled=false;
 }}catch(e){{document.getElementById('s').innerText='Upload failed: '+e; b.disabled=false;}}
}}
</script></body></html>";
    }

    private static string? GetLocalIPv4()
    {
        try
        {
            // Prefer an up, non-virtual interface with a private IPv4.
            var candidates = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up
                          && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                          && ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork
                         && !IPAddress.IsLoopback(a.Address))
                .Select(a => a.Address.ToString())
                .ToList();

            return candidates.FirstOrDefault(IsPrivate) ?? candidates.FirstOrDefault();
        }
        catch { return null; }
    }

    private static bool IsPrivate(string ip)
        => ip.StartsWith("192.168.") || ip.StartsWith("10.")
        || (ip.StartsWith("172.") && int.TryParse(ip.Split('.')[1], out var b) && b >= 16 && b <= 31);

    private static int FindFreePort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    public void Dispose()
    {
        try { _cts?.Cancel(); } catch { }
        try { _listener?.Stop(); } catch { }
        _cts?.Dispose();
    }
}
