namespace Moq.AutoMock.Resolvers;

/// <summary>
/// Resolves calls to retireve AutoMocker with itself.
/// </summary>
public class SelfResolver : SimpleTypeResolver<AutoMocker>
{
    /// <inheritdoc />
    public override void Resolve(MockResolutionContext context)
    {
        if (context.ObjectGraphContext.IsMockCreation)
        {
            return;
        }
        base.Resolve(context);
    }

    /// <inheritdoc />
    protected override AutoMocker GetValue(MockResolutionContext context) 
        => context.AutoMocker;
}
