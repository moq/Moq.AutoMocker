using System;
using System.Linq.Expressions;

namespace Moq.AutoMock
{
    public partial class AutoMocker
    {
        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        public void Verify<T>(Expression<Action<T>> expression)
            where T : class
        {
            var mock = GetMock<T>();
            mock.Verify(expression);
        }


        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        public void Verify<T>(Expression<Action<T>> expression, Times times)
            where T : class
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        public void Verify<T>(Expression<Action<T>> expression, Func<Times> times)
            where T : class
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        public void Verify<T>(Expression<Action<T>> expression, string failMessage)
            where T : class
        {
            var mock = GetMock<T>();
            mock.Verify(expression, failMessage);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        public void Verify<T>(Expression<Action<T>> expression, Times times, string failMessage)
            where T : class
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times, failMessage);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        public void Verify<T>(Expression<Action<T>> expression, Func<Times> times, string failMessage)
            where T : class
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times, failMessage);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        public void Verify<T>(Expression<Func<T, object>> expression)
            where T : class
        {
            var mock = GetMock<T>();

            if (CastChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException(Resources.Strings.VerifyWithValueReturn);

            mock.Verify(expression);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        public void Verify<T>(Expression<Func<T, object>> expression, Times times)
            where T : class
        {
            var mock = GetMock<T>();

            if (CastChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException(Resources.Strings.VerifyWithValueReturn);

            mock.Verify(expression, times);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        public void Verify<T>(Expression<Func<T, object>> expression, Func<Times> times)
            where T : class
        {
            var mock = GetMock<T>();

            if (CastChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException(Resources.Strings.VerifyWithValueReturn);

            mock.Verify(expression, times);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        public void Verify<T>(Expression<Func<T, object>> expression, string failMessage)
            where T : class
        {
            var mock = GetMock<T>();

            if (CastChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException(Resources.Strings.VerifyWithValueReturn);

            mock.Verify(expression, failMessage);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <param name="expression">Expression to verify</param>
        /// <param name="times">The number of times a method is allowed to be called.</param>
        /// <param name="failMessage">Message to show if verification fails.</param>
        public void Verify<T>(Expression<Func<T, object>> expression, Times times, string failMessage)
            where T : class
        {
            var mock = GetMock<T>();

            if (CastChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException(Resources.Strings.VerifyWithValueReturn);

            mock.Verify(expression, times, failMessage);
        }

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed
        /// on the mock. Use in conjunction with the default Moq.MockBehavior.Loose.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        public void Verify<T>()
            where T : class
        {
            var mock = GetMock<T>();
            mock.Verify();
        }

    }
}

