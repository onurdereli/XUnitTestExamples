using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Moq;
using RealWorldUnitTest.Web.Controllers;
using RealWorldUnitTest.Web.Models;
using RealWorldUnitTest.Web.Repository;
using Xunit;

namespace RealWorldUnitTest.Test.ProductTest
{

    public class ProductControllerTest
    {
        private readonly Mock<IRepository<Product>> _mockRepo;

        private readonly ProductsController _controller;

        private List<Product> _products;

        public ProductControllerTest()
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
        public async void Index_ActionExecutes_ReturnView()
        {
            var result = await _controller.Index();

            Assert.IsType<ViewResult>(result);
        }
    }
}
