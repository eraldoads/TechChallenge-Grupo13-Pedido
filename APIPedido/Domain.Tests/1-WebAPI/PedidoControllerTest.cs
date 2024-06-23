using API.Controllers;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Domain.Tests._1_WebAPI
{
    public class PedidoControllerTest
    {
        private readonly Mock<IPedidoService> _mockService;
        private readonly PedidoController _controller;
        private readonly Mock<IPedidoService> _AppService = new();

        public PedidoControllerTest()
        {
            _mockService = new Mock<IPedidoService>();
            _controller = new PedidoController(_AppService.Object);
        }

        //[Trait("Categoria", "PedidoController")]
        //[Fact(DisplayName = "BuscarListaPedidos OkResult")]
        //public async Task GetPedidos_ReturnsOkResult_BuscarListaPedidos()
        //{
        //    // Arrange
        //    _AppService.Setup(service => service.GetPedidos())
        //        .ReturnsAsync(new List<Pedido> { new(), new() });

        //    // Act
        //    var result = await _controller.GetPedidos();

        //    // Assert
        //    Assert.NotNull(result);
        //}

    }
}
