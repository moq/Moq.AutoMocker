# Moq.AutoMock  ![Continuous](https://github.com/moq/Moq.AutoMocker/workflows/Continuous/badge.svg) [![NuGet Status](https://img.shields.io/nuget/v/Moq.AutoMock.svg?style=flat)](https://www.nuget.org/packages/Moq.AutoMock)

An automocking container for Moq. Use this if you're invested in your IoC
container and want to decouple your unit tests from changes to their 
constructor arguments.

Usage
======

Simplest usage is to build an instance that you can unit test.

<!-- snippet: Usasge -->
<a id='snippet-usasge'></a>
```cs
var mocker = new AutoMocker();
var car = mocker.CreateInstance<Car>();
Assert.IsNotNull(car.DriveTrain);
Assert.IsInstanceOfType(car.DriveTrain, typeof(IDriveTrain));
Mock<IDriveTrain> mock = Mock.Get(car.DriveTrain);
```
<sup><a href='/Moq.AutoMock.Tests/Snippets.cs#L10-L18' title='File snippet `usasge` was extracted from'>snippet source</a> | <a href='#snippet-usasge' title='Navigate to start of snippet `usasge`'>anchor</a></sup>
<!-- endSnippet -->

If you have a special instance that you need to use, you can register it
with `.Use(...)`. This is very similar to registrations in a regular IoC
container (i.e. `For<IService>().Use(x)` in StructureMap).

<!-- snippet: Register -->
<a id='snippet-register'></a>
```cs
var mocker = new AutoMocker();

mocker.Use<IDriveTrain>(new DriveTrain());
// OR, setup a Mock
mocker.Use<IDriveTrain>(x => x.ShaftLength == 5);

var car = mocker.CreateInstance<Car>();
```
<sup><a href='/Moq.AutoMock.Tests/Snippets.cs#L23-L33' title='File snippet `register` was extracted from'>snippet source</a> | <a href='#snippet-register' title='Navigate to start of snippet `register`'>anchor</a></sup>
<!-- endSnippet -->

Extracting Mocks
----------------

At some point you might need to get to a mock that was auto-generated. For
this, use the `.Get<>()` or `.GetMock<>()` methods.

<!-- snippet: Extract -->
<a id='snippet-extract'></a>
```cs
var mocker = new AutoMocker();

// Let's say you have a setup that needs verifying
mocker.Use<IDriveTrain>(x => x.Accelerate(42) == true);

var car = mocker.CreateInstance<Car>();
car.Accelerate(42);

// Then extract & verify
var driveTrainMock = mocker.GetMock<IDriveTrain>();
driveTrainMock.VerifyAll();
```
<sup><a href='/Moq.AutoMock.Tests/Snippets.cs#L38-L52' title='File snippet `extract` was extracted from'>snippet source</a> | <a href='#snippet-extract' title='Navigate to start of snippet `extract`'>anchor</a></sup>
<!-- endSnippet -->

Alternately, there's an even faster way to verify all mocks in the container:

<!-- snippet: VerifyAll -->
<a id='snippet-verifyall'></a>
```cs
var mocker = new AutoMocker();
mocker.Use<IDriveTrain>(x => x.Accelerate(42) == true);

var car = mocker.CreateInstance<Car>();
car.Accelerate(42);

// This method verifies all mocks in the container
mocker.VerifyAll();
```
<sup><a href='/Moq.AutoMock.Tests/Snippets.cs#L57-L68' title='File snippet `verifyall` was extracted from'>snippet source</a> | <a href='#snippet-verifyall' title='Navigate to start of snippet `verifyall`'>anchor</a></sup>
<!-- endSnippet -->
