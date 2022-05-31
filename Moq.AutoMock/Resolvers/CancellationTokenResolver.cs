using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moq.AutoMock.Resolvers;
/// <summary>
/// A resolve that can provide a <see cref="CancellationToken"/>
/// </summary>
public class CancellationTokenResolver : IMockResolver
{
    /// <inheritdoc />
    public void Resolve(MockResolutionContext context)
    {
        if (context.RequestType == typeof(CancellationToken))
        {
            if (context.AutoMocker.ResolvedObjects?.TryGetValue(typeof(CancellationTokenSource), out object? ctsObject) == true &&
                ctsObject is CancellationTokenSource cts)
            {
                context.Value = cts.Token;
            }
            else
            {
                context.Value = CancellationToken.None;
            }
        }
    }
}
