using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.AutoMock;

public class Snippets
{
    void Usasge()
    {
        #region Usasge

        var mocker = new AutoMocker();
        var car = mocker.CreateInstance<Car>();
        Assert.IsNotNull(car.DriveTrain);
        Assert.IsInstanceOfType(car.DriveTrain, typeof(IDriveTrain));
        Mock<IDriveTrain> mock = Mock.Get(car.DriveTrain);

        #endregion
    }

    void Register()
    {
        #region Register

        var mocker = new AutoMocker();

        mocker.Use<IDriveTrain>(new DriveTrain());
        // OR, setup a Mock
        mocker.Use<IDriveTrain>(x => x.ShaftLength == 5);

        var car = mocker.CreateInstance<Car>();

        #endregion
    }

    void Extract()
    {
        #region Extract

        var mocker = new AutoMocker();

        // Let's say you have a setup that needs verifying
        mocker.Use<IDriveTrain>(x => x.Accelerate(42) == true);

        var car = mocker.CreateInstance<Car>();
        car.Accelerate(42);

        // Then extract & verify
        var driveTrainMock = mocker.GetMock<IDriveTrain>();
        driveTrainMock.VerifyAll();

        #endregion
    }

    void VerifyAll()
    {
        #region VerifyAll

        var mocker = new AutoMocker();
        mocker.Use<IDriveTrain>(x => x.Accelerate(42) == true);

        var car = mocker.CreateInstance<Car>();
        car.Accelerate(42);

// This method verifies all mocks in the container
        mocker.VerifyAll();

        #endregion
    }
}

internal class DriveTrain : IDriveTrain
{
    public int ShaftLength { get; set; }
    public bool Accelerate(int i)
    {
        throw new System.NotImplementedException();
    }
}

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes")]
public class Car
{
    public IDriveTrain DriveTrain { get; set; } = null!;

    public void Accelerate(int i)
    {
        throw new System.NotImplementedException();
    }
}

public interface IDriveTrain
{
    int ShaftLength { get; set; }
    bool Accelerate(int i);
}
