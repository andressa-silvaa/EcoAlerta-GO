import { useState, useEffect, useRef, useCallback } from 'react';
import { useDebounce } from '../hooks/useDebounce';
import { useMunicipios } from '../hooks/useMunicipios';
import { createDefaultDateRange } from '../utils/dateUtils';
import { validateDateRange } from '../utils/validation';
import { DEBOUNCE_DELAY_MS, DEFAULT_DATE_RANGE_DAYS } from '../constants/appConfig';
import './Filtros.css';

const MAX_DROPDOWN_ITEMS = 10;

const createDefaultFilters = () => {
  const { start, end } = createDefaultDateRange(DEFAULT_DATE_RANGE_DAYS);
  return {
    dataInicio: start,
    dataFim: end,
    municipio: '',
  };
};

function Filtros({ onFiltrosChange }) {
  const [inputs, setInputs] = useState(createDefaultFilters);
  const [erro, setErro] = useState('');
  const [municipiosFiltrados, setMunicipiosFiltrados] = useState([]);
  const [mostrarDropdown, setMostrarDropdown] = useState(false);
  const [buscaMunicipio, setBuscaMunicipio] = useState('');
  const [aguardandoDebounce, setAguardandoDebounce] = useState(false);
  const dropdownRef = useRef(null);

  const { municipios } = useMunicipios();
  const debouncedInputs = useDebounce(inputs, DEBOUNCE_DELAY_MS);

  useEffect(() => {
    setMunicipiosFiltrados(municipios);
  }, [municipios]);

  useEffect(() => {
    if (onFiltrosChange) {
      applyFilters(inputs);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    const isWaitingForDebounce = JSON.stringify(inputs) !== JSON.stringify(debouncedInputs);
    setAguardandoDebounce(isWaitingForDebounce);
  }, [inputs, debouncedInputs]);

  useEffect(() => {
    if (onFiltrosChange) {
      applyFilters(debouncedInputs);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedInputs]);

  useEffect(() => {
    const handleClickOutside = (event) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target)) {
        setMostrarDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const applyFilters = useCallback((filterInputs = inputs) => {
    const dataInicio = filterInputs.dataInicio ? new Date(filterInputs.dataInicio) : null;
    const dataFim = filterInputs.dataFim ? new Date(filterInputs.dataFim) : null;

    const validation = validateDateRange(dataInicio, dataFim);
    if (!validation.isValid) {
      setErro(validation.error);
      return;
    }

    setErro('');
    onFiltrosChange({
      dataInicio,
      dataFim,
      municipio: filterInputs.municipio.trim() || null,
    });
  }, [onFiltrosChange, inputs]);

  const handleDateChange = useCallback((event) => {
    const { name, value } = event.target;
    setInputs((prev) => ({ ...prev, [name]: value }));
    setErro('');
  }, []);

  const filterMunicipios = useCallback((searchTerm) => {
    if (!searchTerm.trim()) {
      return municipios;
    }
    return municipios.filter((m) =>
      m.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [municipios]);

  const handleSearchChange = useCallback((event) => {
    const searchValue = event.target.value;
    setBuscaMunicipio(searchValue);
    setMostrarDropdown(true);
    setMunicipiosFiltrados(filterMunicipios(searchValue));
  }, [filterMunicipios]);

  const applyFiltersWithMunicipio = useCallback((municipio) => {
    const dataInicio = inputs.dataInicio ? new Date(inputs.dataInicio) : null;
    const dataFim = inputs.dataFim ? new Date(inputs.dataFim) : null;

    onFiltrosChange({
      dataInicio,
      dataFim,
      municipio: municipio?.trim() || null,
    });
  }, [inputs, onFiltrosChange]);

  const handleSelectMunicipio = useCallback((municipio) => {
    setInputs((prev) => ({ ...prev, municipio }));
    setBuscaMunicipio(municipio);
    setMostrarDropdown(false);
    applyFiltersWithMunicipio(municipio);
  }, [applyFiltersWithMunicipio]);

  const handleClearMunicipio = useCallback(() => {
    setInputs((prev) => ({ ...prev, municipio: '' }));
    setBuscaMunicipio('');
    setMunicipiosFiltrados(municipios);
    applyFiltersWithMunicipio(null);
  }, [municipios, applyFiltersWithMunicipio]);

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
            onChange={handleDateChange}
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
            onChange={handleDateChange}
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
              onChange={handleSearchChange}
              onFocus={() => setMostrarDropdown(true)}
              placeholder="Selecione um município..."
              className="filtro-input municipio-input"
              autoComplete="off"
            />
            {buscaMunicipio && (
              <button
                type="button"
                className="btn-limpar-input"
                onClick={handleClearMunicipio}
                title="Limpar município"
              >
                ✕
              </button>
            )}
          </div>

          {mostrarDropdown && municipiosFiltrados.length > 0 && (
            <ul className="municipios-dropdown">
              {municipiosFiltrados.slice(0, MAX_DROPDOWN_ITEMS).map((mun) => (
                <li
                  key={mun}
                  onClick={() => handleSelectMunicipio(mun)}
                  className={inputs.municipio === mun ? 'selecionado' : ''}
                >
                  {mun}
                </li>
              ))}
              {municipiosFiltrados.length > MAX_DROPDOWN_ITEMS && (
                <li className="dropdown-info">
                  + {municipiosFiltrados.length - MAX_DROPDOWN_ITEMS} municípios...
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