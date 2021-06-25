using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Models;
using Xunit;

namespace RealWorldUnitTest.Test.ProductTest
{
    public class ProductControllerTestWithSQLite : ProductControllerTest
    {
        public ProductControllerTestWithSQLite()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.OpenAsync();

            SetContextOptions(new DbContextOptionsBuilder<UnitTestDbContext>().UseSqlite(connection).Options);
        }

        [Fact]
        public async Task Create_ModelValidProduct_ReturnRedirectToActionWithSaveProduct()
        {
            Product newProduct = new()
            {
                Name = "Kalem 3",
                Price = 200,
                Stock = 100,
                Color = "Mavi"
            };

            await using (UnitTestDbContext context = new(_contextOptions))
            {
                var category = context.Categories.First();

                newProduct.CategoryId = category.Id;

                ProductsController controller = new(context);

                var result = await controller.Create(newProduct);

                var redirect = Assert.IsType<RedirectToActionResult>(result);

                Assert.Equal("Index", redirect.ActionName);
            }

            await using (UnitTestDbContext context = new(_contextOptions))
            {
                var product = context.Products.FirstOrDefault(x => x.Name == newProduct.Name);

                Assert.Equal(newProduct.Name, product.Name);
            }
        }

        [Theory]
        [InlineData(1)]
        public async Task DeleteCategory_ExistCategoryId_DeletedAllProducts(int categoryId)
        {
            await using (UnitTestDbContext context = new(_contextOptions))
            {
                var category = await context.Categories.FindAsync(categoryId);

                Assert.NotNull(category);

                context.Categories.Remove(category);

                await context.SaveChangesAsync();
            }

            await using (UnitTestDbContext context = new(_contextOptions))
            {
                var products = await context.Products.Where(x => x.CategoryId == categoryId).ToListAsync();

                Assert.Empty(products);
            }
        }
    }
}