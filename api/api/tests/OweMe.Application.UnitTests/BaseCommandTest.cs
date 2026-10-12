using Moq;
using OweMe.Application.Groups;
using OweMe.Application.UnitTests.Groups;

namespace OweMe.Application.UnitTests;

public abstract class BaseCommandTest : IAsyncLifetime
{
    private readonly GroupDbContextMoq _groupDbContextMoq;
    protected readonly Mock<TimeProvider> TimeProvider = new();

    protected readonly Mock<IUserContext> UserContextMock = new();

    protected BaseCommandTest()
    {
        _groupDbContextMoq = GroupDbContextMoq.GroupDbContextCreationOptions.New()
            .WithUserContext(UserContextMock.Object)
            .WithTimeProvider(TimeProvider.Object)
            .Build();
    }

    protected Mock<IGroupContext> GroupContextMock => _groupDbContextMoq.GroupContextMock;

    public virtual async ValueTask InitializeAsync()
    {
        await _groupDbContextMoq.SetupAsync();
    }

    public virtual ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return _groupDbContextMoq.DisposeAsync();
    }
}