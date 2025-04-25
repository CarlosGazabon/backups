using Inventio.Models;
using System;
using System.Linq;

namespace Inventio.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDBContext context)
        {
            context.Database.EnsureCreated();

        }
    }
}