/**
 * Converte Date para string ISO (YYYY-MM-DD)
 */
export const toIsoDate = (date) => {
  if (!date) return null;
  const dateObj = date instanceof Date ? date : new Date(date);
  return Number.isNaN(dateObj.getTime()) ? null : dateObj.toISOString().split('T')[0];
};

/**
 * Converte string no formato DD/MM/YYYY para objeto Date
 */
export const parseBrazilianDate = (dateString) => {
  const [day, month, year] = dateString.split('/').map(Number);
  return new Date(year, month - 1, day);
};

/**
 * Cria intervalo de datas padrão (hoje - N dias até hoje)
 */
export const createDefaultDateRange = (daysBack = 30) => {
  const today = new Date();
  const startDate = new Date();
  startDate.setDate(today.getDate() - daysBack);

  return {
    start: toIsoDate(startDate),
    end: toIsoDate(today),
  };
};

/**
 * Formata Date para exibição em português (DD/MM/YYYY HH:mm)
 */
export const formatBrazilianDateTime = (date) => {
  return new Date(date).toLocaleString('pt-BR');
};

/**
 * Formata número para exibição em português
 */
export const formatNumber = (number, decimals = 0) => {
  return number.toLocaleString('pt-BR', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  });
};

