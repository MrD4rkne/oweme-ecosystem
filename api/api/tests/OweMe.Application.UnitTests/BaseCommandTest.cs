using Moq;
using OweMe.Application.Groups;
using OweMe.Application.Personas;
using OweMe.Application.UnitTests.Groups;

namespace OweMe.Application.UnitTests;

public abstract class BaseCommandTest : IAsyncLifetime
{
    protected readonly Mock<TimeProvider> TimeProvider = new();

    protected readonly Mock<IUserContext> UserContextMock = new();
    private readonly GroupDbContextMoq _groupDbContext;

    protected BaseCommandTest()
    {
        _groupDbContext = GroupDbContextMoq.GroupDbContextCreationOptions.New()
            .WithUserContext(UserContextMock.Object)
            .WithTimeProvider(TimeProvider.Object)
            .Build();
    }

    protected IGroupContext GroupContext => _groupDbContext.GroupContextMock;

    protected IPersonaContext PersonaContext => _groupDbContext.PersonaContextMock;

    public virtual async ValueTask InitializeAsync()
    {
        await _groupDbContext.SetupAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _groupDbContext.DisposeAsync();
    }
}