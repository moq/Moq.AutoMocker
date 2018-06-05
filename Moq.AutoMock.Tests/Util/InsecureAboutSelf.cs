namespace Moq.AutoMock.Tests
{
    public class InsecureAboutSelf
    {
        public bool SelfDepricated { get; set; }

        public void TellJoke()
        {

        }

        protected virtual void SelfDepricate()
        {
            SelfDepricated = true;
        }
    }
}
