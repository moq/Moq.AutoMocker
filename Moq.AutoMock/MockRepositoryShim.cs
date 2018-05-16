using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock
{
    // FRAGILE: Necessary to shim outside mocks into the repository, see https://github.com/moq/moq4/issues/617
    public class MockRepositoryShim : MockRepository
    {
        private readonly FieldInfo defaultBehaviorProp;

        public MockRepositoryShim(MockBehavior defaultBehavior)
            : base(defaultBehavior)
        {
#pragma warning disable 618
            // FRAGILE: MockFactory will be removed in v5
            defaultBehaviorProp = (
                from p in typeof(MockFactory).GetRuntimeFields().ToList()
                where p.Name == "defaultBehavior"
                select p
            ).First();
#pragma warning restore 618
        }

        public MockBehavior MockBehavior
        {
            get => (MockBehavior)this.defaultBehaviorProp.GetValue(this);
            set => this.defaultBehaviorProp.SetValue(this, value);
        }

        public void Add(Mock mock)
        {
            var mocks = (ICollection<Mock>)this.Mocks;
            if (!mocks.Contains(mock))
            {
                mocks.Add(mock);
            }
        }

    }
}
