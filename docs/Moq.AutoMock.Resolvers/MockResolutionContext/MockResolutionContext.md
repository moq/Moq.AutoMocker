# MockResolutionContext constructor

Initializes an instance of MockResolutionContext.

```csharp
public MockResolutionContext(AutoMocker autoMocker, Type requestType, object? initialValue, 
    ObjectGraphContext objectGraphContext)
```

| parameter | description |
| --- | --- |
| autoMocker | The `AutoMocker` instance. |
| requestType | The requested type to resolve. |
| initialValue | The initial value to use. |
| objectGraphContext | Context within the object graph being created. This differs from the MockResolutionContext which is only relevant for a single object creation. |

## See Also

* class [AutoMocker](../../Moq.AutoMock/AutoMocker.md)
* class [ObjectGraphContext](../../Moq.AutoMock/ObjectGraphContext.md)
* class [MockResolutionContext](../MockResolutionContext.md)
* namespace [Moq.AutoMock.Resolvers](../../Moq.AutoMock.md)

<!-- DO NOT EDIT: generated by xmldocmd for Moq.AutoMock.dll -->
