# 🚀 Deploy Backend no Render - 3 CLIQUES!

## ✅ Pré-requisito: CONCLUÍDO
- ✅ Código no GitHub
- ✅ Dockerfile configurado
- ✅ render.yaml configurado

---

## 📋 Faça Agora (3 minutos):

### **1. Acesse Render**
🔗 https://render.com

Clique em **"Get Started for Free"** ou **"Sign In"**

### **2. Login com GitHub**
- Clique em **"GitHub"**
- Autorize o Render

### **3. Criar Novo Serviço**
- Clique em **"New +"** (canto superior direito)
- Selecione **"Web Service"**

### **4. Conectar Repositório**
- Procure por: **"EcoAlerta-GO"**
- Clique em **"Connect"**

### **5. Configurar (IMPORTANTE)**

Preencha assim:

```
Name: ecoalerta-api
Region: Oregon (US West)
Branch: main
Root Directory: backend/EcoAlert.Api
Runtime: Docker
Instance Type: Free
```

**Variáveis de Ambiente:**
Clique em **"Add Environment Variable"** e adicione:

```
ASPNETCORE_URLS = http://+:10000
ASPNETCORE_ENVIRONMENT = Production
Cors__AllowedOrigins__0 = https://frontend-1cqj967q1-andressas-projects-37c54a16.vercel.app
```

### **6. Deploy!**
- Clique em **"Create Web Service"**
- Aguarde 5-10 minutos (primeira vez demora)

---

## 🎯 Depois que o Deploy Terminar:

### **1. Copiar URL do Backend**
Exemplo: `https://ecoalerta-api.onrender.com`

### **2. Atualizar Frontend no Vercel**

Execute:
```bash
cd frontend
vercel env add VITE_API_BASE_URL production
# Cole a URL do Render aqui
# Depois:
vercel --prod
```

---

## ✅ Pronto!

Frontend: https://frontend-1cqj967q1-andressas-projects-37c54a16.vercel.app
Backend: https://ecoalerta-api.onrender.com

**🎉 Projeto 100% ONLINE e GRATUITO!**

---

## ⚠️ Limitações do Plano Free:

- ⏱️ Backend dorme após 15min de inatividade
- 🔄 Primeira request depois disso demora ~30s
- ✅ Perfeito para projeto acadêmico!

---

## 🆘 Problemas?

### Deploy falha?
- Verifique se o Dockerfile está em `backend/EcoAlert.Api/`
- Certifique-se que Root Directory está correto

### Backend não responde?
- Aguarde 30-60 segundos na primeira request (cold start)
- Verifique os logs no dashboard do Render

### CORS error?
- Verifique se adicionou a variável `Cors__AllowedOrigins__0`
- URL deve ser EXATA (sem / no final)

