using Identity.Application.Services;

namespace Identity.Infrastructure.Services;

internal class CorrelationIdProvider : ICorrelationIdProvider
{
    private string _correlationId;

    public CorrelationIdProvider()
    {
        _correlationId = Guid.NewGuid().ToString();
    }
    public string GetCorrelationId()
    {
        return _correlationId;
    }

    public void SetCorrelationId(string correlationId)
    {
        _correlationId = correlationId;
    }
}
