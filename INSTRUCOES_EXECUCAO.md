# 📖 Instruções Completas de Execução

## ✅ Sistema Completo e Funcional

Este sistema está **100% funcional** e pronto para uso. Todas as funcionalidades foram implementadas e testadas.

## 🚀 Execução Rápida

### Opção 1: Execução Manual (Recomendado)

#### Passo 1: Instalar Dependências do Frontend
```bash
cd frontend
npm install
```

#### Passo 2: Iniciar Backend (Terminal 1)
```bash
cd backend/EcoAlert.Api
dotnet run
```
> Observação: por compatibilidade a pasta física do backend ainda se chama `EcoAlert.Api`, embora todo o branding do sistema seja **EcoAlerta**.

Aguarde a mensagem: `Now listening on: http://localhost:5285`

#### Passo 3: Iniciar Frontend (Terminal 2)
```bash
cd frontend
npm run dev
```

Aguarde a mensagem: `Local: http://localhost:5173`

#### Passo 4: Acessar no Navegador
Abra: **http://localhost:5173**

---

## 🎯 Funcionalidades Implementadas

### ✅ Backend (.NET Web API)
- [x] API REST completa com 3 endpoints principais
- [x] Integração com API do INPE (mock realista)
- [x] Filtros por data e município
- [x] Cálculo de estatísticas
- [x] Validações de segurança
- [x] Tratamento de erros
- [x] CORS configurado
- [x] Swagger/OpenAPI
- [x] Headers de segurança

### ✅ Frontend (React)
- [x] Dashboard com cards de estatísticas
- [x] Gráficos interativos (Recharts)
- [x] Mapa interativo (React-Leaflet)
- [x] Filtros funcionais
- [x] Design moderno e responsivo
- [x] Tratamento de erros
- [x] Loading states
- [x] Navegação entre páginas

---

## 🔒 Segurança Implementada

### Backend
- ✅ Validação de parâmetros de entrada
- ✅ Sanitização de dados (município)
- ✅ Headers de segurança HTTP
- ✅ Tratamento de exceções
- ✅ CORS configurado corretamente
- ✅ Validação de datas

### Frontend
- ✅ Validação de formulários
- ✅ Tratamento de erros de rede
- ✅ Mensagens de erro amigáveis
- ✅ Timeout de requisições

---

## 🎨 Interface

### Design
- ✅ Gradientes modernos
- ✅ Animações suaves
- ✅ Cards com hover effects
- ✅ Layout responsivo
- ✅ Cores harmoniosas
- ✅ Tipografia clara

### UX
- ✅ Navegação intuitiva
- ✅ Feedback visual
- ✅ Loading indicators
- ✅ Mensagens de erro claras
- ✅ Filtros fáceis de usar

---

## 📊 Dados

Os dados são **gerados automaticamente** (mock) e incluem:
- Focos de queimadas realistas
- 15 municípios de Goiás
- Coordenadas geográficas corretas
- Datas variadas
- Intensidades e fontes de satélite

**Período padrão**: Últimos 30 dias

---

## 🐛 Solução de Problemas

### Backend não inicia
```bash
# Verificar se .NET está instalado
dotnet --version

# Restaurar dependências
cd backend/EcoAlert.Api
dotnet restore

# Limpar e reconstruir
dotnet clean
dotnet build
dotnet run
```

### Frontend não inicia
```bash
# Verificar Node.js
node --version
npm --version

# Limpar cache e reinstalar
cd frontend
rm -rf node_modules package-lock.json
npm install
npm run dev
```

### Erro de CORS
- Verifique se o backend está rodando
- Confirme a porta no `Program.cs` (linha 57)
- Verifique a URL no `frontend/src/services/api.js`

### Mapa não carrega
- Verifique se o Leaflet foi instalado: `npm list leaflet react-leaflet`
- Verifique o console do navegador (F12)
- Certifique-se de que há dados para exibir

### Dados não aparecem
- Verifique se o backend está respondendo: `http://localhost:5285/api/queimadas`
- Verifique o console do navegador (F12)
- Confirme que os filtros de data estão corretos

---

## 📝 Notas Importantes

1. **Primeira Execução**: Instale as dependências do frontend com `npm install`
2. **Portas**: Backend (5285), Frontend (5173)
3. **Dados**: São mockados (simulados) para desenvolvimento
4. **Banco**: Usa InMemory (dados são perdidos ao reiniciar)
5. **API INPE**: Consome o WFS real do TerraBrasilis/INPE com fallback automático para mock quando offline

---

## 🎓 Para Apresentação Acadêmica

### Pontos Fortes a Destacar:
1. ✅ Arquitetura em camadas bem definida
2. ✅ Separação de responsabilidades
3. ✅ Web Services RESTful
4. ✅ Interface moderna e intuitiva
5. ✅ Segurança implementada
6. ✅ Código documentado
7. ✅ Boas práticas de desenvolvimento

### Demonstração Sugerida:
1. Mostrar o Swagger (documentação da API)
2. Demonstrar os filtros funcionando
3. Mostrar o mapa interativo
4. Explicar a arquitetura
5. Mostrar o código organizado

---

## 🔗 URLs Importantes

- **Frontend**: http://localhost:5173
- **Backend API**: http://localhost:5285
- **Swagger UI**: http://localhost:5285
- **API Endpoint**: http://localhost:5285/api/queimadas

---

## ✨ Melhorias Implementadas

### UI/UX
- Design moderno com gradientes
- Animações suaves
- Cards interativos
- Loading states melhorados
- Mensagens de erro claras

### Segurança
- Validação de entrada
- Sanitização de dados
- Headers de segurança
- CORS configurado

### Performance
- Carregamento paralelo de dados
- Otimizações de renderização
- Cache de requisições

---

**Sistema 100% funcional e pronto para uso! 🎉**

