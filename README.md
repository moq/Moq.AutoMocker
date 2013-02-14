An automocking container for Moq. Use this if you're invested in your IoC
container and want to decouple your unit tests from changes to their 
constructor arguments.

Usage
======

Simplest usage is to build an instance that you can unit test.

```csharp
var mocker = new AutoMocker();
var car = mocker.GetInstance<Car>();

car.DriveTrain.ShouldNotBeNull();
car.DriveTrain.ShouldImplement<IDriveTrain>();
Mock<IDriveTrain> mock = Mock.Get(car.DriveTrain);
```

If you have a special instance that you need to use, you can register it
with `.Use(...)`. This is very similar to registrations in a regular IoC
container (i.e. `For<IService>().Use(x)` in StructureMap).

```csharp
var mocker = new AutoMocker();

mocker.Use<IDriveTrain>(new DriveTrain);
// OR, setup a Mock
mocker.Use<IDriveTrain>(x => x.Shaft.Length == 5);

var car = mocker.GetInstance<Car>();
```

Extracting Mocks
----------------

At some point you might need to get to a mock that was auto-generated. For
this, use the `.Extract<>()` or `.ExtractMock<>()` methods.

```csharp
var mocker = new AutoMocker();

// Let's say you have a setup that needs verifying
mocker.Use<IDriveTrain>(x => x.Accelerate(42) == true);

var car = mocker.GetInstance<Car>();
car.Accelarate(42);

// Then extract & verify
var driveTrainMock = mocker.ExtractMock<IDriveTrain>();
driveTrainMock.VerifyAll();
```

Alternately, there's an even faster way to verify all mocks in the container:

```csharp
var mocker = new AutoMocker();
mocker.Use<IDriveTrain>(x => x.Accelerate(42) == true);

var car = mocker.GetInstance<Car>();
car.Accelarate(42);

// This method verifies all mocks in the container
mocker.VerifyAll();
```
