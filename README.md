Objetivo

Desenvolver um programa em C#, que seja capaz de atender aos requisitos descritos abaixo.

Requisitos

 - O sistema desenvolvido deve avisar, via e-mail, caso a cotação de um ativo da B3 caia mais do que certo nível, ou suba acima de outro.
 - O programa deve ser uma aplicação de console (não há necessidade de interface gráfica).
 - Ele deve ser chamado via linha de comando com 3 parâmetros.
    > O ativo a ser monitorado
    > O preço de referência para venda
    > O preço de referência para compra
    Exemplo:
    > stock-quote-alert.exe PETR4 22.67 22.59
 - Ele deve ler de um arquivo de configuração com:
    > O e-mail de destino dos alertas
    > As configurações de acesso ao servidor de SMTP que irá enviar o e-mail
 - A escolha da API de cotação é livre.
 - O programa deve ficar continuamente monitorando a cotação do ativo enquanto estiver rodando.

Quando o preço for maior que o param2, um e-mail deve ser disparado aconselhando a venda.

Quando o preço for menor que o param3, um e-mail deve ser disparado aconselhando a compra.


---------------------------------------------------------------------------------------------------------------------------------

Para rodar o programa:
  > dotnet run stock-quote-alert.cs PETR4 22,67 22,59
  ou
  > dotnet publish stock-quote-alert.cs -c Release -o dist -p:PublishAot=false
  > .\dist\stock-quote-alert.exe PETR4 22,67 22,59
  - , ou . depende da língua do seu Windows
