# Revisits & Studies Manager — Backend

API REST em **ASP.NET Core 8** para gerenciar revisitas e estudos, com autenticação JWT, senhas criptografadas com BCrypt, persistência em **MongoDB** e documentação interativa via Swagger.

## Tecnologias

- .NET 8 (ASP.NET Core Web API)
- MongoDB.Driver 3.x
- JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- BCrypt.Net-Next (hash de senha)
- Swashbuckle (Swagger UI)

## Como rodar

1. Configure a connection string do MongoDB em `appsettings.json`:

```json
"ConnectionStrings": {
  "MongoDb": "mongodb+srv://<usuario>:<senha>@<cluster>/"
},
"Mongo": {
  "Database": "revisits_studies_db"
}
```

2. Execute:

```bash
dotnet restore
dotnet run
```

A API sobe em `http://localhost:5247` (ou `https://localhost:7089` no profile https). Na primeira execução é criado automaticamente um **índice único** em `users.email`.

## Swagger

Com a aplicação rodando, acesse:

```
http://localhost:5247/swagger
```

O documento é emitido no formato **Swagger 2.0** (`swagger: "2.0"`).

Os endpoints de `revisitAndStudies` exigem autenticação (aparecem com cadeado no Swagger). Para testá-los:

1. Faça login em `POST /api/auth/login` (ou cadastre-se em `POST /api/auth/register`)
2. Copie o `token` retornado
3. Clique no botão **Authorize** no topo da página
4. Digite `Bearer {seu-token}` — **incluindo a palavra "Bearer" e um espaço antes do token** — e confirme

Exemplo: `Bearer eyJhbGciOiJIUzI1NiIs...`

Depois disso, todas as rotas protegidas funcionam direto pelo Swagger.

## Autenticação (JWT)

### `POST /api/auth/register` — Cadastro

```json
{
  "name": "Joao Silva",
  "email": "joao@email.com",
  "password": "123456"
}
```

- Senha mínima de 6 caracteres, armazenada com **hash BCrypt** (nunca em texto puro)
- `201` criado · `409` e-mail já cadastrado · `400` dados inválidos

### `POST /api/auth/login` — Login

```json
{
  "email": "joao@email.com",
  "password": "123456"
}
```

Resposta `200`:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-11-10T18:00:00Z"
}
```

Use o token em todas as rotas protegidas:

```
Authorization: Bearer {token}
```

## Revisit and Studies (CRUD) — `api/revisitAndStudies`

Todas as rotas abaixo exigem o header `Authorization: Bearer {token}` (retornam `401` sem ele).

### Model

| Campo       | Tipo   | Descrição                                        |
| ----------- | ------ | ------------------------------------------------ |
| `id`        | string | ObjectId do MongoDB (gerado automaticamente)     |
| `name`      | string | Nome da pessoa (obrigatório)                     |
| `age`       | int    | Idade (0–130)                                    |
| `dayOfWeek` | int    | `0` = Sunday, `1` = Monday, `2` = Tuesday, `3` = Wednesday, `4` = Thursday, `5` = Friday, `6` = Saturday |
| `details`   | string | Detalhes/observações (opcional)                  |
| `address`   | string | Endereço (obrigatório)                           |
| `type`      | enum   | `Revisit` ou `Study`                             |

### Endpoints

| Método   | Rota                          | Descrição                    |
| -------- | ----------------------------- | ---------------------------- |
| `GET`    | `/api/revisitAndStudies`      | Lista todos                  |
| `GET`    | `/api/revisitAndStudies/{id}` | Busca por id                 |
| `POST`   | `/api/revisitAndStudies`      | Cria registro                |
| `PUT`    | `/api/revisitAndStudies/{id}` | Atualiza registro            |
| `DELETE` | `/api/revisitAndStudies/{id}` | Remove registro (`204`)      |

### Filtro por query params — `GET /api/revisitAndStudies`

| Param       | Valores                                                       |
| ----------- | ------------------------------------------------------------- |
| `dayOfWeek` | `0` a `6` (0=Sunday .. 6=Saturday)                            |
| `type`      | `Revisit`, `Study`                                            |

Os dois filtros podem ser combinados. Exemplos:

```bash
# Todas as revisitas
GET /api/revisitAndStudies?type=Revisit

# Todos os registros de domingo
GET /api/revisitAndStudies?dayOfWeek=0

# Estudos de quarta-feira
GET /api/revisitAndStudies?dayOfWeek=3&type=Study
```

### Exemplo de body (POST/PUT)

```json
{
  "name": "Maria",
  "age": 35,
  "dayOfWeek": 0,
  "details": "Estudo semanal",
  "address": "Rua A, 123",
  "type": "Study"
}
```

## Configuração (`appsettings.json`)

| Chave                          | Descrição                                            |
| ------------------------------ | ---------------------------------------------------- |
| `ConnectionStrings:MongoDb`    | Connection string do MongoDB (local ou Atlas)        |
| `Mongo:Database`               | Nome do banco (padrão: `revisits_studies_db`)        |
| `Jwt:Key`                      | Chave secreta de assinatura do token — **troque em produção** |
| `Jwt:Issuer` / `Jwt:Audience`  | Emissor e audiência validados no token               |
| `Jwt:ExpiryMinutes`            | Tempo de expiração do token (padrão: 60 min)         |

## Testes rápidos

O arquivo `RevisitsStudiesManager.Back.http` contém requisições prontas para todas as rotas (compatível com VS Code REST Client / Rider / Visual Studio).

## Estrutura

```
├── Controllers/
│   ├── AuthController.cs              # register + login (JWT)
│   └── RevisitAndStudiesController.cs # CRUD + filtros
├── Data/
│   └── MongoDbContext.cs              # collections do MongoDB
├── DTOs/                              # Requests/responses da API
├── Models/                            # User, RevisitAndStudy, enums
├── Program.cs                         # DI, JWT, Swagger, índice único
└── appsettings.json                   # Mongo + config JWT
```
