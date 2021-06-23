using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Moq.AutoMock.Tests
{
    [TestClass]
    public class ResolvesSelf
    {
        [TestMethod]
        public void Resolves_self_instance_directly()
        {
            AutoMocker mocker = new();

            var resolved = mocker.Get<AutoMocker>();

            Assert.IsTrue(ReferenceEquals(mocker, resolved));
        }

        [TestMethod]
        public void Resolves_self_instances_when_a_dependency()
        {
            AutoMocker mocker = new();
            mocker.With<HasAutoMockerDependency>();

            var hasDependency = mocker.Get<HasAutoMockerDependency>();

            Assert.IsTrue(ReferenceEquals(mocker, hasDependency.Mocker));
        }

        private record HasAutoMockerDependency(AutoMocker Mocker);
    }
}
