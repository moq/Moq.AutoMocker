# AutoMocker class

An auto-mocking IoC container that generates mock objects using Moq.

```csharp
public class AutoMocker
```

## Public Members

| name | description |
| --- | --- |
| [AutoMocker](AutoMocker/AutoMocker.md)() | Initializes an instance of AutoMockers. |
| [AutoMocker](AutoMocker/AutoMocker.md)(…) | Initializes an instance of AutoMockers. (3 constructors) |
| [CallBase](AutoMocker/CallBase.md) { get; } | Whether the base member virtual implementation will be called for created mocks if no setup is matched. Defaults to `false`. |
| [DefaultValue](AutoMocker/DefaultValue.md) { get; } | Specifies the behavior to use when returning default values for unexpected invocations on loose mocks created by this instance. |
| [MockBehavior](AutoMocker/MockBehavior.md) { get; } | Behavior of created mocks, according to the value set in the constructor. |
| [ResolvedObjects](AutoMocker/ResolvedObjects.md) { get; } | A collection of objects stored in this AutoMocker instance. The keys are the types used when resolving services. |
| [Resolvers](AutoMocker/Resolvers.md) { get; } | A collection of resolves determining how a given dependency will be resolved. |
| [Combine](AutoMocker/Combine.md)(…) | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemented by the same instance. |
| [Combine&lt;TService,TAsWellAs1&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2,TAsWellAs3&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6,TAsWellAs7&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6,TAsWellAs7,TAsWellAs8&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6,TAsWellAs7,TAsWellAs8,TAsWellAs9&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [Combine&lt;TService,TAsWellAs1,TAsWellAs2,TAsWellAs3,TAsWellAs4,TAsWellAs5,TAsWellAs6,TAsWellAs7,TAsWellAs8,TAsWellAs9,TAsWellAs10&gt;](AutoMocker/Combine.md)() | Combines all given types so that they are mocked by the same mock. Some IoC containers call this "Forwarding" one type to other interfaces. In the end, this just means that all given types will be implemnted by the same instance. |
| [CreateInstance](AutoMocker/CreateInstance.md)(…) | Constructs an instance from known services. Any dependencies (constructor arguments) are fulfilled by searching the container or, if not found, automatically generating mocks. (2 methods) |
| [CreateInstance&lt;T&gt;](AutoMocker/CreateInstance.md)() | Constructs an instance from known services. Any dependencies (constructor arguments) are fulfilled by searching the container or, if not found, automatically generating mocks. |
| [CreateInstance&lt;T&gt;](AutoMocker/CreateInstance.md)(…) | Constructs an instance from known services. Any dependencies (constructor arguments) are fulfilled by searching the container or, if not found, automatically generating mocks. |
| [CreateSelfMock&lt;T&gt;](AutoMocker/CreateSelfMock.md)() | Constructs a self-mock from the services available in the container. A self-mock is a concrete object that has virtual and abstract members mocked. The purpose is so that you can test the majority of a class but mock out a resource. This is great for testing abstract classes, or avoiding breaking cohesion even further with a non-abstract class. |
| [CreateSelfMock&lt;T&gt;](AutoMocker/CreateSelfMock.md)(…) | Constructs a self-mock from the services available in the container. A self-mock is a concrete object that has virtual and abstract members mocked. The purpose is so that you can test the majority of a class but mock out a resource. This is great for testing abstract classes, or avoiding breaking cohesion even further with a non-abstract class. |
| [Get](AutoMocker/Get.md)(…) | Searches and retrieves an object from the container that matches the serviceType. This can be a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`. (2 methods) |
| [Get&lt;TService&gt;](AutoMocker/Get.md)() | Searches and retrieves an object from the container that matches TService. This can be a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`. |
| [Get&lt;TService&gt;](AutoMocker/Get.md)(…) | Searches and retrieves an object from the container that matches TService. This can be a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`. |
| [GetMock](AutoMocker/GetMock.md)(…) | Searches and retrieves the mock that the container uses for serviceType. (2 methods) |
| [GetMock&lt;TService&gt;](AutoMocker/GetMock.md)() | Searches and retrieves the mock that the container uses for TService. |
| [GetMock&lt;TService&gt;](AutoMocker/GetMock.md)(…) | Searches and retrieves the mock that the container uses for TService. |
| [Setup&lt;TService&gt;](AutoMocker/Setup.md)(…) | Shortcut for mock.Setup(...), creating the mock when necessary. |
| [Setup&lt;TService,TReturn&gt;](AutoMocker/Setup.md)(…) | Shortcut for mock.Setup(...), creating the mock when necessary. For specific return types. E.g. primitive, structs that cannot be inferred |
| [SetupAllProperties&lt;TService&gt;](AutoMocker/SetupAllProperties.md)() | Shortcut for mock.SetupAllProperties(), creating the mock when necessary |
| [SetupSequence&lt;TService,TReturn&gt;](AutoMocker/SetupSequence.md)(…) | Shortcut for mock.SetupSequence(), creating the mock when necessary |
| [SetupWithAny&lt;TService&gt;](AutoMocker/SetupWithAny.md)(…) | Specifies a setup on the mocked type for a call to a void method. All parameters are filled with IsAny according to the parameter's type. |
| [SetupWithAny&lt;TService,TReturn&gt;](AutoMocker/SetupWithAny.md)(…) | Specifies a setup on the mocked type for a call to a non-void (value-returning) method. All parameters are filled with IsAny according to the parameter's type. |
| [Use](AutoMocker/Use.md)(…) | Adds an instance to the container. |
| [Use&lt;TService&gt;](AutoMocker/Use.md)(…) | Adds an instance to the container. (3 methods) |
| [Verify](AutoMocker/Verify.md)() | This is a shortcut for calling `mock.Verify()` on every mock that we have. |
| [Verify&lt;T&gt;](AutoMocker/Verify.md)() | Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose. |
| [Verify&lt;T&gt;](AutoMocker/Verify.md)(…) | Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose. (11 methods) |
| [Verify&lt;T,TResult&gt;](AutoMocker/Verify.md)(…) | Verify a mock in the container. (5 methods) |
| [VerifyAll](AutoMocker/VerifyAll.md)() | This is a shortcut for calling `mock.VerifyAll()` on every mock that we have. |

## See Also

* namespace [Moq.AutoMock](../Moq.AutoMock.md)

<!-- DO NOT EDIT: generated by xmldocmd for Moq.AutoMock.dll -->
