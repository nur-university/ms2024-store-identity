namespace Identity.Application.Services;

public interface ICorrelationIdProvider
{
    string GetCorrelationId();

    void SetCorrelationId(string correlationId);
}
