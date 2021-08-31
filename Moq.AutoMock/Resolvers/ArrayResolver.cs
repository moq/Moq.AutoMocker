using System;

namespace Moq.AutoMock.Resolvers
{
    /// <summary>
    /// Provides a means to create arrays.
    /// </summary>
    public class ArrayResolver : IMockResolver
    {
        /// <inheritdocs />
        public void Resolve(MockResolutionContext context)
        {
            if (context.RequestType.IsArray && context.RequestType != typeof(string))
            {
                Type elmType = context.RequestType.GetElementType() ?? throw new InvalidOperationException($"Could not determine element type for '{context.RequestType}'");
                MockArrayInstance arrayInstance = new(elmType);
                if (context.AutoMocker.TryGet(elmType, context.ObjectGraphContext, out IInstance? instance) &&
                    !instance.IsMock)
                {
                    arrayInstance.Add(instance);
                }
                context.Value = arrayInstance;
            }
        }
    }
}
