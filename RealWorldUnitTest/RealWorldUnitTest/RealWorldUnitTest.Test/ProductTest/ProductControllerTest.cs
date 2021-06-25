using Microsoft.EntityFrameworkCore;
using RealWorldUnitTest.Web.Models;

namespace RealWorldUnitTest.Test.ProductTest
{
    public class ProductControllerTest
    {
        protected DbContextOptions<UnitTestDbContext> _contextOptions { get; private set; }

        public void SetContextOptions(DbContextOptions<UnitTestDbContext> contextOptions)
        {
            _contextOptions = contextOptions;
            Seed();
        }

        public void Seed()
        {
            using (UnitTestDbContext context = new(_contextOptions))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                context.Categories.Add(new Category { Name = "Kalemler" });
                context.Categories.Add(new Category { Name = "Defterler" });

                context.SaveChanges();

                context.Products.Add(new Product { CategoryId = 1, Name = "Kalem 1", Price = 100, Stock = 20, Color = "Kırmızı" });
                context.Products.Add(new Product { CategoryId = 1, Name = "Kalem 2", Price = 100, Stock = 20, Color = "Mavi" });

                context.SaveChanges();
            }
        }
    }
}
