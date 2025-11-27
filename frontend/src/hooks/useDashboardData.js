import { useEffect, useReducer } from 'react';
import { obterQueimadas } from '../services/api';
import { groupFocosByDay, calculateMunicipioStats, calculateSummary, limitTopMunicipios } from '../utils/statisticsCalculator';
import { MIN_LOADING_TIME_MS } from '../constants/appConfig';

const INITIAL_STATE = {
  resumo: null,
  estatisticasMunicipios: [],
  focosPorDia: [],
  status: 'idle',
  error: null,
};

const ACTION_TYPES = {
  IDLE: 'idle',
  LOADING: 'loading',
  SUCCESS: 'success',
  ERROR: 'error',
};

const dashboardReducer = (state, action) => {
  switch (action.type) {
    case ACTION_TYPES.IDLE:
      return { ...state, status: 'idle', error: null };
    case ACTION_TYPES.LOADING:
      return { ...state, status: 'loading', error: null };
    case ACTION_TYPES.SUCCESS:
      return {
        ...state,
        resumo: action.payload.resumo,
        estatisticasMunicipios: action.payload.estatisticasMunicipios,
        focosPorDia: action.payload.focosPorDia,
        status: 'success',
        error: null,
      };
    case ACTION_TYPES.ERROR:
      return { ...state, status: 'error', error: action.payload };
    default:
      return state;
  }
};

const ensureMinimumLoadingTime = async (startTime) => {
  const elapsed = performance.now() - startTime;
  const remaining = Math.max(0, MIN_LOADING_TIME_MS - elapsed);
  if (remaining > 0) {
    await new Promise((resolve) => setTimeout(resolve, remaining));
  }
};

const fetchDashboardData = async (filtros) => {
  const queimadas = await obterQueimadas(
    filtros.dataInicio,
    filtros.dataFim,
    filtros.municipio
  );

  const resumo = calculateSummary(queimadas);
  const estatisticasMunicipios = calculateMunicipioStats(queimadas);
  const focosPorDia = groupFocosByDay(queimadas);

  return {
    resumo,
    estatisticasMunicipios: limitTopMunicipios(estatisticasMunicipios),
    focosPorDia,
  };
};

const useDashboardData = (filtros) => {
  const [state, dispatch] = useReducer(dashboardReducer, INITIAL_STATE);

  useEffect(() => {
    if (!filtros) {
      dispatch({ type: ACTION_TYPES.IDLE });
      return;
    }

    let isActive = true;

    const loadData = async () => {
      const startTime = performance.now();
      dispatch({ type: ACTION_TYPES.LOADING });

      try {
        const data = await fetchDashboardData(filtros);

        await ensureMinimumLoadingTime(startTime);

        if (!isActive) return;

        dispatch({
          type: ACTION_TYPES.SUCCESS,
          payload: data,
        });
      } catch (error) {
        if (!isActive) return;

        dispatch({
          type: ACTION_TYPES.ERROR,
          payload: error.message || 'Erro ao carregar dados do servidor.',
        });
      }
    };

    loadData();

    return () => {
      isActive = false;
    };
  }, [filtros]);

  return state;
};

export default useDashboardData;

