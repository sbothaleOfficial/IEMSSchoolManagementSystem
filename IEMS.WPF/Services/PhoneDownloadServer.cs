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
/// A tiny single-purpose web server (raw TCP, no admin/URL reservation) that serves ONE file to a
/// phone on the same network. The phone scans a QR of the URL, opens a small page and taps to
/// open/download the file (e.g. an ID card or certificate PDF).
/// </summary>
public sealed class PhoneDownloadServer : IDisposable
{
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;

    private readonly byte[] _data;
    private readonly string _fileName;
    private readonly string _contentType;
    private readonly string _displayName;

    public string Token { get; } = Guid.NewGuid().ToString("N").Substring(0, 8);
    public string Url { get; private set; } = string.Empty;

    /// <summary>Raised (on a background thread) the first time the phone opens the file.</summary>
    public event Action? Downloaded;

    public PhoneDownloadServer(byte[] data, string fileName, string contentType, string displayName)
    {
        _data = data;
        _fileName = string.IsNullOrWhiteSpace(fileName) ? "document" : fileName;
        _contentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType;
        _displayName = string.IsNullOrWhiteSpace(displayName) ? _fileName : displayName;
    }

    public string Start()
    {
        var ip = GetLocalIPv4() ?? throw new InvalidOperationException(
            "Could not find this PC's Wi-Fi/LAN address. Make sure the PC is on the same network as the phone.");
        int port = FindFreePort();
        _listener = new TcpListener(IPAddress.Any, port);
        _listener.Start();
        Url = $"http://{ip}:{port}/d/{Token}";
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
                var sb = new StringBuilder();
                var buf = new byte[1];
                int matched = 0;
                while (matched < 4)
                {
                    int n = await stream.ReadAsync(buf, 0, 1);
                    if (n == 0) return;
                    char c = (char)buf[0];
                    sb.Append(c);
                    if ((matched == 0 && c == '\r') || (matched == 1 && c == '\n') ||
                        (matched == 2 && c == '\r') || (matched == 3 && c == '\n')) matched++;
                    else matched = (c == '\r') ? 1 : 0;
                    if (sb.Length > 8192) return;
                }

                var firstLine = sb.ToString().Split('\n')[0].Trim();
                var parts = firstLine.Split(' ');
                if (parts.Length < 2) return;
                string path = parts[1];

                if (path == $"/d/{Token}/file")
                {
                    Downloaded?.Invoke();
                    var disp = $"inline; filename=\"{_fileName}\"";
                    var head = $"HTTP/1.1 200 OK\r\nContent-Type: {_contentType}\r\nContent-Length: {_data.Length}\r\nContent-Disposition: {disp}\r\nConnection: close\r\nCache-Control: no-store\r\n\r\n";
                    var headBytes = Encoding.ASCII.GetBytes(head);
                    await stream.WriteAsync(headBytes, 0, headBytes.Length);
                    await stream.WriteAsync(_data, 0, _data.Length);
                    await stream.FlushAsync();
                    return;
                }

                if (path.StartsWith($"/d/{Token}"))
                {
                    await WriteHtmlAsync(stream, BuildPage());
                    return;
                }

                await WriteAsync(stream, "404 Not Found", "text/plain", "Not found");
            }
        }
        catch { /* ignore per-connection errors */ }
    }

    private string BuildPage()
    {
        var safe = WebUtility.HtmlEncode(_displayName);
        return $@"<!doctype html><html><head><meta charset='utf-8'>
<meta name='viewport' content='width=device-width, initial-scale=1'><title>{safe}</title>
<style>
 body{{font-family:-apple-system,Segoe UI,Roboto,sans-serif;background:#f2f4f8;margin:0;padding:24px;color:#13235b;text-align:center}}
 .card{{max-width:420px;margin:0 auto;background:#fff;border-radius:14px;padding:26px;box-shadow:0 4px 16px rgba(0,0,0,.1)}}
 h2{{margin:0 0 6px}} .sub{{color:#667;font-size:14px;margin-bottom:20px}}
 a.btn{{display:block;text-decoration:none;padding:15px;font-size:17px;border-radius:10px;background:#0070D0;color:#fff;font-weight:600}}
 .tip{{color:#889;font-size:13px;margin-top:14px}}
</style></head><body><div class='card'>
<h2>{safe}</h2>
<div class='sub'>From Inspire English Medium School, Mardi</div>
<a class='btn' href='/d/{Token}/file'>Open / Download</a>
<div class='tip'>Opens the document. Use your browser's share/save to keep it.</div>
</div></body></html>";
    }

    private static async Task WriteHtmlAsync(NetworkStream s, string body) => await WriteAsync(s, "200 OK", "text/html; charset=utf-8", body);

    private static async Task WriteAsync(NetworkStream stream, string status, string contentType, string body)
    {
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var head = $"HTTP/1.1 {status}\r\nContent-Type: {contentType}\r\nContent-Length: {bodyBytes.Length}\r\nConnection: close\r\nCache-Control: no-store\r\n\r\n";
        var headBytes = Encoding.ASCII.GetBytes(head);
        await stream.WriteAsync(headBytes, 0, headBytes.Length);
        await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
        await stream.FlushAsync();
    }

    private static string? GetLocalIPv4()
    {
        try
        {
            var candidates = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up
                          && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback
                          && ni.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(a.Address))
                .Select(a => a.Address.ToString()).ToList();
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
