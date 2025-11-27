import { useState, useCallback } from 'react';
import { obterQueimadas } from '../services/api';
import { MIN_LOADING_TIME_MS } from '../constants/appConfig';

const ensureMinimumLoadingTime = async (startTime) => {
  const elapsed = performance.now() - startTime;
  const remaining = Math.max(0, MIN_LOADING_TIME_MS - elapsed);
  if (remaining > 0) {
    await new Promise((resolve) => setTimeout(resolve, remaining));
  }
};

export const useMapData = () => {
  const [queimadas, setQueimadas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const loadData = useCallback(async (filtros) => {
    if (!filtros) return;

    const startTime = performance.now();
    setLoading(true);
    setError(null);

    try {
      const data = await obterQueimadas(
        filtros.dataInicio,
        filtros.dataFim,
        filtros.municipio
      );

      await ensureMinimumLoadingTime(startTime);
      setQueimadas(data);
    } catch (err) {
      setError(err.message || 'Erro ao carregar dados do servidor');
    } finally {
      setLoading(false);
    }
  }, []);

  return { queimadas, loading, error, loadData };
};

