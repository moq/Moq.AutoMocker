# Moq.AutoMock  ![Continuous](https://github.com/moq/Moq.AutoMocker/workflows/Continuous/badge.svg) [![NuGet Status](https://img.shields.io/nuget/v/Moq.AutoMock.svg?style=flat)](https://www.nuget.org/packages/Moq.AutoMock)

An automocking container for Moq. Use this if you're invested in your IoC
container and want to decouple your unit tests from changes to their 
constructor arguments.

Usage
======

Simplest usage is to build an instance that you can unit test.

```csharp
var mocker = new AutoMocker();
var car = mocker.CreateInstance<Car>();

car.DriveTrain.ShouldNotBeNull();
car.DriveTrain.ShouldImplement<IDriveTrain>();
Mock<IDriveTrain> mock = Mock.Get(car.DriveTrain);
```

If you have a special instance that you need to use, you can register it
with `.Use(...)`. This is very similar to registrations in a regular IoC
container (i.e. `For<IService>().Use(x)` in StructureMap).

```csharp
var mocker = new AutoMocker();

mocker.Use<IDriveTrain>(new DriveTrain());
// OR, setup a Mock
mocker.Use<IDriveTrain>(x => x.Shaft.Length == 5);

var car = mocker.CreateInstance<Car>();
```

Extracting Mocks
----------------

At some point you might need to get to a mock that was auto-generated. For
this, use the `.Get<>()` or `.GetMock<>()` methods.

```csharp
var mocker = new AutoMocker();

// Let's say you have a setup that needs verifying
mocker.Use<IDriveTrain>(x => x.Accelerate(42) == true);

var car = mocker.CreateInstance<Car>();
car.Accelerate(42);

// Then extract & verify
var driveTrainMock = mocker.GetMock<IDriveTrain>();
driveTrainMock.VerifyAll();
```

Alternately, there's an even faster way to verify all mocks in the container:

```csharp
var mocker = new AutoMocker();
mocker.Use<IDriveTrain>(x => x.Accelerate(42) == true);

var car = mocker.CreateInstance<Car>();
car.Accelerate(42);

// This method verifies all mocks in the container
mocker.VerifyAll();
```

Documentation
=============

For more detailed documentation, including information about the built-in source generators that can automatically generate test boilerplate code, see the [docs folder](docs/).

- [AutoMocker API Reference](docs/Moq.AutoMock.md)
- [Source Generators](docs/SourceGenerators.md) - Learn about automatic code generation for constructor tests, options configuration, logging, and more
