import { parseBrazilianDate } from './dateUtils';

/**
 * Agrupa focos de queimadas por dia
 */
export const groupFocosByDay = (queimadas = []) => {
  const groupMap = new Map();

  queimadas.forEach((queimada) => {
    const date = new Date(queimada.dataHora);
    if (Number.isNaN(date.getTime())) return;

    const dateLabel = date.toLocaleDateString('pt-BR');
    groupMap.set(dateLabel, (groupMap.get(dateLabel) || 0) + 1);
  });

  return Array.from(groupMap.entries())
    .map(([data, total]) => ({ data, total }))
    .sort((a, b) => parseBrazilianDate(a.data) - parseBrazilianDate(b.data));
};

/**
 * Calcula estatísticas agrupadas por município
 */
export const calculateMunicipioStats = (queimadas = []) => {
  const municipioMap = new Map();

  queimadas.forEach((queimada) => {
    const municipio = queimada.municipio || 'Desconhecido';
    municipioMap.set(municipio, (municipioMap.get(municipio) || 0) + 1);
  });

  return Array.from(municipioMap.entries())
    .map(([municipio, totalFocos]) => ({ municipio, totalFocos }))
    .sort((a, b) => b.totalFocos - a.totalFocos);
};

/**
 * Calcula resumo geral das estatísticas
 */
export const calculateSummary = (queimadas = []) => {
  if (!queimadas.length) {
    return {
      totalFocos: 0,
      totalMunicipiosAfetados: 0,
      mediaFocosPorDia: 0,
      dataComMaisFocos: null,
      focosNaDataMaxima: 0,
    };
  }

  const uniqueMunicipios = new Set(queimadas.map((q) => q.municipio));
  const focosByDay = new Map();

  queimadas.forEach((queimada) => {
    const dateLabel = new Date(queimada.dataHora).toLocaleDateString('pt-BR');
    focosByDay.set(dateLabel, (focosByDay.get(dateLabel) || 0) + 1);
  });

  const totalDays = focosByDay.size || 1;
  let maxDate = null;
  let maxFocos = 0;

  focosByDay.forEach((count, date) => {
    if (count > maxFocos) {
      maxFocos = count;
      maxDate = date;
    }
  });

  return {
    totalFocos: queimadas.length,
    totalMunicipiosAfetados: uniqueMunicipios.size,
    mediaFocosPorDia: queimadas.length / totalDays,
    dataComMaisFocos: maxDate,
    focosNaDataMaxima: maxFocos,
  };
};

/**
 * Limita lista de municípios ao top N
 */
export const limitTopMunicipios = (municipios = [], limit = 10) => {
  return municipios.slice(0, limit);
};

