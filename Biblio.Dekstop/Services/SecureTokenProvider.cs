using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Biblio.Dekstop.Services;

public class SecureTokenProvider : ISecurityTokenProvider
{
 private readonly string _filePath;

 public SecureTokenProvider()
 {
 var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Biblio");
 if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
 _filePath = Path.Combine(dir, "token.bin");
 }

 public void SetToken(string? token)
 {
 if (string.IsNullOrWhiteSpace(token))
 {
 try { if (File.Exists(_filePath)) File.Delete(_filePath); } catch { }
 return;
 }

 var bytes = Encoding.UTF8.GetBytes(token);
 try
 {
 var protectedBytes = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);
 File.WriteAllBytes(_filePath, protectedBytes);
 }
 catch
 {
 // swallow exceptions to avoid crashing UI; token won't be persisted
 }
 }

 public string? GetToken()
 {
 try
 {
 if (!File.Exists(_filePath)) return null;
 var protectedBytes = File.ReadAllBytes(_filePath);
 var bytes = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
 return Encoding.UTF8.GetString(bytes);
 }
 catch
 {
 return null;
 }
 }
}
