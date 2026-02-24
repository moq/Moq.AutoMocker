using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock;

public partial class AutoMocker
{
    /// <summary>
    /// Inserts a resolver after the first occurrence of a resolver of type <typeparamref name="TResolver"/> in the resolver list.
    /// </summary>
    /// <typeparam name="TResolver">The target resolve to insert after.</typeparam>
    /// <param name="resolver">The new resolver to add.</param>
    /// <returns>This <see cref="AutoMocker"/> instances</returns>
    /// <exception cref="InvalidOperationException">Thrown when no resolver instance of type <typeparamref name="TResolver"/> is found</exception>
    public AutoMocker InsertResolverAfter<TResolver>(IMockResolver resolver) where TResolver : IMockResolver
    {
        for (int i = 0; i < Resolvers.Count; i++)
        {
            if (Resolvers[i] is TResolver)
            {
                Resolvers.Insert(i + 1, resolver);
                return this;
            }
        }
        throw new InvalidOperationException($"Could not find resolve of type {typeof(TResolver).FullName} in {nameof(AutoMocker)} instance");
    }

    /// <summary>
    /// Inserts a resolver before the first occurrence of a resolver of type <typeparamref name="TResolver"/> in the resolver list.
    /// </summary>
    /// <typeparam name="TResolver">The target resolve to insert before.</typeparam>
    /// <param name="resolver">The new resolver to add.</param>
    /// <returns>This <see cref="AutoMocker"/> instances</returns>
    /// <exception cref="InvalidOperationException">Thrown when no resolver instance of type <typeparamref name="TResolver"/> is found</exception>
    public AutoMocker InsertResolverBefore<TResolver>(IMockResolver resolver) 
        where TResolver : IMockResolver
    {
        for (int i = 0; i < Resolvers.Count; i++)
        {
            if (Resolvers[i] is TResolver)
            {
                Resolvers.Insert(i, resolver);
                return this;
            }
        }
        throw new InvalidOperationException($"Could not find resolve of type {typeof(TResolver).FullName} in {nameof(AutoMocker)} instance");
    }
}
