import { useState, useEffect } from 'react';

const IBGE_API_URL = 'https://servicodados.ibge.gov.br/api/v1/localidades/estados/GO/municipios?orderBy=nome';

export const useMunicipios = () => {
  const [municipios, setMunicipios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const loadMunicipios = async () => {
      try {
        const response = await fetch(IBGE_API_URL);
        const data = await response.json();
        const names = data.map((item) => item.nome);
        setMunicipios(names);
      } catch (err) {
        console.error('Erro ao carregar municípios:', err);
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    loadMunicipios();
  }, []);

  return { municipios, loading, error };
};

