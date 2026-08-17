export interface Pagamento {
    idTransacao: string | any;
    idContrato : string | any;
    dataProcessamento : string;
    status : string;
    falha : string;
}
