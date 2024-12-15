namespace Todo.WebApi
{
    using System.Threading.Tasks;

    using NUnit.Framework;

    using VerifyNUnit;

    [TestFixture]
    public class VerifyChecksTests
    {
        [Test]
        public Task Run() => VerifyChecks.Run();
    }
}
