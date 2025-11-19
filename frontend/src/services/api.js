import axios from 'axios';

/**
 * Serviço de API para comunicação com o backend .NET.
 * 
 * Este serviço encapsula todas as chamadas HTTP aos Web Services do backend,
 * seguindo o padrão de separação de responsabilidades.
 * 
 * Base URL configurável - em produção, pode apontar para o backend hospedado.
 */
// Configuração da URL base da API do backend .NET
// Em desenvolvimento, o backend geralmente roda em http://localhost:5285 ou https://localhost:7160
// Para produção, configure a variável de ambiente VITE_API_BASE_URL
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5285';

// Cria instância do axios com configurações padrão
const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 30000, // 30 segundos
});

/**
 * Tratamento global de erros HTTP.
 * Converte erros de rede/HTTP em mensagens amigáveis para o usuário.
 */
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response) {
      // Erro com resposta do servidor
      console.error('Erro na resposta da API:', error.response.data);
      throw new Error(error.response.data?.message || 'Erro ao processar requisição');
    } else if (error.request) {
      // Erro de rede (sem resposta)
      console.error('Erro de rede:', error.request);
      throw new Error('Não foi possível conectar ao servidor. Verifique sua conexão.');
    } else {
      // Outro tipo de erro
      console.error('Erro:', error.message);
      throw new Error(error.message || 'Erro desconhecido');
    }
  }
);

/**
 * Obtém lista de queimadas com filtros opcionais.
 * 
 * @param {Date|null} dataInicio - Data inicial do período
 * @param {Date|null} dataFim - Data final do período
 * @param {string|null} municipio - Nome do município para filtrar
 * @returns {Promise<Array>} Lista de queimadas
 */
export const obterQueimadas = async (dataInicio = null, dataFim = null, municipio = null) => {
  try {
    const params = new URLSearchParams();
    
    if (dataInicio) {
      params.append('dataInicio', dataInicio.toISOString().split('T')[0]);
    }
    if (dataFim) {
      params.append('dataFim', dataFim.toISOString().split('T')[0]);
    }
    if (municipio) {
      params.append('municipio', municipio);
    }

    const response = await apiClient.get(`/api/queimadas?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Erro ao obter queimadas:', error);
    throw error;
  }
};

/**
 * Obtém estatísticas de focos agrupados por município.
 * 
 * @param {Date|null} dataInicio - Data inicial do período
 * @param {Date|null} dataFim - Data final do período
 * @returns {Promise<Array>} Lista de estatísticas por município
 */
export const obterEstatisticasPorMunicipio = async (dataInicio = null, dataFim = null) => {
  try {
    const params = new URLSearchParams();
    
    if (dataInicio) {
      params.append('dataInicio', dataInicio.toISOString().split('T')[0]);
    }
    if (dataFim) {
      params.append('dataFim', dataFim.toISOString().split('T')[0]);
    }

    const response = await apiClient.get(`/api/queimadas/estatisticas/municipios?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Erro ao obter estatísticas por município:', error);
    throw error;
  }
};

/**
 * Obtém resumo geral das estatísticas de queimadas.
 * 
 * @param {Date|null} dataInicio - Data inicial do período
 * @param {Date|null} dataFim - Data final do período
 * @returns {Promise<Object>} Resumo das estatísticas
 */
export const obterResumoEstatisticas = async (dataInicio = null, dataFim = null) => {
  try {
    const params = new URLSearchParams();
    
    if (dataInicio) {
      params.append('dataInicio', dataInicio.toISOString().split('T')[0]);
    }
    if (dataFim) {
      params.append('dataFim', dataFim.toISOString().split('T')[0]);
    }

    const response = await apiClient.get(`/api/queimadas/estatisticas/resumo?${params.toString()}`);
    return response.data;
  } catch (error) {
    console.error('Erro ao obter resumo de estatísticas:', error);
    throw error;
  }
};

