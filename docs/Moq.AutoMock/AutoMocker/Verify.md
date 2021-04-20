# AutoMocker.Verify method (1 of 18)

This is a shortcut for calling `mock.Verify()` on every mock that we have.

```csharp
public void Verify()
```

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (2 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>()
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (3 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Action<T>> expression)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (4 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Func<T, object>> expression)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (5 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Action<T>> expression, Func<Times> times)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| times | The number of times a method is allowed to be called. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (6 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Action<T>> expression, string failMessage)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| failMessage | Message to show if verification fails. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (7 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Action<T>> expression, Times times)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| times | The number of times a method is allowed to be called. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (8 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Func<T, object>> expression, Func<Times> times)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| times | The number of times a method is allowed to be called. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (9 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Func<T, object>> expression, string failMessage)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| failMessage | Message to show if verification fails. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (10 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Func<T, object>> expression, Times times)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| times | The number of times a method is allowed to be called. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (11 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Action<T>> expression, Func<Times> times, string failMessage)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| times | The number of times a method is allowed to be called. |
| failMessage | Message to show if verification fails. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (12 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Action<T>> expression, Times times, string failMessage)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| times | The number of times a method is allowed to be called. |
| failMessage | Message to show if verification fails. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T&gt; method (13 of 18)

Verifies that a specific invocation matching the given expression was performed on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.

```csharp
public void Verify<T>(Expression<Func<T, object>> expression, Times times, string failMessage)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| expression | Expression to verify |
| times | The number of times a method is allowed to be called. |
| failMessage | Message to show if verification fails. |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T,TResult&gt; method (14 of 18)

Verify a mock in the container.

```csharp
public void Verify<T, TResult>(Expression<Func<T, TResult>> expression)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| TResult | Return type of the full expression |
| expression |  |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T,TResult&gt; method (15 of 18)

Verify a mock in the container.

```csharp
public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Func<Times> times)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| TResult | Return type of the full expression |
| expression |  |
| times |  |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T,TResult&gt; method (16 of 18)

Verify a mock in the container.

```csharp
public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, string failMessage)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| TResult | Return type of the full expression |
| expression |  |
| failMessage |  |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T,TResult&gt; method (17 of 18)

Verify a mock in the container.

```csharp
public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| TResult | Return type of the full expression |
| expression |  |
| times |  |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

---

# AutoMocker.Verify&lt;T,TResult&gt; method (18 of 18)

Verify a mock in the container.

```csharp
public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times, 
    string failMessage)
    where T : class
```

| parameter | description |
| --- | --- |
| T | Type of the mock |
| TResult | Return type of the full expression |
| expression |  |
| times |  |
| failMessage |  |

## See Also

* class [AutoMocker](../AutoMocker.md)
* namespace [Moq.AutoMock](../../Moq.AutoMock.md)

<!-- DO NOT EDIT: generated by xmldocmd for Moq.AutoMock.dll -->
