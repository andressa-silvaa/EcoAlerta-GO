# 🚀 Guia Rápido de Execução

## Pré-requisitos
- .NET 8 SDK instalado
- Node.js 18+ instalado
- NPM ou Yarn

## Passos para Executar

### 1️⃣ Instalar Dependências do Frontend (primeira vez)

```bash
cd frontend
npm install
```

### 2️⃣ Iniciar o Backend

**Terminal 1:**
```bash
cd backend/EcoAlert.Api
dotnet restore
dotnet run
```
> Observação: a pasta física permanece como `EcoAlert.Api` (nome legado), embora o projeto esteja publicado como **EcoAlerta**.

**Aguarde** até ver a mensagem indicando que o servidor está rodando em `http://localhost:5285`

✅ **Swagger disponível em**: `http://localhost:5285` (documentação interativa da API)

### 3️⃣ Iniciar o Frontend (em outro terminal)

**Terminal 2:**
```bash
cd frontend
npm run dev
```

**Aguarde** até ver a mensagem indicando que o servidor está rodando em `http://localhost:5173`

### 4️⃣ Acessar a Aplicação

Abra seu navegador em: **http://localhost:5173**

🎉 **Pronto!** O sistema está funcionando!

## 🎯 Funcionalidades Disponíveis

1. **Dashboard** (`/`): 
   - Cards com estatísticas
   - Gráficos de focos por dia e por município
   - Filtros por data e município

2. **Mapa** (`/mapa`):
   - Visualização geográfica dos focos
   - Marcadores clicáveis com detalhes
   - Filtros integrados

## ⚠️ Problemas Comuns

### Backend não inicia
- Verifique se a porta 5285 está disponível
- Execute `dotnet restore` antes de `dotnet run`

### Frontend não consegue conectar ao backend
- Verifique se o backend está rodando
- Confirme a URL no arquivo `frontend/src/services/api.js` (padrão: `http://localhost:5285`)
- Verifique o CORS no `Program.cs` do backend

### Erro ao carregar mapa
- Verifique se as dependências foram instaladas: `npm install`
- Certifique-se de que o Leaflet foi instalado corretamente

## 📝 Notas

- Os dados são **mockados** (simulados) - não vêm da API real do INPE
- O banco de dados está em **memória** - dados são perdidos ao reiniciar
- Para produção, configure um banco de dados remoto em `appsettings.json`

## 🔗 Links Úteis

- Backend API: http://localhost:5285
- Swagger UI: http://localhost:5285
- Frontend: http://localhost:5173

