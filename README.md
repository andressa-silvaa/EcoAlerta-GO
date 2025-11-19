# 🔥 EcoAlerta GO - Sistema de Monitoramento de Queimadas em Goiás

## 📋 Descrição

Sistema acadêmico desenvolvido para monitoramento de focos de queimadas no estado de Goiás, utilizando **Web Services** para gestão ambiental. A aplicação consome dados de queimadas (atualmente via mock da API do INPE), filtra os dados para Goiás, e disponibiliza essas informações através de uma **API REST** em .NET, consumida por um **frontend em React**.

## 🏗️ Arquitetura

```
Frontend (React) ↔ Backend (.NET Web API) ↔ API INPE (externa/simulada)
                            ↕
                  Banco de Dados Remoto (configurável)
```

### Componentes Principais

- **Backend**: .NET 8 Web API com arquitetura em camadas (Controllers, Services, DTOs, Clients)
- **Frontend**: React (Vite) com React Router, React-Leaflet e Recharts
- **Banco de Dados**: Configurável para bancos remotos (MongoDB Atlas, Railway, Render, Supabase, etc.)
- **API Externa**: Integração com API do INPE (simulada via mock para desenvolvimento)

## 🚀 Tecnologias Utilizadas

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- Swagger/OpenAPI
- Injeção de Dependência

### Frontend
- React 19
- Vite
- React Router DOM
- React-Leaflet (mapas interativos)
- Recharts (gráficos)
- Axios (chamadas HTTP)

## 📁 Estrutura do Projeto

```
EcoAlert-GO/
├── backend/
│   └── EcoAlert.Api/      # Projeto EcoAlerta.Api (nome físico legado)
│       ├── Clients/          # Cliente HTTP para API do INPE
│       ├── Controllers/      # Endpoints REST
│       ├── Services/         # Regras de negócio
│       ├── Models/           # Modelos de domínio
│       ├── DTOs/             # Data Transfer Objects
│       ├── Data/             # DbContext (Entity Framework)
│       └── Program.cs        # Configuração da aplicação
│
└── frontend/
    └── src/
        ├── components/       # Componentes reutilizáveis
        ├── pages/            # Páginas (Dashboard, Mapa)
        ├── services/         # Serviços de API (chamadas HTTP)
        └── App.jsx           # Roteamento principal
```

## 🔧 Pré-requisitos

- **.NET 8 SDK** (ou superior): [Download](https://dotnet.microsoft.com/download)
- **Node.js 18+** e npm: [Download](https://nodejs.org/)
- Editor de código (VS Code, Visual Studio, Rider, etc.)

## 📦 Instalação e Execução

### 1. Backend (.NET Web API)

1. Navegue até a pasta do backend (pasta física `EcoAlert.Api`):
```bash
cd backend/EcoAlert.Api
```
> Observação: o diretório físico preserva o nome legado `EcoAlert.Api`, mas todo o branding público foi atualizado para **EcoAlerta**.

2. Restaure as dependências NuGet:
```bash
dotnet restore
```

3. Execute o projeto:
```bash
dotnet run
```

O backend estará disponível em:
- **HTTP**: `http://localhost:5285`
- **HTTPS**: `https://localhost:7160`
- **Swagger UI**: `http://localhost:5285` (ou `https://localhost:7160`)

### 2. Frontend (React)

1. Navegue até a pasta do frontend:
```bash
cd frontend
```

2. Instale as dependências:
```bash
npm install
```

3. Execute o projeto em modo desenvolvimento:
```bash
npm run dev
```

O frontend estará disponível em `http://localhost:5173` (porta padrão do Vite).

### 3. Configuração da URL da API

Se o backend estiver rodando em uma porta diferente, você pode configurar a URL da API no frontend:

1. Crie um arquivo `.env` na pasta `frontend/`:
```env
VITE_API_BASE_URL=http://localhost:5285
```

2. Ou ajuste diretamente em `frontend/src/services/api.js`.

## 🌐 Endpoints da API

### GET `/api/queimadas`
Obtém lista de focos de queimadas com filtros opcionais.

**Query Parameters:**
- `dataInicio` (opcional): Data inicial do período (formato: YYYY-MM-DD)
- `dataFim` (opcional): Data final do período (formato: YYYY-MM-DD)
- `municipio` (opcional): Nome do município para filtrar

**Exemplo:**
```
GET /api/queimadas?dataInicio=2024-01-01&dataFim=2024-01-31&municipio=Goiânia
```

### GET `/api/queimadas/estatisticas/municipios`
Obtém estatísticas de focos agrupados por município.

**Query Parameters:**
- `dataInicio` (opcional)
- `dataFim` (opcional)

### GET `/api/queimadas/estatisticas/resumo`
Obtém resumo geral das estatísticas (total de focos, municípios afetados, etc.).

**Query Parameters:**
- `dataInicio` (opcional)
- `dataFim` (opcional)

## 📊 Funcionalidades

### Dashboard
- **Cards de Estatísticas**: Total de focos, municípios afetados, média por dia, dia com mais focos
- **Gráficos**:
  - Focos por dia (linha temporal)
  - Top 10 municípios com mais focos (gráfico de barras)
- **Filtros**: Por intervalo de datas e município

### Mapa Interativo
- **Visualização Geográfica**: Mapa centrado no estado de Goiás
- **Marcadores**: Cada foco de queimada é plotado como um marcador vermelho
- **Popups**: Ao clicar em um marcador, exibe detalhes (data/hora, município, coordenadas, intensidade, fonte)
- **Filtros**: Mesmos filtros do dashboard, aplicados dinamicamente ao mapa

## 🔒 CORS

O backend está configurado para permitir requisições do frontend nas portas:
- `http://localhost:5173` (Vite)
- `http://localhost:3000` (Create React App)

Para adicionar outras origens, edite `Program.cs` no backend.

## 💾 Banco de Dados

Atualmente, o projeto usa **Entity Framework InMemory** para desenvolvimento. Para usar um banco remoto:

1. Escolha um serviço gratuito (MongoDB Atlas, Railway, Render, Supabase, etc.)
2. Configure a connection string em `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "sua-connection-string-aqui"
  }
}
```

3. Descomente e ajuste o código em `Program.cs` para usar a connection string:
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
options.UseSqlServer(connectionString); // Para SQL Server
// ou
options.UseNpgsql(connectionString); // Para PostgreSQL
```

## 📝 Notas Importantes

### API do INPE (dados reais)

O backend já está integrado ao serviço **WFS** público do Programa Queimadas (TerraBrasilis). Os dados reais são buscados diretamente do endpoint `https://terrabrasilis.dpi.inpe.br/queimadas/geoserver/wfs` com filtros por estado e intervalo de datas.  
Quando o serviço estiver indisponível (ex.: defesa de APS sem Internet), o sistema ativa automaticamente o fallback mockado.

Para ajustar o consumo da API real, edite a seção `InpeApi` em `appsettings.json`:

```json
{
  "InpeApi": {
    "BaseUrl": "https://terrabrasilis.dpi.inpe.br/queimadas/geoserver/",
    "Resource": "wfs",
    "LayerTemplate": "dados_abertos:focos_{0}_br_todosats",
    "CurrentYearLayer": "dados_abertos:focos_ano_atual_br_todosats",
    "DefaultPais": "Brasil",
    "DefaultEstado": "GO",
    "EstadoFiltro": "GOIÁS",
    "TimeoutSeconds": 30,
    "MaxFeatures": 10000,
    "OutputFormat": "application/json"
  }
}
```

> Se em algum momento o INPE exigir autenticação, basta preencher o campo `ApiToken` e o cliente já enviará o header `Authorization`.

## 🎓 Contexto Acadêmico

Este sistema foi desenvolvido como trabalho acadêmico (APS) sobre:
- Desenvolvimento de aplicações utilizando **Web Services**
- Área de aplicação: **Gestão Ambiental**
- Tema específico: **Monitoramento de focos de queimadas no estado de Goiás**

O código foi desenvolvido com foco em:
- ✅ Organização e boas práticas de arquitetura
- ✅ Separação de responsabilidades (camadas)
- ✅ Documentação no código
- ✅ Padrões REST para Web Services
- ✅ Interface responsiva e intuitiva

## 📚 Documentação Adicional

- Documentação da API está disponível via **Swagger** quando o backend está rodando
- Código comentado em português para facilitar entendimento acadêmico
- Estrutura organizada para demonstração de conceitos de Web Services

## 🤝 Contribuindo

Este é um projeto acadêmico, mas sugestões e melhorias são bem-vindas!

## 📄 Licença

Projeto acadêmico desenvolvido para fins educacionais.

---

**Desenvolvido com ❤️ para monitoramento ambiental**

