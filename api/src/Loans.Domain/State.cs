namespace Loans.Domain;

public sealed class State
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Abbreviation { get; private set; } = string.Empty;
    public bool IsNotAllowed { get; private set; }

    private State() { }

    public State(int id, string name, string abbreviation, bool isNotAllowed = false)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(abbreviation);
        Id = id;
        Name = name;
        Abbreviation = abbreviation;
        IsNotAllowed = isNotAllowed;
    }

    public void SetNotAllowed(bool isNotAllowed) => IsNotAllowed = isNotAllowed;
}
