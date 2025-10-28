namespace Biblio.Dekstop.Services;

public interface ISecurityTokenProvider
{
 void SetToken(string? token);
 string? GetToken();
}
