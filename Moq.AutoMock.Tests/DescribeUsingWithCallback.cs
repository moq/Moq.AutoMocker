namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeUsingWithCallback
{
    [TestMethod]
    public void You_can_register_a_callback_to_configure_a_mock()
    {
        AutoMocker mocker = new();
        mocker.Use<IService2>(mocker =>
        {
            return new Service2();
        });
        
        var instance = mocker.Get<IService2>();
        Assert.IsInstanceOfType(instance, typeof(Service2));
    }

    [TestMethod]
    public void Service_callback_is_not_invoked_until_the_service_is_requested()
    {
        AutoMocker mocker = new();
        bool callbackInvoked = false;
        mocker.Use<IService2>(mocker =>
        {
            callbackInvoked = true;
            return new Service2();
        });

        
        bool beforeRequest = callbackInvoked;
        _ = mocker.CreateInstance<WithService>();

        Assert.IsFalse(beforeRequest, "Callback should not have been invoked yet.");
        Assert.IsTrue(callbackInvoked, "Callback should have been invoked when the service was requested.");
    }

    [TestMethod]
    public void Service_created_from_a_callback_is_cached()
    {
        AutoMocker mocker = new();
        int callbackCount = 0;
        mocker.Use<IService2>(() =>
        {
            callbackCount++;
            return new Service2();
        });

        _ = mocker.CreateInstance<WithService>();
        _ = mocker.CreateInstance<WithService>();
        
        Assert.AreEqual(1, callbackCount);
    }
}
