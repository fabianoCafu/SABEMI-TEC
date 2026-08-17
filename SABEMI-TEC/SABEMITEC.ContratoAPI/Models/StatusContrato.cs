namespace SABEMITEC.ContratoAPI.Models
{
    public class StatusContrato
    {
        public Guid Id { get; set; }
        public string? IdTransacao { get; set; }
        public string ? IdContrato { get; set; }
        public string? Status { get; set; }
        public string? Falha { get; set; }
        public DateTime DataProcessamento { get; set; }

        public StatusContrato() { }

        public StatusContrato(
            string idTransacao, 
            string idContrato,
            string status,
            string falha) 
        {
            IdTransacao  = idTransacao;
            IdContrato  = idContrato;
            Status = status;
            Falha  = falha;
            DataProcessamento = DateTime.Now;
        }
    }
}
