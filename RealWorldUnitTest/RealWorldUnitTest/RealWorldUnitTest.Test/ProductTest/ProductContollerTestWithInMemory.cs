using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Models;
using Xunit;

namespace RealWorldUnitTest.Test.ProductTest
{
    public class ProductContollerTestWithInMemory : ProductControllerTest
    {
        public ProductContollerTestWithInMemory()
        {
            SetContextOptions(new DbContextOptionsBuilder<UnitTestDbContext>().UseInMemoryDatabase("UnitTestInMemoryDb").Options);
        }

        [Fact]
        public async void CreatePost_ModelValidProduct_ReturnRedirectToActionWithSaveProduct()
        {
            Product newProduct = new()
            {
                Name = "Kalem 3",
                Price = 50,
                Stock = 10,
                Color = "Kırmızı"
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
            await using (var context = new UnitTestDbContext(_contextOptions))
            {
                var category = await context.Categories.FindAsync(categoryId);

                context.Categories.Remove(category);

                await context.SaveChangesAsync();
            }

            await using (var context = new UnitTestDbContext(_contextOptions))
            {
                var products = await context.Products.Where(x => x.CategoryId == categoryId).ToListAsync();
                // İlişkisel veritabanı kullanıldığında hata dönmemiş olacak. Ef core memoryde bu hata verir.
                Assert.Empty(products);
            }
        }
    }
}
