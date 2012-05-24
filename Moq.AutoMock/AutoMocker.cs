using System;

namespace Moq.AutoMock
{
    public class AutoMocker
    {
        public T GetInstance<T>()
            where T : class
        {
            return Activator.CreateInstance<T>();
        }
    }
}
