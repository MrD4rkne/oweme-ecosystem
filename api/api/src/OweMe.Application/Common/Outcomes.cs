namespace OweMe.Application.Common;

/// <summary>
/// The entity does not exist or the caller must not learn that it does.
/// </summary>
public sealed record NotFound(string Detail);

/// <summary>
/// The caller can see the entity but may not change it.
/// </summary>
public sealed record Forbidden(string Detail);
