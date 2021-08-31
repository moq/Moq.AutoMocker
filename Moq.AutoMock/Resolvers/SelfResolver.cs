namespace Moq.AutoMock.Resolvers
{
    /// <summary>
    /// Resolves calls to retireve AutoMocker with itself.
    /// </summary>
    public class SelfResolver : IMockResolver
    {
        /// <summary>
        /// Resolves a request for AutoMocker with itself.
        /// </summary>
        /// <param name="context">The mock resolution context</param>
        public void Resolve(MockResolutionContext context)
        {
            if (context.RequestType == typeof(AutoMocker))
            {
                context.Value = context.AutoMocker;
            }
        }
    }
}
