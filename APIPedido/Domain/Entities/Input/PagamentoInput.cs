using Newtonsoft.Json;

namespace Domain.Entities.Input
{
    public class PagamentoInput
    {
        [JsonProperty("Id")]
        public string? Id { get; set; }

        [JsonProperty("statusPagamento")]
        public string? statusPagamento { get; set; }

        [JsonProperty("valorPagamento")]
        public float valorPagamento { get; set; } // Valor do pagamento

        [JsonProperty("metodoPagamento")]
        public string? metodoPagamento { get; set; }

        public DateTime dataPagamento { get; set; } // A data do pedido

        [JsonProperty("idPedido")]
        public int idPedido { get; set; } // Id do Pedido pago.
    }
}
