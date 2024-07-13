# TechChallenge-Grupo13-Pedido
Este repositório é dedicado ao microsserviço de Pedido, o qual foi desmembrado do monolito criado para a lanchonete durante a evolução da pós-graduação em Arquitetura de Software da FIAP.

Tanto o build e push para o repositório no ECR da AWS usando Terraform são realizados via Github Actions.

## 🖥️ Grupo 13 - Integrantes
🧑🏻‍💻 *<b>RM352133</b>*: Eduardo de Jesus Coruja </br>
🧑🏻‍💻 *<b>RM352316</b>*: Eraldo Antonio Rodrigues </br>
🧑🏻‍💻 *<b>RM352032</b>*: Luís Felipe Amengual Tatsch </br>

## Saga
Na fase 5, evoluímos o nosso sistema e passamos a utilizar o padrão SAGA, no qual a comunicação entre os microsserviços ocorre por meio de mensageria.

Optamos pelo padrão de Saga Coreografada, pois, o fluxo é simples e não há necessidade de uma orquestração mais elaborada.

O Processo inicia no momento da criação do Pedido, onde é realizada a gravação no banco de dados e inserida uma mensagem na fila informando que um novo pedido foi criado. Ambas operações ocorrem dentro de uma mesma transação para garantirmos que as duas se completem ou nenhuma delas.

Caso ocorra falha em uma das operações dentro da transação, seja de gravação no banco de dados MySQL ou de publicação da mensagem na fila, nenhuma delas se completa e voltamos ao estado anterior.

Também em uma transação atômica, o microsserviço de Pagamento lê a mensagem da fila <b>novo_pedido</b> e grava um pagamento com status <b>Pendente</b> no MongoDB, relativo ao Id do pedido recebido na mensagem. 

Após o cliente realizar o processo de pagamento via Mercado Pago, o endpoint de webhook do microsserviço Pagamento recebe a notificação do Mercado Pago e atualiza o status do pagamento no MongoDB. Se aprovado, o status do pagamento é atualizado para <b>Aprovado</b> e uma mensagem é publicada na fila <b>pagamento_aprovado</b> para que o microsserviço de Pedido dê andamento ao processo atualizando o status do pedido para <b>Em preparação</b>.

![image](https://github.com/user-attachments/assets/c1885508-c5c3-46e9-86b6-22a309781401)

Abaixo, temos o trecho de código no qual o microsserviço Pedido grava um novo pedido no MySQL e insere uma mensagem na fila <b>novo_pedido</b> em uma transação:

![image](https://github.com/user-attachments/assets/6545ce67-8264-4bf5-b1b3-18dc6df2462e)

Ocorrendo um indisponilidade no MySQL que impossibilite a atualização do status do pedido para <b>Em preparação</b> após a aprovação do pagamento, a mensagem é recolocada na fila <b>pagamento_aprovado</b> para novas tentativas até o limite definido na variável de ambiente <b>QTDE_RETRY_PAGAMENTO</b>.

![image](https://github.com/user-attachments/assets/791b4061-3a1c-478c-8b3b-af326d744e8b)

O microsserviço Pedido fica lendo continuamente a fila <b>pagamento_erro</b> e altera status dos pedidos que constam nas mensagens para <b>Cancelado</b>, pois, para estes, ocorreu erro no processo de pagamento.

Caso o pagamento seja rejeitado pelo Mercado Pago, o cliente receberá a notificação no aplicativo e poderá alterar a forma de pagamento. Enquanto o pagamento não for aprovado, o pedido continuará com status <b>Recebido</b> e o pagamento com status <b>Pendente</b>.

## Arquitetura
Na fase 5, adicionamos o RabbitMQ como broker de mensageria para implementarmos o padrão SAGA. 

Para rodar a aplicação, provisionamos toda a infraestrutura utilizando Terraform. Os links abaixo correspondem aos repositórios dos elementos de infraestrutura:

https://github.com/eraldoads/TechChallenge-Grupo13-K8sTerraform

https://github.com/eraldoads/TechChallenge-Grupo13-BDTerraform

https://github.com/eraldoads/TechChallenge-Grupo13-BDTerraformMongo

https://github.com/eraldoads/TechChallenge-Grupo13-RabbitMQ

Quando disparamos a Github Action, é realizado o build da aplicação e o push para o repositório criado previamente no Elastic Container Registry (ECS).
Ao final da action, é atualizada a Service no Elastic Container Service (ECS), executando assim a service que irá realizar a criação do container.

![image](https://github.com/user-attachments/assets/298b69c5-344e-4391-a2ac-c78239566d07)

Para este microsserviço, utilizamos .NET 8.0, o que também representa uma evolução de tecnologia em relação ao monolito, o qual foi baseado no .NET 6.0 .
