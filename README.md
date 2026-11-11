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

**Cada usuário só acessa os próprios registros**: o `userId` do usuário autenticado (claim `sub` do JWT) é salvo na criação e todas as buscas/updates/deletes são filtrados por ele — um usuário não vê nem altera registros de outro.

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
| `lastVisit` | string | Data da última visita em formato ISO 8601 (opcional). Aceita `"2026-11-09"` ou `"2026-11-09T00:00:00.000Z"` |
| `userId`    | string | ObjectId do usuário dono (preenchido automaticamente pelo token) |

### Endpoints

| Método   | Rota                          | Descrição                    |
| -------- | ----------------------------- | ---------------------------- |
| `GET`    | `/api/revisitAndStudies`      | Lista todos                  |
| `GET`    | `/api/revisitAndStudies/{id}` | Busca por id                 |
| `POST`   | `/api/revisitAndStudies`      | Cria registro                |
| `PATCH`  | `/api/revisitAndStudies/{id}` | Atualização parcial          |
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

### PATCH — atualização parcial

`PATCH /api/revisitAndStudies/{id}` altera **somente os campos enviados** — os demais permanecem inalterados. Todos os campos do body são opcionais; enviar um body vazio retorna `400`.

```json
{
  "name": "Maria Silva",
  "lastVisit": "2026-11-10T00:00:00.000Z"
}
```

### Exemplo de body (POST)

```json
{
  "name": "Maria",
  "age": 35,
  "dayOfWeek": 0,
  "details": "Estudo semanal",
  "address": "Rua A, 123",
  "type": "Study",
  "lastVisit": "2026-11-09"
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

## Deploy (Render)

O projeto inclui um `Dockerfile` — no Render, crie um **Web Service** com runtime **Docker** apontando para este repositório.

### Variáveis de ambiente no Render

| Key                          | Valor                                                        |
| ---------------------------- | ------------------------------------------------------------ |
| `ConnectionStrings__MongoDb` | Sua connection string do MongoDB Atlas (com usuário e senha) |
| `Jwt__Key`                   | Chave secreta forte para assinar os tokens (gere uma nova)   |
| `Jwt__Issuer`                | `RevisitsStudiesManager` (opcional — já é o padrão)          |
| `Jwt__Audience`              | `RevisitsStudiesManager` (opcional — já é o padrão)          |
| `ASPNETCORE_ENVIRONMENT`     | `Production`                                                 |
| `Cors__AllowedOrigins__0`    | URL do seu frontend em produção (ex: `https://meu-front.vercel.app`) |

### CORS

- **Local**: qualquer origem `localhost`/`127.0.0.1` em qualquer porta é liberada automaticamente
- **Produção**: adicione as URLs do frontend em `Cors:AllowedOrigins` ou via env vars `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.

> O `__` (dois underlines) é o separador de configuração do .NET — `ConnectionStrings__MongoDb` equivale a `ConnectionStrings:MongoDb` no appsettings.

A aplicação detecta automaticamente a variável `PORT` injetada pelo Render e faz bind em `0.0.0.0:{PORT}`.

### MongoDB Atlas

O Render usa IPs de saída dinâmicos. No Atlas, em **Network Access**, libere `0.0.0.0/0` (ou os IPs do Render) para permitir a conexão.

### Segredos locais

A connection string real e a chave JWT ficam em `dotnet user-secrets` (fora do git):

```bash
dotnet user-secrets set "ConnectionStrings:MongoDb" "<sua-connection-string>"
dotnet user-secrets list
```

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
