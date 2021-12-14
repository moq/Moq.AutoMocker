namespace Moq.AutoMock.Resolvers;

#if NET45
//Array.Empty does not exist in net45
//This duplicates what was added to .NET Core
internal static class Array
{
    public static T[] Empty<T>()
    {
        return EmptyArray<T>.Value;
    }

    private static class EmptyArray<T>
    {
#pragma warning disable CA1825 // this is the implementation of Array.Empty<T>()
        internal static readonly T[] Value = new T[0];
#pragma warning restore CA1825
    }
}
#endif
