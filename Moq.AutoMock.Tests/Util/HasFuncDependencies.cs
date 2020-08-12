using System;

namespace Moq.AutoMock.Tests.Util
{
    public class HasFuncDependencies
    {
        public Func<WithService> WithServiceFactory { get; }
        
        public HasFuncDependencies(Func<WithService> withServiceFactory)
        {
            WithServiceFactory = withServiceFactory;
        }
    }
}