export const toIsoDate = (date) => {
  if (!date) return null;
  const dateObj = date instanceof Date ? date : new Date(date);
  return Number.isNaN(dateObj.getTime()) ? null : dateObj.toISOString().split('T')[0];
};

export const parseBrazilianDate = (dateString) => {
  const [day, month, year] = dateString.split('/').map(Number);
  return new Date(year, month - 1, day);
};

export const createDefaultDateRange = (daysBack = 30) => {
  const today = new Date();
  const startDate = new Date();
  startDate.setDate(today.getDate() - daysBack);

  return {
    start: toIsoDate(startDate),
    end: toIsoDate(today),
  };
};

export const formatBrazilianDateTime = (date) => {
  return new Date(date).toLocaleString('pt-BR');
};

export const formatNumber = (number, decimals = 0) => {
  return number.toLocaleString('pt-BR', {
    minimumFractionDigits: decimals,
    maximumFractionDigits: decimals,
  });
};

