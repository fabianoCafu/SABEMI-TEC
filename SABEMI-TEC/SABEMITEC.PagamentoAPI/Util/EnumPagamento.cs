using System.ComponentModel;

namespace SABEMITEC.PagamentoAPI.Util
{
    public static class EnumPagamento
    {
        public enum EnumStatusPagamento
        {
            [Description("QUITAÇÃO")]
            Quitacao,

            [Description("PARCELAMENTO")]
            Parcelamento
        }

        public enum StatusContrato
        {
            [Description("SUCESSO")]
            Sucesso,

            [Description("ERRO")]
            Erro
        }
    }
}
