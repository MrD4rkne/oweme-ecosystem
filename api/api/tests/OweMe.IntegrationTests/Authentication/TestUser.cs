namespace OweMe.IntegrationTests.Authentication;

public sealed record TestUser
{
    public TestUser()
    {
        Id = Guid.NewGuid();
        Email = $"user{Guid.NewGuid()}@owe.me";
    }

    public TestUser(Guid id, string email)
    {
        Id = id;
        Email = email;
    }

    public Guid Id { get; private init; }

    public string Email { get; private init; }

    public TestUser WithEmail(string email)
    {
        return this with { Email = email };
    }

    public TestUser WithId(Guid id)
    {
        return this with { Id = id };
    }
}