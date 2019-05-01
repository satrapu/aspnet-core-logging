using System;

namespace Todo.WebApi.IntegrationTests.Infrastructure
{
    public static class TestUserSettings
    {
        public static readonly string UserId = Guid.NewGuid().ToString("N");
        public static readonly string Name = "satrapu";
        public static readonly string Email = "satrapu@server.com";
    }
}
