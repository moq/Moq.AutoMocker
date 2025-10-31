using System.Linq.Expressions;
using Moq.AutoMock.Resolvers;

namespace Moq.AutoMock.Tests;

[TestClass]
public class DescribeVerify
{
    [TestMethod]
    public void It_throws_if_cache_is_not_registered()
    {
        AutoMocker mocker = new();
        mocker.Resolvers.Remove(mocker.Resolvers.OfType<CacheResolver>().Single());

        Assert.Throws<InvalidOperationException>(() => mocker.Verify());
    }

    [TestMethod]
    public void It_throws_if_func_expression_is_null()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<object, object>(null!));
    }

    [TestMethod]
    public void It_verifies_mock_with_func_expression()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7, object>(x => x.ReturnValue("foo"));
    }

    [TestMethod]
    public void It_throws_if_func_expression_with_times_is_null()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7, object>(null!, Times.Never()));
    }

    [TestMethod]
    public void It_verifies_mock_with_func_expression_and_times()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), Times.Once());
    }

    [TestMethod]
    public void It_throws_if_func_expression_with_times_func_has_null_func_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7, object>(null!, Times.Never));
    }

    [TestMethod]
    public void It_throws_if_func_expression_with_times_func_has_null_times_func()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), (Func<Times>)null!));
    }

    [TestMethod]
    public void It_verifies_mock_with_func_expression_and_times_func()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), Times.Once);
    }

    [TestMethod]
    public void It_throws_if_func_expression_with_fail_message_has_null_func_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7, object>(null!, "fail"));
    }

    [TestMethod]
    public void It_verifies_mock_with_func_expression_and_fail_message()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), "fail");
    }

    [TestMethod]
    public void It_throws_if_func_expression_with_times_and_fail_message_has_null_func_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7, object>(null!, Times.Never(), "fail"));
    }

    [TestMethod]
    public void It_throws_if_func_expression_with_times_and_fail_message_has_null_fail_message()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), Times.Never(), null!));
    }

    [TestMethod]
    public void It_verifies_mock_with_func_expression_and_times_and_fail_message()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7, object>(x => x.ReturnValue("foo"), Times.Once(), "fail");
    }

    [TestMethod]
    public void It_throws_with_null_action_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Action<IService7>>)null!));
    }

    [TestMethod]
    public void It_verifies_with_action_expression()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.Void("foo");

        mocker.Verify<IService7>(x => x.Void("foo"));
    }

    [TestMethod]
    public void It_throws_with_action_expression_and_times_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Action<IService7>>)null!, Times.Never()));
    }

    [TestMethod]
    public void It_verifies_with_action_expression_and_times()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.Void("foo");

        mocker.Verify<IService7>(x => x.Void("foo"), Times.Once());
    }

    [TestMethod]
    public void It_throws_with_action_expression_and_times_func_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Action<IService7>>)null!, Times.Never));
    }

    [TestMethod]
    public void It_throws_with_action_expression_and_times_func_with_null_times_func()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>(x => x.Void("foo"), (Func<Times>)null!));
    }

    [TestMethod]
    public void It_verifies_with_action_expression_and_times_func()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.Void("foo");

        mocker.Verify<IService7>(x => x.Void("foo"), Times.Once);
    }

    [TestMethod]
    public void It_throws_with_action_expression_and_failure_message_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Action<IService7>>)null!, "fail"));
    }

    [TestMethod]
    public void It_verifies_with_action_expression_and_failure_message()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.Void("foo");

        mocker.Verify<IService7>(x => x.Void("foo"), "fail");
    }

    [TestMethod]
    public void It_throws_with_action_expression_and_times_and_failure_message_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Action<IService7>>)null!, Times.Never(), "fail"));
    }

    [TestMethod]
    public void It_verifies_with_action_expression_and_times_and_failure_message()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.Void("foo");

        mocker.Verify<IService7>(x => x.Void("foo"), Times.Once(), "fail");
    }

    [TestMethod]
    public void It_throws_with_action_expression_and_times_func_and_failure_message_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Action<IService7>>)null!, Times.Never, "fail"));
    }

    [TestMethod]
    public void It_throws_with_action_expression_and_times_func_and_failure_message_with_null_times_func()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>(x => x.Void("foo"), null!, "fail"));
    }

    [TestMethod]
    public void It_verifies_with_action_expression_and_times_func_and_failure_message()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.Void("foo");

        mocker.Verify<IService7>(x => x.Void("foo"), Times.Once, "fail");
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Func<IService7, object>>)null!));
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_with_primitive_return_type()
    {
        AutoMocker mocker = new();

        Assert.Throws<NotSupportedException>(() => mocker.Verify<IService7>(x => (int)x.ReturnValue("foo")));
    }

    [TestMethod]
    public void It_verifies_with_func_object_expression()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7>(x => x.ReturnValue("foo"));
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_times_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Func<IService7, object>>)null!, Times.Never()));
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_times_with_primitive_return_type()
    {
        AutoMocker mocker = new();

        Assert.Throws<NotSupportedException>(() => mocker.Verify<IService7>(x => (int)x.ReturnValue("foo"), Times.Never()));
    }

    [TestMethod]
    public void It_verifies_with_func_object_expression_and_times()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7>(x => x.ReturnValue("foo"), Times.Once());
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_times_func_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Func<IService7, object>>)null!, Times.Never));
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_times_func_with_null_times_func()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>(x => x.ReturnValue("foo"), (Func<Times>)null!));
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_times_func_with_primitive_return_type()
    {
        AutoMocker mocker = new();

        Assert.Throws<NotSupportedException>(() => mocker.Verify<IService7>(x => (int)x.ReturnValue("foo"), Times.Never));
    }

    [TestMethod]
    public void It_verifies_with_func_object_expression_and_times_func()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7>(x => x.ReturnValue("foo"), Times.Once);
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_failure_message_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Func<IService7, object>>)null!, "fail"));
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_failure_message_with_primitive_return_type()
    {
        AutoMocker mocker = new();

        Assert.Throws<NotSupportedException>(() => mocker.Verify<IService7>(x => (int)x.ReturnValue("foo"), "fail"));
    }

    [TestMethod]
    public void It_verifies_with_func_object_expression_and_failure_message()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7>(x => x.ReturnValue("foo"), "fail");
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_times_and_failure_message_with_null_expression()
    {
        AutoMocker mocker = new();

        Assert.Throws<ArgumentNullException>(() => mocker.Verify<IService7>((Expression<Func<IService7, object>>)null!, Times.Never(), "fail"));
    }

    [TestMethod]
    public void It_throws_with_func_object_expression_and_times_and_failure_message_with_primitive_return_type()
    {
        AutoMocker mocker = new();

        Assert.Throws<NotSupportedException>(() => mocker.Verify<IService7>(x => (int)x.ReturnValue("foo"), Times.Never(), "fail"));
    }

    [TestMethod]
    public void It_verifies_with_func_object_expression_and_times_failure_message()
    {
        AutoMocker mocker = new();
        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7>(x => x.ReturnValue("foo"), Times.Once(), "fail");
    }

    [TestMethod]
    public void It_verifies_mock_by_type()
    {
        AutoMocker mocker = new();
        mocker.Setup<IService7>(x => x.ReturnValue("foo"));

        var mock = mocker.GetMock<IService7>();
        mock.Object.ReturnValue("foo");

        mocker.Verify<IService7>();
    }
}
