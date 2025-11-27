import { useState, useEffect, useRef, useCallback } from 'react';
import { useDebounce } from '../hooks/useDebounce';
import './Filtros.css';

const criarIntervaloPadrao = () => {
  const hoje = new Date();
  const inicio = new Date();
  inicio.setDate(hoje.getDate() - 30);

  return {
    dataInicio: inicio.toISOString().split('T')[0],
    dataFim: hoje.toISOString().split('T')[0],
    municipio: '',
  };
};

function Filtros({ onFiltrosChange }) {
  const [inputs, setInputs] = useState(criarIntervaloPadrao);
  const [erro, setErro] = useState('');
  const [municipios, setMunicipios] = useState([]);
  const [municipiosFiltrados, setMunicipiosFiltrados] = useState([]);
  const [mostrarDropdown, setMostrarDropdown] = useState(false);
  const [buscaMunicipio, setBuscaMunicipio] = useState('');
  const [aguardandoDebounce, setAguardandoDebounce] = useState(false);
  const dropdownRef = useRef(null);
  
  // Debounce para evitar múltiplas requisições ao ajustar datas
  const debouncedInputs = useDebounce(inputs, 800);

  // Busca municípios de Goiás da API do IBGE
  useEffect(() => {
    const carregarMunicipios = async () => {
      try {
        const response = await fetch(
          'https://servicodados.ibge.gov.br/api/v1/localidades/estados/GO/municipios?orderBy=nome'
        );
        const dados = await response.json();
        const nomes = dados.map(m => m.nome);
        setMunicipios(nomes);
        setMunicipiosFiltrados(nomes);
      } catch (error) {
        console.error('Erro ao carregar municípios:', error);
      }
    };

    carregarMunicipios();
  }, []);

  // Carrega dados iniciais
  useEffect(() => {
    if (onFiltrosChange) {
      aplicarFiltros();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Monitora mudanças nos inputs para mostrar indicador de debounce
  useEffect(() => {
    // Se inputs mudaram mas debounced ainda não, está aguardando
    const inputsStr = JSON.stringify(inputs);
    const debouncedStr = JSON.stringify(debouncedInputs);
    
    if (inputsStr !== debouncedStr) {
      setAguardandoDebounce(true);
    } else {
      setAguardandoDebounce(false);
    }
  }, [inputs, debouncedInputs]);

  // Aplica filtros automaticamente quando debounce terminar
  useEffect(() => {
    if (onFiltrosChange) {
      aplicarFiltros(debouncedInputs);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedInputs]);

  // Fecha dropdown ao clicar fora
  useEffect(() => {
    const handleClickFora = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setMostrarDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickFora);
    return () => document.removeEventListener('mousedown', handleClickFora);
  }, []);

  const aplicarFiltros = useCallback((novosInputs = inputs) => {
    const dataInicio = novosInputs.dataInicio ? new Date(novosInputs.dataInicio) : null;
    const dataFim = novosInputs.dataFim ? new Date(novosInputs.dataFim) : null;

    if (dataInicio && dataFim && dataInicio > dataFim) {
      setErro('A data de início deve ser anterior à data de fim');
      return;
    }

    setErro('');
    onFiltrosChange({
      dataInicio,
      dataFim,
      municipio: novosInputs.municipio.trim() || null,
    });
  }, [onFiltrosChange]);

  const handleDataChange = useCallback((event) => {
    const { name, value } = event.target;
    setInputs(prev => ({ ...prev, [name]: value }));
    setErro('');
    
    // Não aplica imediatamente - será aplicado via debounce
  }, []);

  const handleBuscaMunicipioChange = useCallback((event) => {
    const busca = event.target.value;
    setBuscaMunicipio(busca);
    setMostrarDropdown(true);

    if (!busca.trim()) {
      setMunicipiosFiltrados(municipios);
    } else {
      const filtrados = municipios.filter(m =>
        m.toLowerCase().includes(busca.toLowerCase())
      );
      setMunicipiosFiltrados(filtrados);
    }
  }, [municipios]);

  const handleSelecionarMunicipio = useCallback((municipio) => {
    const novosInputs = { ...inputs, municipio };
    setInputs(novosInputs);
    setBuscaMunicipio(municipio);
    setMostrarDropdown(false);
    
    // Aplica filtros instantaneamente ao selecionar município
    const dataInicio = inputs.dataInicio ? new Date(inputs.dataInicio) : null;
    const dataFim = inputs.dataFim ? new Date(inputs.dataFim) : null;

    onFiltrosChange({
      dataInicio,
      dataFim,
      municipio: municipio.trim(),
    });
  }, [inputs, onFiltrosChange]);

  const handleLimparMunicipio = useCallback(() => {
    setInputs((prev) => ({ ...prev, municipio: '' }));
    setBuscaMunicipio('');
    setMunicipiosFiltrados(municipios);
    
    // Aplica filtros instantaneamente sem município
    const dataInicio = inputs.dataInicio ? new Date(inputs.dataInicio) : null;
    const dataFim = inputs.dataFim ? new Date(inputs.dataFim) : null;

    onFiltrosChange({
      dataInicio,
      dataFim,
      municipio: null,
    });
  }, [inputs, municipios, onFiltrosChange]);

  return (
    <div className="filtros-container">
      <div className="filtros-header">
        <h3>Filtros de Pesquisa</h3>
        <span className="filtros-icone">⚙</span>
      </div>

      <div className="filtros-grid">
        <div className="filtro-item">
          <label htmlFor="dataInicio">
            <span className="label-icone">📅</span>
            Data Início
          </label>
          <input
            type="date"
            id="dataInicio"
            name="dataInicio"
            value={inputs.dataInicio}
            onChange={handleDataChange}
            className="filtro-input"
          />
        </div>

        <div className="filtro-item">
          <label htmlFor="dataFim">
            <span className="label-icone">📅</span>
            Data Fim
          </label>
          <input
            type="date"
            id="dataFim"
            name="dataFim"
            value={inputs.dataFim}
            onChange={handleDataChange}
            className="filtro-input"
          />
        </div>

        <div className="filtro-item municipio-select" ref={dropdownRef}>
          <label htmlFor="municipio">
            <span className="label-icone">📍</span>
            Município <span className="opcional">(opcional)</span>
          </label>
          <div className="input-wrapper">
            <input
              type="text"
              id="municipio"
              name="municipio"
              value={buscaMunicipio}
              onChange={handleBuscaMunicipioChange}
              onFocus={() => setMostrarDropdown(true)}
              placeholder="Selecione um município..."
              className="filtro-input municipio-input"
              autoComplete="off"
            />
            {buscaMunicipio && (
              <button
                type="button"
                className="btn-limpar-input"
                onClick={handleLimparMunicipio}
                title="Limpar município"
              >
                ✕
              </button>
            )}
          </div>

          {mostrarDropdown && municipiosFiltrados.length > 0 && (
            <ul className="municipios-dropdown">
              {municipiosFiltrados.slice(0, 10).map((mun) => (
                <li
                  key={mun}
                  onClick={() => handleSelecionarMunicipio(mun)}
                  className={inputs.municipio === mun ? 'selecionado' : ''}
                >
                  {mun}
                </li>
              ))}
              {municipiosFiltrados.length > 10 && (
                <li className="dropdown-info">
                  + {municipiosFiltrados.length - 10} municípios...
                </li>
              )}
            </ul>
          )}
        </div>
      </div>

      {erro && (
        <div className="filtro-alerta">
          <span className="alerta-icone">⚠</span>
          {erro}
        </div>
      )}

      {aguardandoDebounce && (
        <div className="filtro-aguardando">
          <span className="aguardando-spinner"></span>
          <span>Aguardando conclusão dos ajustes...</span>
        </div>
      )}
    </div>
  );
}

export default Filtros;