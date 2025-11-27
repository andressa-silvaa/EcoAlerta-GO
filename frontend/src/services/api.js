import axios from 'axios';
import { apiCache } from '../utils/cache';
import { toIsoDate } from '../utils/dateUtils';
import { API_BASE_URL } from '../constants/appConfig';

const REQUEST_TIMEOUT_MS = 120000;

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: REQUEST_TIMEOUT_MS,
});

const setupRequestInterceptor = () => {
  apiClient.interceptors.request.use(
    (config) => {
      console.log(`🚀 Request: ${config.method?.toUpperCase()} ${config.url}`);
      config.metadata = { startTime: new Date() };
      return config;
    },
    (error) => {
      console.error('❌ Request error:', error);
      return Promise.reject(error);
    }
  );
};

const setupResponseInterceptor = () => {
  apiClient.interceptors.response.use(
    (response) => {
      const duration = new Date() - response.config.metadata.startTime;
      console.log(`✅ Response: ${response.status} in ${duration}ms`);
      return response;
    },
    (error) => {
      handleRequestError(error);
    }
  );
};

const handleRequestError = (error) => {
  const duration = error.config?.metadata?.startTime
    ? new Date() - error.config.metadata.startTime
    : 0;

  console.error('❌ Request failed:', {
    url: error.config?.url,
    status: error.response?.status,
    duration: `${duration}ms`,
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
};

const normalizeFilters = (dataInicio, dataFim, municipio) => ({
  dataInicio: toIsoDate(dataInicio),
  dataFim: toIsoDate(dataFim),
  municipio: municipio?.trim() || null,
});

const buildQueryString = (filters = {}) => {
  const params = new URLSearchParams();
  Object.entries(filters).forEach(([key, value]) => {
    if (value) params.append(key, value);
  });
  return params.toString() ? `?${params.toString()}` : '';
};

const fetchWithCache = async (path, filters) => {
  const cacheKey = apiCache.generateKey(path, filters);
  const cachedData = apiCache.get(cacheKey);

  if (cachedData) {
    return cachedData;
  }

  const query = buildQueryString(filters);
  const { data } = await apiClient.get(`${path}${query}`);

  apiCache.set(cacheKey, data);
  return data;
};

const clearCacheOnFirstLoad = () => {
  if (typeof window === 'undefined') return;

  const cacheKey = 'cache_limpo';
  const lastCleared = sessionStorage.getItem(cacheKey);

  if (!lastCleared) {
    console.log('🔄 First load - clearing cache');
    apiCache.clear();
    sessionStorage.setItem(cacheKey, Date.now().toString());
  }
};

setupRequestInterceptor();
setupResponseInterceptor();
clearCacheOnFirstLoad();

export const obterQueimadas = (dataInicio = null, dataFim = null, municipio = null) =>
  fetchWithCache('/api/queimadas', normalizeFilters(dataInicio, dataFim, municipio));

export const obterEstatisticasPorMunicipio = (dataInicio = null, dataFim = null) =>
  fetchWithCache('/api/queimadas/estatisticas/municipios', normalizeFilters(dataInicio, dataFim));

export const obterResumoEstatisticas = (dataInicio = null, dataFim = null) =>
  fetchWithCache('/api/queimadas/estatisticas/resumo', normalizeFilters(dataInicio, dataFim));

export const limparCache = () => {
  apiCache.clear();
  console.log('🗑️ Cache cleared');
};

