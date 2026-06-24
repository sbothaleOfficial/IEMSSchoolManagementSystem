param([string]$Out)
Add-Type @"
using System; using System.Text; using System.Runtime.InteropServices; using System.Collections.Generic;
public class Cap {
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc f, IntPtr p);
  public delegate bool EnumWindowsProc(IntPtr h, IntPtr p);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h, out RECT r);
  [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h, IntPtr after, int x, int y, int cx, int cy, uint flags);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h, int n);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  public struct RECT { public int Left, Top, Right, Bottom; }
  public static IntPtr Find(string title, bool exact) {
    IntPtr found = IntPtr.Zero;
    EnumWindows((h,p)=>{ if(IsWindowVisible(h)){ var sb=new StringBuilder(256); GetWindowText(h,sb,256); var t=sb.ToString();
      if((exact && t==title) || (!exact && t.Contains(title))){ found=h; return false; } } return true; }, IntPtr.Zero);
    return found;
  }
}
"@
$claude = [Cap]::Find("Claude", $true)
$rect = New-Object Cap+RECT
$haveClaude = $false
if ($claude -ne [IntPtr]::Zero) { [Cap]::GetWindowRect($claude, [ref]$rect) | Out-Null; $haveClaude = $true
  [Cap]::SetWindowPos($claude, [IntPtr]::Zero, -4000, -4000, 0, 0, 0x0001) | Out-Null }  # SWP_NOSIZE move off-screen
# Maximize + foreground the main IEMS window so the app fills the screen behind any module dialog
$main = [Cap]::Find("IEMS - School Management System", $false)
if ($main -ne [IntPtr]::Zero) { [Cap]::ShowWindow($main, 3) | Out-Null }  # SW_MAXIMIZE
$login = [Cap]::Find("IEMS - Login", $false)
if ($login -ne [IntPtr]::Zero) { [Cap]::SetForegroundWindow($login) | Out-Null }
# Bring any module dialog back to front (it was behind nothing; just ensure IEMS is foreground)
Start-Sleep -Milliseconds 350
Add-Type -AssemblyName System.Windows.Forms,System.Drawing
$b = [System.Windows.Forms.SystemInformation]::VirtualScreen
$bmp = New-Object System.Drawing.Bitmap $b.Width, $b.Height
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.CopyFromScreen($b.Location, [System.Drawing.Point]::Empty, $b.Size)
$bmp.Save($Out, [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
# Restore Claude window to where it was
if ($haveClaude) { [Cap]::SetWindowPos($claude, [IntPtr]::Zero, $rect.Left, $rect.Top, 0, 0, 0x0001) | Out-Null }
"saved $Out"
