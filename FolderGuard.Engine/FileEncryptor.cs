/* * FILE SPEC:
 * ---------------------------------------------------------------------------
 * The encrypted file follows an "Encrypt-then-MAC" construction:
 * * [Offset]      [Size]    [Description]
 *    0             16        Salt (for PBKDF2 key derivation)
 *    16            16        IV (Initialization Vector for AES-CBC)
 *    32            Variable  Ciphertext (AES-256)
 *    End-32        32        HMAC-SHA256 (Integrity tag of all preceding data)
 * ---------------------------------------------------------------------------
 */
using System.Security.Cryptography;
public class FileEncryptor
{
	public static void EncryptFile(EncryptRequest req)
	{
		try
		{
			// 128 bit salt
			byte[] salt = RandomNumberGenerator.GetBytes(16);
			using (Aes aes = Aes.Create())
			{
				// Generate keys from the user password
				using var deriveBytes = new Rfc2898DeriveBytes(req.Password, salt, req.Iterations, HashAlgorithmName.SHA256);
				// Derive two separate keys: one for encryption and one for the HMAC integrity check
				aes.Key = deriveBytes.GetBytes(32);
				byte[] hmacKey = deriveBytes.GetBytes(32);
				byte[] iv = aes.IV;

				byte[] computedHash;

				using (FileStream fsOutput = new FileStream(req.FileOut, FileMode.Create))
				{
					// Write Salt and IV to the start of the file for use during decryption
					fsOutput.Write(salt, 0, salt.Length);
					fsOutput.Write(iv, 0, iv.Length);

					// Input -> Encryptor -> HMAC -> Output
					using (HMACSHA256 hmac = new HMACSHA256(hmacKey))
					{
						using (CryptoStream csHmac = new CryptoStream(fsOutput, hmac, CryptoStreamMode.Write, leaveOpen: true))
						{
							using (ICryptoTransform encryptor = aes.CreateEncryptor())
								using (CryptoStream csEncrypt = new CryptoStream(csHmac, encryptor, CryptoStreamMode.Write))
								using (FileStream fsInput = new FileStream(req.FileIn, FileMode.Open))
								{
									fsInput.CopyTo(csEncrypt);
									// Ensure all buffered data is pushed through
									csEncrypt.FlushFinalBlock();
								}
						}
						computedHash = hmac.Hash ?? throw new Exception("Hash failed");
					}

					// Add the hash to the end of the file
					fsOutput.Write(computedHash, 0, computedHash.Length);
				}
			}
		}
		finally
		{
			// Clear password from memory
			if (req.Password != null)
			{
				Array.Clear(req.Password, 0, req.Password.Length);
			}
		}
	}
}
