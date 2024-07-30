using Domain.ValueObjects;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Produto
    {
        public int IdProduto { get; set; }
        public string? NomeProduto { get; set; }
        public float ValorProduto { get; set; }
        public int IdCategoria { get; set; }
        public string? DescricaoProduto { get; set; }
        public string? ImagemProduto { get; set; }

    }
}
