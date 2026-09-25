namespace Loans.Api.Contracts;

public sealed record StateResponse(int Id, string Name, string Abbreviation, bool IsNotAllowed);
