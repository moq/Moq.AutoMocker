namespace Moq.AutoMock.Resolvers;

/// <summary>
/// Resolves calls to retireve AutoMocker with itself.
/// </summary>
public class SelfResolver : SimpleTypeResolver<AutoMocker>
{
    /// <inheritdoc />
    protected override AutoMocker GetValue(MockResolutionContext context)
        => context.AutoMocker;
}
