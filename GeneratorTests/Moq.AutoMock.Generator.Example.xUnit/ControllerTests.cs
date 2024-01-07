extern alias ExternExample;

using Castle.Core.Logging;
using System.Xml.Linq;
using Controller = ExternExample::Moq.AutoMock.Generator.Example.Controller;

namespace Moq.AutoMock.Generator.Example.xUnit;

[ConstructorTests(TargetType = typeof(Controller))]
public partial class ControllerTests
{
    partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName)
    {
        mocker.Use<string>("");
    }
}

