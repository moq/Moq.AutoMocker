# AutoMocker.CreateSelfMock&lt;T&gt; method (1 of 2)

Constructs a self-mock from the services available in the container. A self-mock is a concrete object that has virtual and abstract members mocked. The purpose is so that you can test the majority of a class but mock out a resource. This is great for testing abstract classes, or avoiding breaking cohesion even further with a non-abstract class.

```csharp
public T CreateSelfMock<T>()
    where T : class
```

| parameter | description |
| --- | --- |
| T | The instance that you want to build |

## Return Value

An instance with virtual and abstract members mocked

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.CreateSelfMock&lt;T&gt; method (2 of 2)

Constructs a self-mock from the services available in the container. A self-mock is a concrete object that has virtual and abstract members mocked. The purpose is so that you can test the majority of a class but mock out a resource. This is great for testing abstract classes, or avoiding breaking cohesion even further with a non-abstract class.

```csharp
public T CreateSelfMock<T>(bool enablePrivate)
    where T : class
```

| parameter | description |
| --- | --- |
| T | The instance that you want to build |
| enablePrivate | When true, non-public constructors will also be used to create mocks. |

## Return Value

An instance with virtual and abstract members mocked

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

<!-- DO NOT EDIT: generated by xmldocmd for Moq.AutoMock.dll -->
