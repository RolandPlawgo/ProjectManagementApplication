using Microsoft.EntityFrameworkCore;
using ProjectManagementApplication.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagementApplication_UnitTests
{
    internal class DbContextHelper
    {
        public static ApplicationDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
            return new ApplicationDbContext(options);
        }
    }
}
