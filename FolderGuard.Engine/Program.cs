using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Specialized;

class Program
{
	static async Task<int> Main()
	{
		try
		{
			string inputJson = await Console.In.ReadToEndAsync();
			if (string.IsNullOrWhiteSpace(inputJson)) return 1;

			var request = JsonSerializer.Deserialize<EncryptRequest>(inputJson);
			if (request == null) return 1;

			switch (request.Action.ToLower())
			{
				case "encrypt":
				case "-e":
					Console.WriteLine($"Encrypting {request.FileIn}...");
					FileEncryptor.EncryptFile(request);
					Console.WriteLine("Success.");
					break;

				case "decrypt":
				case "-d":
					Console.WriteLine($"Decrypting {request.FileIn}...");
					//FileDecryptor.DecryptFile(request);
					//Console.WriteLine("Success.");
					break;

				case "help":
				case "-h":
					Console.WriteLine("JSON Format: { \"Action\": \"encrypt\", \"Password\": \"...\", \"FileIn\": \"...\", \"FileOut\": \"...\" }");
					break;

				default:
					Console.WriteLine("Unknown action.");
					return 1;
			}

			return 0;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"ERROR: {ex.Message}");
			return 10;
		}
	}
}
