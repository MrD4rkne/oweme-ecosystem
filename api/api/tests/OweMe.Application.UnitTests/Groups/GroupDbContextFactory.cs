using Microsoft.EntityFrameworkCore;
using Moq;
using OweMe.Application.Groups;
using OweMe.Persistence.Groups;
using OweMe.Tests.Common;

namespace OweMe.Application.UnitTests.Groups;

public class GroupDbContextMoq : PostgresTestBase
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;
    private Mock<GroupDbContext>? _groupContextMock;

    private GroupDbContextMoq(TimeProvider timeProvider,
        IUserContext userContext)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }

    public Mock<IGroupContext> GroupContextMock
    {
        get
        {
            if (_groupContextMock is null)
                throw new InvalidOperationException(
                    $"GroupContextMock is not initialized. Call {nameof(SetupAsync)} first.");

            return _groupContextMock.As<IGroupContext>();
        }
    }

    public static GroupDbContextMoq Create(GroupDbContextCreationOptions options)
    {
        if (options.TimeProvider is null) options = options.WithTimeProvider(new Mock<TimeProvider>().Object);

        if (options.UserContext is null) options = options.WithUserContext(new Mock<IUserContext>().Object);

        return new GroupDbContextMoq(options.TimeProvider!, options.UserContext!);
    }

    public override async Task SetupAsync()
    {
        await base.SetupAsync();

        var dbOptions = new DbContextOptionsBuilder<GroupDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _groupContextMock = new Mock<GroupDbContext>(
            dbOptions,
            _timeProvider,
            _userContext
        )
        {
            CallBase = true
        };

        await _groupContextMock.Object.Database.EnsureCreatedAsync();
    }

    public IGroupContext GetGroupContext()
    {
        return GroupContextMock.Object;
    }

    public readonly struct GroupDbContextCreationOptions()
    {
        public TimeProvider? TimeProvider { get; init; } = null;
        public IUserContext? UserContext { get; init; } = null;

        public GroupDbContextCreationOptions WithOptions(DbContextOptions<GroupDbContext> options)
        {
            return new GroupDbContextCreationOptions
            {
                TimeProvider = TimeProvider,
                UserContext = UserContext
            };
        }

        public GroupDbContextCreationOptions WithTimeProvider(TimeProvider timeProvider)
        {
            return new GroupDbContextCreationOptions
            {
                TimeProvider = timeProvider,
                UserContext = UserContext
            };
        }

        public GroupDbContextCreationOptions WithUserContext(IUserContext userContext)
        {
            return new GroupDbContextCreationOptions
            {
                TimeProvider = TimeProvider,
                UserContext = userContext
            };
        }

        public static GroupDbContextCreationOptions New()
        {
            return new GroupDbContextCreationOptions();
        }

        public GroupDbContextMoq Build()
        {
            return Create(this);
        }
    }
}