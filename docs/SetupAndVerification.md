# Setup and verification

Namespace: `Moq.AutoMock`. [Documentation index](README.md).

These `AutoMocker` shortcuts retrieve or create the service's mock, then delegate to Moq. All service type parameters are constrained to `class`. Null setup/verification expressions throw `ArgumentNullException`; verification failures throw `MockException`.

## Setup

```csharp
ISetup<TService> Setup<TService>(Expression<Action<TService>> setup)
ISetup<TService, TReturn> Setup<TService, TReturn>(
    Expression<Func<TService, TReturn>> setup)
```

Use the first overload for void methods and the second for return values or properties. Both return Moq's fluent setup API.

```csharp
mocker.Setup<IStore>(x => x.Save("item")).Verifiable();
mocker.Setup<IStore, string>(x => x.Find(42)).Returns("item");
```

## SetupSequence

```csharp
ISetupSequentialResult<TReturn> SetupSequence<TService, TReturn>(
    Expression<Func<TService, TReturn>> setup)
```

Configures successive return values or exceptions.

```csharp
mocker.SetupSequence<IStore, string>(x => x.Find(42))
    .Returns("first")
    .Returns("second")
    .Throws(new InvalidOperationException("No more items."));
```

## SetupAllProperties

```csharp
Mock<TService> SetupAllProperties<TService>()
```

Enables property value tracking and returns the mock.

```csharp
Mock<ISettings> settings = mocker.SetupAllProperties<ISettings>();
settings.Object.Name = "test";
string name = mocker.Get<ISettings>().Name; // "test"
```

## SetupWithAny / MockExtensions.SetupWithAny

```csharp
// AutoMocker methods:
ISetup<TService> SetupWithAny<TService>(string methodName)
ISetup<TService, TReturn> SetupWithAny<TService, TReturn>(string methodName)

// MockExtensions extension methods:
ISetup<T> SetupWithAny<T>(this Mock<T> mock, string methodName)
ISetup<T, TResult> SetupWithAny<T, TResult>(this Mock<T> mock, string methodName)
```

Sets up a method by name, supplying `It.IsAny<T>()` for each argument. Use the return-type overload for non-void methods. The method must not be overloaded: missing methods throw `MissingMethodException`, ambiguous names throw `AmbiguousMatchException`, and null names or mock receivers throw `ArgumentNullException`.

```csharp
mocker.SetupWithAny<IStore>(nameof(IStore.Save)).Verifiable();
mocker.SetupWithAny<IStore, string>(nameof(IStore.Find)).Returns("item");

var store = new Mock<IStore>();
store.SetupWithAny<IStore, string>(nameof(IStore.Find)).Returns("item");
```

## Verify

```csharp
void Verify()
void Verify<T>() where T : class
```

`Verify()` checks verifiable setups on every tracked mock. `Verify<T>()` checks only the requested mock. Neither means "verify all setups"; use `VerifyAll` for that.

Expression overloads verify matching calls. All support `where T : class`:

```csharp
void Verify<T>(Expression<Action<T>> expression)
void Verify<T>(Expression<Action<T>> expression, Times times)
void Verify<T>(Expression<Action<T>> expression, Func<Times> times)
void Verify<T>(Expression<Action<T>> expression, string failMessage)
void Verify<T>(Expression<Action<T>> expression, Times times, string failMessage)
void Verify<T>(Expression<Action<T>> expression, Func<Times> times, string failMessage)

void Verify<T>(Expression<Func<T, object>> expression)
void Verify<T>(Expression<Func<T, object>> expression, Times times)
void Verify<T>(Expression<Func<T, object>> expression, Func<Times> times)
void Verify<T>(Expression<Func<T, object>> expression, string failMessage)
void Verify<T>(Expression<Func<T, object>> expression, Times times, string failMessage)

void Verify<T, TResult>(Expression<Func<T, TResult>> expression)
void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times)
void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Func<Times> times)
void Verify<T, TResult>(Expression<Func<T, TResult>> expression, string failMessage)
void Verify<T, TResult>(
    Expression<Func<T, TResult>> expression, Times times, string failMessage)
```

Without `times`, expression overloads require at least one call. `Func<Times>` supports deferred counts. Use `Verify<T, TResult>` for value-type returns: the `object` expression overloads reject boxing a value-type result with `NotSupportedException`.

```csharp
mocker.Setup<IStore>(x => x.Save("item")).Verifiable();
mocker.Get<IStore>().Save("item");
mocker.Verify<IStore>(x => x.Save("item"), Times.Once(), "Save was not called.");
mocker.Verify<IStore>();
mocker.Verify();

mocker.Get<ICounter>().Next();
mocker.Verify<ICounter, int>(x => x.Next(), Times.Once());
```

## VerifyAll

```csharp
void VerifyAll(bool ignoreMissingSetups = false)
```

Checks every setup on every tracked mock, including setups not marked verifiable. If no tracked mock has any setups, throws `InvalidOperationException` unless `ignoreMissingSetups` is true. That flag only permits an empty set; it does not suppress unmet expectations.

```csharp
mocker.Setup<IStore, string>(x => x.Find(42)).Returns("item");
mocker.Get<IStore>().Find(42);
mocker.VerifyAll();

var empty = new AutoMocker();
empty.VerifyAll(ignoreMissingSetups: true);
```
