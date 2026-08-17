namespace SABEMITEC.Shared.Event
{
    public record EventoStatusContrato(
        string? IdTransacao,
        string? IdContrato,
        string? Status,
        string? Falha = null
    );
}

