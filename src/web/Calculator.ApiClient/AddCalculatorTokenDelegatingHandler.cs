using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Calculator.ApiClient;

public class AddCalculatorTokenDelegatingHandler : DelegatingHandler
{
    private readonly ICalculatorTokenProvider _tokenProvider;

    public AddCalculatorTokenDelegatingHandler(ICalculatorTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await _tokenProvider.ApplyTokenTo(request);
        return await base.SendAsync(request, cancellationToken);
    }
}
