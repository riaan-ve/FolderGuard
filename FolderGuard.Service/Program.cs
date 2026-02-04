using FolderGuard.Service;
using FolderGuard.Service.Services;
using System.Runtime.InteropServices;


var builder = Host.CreateApplicationBuilder(args);

// Get the section from appsettings.json
var encryptionOptions = builder.Configuration
    .GetSection("EncryptionSettings")
    .Get<EncryptionOptions>() ?? new EncryptionOptions();

 encryptionOptions.Key = SecurityUtils.GetMaskedBytes();

// If on windows hide console after retrieving password
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
	// Win32 Imports
	[DllImport("kernel32.dll")] static extern IntPtr GetConsoleWindow();
	[DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    var handle = GetConsoleWindow();
    ShowWindow(handle, 0); 
}
// TODO: Handle linux console
if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
	Console.WriteLine("[+] Backgrounding initiated. You can now minimize this terminal.");
}

builder.Services.AddSingleton(encryptionOptions);
builder.Services.AddSingleton<IEncryptionEngine, ExternalEncryptionEngine>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
Array.Clear(encryptionOptions.Key, 0, encryptionOptions.Key.Length);
