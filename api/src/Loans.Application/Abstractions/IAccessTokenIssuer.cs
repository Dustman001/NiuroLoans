using Loans.Domain;

namespace Loans.Application.Abstractions;

public interface IAccessTokenIssuer
{
    string Issue(User user);
}
