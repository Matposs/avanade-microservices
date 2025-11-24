# 🛒 Avanade Microservices – E-commerce

Este projeto simula uma plataforma de e-commerce com **arquitetura de microserviços**, desenvolvida em **.NET Core (C#)**.  
O sistema é composto por quatro serviços independentes que se comunicam entre si para gerenciar **estoque, vendas, autenticação e roteamento via API Gateway**.

---

## 📐 Arquitetura

```mermaid
graph TD
  Client --> Gateway
  Gateway --> Auth
  Gateway --> Vendas
  Gateway --> Estoque
  Vendas -- "event: OrderConfirmed" --> RabbitMQ
  RabbitMQ -- "consume: UpdateStock" --> Estoque
⚙️ Tecnologias Utilizadas
.NET Core / C#

Entity Framework Core

SQL Server

RabbitMQ

JWT (JSON Web Token)

API Gateway (Ocelot/YARP)

Docker & Docker Compose

🚀 Funcionalidades
Microserviço de Estoque
Cadastro de produtos (nome, descrição, preço, quantidade)

Consulta de produtos e estoque disponível

Atualização automática do estoque após vendas

Microserviço de Vendas
Criação de pedidos com validação de estoque

Consulta de pedidos

Notificação de vendas para o serviço de estoque via RabbitMQ

Microserviço de Autenticação
Registro e login de usuários

Geração de tokens JWT para acesso seguro

API Gateway
Roteamento centralizado das requisições

Autenticação e autorização via JWT

🐳 Como Rodar com Docker Compose
Pré-requisitos:

Docker

Docker Compose

Passo a passo:

bash
# Clone o repositório
git clone https://github.com/Matposs/avanade-microservices.git
cd avanade-microservices

# Suba os serviços
docker compose up -d

# Acesse os serviços
# Gateway: http://localhost:5000
# Estoque: http://localhost:5001
# Vendas: http://localhost:5002
# Auth: http://localhost:5003
# RabbitMQ Management: http://localhost:15672 (user: guest / pass: guest)
📌 Endpoints Principais (exemplos)
Auth
POST /auth/register → cria usuário

POST /auth/login → retorna JWT

Estoque
POST /estoque/produtos → cadastra produto

GET /estoque/produtos → lista produtos

Vendas
POST /vendas/pedidos → cria pedido

GET /vendas/pedidos/{id} → consulta pedido

🧪 Testes
Testes unitários básicos para cadastro de produtos e criação de pedidos.

Para rodar:

bash
dotnet test

👨‍💻 Autor
Projeto desenvolvido por Matheus Poss como estudo de arquitetura de microserviços em .NET e finalização do curso Avanade Backend+IA.