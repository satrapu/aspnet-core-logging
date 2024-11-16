using System.Threading.Tasks;

using NUnit.Framework;

using VerifyNUnit;

namespace Todo.WebApi
{
    [TestFixture]
    public class VerifyChecksTests
    {
        [Test]
        public Task Run() => VerifyChecks.Run();
    }
}
