import { useEffect, useState } from 'react';

/**
 * Hook para fazer debounce de valores
 * Útil para evitar múltiplas chamadas de API enquanto o usuário digita/ajusta filtros
 */
export function useDebounce(value, delay = 500) {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

