using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Models;
using RealWorldUnitTest.Web.Repository;
using Xunit;

namespace RealWorldUnitTest.Test.ProductTest
{
    public class ProductApiControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;

        private readonly ProductsApiController _controller;

        private List<Product> _products;

        public ProductApiControllerTest()
        {
            _mockRepo = new();
            _controller = new(_mockRepo.Object);
            _products = new()
            {
                new()
                {
                    Id = 1,
                    Color = "Kırmızı",
                    Name = "Kalem",
                    Price = 50,
                    Stock = 50
                },
                new()
                {
                    Id = 2,
                    Color = "Mavi",
                    Name = "Defter",
                    Price = 100,
                    Stock = 100
                }
            };
        }

        [Fact]
        public async void GetProducts_ActionExecutes_ReturnOkResultWithProduct()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(_products);

            var result = await _controller.GetProducts();

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProducts = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value);

            Assert.Equal(2, returnProducts.ToList().Count);
        }

        [Theory]
        [InlineData(0)]
        public async void GetProduct_IdIsNull_ReturnNotFound(int productId)
        {
            Product product = null;
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void GetProduct_IdValid_ReturnOkResult(int productId)
        {
            var product = GetProduct(productId);

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.GetProduct(productId);

            var okResult = Assert.IsType<OkObjectResult>(result);

            var returnProduct = Assert.IsAssignableFrom<Product>(okResult.Value);
            
            Assert.Equal(productId, returnProduct.Id);

            _mockRepo.Verify(repo=> repo.GetById(productId),Times.Once);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_IdIsNotEqualProduct_ReturnBadRequestResult(int productId)
        {
            var product = GetProduct(productId);

            var result = _controller.PutProduct(0, product);

            Assert.IsType<BadRequestResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void PutProduct_ActionExecutes_ReturnNoContext(int productId)
        {
            var product = GetProduct(productId);

            _mockRepo.Setup(repo => repo.Update(product));

            var result = _controller.PutProduct(productId, product);

            _mockRepo.Verify(repo=> repo.Update(product), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async void PostProduct_ActionExecutes_ReturnCreatedAtAction()
        {
            Product product = _products.First();

            _mockRepo.Setup(repo => repo.Create(product)).Returns(Task.CompletedTask);

            var result = await _controller.PostProduct(product);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);

            _mockRepo.Verify(repo=> repo.Create(product), Times.Once);

            Assert.Equal("GetProduct", createdAtActionResult.ActionName);
        }

        [Theory]
        [InlineData(0)]
        public async void DeleteProduct_IdInvalid_ReturnNotFound(int productId)
        {
            Product product = null;
            
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.DeleteProduct(productId);

            Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public async void DeleteProduct_ActionExecutes_ReturnNoContent(int productId)
        {
            var product = GetProduct(productId);

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            _mockRepo.Setup(repo => repo.Delete(product));

            var result = await _controller.DeleteProduct(productId);

            _mockRepo.Verify(repo=> repo.Delete(product), Times.Once);

            Assert.IsType<NoContentResult>(result);
        }

        private Product GetProduct(int productId)
        {
            return _products.First(x => x.Id == productId);
        }
    }
}
