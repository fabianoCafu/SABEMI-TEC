using SABEMITEC.PagamentoAPI.DTO;
using System.Text.Json;

namespace SABEMITEC.PagamentoAPI.Model
{
    public class EventoBruto
    {
        public Guid? Id { get; init; }
        public string Payload { get; set; } = string.Empty;
        public DateTime? DataRecebimento { get; set; }

        public EventoBruto() { }

        public EventoBruto(PagamentoDTO pagamentoDto) 
        {
            Payload = JsonSerializer.Serialize(pagamentoDto).ToString();
            DataRecebimento = DateTime.Now;
        }
    }
}
