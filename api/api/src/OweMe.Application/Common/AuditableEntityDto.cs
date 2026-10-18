using OweMe.Domain.Users;

namespace OweMe.Application.Common;

public record AuditableEntityDto(
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    UserId CreatedBy,
    UserId? UpdatedBy);