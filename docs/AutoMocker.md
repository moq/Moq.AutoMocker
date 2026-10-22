# AutoMocker

Namespace: `Moq.AutoMock`. [Documentation index](README.md).

## Constructors and properties

```csharp
AutoMocker()
AutoMocker(MockBehavior mockBehavior)
AutoMocker(MockBehavior mockBehavior, DefaultValue defaultValue)
AutoMocker(MockBehavior mockBehavior, DefaultValue defaultValue, bool callBase)
AutoMocker(
    MockBehavior mockBehavior, DefaultValue defaultValue,
    DefaultValueProvider? defaultValueProvider, bool callBase)
```

Defaults: `MockBehavior.Default` (loose), `DefaultValue.Empty`, no custom provider, `callBase: false`. These settings apply to automatically created mocks, not explicitly supplied mocks.

| Property | Type | Meaning |
| --- | --- | --- |
| `MockBehavior` | `MockBehavior` | Behavior of created mocks |
| `DefaultValue` | `DefaultValue` | Default return-value strategy for loose mocks |
| `DefaultValueProvider` | `DefaultValueProvider?` | Optional custom return-value provider |
| `CallBase` | `bool` | Call real virtual implementations when no setup matches |
| `Resolvers` | `IList<IMockResolver>` | Mutable, ordered [resolution pipeline](Resolvers.md) |
| `ResolvedObjects` | `IReadOnlyDictionary<Type, object?>` | Snapshot of cached values; mocked entries contain `Mock` wrappers, not their `.Object` |

The setting properties are read-only.

```csharp
var mocker = new AutoMocker(MockBehavior.Strict, DefaultValue.Empty);
Mock<IStore> store = mocker.GetMock<IStore>();
object? cachedMock = mocker.ResolvedObjects[typeof(IStore)];
bool callsBase = mocker.CallBase;
```

## CreateInstance

```csharp
T CreateInstance<T>() where T : class
T CreateInstance<T>(bool enablePrivate) where T : class
object CreateInstance(Type type)
object CreateInstance(Type type, bool enablePrivate)
```

Constructs a real object using the longest resolvable constructor. Dependencies come from registrations or resolvers. Each call creates a new root object; dependencies are normally cached. `enablePrivate` defaults to `false`; `true` includes non-public constructors, but does not bypass proxy accessibility restrictions.

Throws `ArgumentNullException` for a null type, or [ObjectCreationException](Diagnostics.md#objectcreationexception) when no constructor can be used. Exceptions thrown by the constructor propagate.

```csharp
var service = mocker.CreateInstance<OrderService>();
var another = (OrderService)mocker.CreateInstance(typeof(OrderService));
var internalService = mocker.CreateInstance<InternalService>(enablePrivate: true);
```

## Get

```csharp
TService Get<TService>()
TService Get<TService>(bool enablePrivate)
object Get(Type serviceType)
object Get(Type serviceType, bool enablePrivate)
```

Returns the registered or cached service object, otherwise resolves and caches one. Unlike `CreateInstance`, an unregistered class normally resolves to a mock's `.Object`, not a real instance. `enablePrivate` defaults to `false`. Registered nulls are returned as null (or `default(TService)` by generic overloads).

Throws `ArgumentNullException` for a null type or `ArgumentException` if resolution fails.

```csharp
IStore store = mocker.Get<IStore>();
object sameStore = mocker.Get(typeof(IStore));
CancellationToken token = mocker.Get<CancellationToken>();
```

### IServiceProvider.GetService

`AutoMocker` explicitly implements `IServiceProvider.GetService(Type)`. It uses the same pipeline and cache, returning null when resolution fails rather than throwing the unresolved-service `ArgumentException`.

```csharp
IServiceProvider provider = mocker;
var store = (IStore?)provider.GetService(typeof(IStore));
```

## GetMock

```csharp
Mock<TService> GetMock<TService>() where TService : class
Mock<TService> GetMock<TService>(bool enablePrivate) where TService : class
Mock GetMock(Type serviceType)
Mock GetMock(Type serviceType, bool enablePrivate)
```

Returns or creates the cached `Mock` wrapper. `enablePrivate` defaults to `false`. Throws `ArgumentException` if the service is not stored as a mock and `ArgumentNullException` for a null type. Register supplied mocks with `Use(Mock<T>)`, rather than only registering their `.Object`.

```csharp
Mock<IStore> store = mocker.GetMock<IStore>();
store.Setup(x => x.Find(42)).Returns("item");
IStore dependency = store.Object;
```

## Use

All overloads return the same `AutoMocker` for chaining.

```csharp
AutoMocker Use<TService>(TService? service)
AutoMocker Use(Type type, object? service)
AutoMocker Use<TService>(Mock<TService> mockedService) where TService : class
AutoMocker Use<TService>(Expression<Func<TService, bool>> setup) where TService : class
AutoMocker Use<TService>(Func<AutoMocker, TService> factory) where TService : class
AutoMocker Use<TService>(Func<TService> factory) where TService : class
AutoMocker Use<TService, TImplementation>()
    where TService : class
    where TImplementation : class, TService
```

Instance overloads register immediately, including null, replacing an existing different value. Registering the same instance twice under the same type throws `InvalidOperationException`; incompatible values throw `ArgumentException`. The expression overload uses `Mock.Of` setup syntax.

Factories run on first resolution and cache their result. Existing cached values take precedence. `Use<TService, TImplementation>()` defers to `Get<TImplementation>()`, so the implementation is normally mocked. Use `With` to construct a real implementation.

```csharp
mocker.Use<IStore>(new Store())
    .Use<string>("test");
mocker.Use(typeof(int), 42);
mocker.Use(new Mock<IClock>());
mocker.Use<ISettings>(x => x.Name == "test");
mocker.Use<IOrderService>(m => new OrderService(m.Get<IStore>()));
mocker.Use<IWorker>(() => new Worker());
mocker.Use<IReader, Reader>();
```

## With

```csharp
TImplementation With<TImplementation>() where TImplementation : class
TImplementation With<TService, TImplementation>()
    where TImplementation : class, TService
object With(Type implementationType)
object With(Type serviceType, Type implementationType)
```

Constructs a real implementation immediately, registers it, and returns it. One-type overloads register under the implementation type; two-type overloads register only under the service type.

```csharp
Store store = mocker.With<IStore, Store>();
OrderService service = mocker.With<OrderService>();
object clock = mocker.With(typeof(IClock), typeof(Clock));
```

## CreateSelfMock

```csharp
T CreateSelfMock<T>() where T : class
T CreateSelfMock<T>(bool enablePrivate) where T : class
T CreateSelfMock<T>(
    bool enablePrivate = false,
    MockBehavior? mockBehavior = null,
    DefaultValue? defaultValue = null,
    DefaultValueProvider? defaultValueProvider = null,
    bool? callBase = null) where T : class
```

Creates a class proxy with constructor dependencies resolved by the container. The root is not registered. Virtual and abstract members can be configured through `Mock.Get(result)`; non-virtual members run normally. Unspecified settings inherit the container's settings. To run real virtual implementations when no setup matches, pass `callBase: true`.

```csharp
var worker = mocker.CreateSelfMock<Worker>(callBase: true);
Mock.Get(worker).Setup(x => x.Load()).Returns("test");
```

## WithSelfMock

```csharp
T WithSelfMock<T>(
    bool enablePrivate = false, MockBehavior? mockBehavior = null,
    DefaultValue? defaultValue = null,
    DefaultValueProvider? defaultValueProvider = null, bool? callBase = null)
    where T : class
Mock<TService> WithSelfMock<TService, TImplementation>(
    bool enablePrivate = false, MockBehavior? mockBehavior = null,
    DefaultValue? defaultValue = null,
    DefaultValueProvider? defaultValueProvider = null, bool? callBase = null)
    where TImplementation : class, TService
    where TService : class
object WithSelfMock(
    Type implementationType,
    bool enablePrivate = false, MockBehavior? mockBehavior = null,
    DefaultValue? defaultValue = null,
    DefaultValueProvider? defaultValueProvider = null, bool? callBase = null)
object WithSelfMock(
    Type serviceType, Type implementationType,
    bool enablePrivate = false, MockBehavior? mockBehavior = null,
    DefaultValue? defaultValue = null,
    DefaultValueProvider? defaultValueProvider = null, bool? callBase = null)
```

Like `CreateSelfMock`, but registers the created proxy. Two-type overloads register it under both types; the service type must be an interface suitable for `Mock.As<TService>()`. Note the return difference: the two-type generic overload returns `Mock<TService>`; all other overloads return the service object.

```csharp
Mock<IWorker> worker = mocker.WithSelfMock<IWorker, Worker>(callBase: true);
worker.Setup(x => x.Load()).Returns("test");
Worker sameWorker = mocker.Get<Worker>();
```

## Combine

```csharp
void Combine(Type type, params Type[] forwardTo)
void Combine<TService, TAsWellAs1>()
// Generic overloads accept 2 through 11 type arguments.
```

Forwards all supplied types to the first type's mock. Additional types must be interfaces supported by Moq's `As<T>()`. Call before accessing the mock's `.Object` or resolving consumers; Moq cannot add new interfaces after the proxy is initialized. The first service must resolve to a mock, not a real instance.

```csharp
mocker.Combine<IReader, IWriter>();
IReader reader = mocker.Get<IReader>();
IWriter writer = mocker.Get<IWriter>();
bool same = ReferenceEquals(reader, writer); // true

// Equivalent runtime-type form, on a separate container:
var runtimeMocker = new AutoMocker();
runtimeMocker.Combine(typeof(IReader), typeof(IWriter));
```

## AsDisposable

```csharp
IDisposable AsDisposable()
```

Returns a cleanup handle for values in `ResolvedObjects` implementing `IDisposable`. Dispose it after exercising the test. Mocked entries are `Mock` wrappers, so their disposable `.Object` values are not disposed by this handle. It also excludes uncached roots from `CreateInstance` and does not perform asynchronous disposal.

```csharp
var mocker = new AutoMocker();
using var cleanup = mocker.AsDisposable();
mocker.Use<Stream>(new MemoryStream());
var service = mocker.CreateInstance<OrderService>();
```

## Setup, verification, and resolver insertion

See [Setup and verification](SetupAndVerification.md) for all setup and verification overloads, and [Resolvers](Resolvers.md#pipeline) for `InsertResolverBefore` and `InsertResolverAfter`.
