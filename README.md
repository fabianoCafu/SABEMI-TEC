1. O Cenário
 
  A Sabemi precisa processar notificações de pagamentos (Webhooks) vindas de um
  banco parceiro. Essas notificações confirmam a liquidação de seguros ou parcelas de
  empréstimos. Sua missão é construir o serviço que recebe esses dados, garante que não
  haja duplicidade e exibe o status em um painel administrativo.

2. Requisitos Técnicos
 
  Backend (.NET)
  • Endpoint de Recebimento: Criar um endpoint POST /webhooks/pagamento
  que receba um JSON contendo: id_transacao, id_contrato, valor,
  data_pagamento e status.

  • Segurança: Implementar uma validação simples de "Signature" ou "ApiKey" no
  Header da requisição.

  • Idempotência: O sistema não pode processar o mesmo id_transacao duas
  vezes (mesmo que o banco envie a notificação repetidamente por erro de rede).

  • Persistência: Salvar os eventos em um banco de dados (PostgreSQL ou SQL
  Server). Deve haver uma tabela de "Log de Eventos Brutos" e uma tabela de
  "Status do Contrato".

  • Resiliência: Simular que o processamento da regra de negócio é pesado (ex: um
  setTimeout de 2 segundos). O endpoint deve responder rápido ao banco,
  enquanto o processamento acontece em "background".
  Frontend (React, Angular ou Next.js)

  • Dashboard: Uma tela simples que liste os pagamentos recebidos em tempo real
  (ou via refresh).

  • Filtros: Filtrar por status (Sucesso/Erro) e por ID do Contrato.

  • Visualização de Erros: Se um evento falhar na validação, ele deve aparecer
  com um alerta visual claro no painel.

---
Fluxo sugerido pela IA.

<img width="698" height="559" alt="Sem título" src="https://github.com/user-attachments/assets/d821dc66-83e0-418c-900c-6278785f33e0" />


