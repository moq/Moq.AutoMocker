using System;
using System.Linq.Expressions;
using Moq;
using Moq.Language.Flow;

namespace Moq.AutoMock
{
	public partial class AutoMocker
	{
		
		public void Verify<T>(Expression<Action<T>> expression)
			where T : class
		{
			var mock = GetMock<T>();
			
						
			mock.Verify(expression);
		}

		
		public void Verify<T>(Expression<Action<T>> expression, Times times)
			where T : class
		{
			var mock = GetMock<T>();
			
						
			mock.Verify(expression, times);
		}

		
		public void Verify<T>(Expression<Action<T>> expression, Func<Times> times)
			where T : class
		{
			var mock = GetMock<T>();
			
						
			mock.Verify(expression, times);
		}

		
		public void Verify<T>(Expression<Action<T>> expression, String failMessage)
			where T : class
		{
			var mock = GetMock<T>();
			
						
			mock.Verify(expression, failMessage);
		}

		
		public void Verify<T>(Expression<Action<T>> expression, Times times, String failMessage)
			where T : class
		{
			var mock = GetMock<T>();
			
						
			mock.Verify(expression, times, failMessage);
		}

		
		public void Verify<T>(Expression<Action<T>> expression, Func<Times> times, String failMessage)
			where T : class
		{
			var mock = GetMock<T>();
			
						
			mock.Verify(expression, times, failMessage);
		}

		
		public void Verify<T>(Expression<Func<T, object>> expression)
			where T : class
		{
			var mock = GetMock<T>();

            if (castChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException("Use the Verify overload that allows specifying TReturn if the setup returns a value type");

			mock.Verify(expression);
		}

		
		public void Verify<T>(Expression<Func<T, object>> expression, Times times)
			where T : class
		{
			var mock = GetMock<T>();

            if (castChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException("Use the Verify overload that allows specifying TReturn if the setup returns a value type");

			mock.Verify(expression, times);
		}

		
		public void Verify<T>(Expression<Func<T, object>> expression, Func<Times> times)
			where T : class
		{
			var mock = GetMock<T>();

            if (castChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException("Use the Verify overload that allows specifying TReturn if the setup returns a value type");

			mock.Verify(expression, times);
		}

		
		public void Verify<T>(Expression<Func<T, object>> expression, String failMessage)
			where T : class
		{
			var mock = GetMock<T>();

            if (castChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException("Use the Verify overload that allows specifying TReturn if the setup returns a value type");

			mock.Verify(expression, failMessage);
		}

		
		public void Verify<T>(Expression<Func<T, object>> expression, Times times, String failMessage)
			where T : class
		{
			var mock = GetMock<T>();

            if (castChecker.DoesReturnPrimitive(expression))
                throw new NotSupportedException("Use the Verify overload that allows specifying TReturn if the setup returns a value type");

			mock.Verify(expression, times, failMessage);
		}

		
		public void Verify<T>()
			where T : class
		{
			var mock = GetMock<T>();
			
						
			mock.Verify();
		}

			}
}

