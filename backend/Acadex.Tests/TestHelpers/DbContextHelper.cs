using Acadex.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace Acadex.Tests.TestHelpers;

public static class DbContextHelper
{
    public static AcadexDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AcadexDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AcadexDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
