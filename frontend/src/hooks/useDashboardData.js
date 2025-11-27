import { useEffect, useReducer } from 'react';
import {
  obterEstatisticasPorMunicipio,
  obterQueimadas,
  obterResumoEstatisticas,
} from '../services/api';

const initialState = {
  resumo: null,
  estatisticasMunicipios: [],
  focosPorDia: [],
  status: 'idle',
  error: null,
};

const actionTypes = {
  idle: 'idle',
  loading: 'loading',
  success: 'success',
  error: 'error',
};

const reducer = (state, action) => {
  switch (action.type) {
    case actionTypes.idle:
      return { ...state, status: 'idle', error: null };
    case actionTypes.loading:
      return { ...state, status: 'loading', error: null };
    case actionTypes.success:
      return {
        ...state,
        resumo: action.payload.resumo,
        estatisticasMunicipios: action.payload.estatisticasMunicipios,
        focosPorDia: action.payload.focosPorDia,
        status: 'success',
        error: null,
      };
    case actionTypes.error:
      return { ...state, status: 'error', error: action.payload };
    default:
      return state;
  }
};

const parsePtBrDate = (value) => {
  const [dia, mes, ano] = value.split('/');
  return new Date(Number(ano), Number(mes) - 1, Number(dia));
};

const agruparFocosPorDia = (queimadas = []) => {
  const mapa = new Map();

  queimadas.forEach((queimada) => {
    const data = new Date(queimada.dataHora);
    if (Number.isNaN(data.getTime())) {
      return;
    }

    const label = data.toLocaleDateString('pt-BR');
    mapa.set(label, (mapa.get(label) || 0) + 1);
  });

  return Array.from(mapa.entries())
    .map(([data, total]) => ({ data, total }))
    .sort((a, b) => parsePtBrDate(a.data) - parsePtBrDate(b.data));
};

const limitarMunicipios = (dados = []) => dados.slice(0, 10);

const calcularResumo = (queimadas = []) => {
  if (!queimadas.length) {
    return {
      totalFocos: 0,
      totalMunicipiosAfetados: 0,
      mediaFocosPorDia: 0,
      dataComMaisFocos: null,
      focosNaDataMaxima: 0
    };
  }

  const municipiosUnicos = new Set(queimadas.map(q => q.municipio));
  const focosPorDia = new Map();

  queimadas.forEach(q => {
    const data = new Date(q.dataHora).toLocaleDateString('pt-BR');
    focosPorDia.set(data, (focosPorDia.get(data) || 0) + 1);
  });

  const diasComDados = focosPorDia.size || 1;
  let dataMaxima = null;
  let focosMaximos = 0;

  focosPorDia.forEach((focos, data) => {
    if (focos > focosMaximos) {
      focosMaximos = focos;
      dataMaxima = data;
    }
  });

  return {
    totalFocos: queimadas.length,
    totalMunicipiosAfetados: municipiosUnicos.size,
    mediaFocosPorDia: queimadas.length / diasComDados,
    dataComMaisFocos: dataMaxima,
    focosNaDataMaxima: focosMaximos
  };
};

const calcularEstatisticasPorMunicipio = (queimadas = []) => {
  const contagemPorMunicipio = new Map();

  queimadas.forEach(q => {
    const municipio = q.municipio || 'Desconhecido';
    contagemPorMunicipio.set(municipio, (contagemPorMunicipio.get(municipio) || 0) + 1);
  });

  return Array.from(contagemPorMunicipio.entries())
    .map(([municipio, totalFocos]) => ({ municipio, totalFocos }))
    .sort((a, b) => b.totalFocos - a.totalFocos);
};

const useDashboardData = (filtros) => {
  const [state, dispatch] = useReducer(reducer, initialState);

  useEffect(() => {
    if (!filtros) {
      dispatch({ type: actionTypes.idle });
      return;
    }

    let ativo = true;

    const carregar = async () => {
      console.log('📊 Dashboard: Iniciando carregamento de dados...', filtros);
      const startTime = performance.now();
      
      dispatch({ type: actionTypes.loading });

      try {
        console.log('📡 Dashboard: Fazendo requisições...', {
          dataInicio: filtros.dataInicio,
          dataFim: filtros.dataFim,
          municipio: filtros.municipio || 'Todos os municípios'
        });
        
        // Busca dados com filtro de município direto na API (mesma lógica do Mapa)
        const queimadas = await obterQueimadas(
          filtros.dataInicio, 
          filtros.dataFim, 
          filtros.municipio  // Envia o município para a API filtrar
        );

        console.log('📦 Dados recebidos da API (já filtrados):', {
          total: queimadas.length,
          filtroMunicipio: filtros.municipio || 'Nenhum',
          primeiros3: queimadas.slice(0, 3).map(q => ({ municipio: q.municipio, data: q.dataHora }))
        });

        if (!ativo) {
          return;
        }

        // Calcula estatísticas com base nos dados já filtrados pela API
        const resumoCalculado = calcularResumo(queimadas);
        const estatisticasMunicipiosCalculadas = calcularEstatisticasPorMunicipio(queimadas);
        const focosPorDiaCalculados = agruparFocosPorDia(queimadas);

        console.log('📈 Estatísticas calculadas:', {
          resumo: resumoCalculado,
          municipios: estatisticasMunicipiosCalculadas.length,
          diasComFocos: focosPorDiaCalculados.length
        });

        const endTime = performance.now();
        const tempoDecorrido = ((endTime - startTime) / 1000).toFixed(2);
        console.log(`✅ Dashboard: Dados processados em ${tempoDecorrido}s`);

        // Garante mínimo de 300ms de loading para feedback visual
        const tempoMinimo = 300;
        const tempoRestante = Math.max(0, tempoMinimo - (endTime - startTime));
        
        await new Promise(resolve => setTimeout(resolve, tempoRestante));

        if (!ativo) {
          return;
        }

        dispatch({
          type: actionTypes.success,
          payload: {
            resumo: resumoCalculado,
            estatisticasMunicipios: limitarMunicipios(estatisticasMunicipiosCalculadas),
            focosPorDia: focosPorDiaCalculados,
          },
        });
      } catch (error) {
        if (!ativo) {
          return;
        }

        const endTime = performance.now();
        const tempoDecorrido = ((endTime - startTime) / 1000).toFixed(2);
        console.error(`❌ Dashboard: Erro após ${tempoDecorrido}s:`, error);

        dispatch({
          type: actionTypes.error,
          payload: error.message || 'Erro ao carregar dados do servidor.',
        });
      }
    };

    carregar();

    return () => {
      ativo = false;
    };
  }, [filtros]);

  return state;
};

export default useDashboardData;

