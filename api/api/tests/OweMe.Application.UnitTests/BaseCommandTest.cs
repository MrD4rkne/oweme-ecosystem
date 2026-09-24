using Moq;
using OweMe.Application.Groups;
using OweMe.Application.UnitTests.Groups;

namespace OweMe.Application.UnitTests;

public abstract class BaseCommandTest : IAsyncLifetime
{
    private readonly GroupDbContextMoq _groupDbContextMoq;
    protected readonly Mock<TimeProvider> _timeProvider = new();

    protected readonly Mock<IUserContext> _userContextMock = new();

    protected BaseCommandTest()
    {
        _groupDbContextMoq = GroupDbContextMoq.GroupDbContextCreationOptions.New()
            .WithUserContext(_userContextMock.Object)
            .WithTimeProvider(_timeProvider.Object)
            .Build();
    }

    protected Mock<IGroupContext> _groupContextMock => _groupDbContextMoq.GroupContextMock;

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