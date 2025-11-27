import axios from 'axios';
import { apiCache } from '../utils/cache';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5285';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 120000, // Aumentado para 120 segundos (2 minutos)
});

// Interceptor de requisição para log
apiClient.interceptors.request.use(
  (config) => {
    console.log(`🚀 Requisição iniciada: ${config.method?.toUpperCase()} ${config.url}`);
    config.metadata = { startTime: new Date() };
    return config;
  },
  (error) => {
    console.error('❌ Erro ao iniciar requisição:', error);
    return Promise.reject(error);
  }
);

// Interceptor de resposta para log e tratamento de erros
apiClient.interceptors.response.use(
  (response) => {
    const duration = new Date() - response.config.metadata.startTime;
    console.log(`✅ Resposta recebida: ${response.status} em ${duration}ms - ${response.config.url}`);
    return response;
  },
  (error) => {
    const duration = error.config?.metadata?.startTime 
      ? new Date() - error.config.metadata.startTime 
      : 0;
    
    console.error('❌ Erro na requisição:', {
      url: error.config?.url,
      status: error.response?.status,
      data: error.response?.data,
      message: error.message,
      code: error.code,
      duration: `${duration}ms`
    });

    if (error.response) {
      const message = error.response.data?.message || 'Erro ao processar requisição';
      throw new Error(message);
    }

    if (error.request) {
      if (error.code === 'ECONNABORTED') {
        throw new Error('A requisição demorou muito. O servidor pode estar sobrecarregado.');
      }
      throw new Error('Não foi possível conectar ao servidor. Verifique sua conexão.');
    }

    throw new Error(error.message || 'Erro desconhecido');
  }
);

const toIsoDate = (value) => {
  if (!value) return null;
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? null : date.toISOString().split('T')[0];
};

const normalizeFilters = (dataInicio, dataFim, municipio) => ({
  dataInicio: toIsoDate(dataInicio),
  dataFim: toIsoDate(dataFim),
  municipio: municipio?.trim() || null,
});

const buildQuery = (filters = {}) => {
  const search = new URLSearchParams();

  Object.entries(filters).forEach(([key, value]) => {
    if (value) {
      search.append(key, value);
    }
  });

  const query = search.toString();
  return query ? `?${query}` : '';
};

const get = async (path, filters) => {
  const query = buildQuery(filters);
  const fullPath = `${path}${query}`;
  
  // Verifica cache primeiro
  const cacheKey = apiCache.generateKey(path, filters);
  const cachedData = apiCache.get(cacheKey);
  
  if (cachedData) {
    return cachedData;
  }
  
  // Se não estiver em cache, busca da API
  const { data } = await apiClient.get(fullPath);
  
  // Armazena no cache
  apiCache.set(cacheKey, data);
  
  return data;
};

export const obterQueimadas = (dataInicio = null, dataFim = null, municipio = null) =>
  get('/api/queimadas', normalizeFilters(dataInicio, dataFim, municipio));

export const obterEstatisticasPorMunicipio = (dataInicio = null, dataFim = null) =>
  get('/api/queimadas/estatisticas/municipios', normalizeFilters(dataInicio, dataFim));

export const obterResumoEstatisticas = (dataInicio = null, dataFim = null) =>
  get('/api/queimadas/estatisticas/resumo', normalizeFilters(dataInicio, dataFim));

// Função para limpar o cache manualmente se necessário
export const limparCache = () => {
  apiCache.clear();
  console.log('🗑️ Cache limpo com sucesso');
};

// Limpa o cache ao carregar a aplicação (para garantir dados frescos após mudanças)
if (typeof window !== 'undefined') {
  // Limpa cache uma vez ao carregar a página
  const ultimaLimpeza = sessionStorage.getItem('cache_limpo');
  if (!ultimaLimpeza) {
    console.log('🔄 Primeira carga - limpando cache');
    apiCache.clear();
    sessionStorage.setItem('cache_limpo', Date.now().toString());
  }
}

