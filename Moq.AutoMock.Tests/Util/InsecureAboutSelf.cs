namespace Moq.AutoMock.Tests.Util
{
    public class InsecureAboutSelf
    {
        public bool SelfDepricated { get; set; }

#pragma warning disable CA1822      //Member does not access instance data and can be marked as static
        public void TellJoke()
        {

        }
#pragma warning restore CA1822

        protected virtual void SelfDepricate()
        {
            SelfDepricated = true;
        }
    }
}
