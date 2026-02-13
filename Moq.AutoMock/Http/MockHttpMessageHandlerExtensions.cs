using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Moq.Language;
using Moq.Language.Flow;
using Moq.Protected;

namespace Moq.AutoMock.Http;

using System.Net.Http;

[EditorBrowsable(EditorBrowsableState.Never)]
public static partial class MockHttpMessageHandlerExtensions
{
    /// <summary>
    /// Creates a new <see cref="HttpClient" /> backed by this handler.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    public static HttpClient CreateClient(this Mock<HttpMessageHandler> handler)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        return new HttpClient(handler.Object, false);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="requestUri">The requested Uri</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpGet(this AutoMocker mocker, string? requestUri = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpGet(requestUri);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpGet(this AutoMocker mocker, Expression<Func<HttpRequestMessage, bool>> match)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpGet(match);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">The requested Uri</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpGet(this Mock<HttpMessageHandler> handler, string? requestUri = null)
    {
        return handler.SetupHttpGet(r => MatchesRequestUri(r.RequestUri, requestUri));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpGet(this Mock<HttpMessageHandler> handler, Expression<Func<HttpRequestMessage, bool>> match)
    {
        return SetupHttp(handler, x => x.SendAsync(
                It.Is(CombineHttpMethod(match, HttpMethod.Get)),
                It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="requestUri">The requested Uri</param>
    /// <param name="content">Optional request content content to match.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpPost(this AutoMocker mocker, 
        string? requestUri = null, string? content = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpPost(requestUri, content);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpPost(this AutoMocker mocker, Expression<Func<HttpRequestMessage, bool>> match)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpPost(match);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">The requested Uri</param>
    /// <param name="content">Optional request content content to match.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpPost(this Mock<HttpMessageHandler> handler, 
        string? requestUri = null, string? content = null)
    {
        return handler.SetupHttpPost(r => MatchesRequestUri(r.RequestUri, requestUri) && ContentEquals(r.Content, content));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpPost(this Mock<HttpMessageHandler> handler, Expression<Func<HttpRequestMessage, bool>> match)
    {
        return SetupHttp(handler, x => x.SendAsync(
                It.Is(CombineHttpMethod(match, HttpMethod.Post)),
                It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="requestUri">The requested Uri</param>
    /// <param name="content">Optional request content content to match.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpPut(this AutoMocker mocker, 
        string? requestUri = null, string? content = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpPut(requestUri, content);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpPut(this AutoMocker mocker, Expression<Func<HttpRequestMessage, bool>> match)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpPut(match);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">The requested Uri</param>
    /// <param name="content">Optional request content content to match.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpPut(this Mock<HttpMessageHandler> handler, 
        string? requestUri = null, string? content = null)
    {
        return handler.SetupHttpPut(r => MatchesRequestUri(r.RequestUri, requestUri) && ContentEquals(r.Content, content));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpPut(this Mock<HttpMessageHandler> handler, Expression<Func<HttpRequestMessage, bool>> match)
    {
        return SetupHttp(handler, x => x.SendAsync(
                It.Is(CombineHttpMethod(match, HttpMethod.Put)),
                It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="requestUri">The requested Uri</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpDelete(this AutoMocker mocker, string? requestUri = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpDelete(requestUri);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpDelete(this AutoMocker mocker, Expression<Func<HttpRequestMessage, bool>> match)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpDelete(match);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">The requested Uri</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpDelete(this Mock<HttpMessageHandler> handler, string? requestUri = null)
    {
        return SetupHttp(handler, x => x.SendAsync(
                It.Is<HttpRequestMessage>(
                    r => r.Method == HttpMethod.Delete && 
                         MatchesRequestUri(r.RequestUri, requestUri)),
                It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpDelete(this Mock<HttpMessageHandler> handler, Expression<Func<HttpRequestMessage, bool>> match)
    {
        return SetupHttp(handler, x => x.SendAsync(
                It.Is(CombineHttpMethod(match, HttpMethod.Delete)),
                It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="requestUri">The requested Uri</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpHead(this AutoMocker mocker, string? requestUri = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpHead(requestUri);
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpHead(this AutoMocker mocker, Expression<Func<HttpRequestMessage, bool>> match)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupHttpHead(match);
    }


    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">The requested Uri</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpHead(this Mock<HttpMessageHandler> handler, string? requestUri = null)
    {
        return SetupHttp(handler, x => x.SendAsync(
                It.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Head && MatchesRequestUri(r.RequestUri, requestUri)),
                It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupHttpHead(this Mock<HttpMessageHandler> handler, Expression<Func<HttpRequestMessage, bool>> match)
    {
        return SetupHttp(handler, x => x.SendAsync(
                It.Is(CombineHttpMethod(match, HttpMethod.Head)),
                It.IsAny<CancellationToken>()));
    }

    /// <summary>
    /// Specifies a setup on the mocked type for a call to a value-returning method.
    /// </summary>
    /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
    public static ISetup<HttpMessageHandler, TResult> SetupHttp<TResult>(this Mock<HttpMessageHandler> handler, Expression<Func<IHttpMessageHandler, TResult>> expression)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        return handler.Protected().As<IHttpMessageHandler>().Setup(expression);
    }

    /// <summary>
    /// Return a sequence of values, one per call.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Moq has two types of sequences:
    /// </para>
    /// <para>
    ///   1. SetupSequence() which creates one setup that returns values in sequence, and
    ///   2. InSequence().Setup() which creates multiple setups under When() conditions
    ///      to ensure that they only match in order
    /// </para>
    /// <para>
    ///   This is the former; the latter is <see cref="Setup(ISetupConditionResult{HttpMessageHandler}, Expression{Func{IHttpMessageHandler, Task{HttpResponseMessage}}})" />.
    /// </para>
    /// </remarks>
    /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
    public static ISetupSequentialResult<TResult> SetupSequence<TResult>(this Mock<HttpMessageHandler> handler, Expression<Func<IHttpMessageHandler, TResult>> expression)
    {
        if (handler == null)
            throw new ArgumentNullException(nameof(handler));

        return handler.Protected().As<IHttpMessageHandler>().SetupSequence(expression);
    }

    /// <summary>
    /// Return a sequence of values, one per call.
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Moq has two types of sequences:
    /// </para>
    /// <para>
    ///   1. SetupSequence() which creates one setup that returns values in sequence, and
    ///   2. InSequence().Setup() which creates multiple setups under When() conditions
    ///      to ensure that they only match in order
    /// </para>
    /// <para>
    ///   This is the former; the latter is <see cref="Setup(ISetupConditionResult{HttpMessageHandler}, Expression{Func{IHttpMessageHandler, Task{HttpResponseMessage}}})" />.
    /// </para>
    /// </remarks>
    /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
    public static ISetupSequentialResult<TResult> SetupHttpSequence<TResult>(this AutoMocker mocker, 
        Expression<Func<IHttpMessageHandler, TResult>> expression)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));
        return mocker.GetMock<HttpMessageHandler>().SetupSequence(expression);
    }

    /// <summary>
    /// Specifies a conditional setup for <see cref="IHttpMessageHandler.SendAsync(HttpRequestMessage, CancellationToken)" />
    /// by modifying the expression tree similar to <see cref="ProtectedAsMock{T, TAnalog}" />, as Moq does not currently
    /// support When() conditions or InSequence() in conjunction with Protected().
    /// </summary>
    /// <remarks>
    /// <para>
    ///   Moq has two types of sequences:
    /// </para>
    /// <para>
    ///   1. SetupSequence() which creates one setup that returns values in sequence, and
    ///   2. InSequence().Setup() which creates multiple setups under When() conditions
    ///      to ensure that they only match in order
    /// </para>
    /// <para>
    ///   This is the latter; the former is <see cref="SetupSequence{TResult}(Mock{HttpMessageHandler}, Expression{Func{IHttpMessageHandler, TResult}})" />.
    /// </para>
    /// </remarks>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock in the context of a When() or InSequence() condition.</param>
    /// <param name="expression">A lambda expression in the form of <c>x => x.SendAsync(...)</c>.</param>
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> Setup(this ISetupConditionResult<HttpMessageHandler> handler, Expression<Func<IHttpMessageHandler, Task<HttpResponseMessage>>> expression)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        if (expression is null)
            throw new ArgumentNullException(nameof(expression));

        // Expression should be a method call
        if (expression.Body is not MethodCallExpression methodCall)
            throw new ArgumentException("Expression is not a method call.", nameof(expression));

        // The method should be called on the interface parameter
        if (!(methodCall.Object is ParameterExpression left && left.Type == typeof(IHttpMessageHandler)))
            throw new ArgumentException("Object of method call is not the parameter.", nameof(expression));

        // The called method should be SendAsync
        if (methodCall.Method.Name != "SendAsync")
            throw new ArgumentException("Expression is not a SendAsync() method call.", nameof(expression));

        // Use reflection to get the protected method
        MethodInfo targetMethod = typeof(HttpMessageHandler).GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        // Replace the method call in the expression
        ParameterExpression targetParameter = Expression.Parameter(typeof(HttpMessageHandler), left.Name);
        MethodCallExpression targetMethodCall = Expression.Call(targetParameter, targetMethod, methodCall.Arguments);

        // Create a new lambda with this method call
        var rewrittenExpression = (Expression<Func<HttpMessageHandler, Task<HttpResponseMessage>>>) Expression.Lambda(targetMethodCall, targetParameter);

        // Use this lambda with the stock Setup() method
        return handler.Setup(rewrittenExpression);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="expression">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void Verify<TResult>(this Mock<HttpMessageHandler> handler, Expression<Func<IHttpMessageHandler, TResult>> expression, Times? times = null, string? failMessage = null)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        handler.Protected().As<IHttpMessageHandler>().Verify(expression, times, failMessage!);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpGet(this AutoMocker mocker, string requestUri, Times? times = null, string? failMessage = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));

        mocker.GetMock<HttpMessageHandler>()
            .VerifyHttpGet(requestUri, times, failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpGet(this Mock<HttpMessageHandler> handler, string requestUri, Times? times = null, string? failMessage = null)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        handler.VerifyHttp(r => r.Method == HttpMethod.Get && 
                                MatchesRequestUri(r.RequestUri, requestUri),
                                times, failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="content">The expected content of the request.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpPost(this AutoMocker mocker, string requestUri, string? content = null, Times? times = null, string? failMessage = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));

        mocker.GetMock<HttpMessageHandler>()
            .VerifyHttpPost(requestUri, content, times, failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="content">The expected content of the request.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpPost(this Mock<HttpMessageHandler> handler, string requestUri, string? content = null, Times? times = null, string? failMessage = null)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        handler.VerifyHttp(r => r.Method == HttpMethod.Post &&
                           MatchesRequestUri(r.RequestUri, requestUri) &&
                           ContentEquals(r.Content, content), 
                           times, failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="content">The expected content of the request.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpPut(this AutoMocker mocker, string requestUri, string? content = null, Times? times = null, string? failMessage = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));

        mocker.GetMock<HttpMessageHandler>()
            .VerifyHttpPut(requestUri, content, times, failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="content">The expected content of the request.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpPut(this Mock<HttpMessageHandler> handler, string requestUri, string? content = null, Times? times = null, string? failMessage = null)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        handler.VerifyHttp(r => r.Method == HttpMethod.Put &&
                           MatchesRequestUri(r.RequestUri, requestUri) &&
                           ContentEquals(r.Content, content),
                           times, failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpDelete(this AutoMocker mocker, string requestUri, Times? times = null, string? failMessage = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));

        mocker.GetMock<HttpMessageHandler>()
            .VerifyHttpDelete(requestUri, times, failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpDelete(this Mock<HttpMessageHandler> handler, string requestUri, Times? times = null, string? failMessage = null)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        handler.VerifyHttp(r => r.Method == HttpMethod.Delete &&
                                MatchesRequestUri(r.RequestUri, requestUri),
                                times, failMessage);
    }


    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="mocker">The <see cref="AutoMocker" /> instance.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpHead(this AutoMocker mocker, string requestUri, Times? times = null, string? failMessage = null)
    {
        if (mocker is null)
            throw new ArgumentNullException(nameof(mocker));

        mocker.GetMock<HttpMessageHandler>()
            .VerifyHttpHead(requestUri, times, failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="requestUri">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttpHead(this Mock<HttpMessageHandler> handler, string requestUri, Times? times = null, string? failMessage = null)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        handler.VerifyHttp(r => r.Method == HttpMethod.Head &&
                                MatchesRequestUri(r.RequestUri, requestUri),
                                times, failMessage);
    }

    /// <summary>
    /// Verifies that a specifi>c invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="match">The predicate used to match the <see cref="HttpRequestMessage"/>.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttp(this Mock<HttpMessageHandler> handler, Expression<Func<HttpRequestMessage, bool>> match, Times? times = null, string? failMessage = null)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        handler.VerifyHttp(x => x.SendAsync(
            It.Is(match),
            It.IsAny<CancellationToken>()),
            times,
            failMessage);
    }

    /// <summary>
    /// Verifies that a specific invocation matching the given expression was performed on the mock.
    /// Use in conjunction with the default <see cref="MockBehavior.Loose" />.
    /// </summary>
    /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
    /// <param name="handler">The <see cref="HttpMessageHandler" /> mock.</param>
    /// <param name="expression">Lambda expression that specifies the method invocation.</param>
    /// <param name="times">
    /// Number of times that the invocation is expected to have occurred.
    /// If omitted, assumed to be <see cref="Times.AtLeastOnce" />.
    /// </param>
    /// <param name="failMessage">Message to include in the thrown <see cref="MockException" /> if verification fails.</param>
    /// <exception cref="MockException">The specified invocation did not occur (or did not occur the specified number of times).</exception>
    public static void VerifyHttp<TResult>(this Mock<HttpMessageHandler> handler, Expression<Func<IHttpMessageHandler, TResult>> expression, Times? times = null, string? failMessage = null)
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        handler.Protected().As<IHttpMessageHandler>().Verify(expression, times, failMessage!);
    }

    private static bool ContentEquals(HttpContent? actual, string? expected)
    {
        //NB: In this case no expectation was provided so match everything
        if (expected is null) return true;
        if (actual is null) return false;

        string actualContent = actual.ReadAsStringAsync().Result;
        return string.Equals(actualContent, expected, StringComparison.Ordinal);
    }

    private static bool MatchesRequestUri(Uri requestUri, string? expected)
    {
        //NB: In this case no expectation was provided so match everything
        if (expected is null) return true;

        return requestUri.ToString().Contains(expected);
    }

    private static Expression<Func<HttpRequestMessage, bool>> CombineHttpMethod(Expression<Func<HttpRequestMessage, bool>> expression, HttpMethod desiredMethod)
    {
        // Expression to check if the method matches the desired HTTP method
        Expression<Func<HttpRequestMessage, bool>> methodCheck = r => r.Method == desiredMethod;
        // Combine the original expression with the method check using a logical AND
        var parameter = Expression.Parameter(typeof(HttpRequestMessage), "request");
        var combinedBody = Expression.AndAlso(
            Expression.Invoke(expression, parameter),
            Expression.Invoke(methodCheck, parameter)
        );
        return Expression.Lambda<Func<HttpRequestMessage, bool>>(combinedBody, parameter);
    }
}
