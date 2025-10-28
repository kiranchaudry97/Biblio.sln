namespace Biblio.Dekstop.Services;

public class InMemoryTokenProvider : ISecurityTokenProvider
{
 private string? _token;
 public void SetToken(string? token) => _token = token;
 public string? GetToken() => _token;
}
