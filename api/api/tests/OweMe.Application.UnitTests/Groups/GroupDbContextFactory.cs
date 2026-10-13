using Microsoft.EntityFrameworkCore;
using Moq;
using OweMe.Application.Groups;
using OweMe.Application.Personas;
using OweMe.Persistence.Data;
using OweMe.Tests.Common;

namespace OweMe.Application.UnitTests.Groups;

public class GroupDbContextMoq : PostgresTestBase
{
    private readonly TimeProvider _timeProvider;
    private readonly IUserContext _userContext;

    private GroupDbContextMoq(TimeProvider timeProvider,
        IUserContext userContext)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
    }
    
    public Mock<IGroupContext> GroupContextMock { get; private set; } = null!;

    public Mock<IPersonaContext> PersonaContextMock { get; private set; } = null!;

    private static GroupDbContextMoq Create(GroupDbContextCreationOptions options)
    {
        if (options.TimeProvider is null) options = options.WithTimeProvider(new Mock<TimeProvider>().Object);

        if (options.UserContext is null) options = options.WithUserContext(new Mock<IUserContext>().Object);

        return new GroupDbContextMoq(options.TimeProvider!, options.UserContext!);
    }

    public override async Task SetupAsync()
    {
        await base.SetupAsync();

        var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        var groupContextMock = new Mock<ApplicationDbContext>(
            dbOptions,
            _timeProvider,
            _userContext
        )
        {
            CallBase = true
        };
        await groupContextMock.Object.Database.EnsureCreatedAsync();
        
        GroupContextMock = groupContextMock.As<IGroupContext>();
        PersonaContextMock = groupContextMock.As<IPersonaContext>();
    }

    public readonly struct GroupDbContextCreationOptions()
    {
        public TimeProvider? TimeProvider { get; private init; } = null;
        public IUserContext? UserContext { get; private init; } = null;

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