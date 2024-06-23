using Domain.ValueObjects;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string? Nome { get; set; }
        public string? Sobrenome { get; set; }
        public string? CPF { get; set; }
        public string? Email { get; set; }
    }
}
