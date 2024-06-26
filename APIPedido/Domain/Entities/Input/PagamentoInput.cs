using Newtonsoft.Json;

namespace Domain.Entities.Input
{
    public class PagamentoInput
    {
        [JsonProperty("statusPagamento")]
        public string? StatusPagamento { get; set; }

        [JsonProperty("valorPagamento")]
        public float ValorPagamento { get; set; } // Valor do pagamento

        [JsonProperty("metodoPagamento")]
        public string? MetodoPagamento { get; set; }

        public DateTime DataPagamento { get; set; } // A data do pedido

        [JsonProperty("idPedido")]
        public int IdPedido { get; set; } // Id do Pedido pago.
    }
}
