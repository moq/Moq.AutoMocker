using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moq.AutoMock.Generator.Example.xUnit;

[ConstructorTests(TargetType = typeof(ControllerString))]
public partial class ControllerStringTests
{
    partial void AutoMockerTestSetup(Moq.AutoMock.AutoMocker mocker, string testName)
    {
        mocker.Use<string>("");
    }
}
