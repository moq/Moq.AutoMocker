namespace Moq.AutoMock.Tests
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
