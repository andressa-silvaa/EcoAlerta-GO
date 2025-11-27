/**
 * Valida se data de início é anterior à data de fim
 */
export const validateDateRange = (startDate, endDate) => {
  if (!startDate || !endDate) return { isValid: true };

  const start = new Date(startDate);
  const end = new Date(endDate);

  if (start > end) {
    return {
      isValid: false,
      error: 'A data de início deve ser anterior à data de fim',
    };
  }

  return { isValid: true };
};

/**
 * Sanitiza nome de município (remove caracteres inválidos)
 */
export const sanitizeMunicipioName = (name) => {
  if (!name) return null;
  const trimmed = name.trim();
  return trimmed || null;
};

