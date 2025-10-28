using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Biblio.Dekstop.Services;

public class AuthHeaderHandler : DelegatingHandler
{
 private readonly ISecurityTokenProvider _tokenProvider;
 public AuthHeaderHandler(ISecurityTokenProvider tokenProvider)
 {
 _tokenProvider = tokenProvider;
 }

 protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
 {
 var token = _tokenProvider.GetToken();
 if (!string.IsNullOrWhiteSpace(token))
 {
 request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
 }
 return base.SendAsync(request, cancellationToken);
}
}
