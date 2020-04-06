namespace Moq.AutoMock.Tests.Util
{
    public class WithSealedParams
    {
        public string Sealed { get; set; }

        public WithSealedParams(string @sealed)
        {
            Sealed = @sealed;
        }
    }
}
