using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

        private readonly List<Product> _products;

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

        [Fact]
        public async void Index_ActionExecutes_ReturnProductList()
        {
            _mockRepo.Setup(repo => repo.GetAll()).ReturnsAsync(_products);

            var result = await _controller.Index();

            //Dönüş tipi ViewResult olduğu kontrol edilir
            ViewResult viewResult = Assert.IsType<ViewResult>(result);

            //Gelen Datanın modeli IEnumerable<Product> olduğu kontrol edilir
            var productList = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);

            //Dönen datanın eleman sayısı 2 mi olduğu kontrol edilir
            Assert.Equal<int>(2, productList.Count());
        }

        [Fact]
        public async void Details_IdIsNull_ReturnRedirectToIndexAction()
        {
            //Id null durumu sağlanır
            var result = await _controller.Details(null);

            //Id null olduğunda dönüş tipi RedirectToActionResult olduğu kontrol edilir
            var redirect = Assert.IsType<RedirectToActionResult>(result);

            //Dönüş tipinin hangi action'a denk geldiği kontrol edilir
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void Details_IdInValid_ReturnNotFound()
        {
            Product product = null;
            //Db'de product olmama durumu kontrolü için mock oluşturulur
            _mockRepo.Setup(repo => repo.GetById(0)).ReturnsAsync(product);

            //Oluşturulan mock'a göre details çağrılır
            var result = await _controller.Details(0);

            //Controllerdan gelen tipin NotFoundResult olması beklenir
            var redirect = Assert.IsType<NotFoundResult>(result);

            //NotFoundResult olan tipin statusCode'unun 404 olması beklenir
            Assert.Equal<int>(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(1)]
        public async void Details_IdValid_ReturnProduct(int productId)
        {
            //Mock listeden gelen Id'yi alır
            var product = _products.First(x => x.Id == productId);

            //Gelen Idye göre setup oluşturulur
            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            //Db'den belirtilen Id'ye ait kayıt getirilir
            var result = await _controller.Details(productId);

            //Gelen kayıt tipi başarılı sonuçlanıp ViewResult geldi mi diye kontrol edilir
            var viewResult = Assert.IsType<ViewResult>(result);

            //Gelen viewResult'ın modeli Product tipi mi kontrol edilir
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            //seçilen mock product ile gelen resultProduct kontrolleri sağlanır
            Assert.Equal(product.Id, resultProduct.Id);
            Assert.Equal(product.Name, resultProduct.Name);
        }

        [Fact]
        public void Create_ActionExecutes_ReturnView()
        {
            var result = _controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async void CreatePost_InValidModelState_ReturnView()
        {
            //Name alanı için zorunlu kontrol ekler
            _controller.ModelState.AddModelError("Name", "Name alanı gereklidir");

            var result = await _controller.Create(_products.First());

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(viewResult.Model);
        }

        [Fact]
        public async void CreatePost_ValidModelState_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Create(_products.First());

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async void CreatePost_ValidModelState_CreateMethodExecute()
        {
            Product newProduct = null;
            
            //It.IsAny ile bir product tipi alabileceğini ve callback ile aldığı product'ı dönüş olacak eşleştireceğini belirtir
            _mockRepo.Setup(repo => repo.Create(It.IsAny<Product>())).Callback<Product>(x => newProduct = x);
            
            var result = await _controller.Create(_products.First());

            //repo üzerinden create methodunun en az 1 kere çalışıp çalışmadığını kontrol eder
            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()),Times.Once);

            Assert.Equal(_products.First().Id, newProduct.Id);
        }

        [Fact]
        public async void CreatePost_InValidModelState_NeverCreateExecute()
        {
            _controller.ModelState.AddModelError("Name","");

            var result = await _controller.Create(_products.First());

            _mockRepo.Verify(repo => repo.Create(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async void Edit_IdIsNull_ReturnRedirectToIndexAction()
        {
            var result = await _controller.Edit(null);

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(0)]
        public async void Edit_IdInValid_ReturnNotFound(int productId)
        {
            Product product = null;

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Edit(productId);

            var redirect = Assert.IsType<NotFoundResult>(result);

            Assert.Equal(404, redirect.StatusCode);
        }

        [Theory]
        [InlineData(2)]
        public async void Edit_ActionExecute_ReturnProduct(int productId)
        {
            var product = _products.First(x => x.Id == productId);

            _mockRepo.Setup(repo => repo.GetById(productId)).ReturnsAsync(product);

            var result = await _controller.Edit(productId);

            var viewResult = Assert.IsType<ViewResult>(result);

            //Referans belirtilip belirtilmediğini kontrol eder. Interface, miras alınma gibi durumlar kontrol edilir
            //Burada product olarak atanıp atanamadığını kontrol eder
            var resultProduct = Assert.IsAssignableFrom<Product>(viewResult.Model);

            Assert.Equal(product.Id, resultProduct.Id);
        }

        [Theory]
        [InlineData(1)]
        public void EditPost_IdIsNotEqualProduct_ReturnNotFound(int productId)
        {
            var result = _controller.Edit(2, _products.First(x => x.Id == productId));

            var redirect = Assert.IsType<NotFoundResult>(result);
        }

        [Theory]
        [InlineData(1)]
        public void EditPost_InValidModelState_ReturnView(int productId)
        {
            _controller.ModelState.AddModelError("Name","");

            var result = _controller.Edit(productId, _products.First(x => x.Id == productId));

            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.IsType<Product>(viewResult.Model);
        }

        [Theory]
        [InlineData(1)]
        public void EditPost_ValidModelState_ReturnRedirectToIndexAction(int productId)
        {
            var result = _controller.Edit(productId, _products.First(x => x.Id == productId));

            var redirect = Assert.IsType<RedirectToActionResult>(result);

            Assert.Equal("Index", redirect.ActionName);
        }

        [Theory]
        [InlineData(1)]
        public void EditPost_ValidModelState_UpdateMethodExecute(int productId)
        {
            var product = _products.First(x => x.Id == productId);
            _mockRepo.Setup(repo => repo.Update(product));

            _controller.Edit(productId, product);

            _mockRepo.Verify(repo=> repo.Update(It.IsAny<Product>()),Times.Once);
        }
    }
}
