/**
 * Componente de Filtros para o sistema de monitoramento de queimadas.
 * 
 * Permite ao usuário filtrar os dados por:
 * - Intervalo de datas (data início e data fim)
 * - Município (opcional)
 * 
 * Quando os filtros são alterados, notifica o componente pai através de callbacks.
 */
import { useState, useEffect } from 'react';
import './Filtros.css';

function Filtros({ onFiltrosChange, municipios = [] }) {
  // Estado para controlar os valores dos filtros
  const [dataInicio, setDataInicio] = useState('');
  const [dataFim, setDataFim] = useState('');
  const [municipio, setMunicipio] = useState('');

  // Define valores padrão: últimos 30 dias
  useEffect(() => {
    const hoje = new Date();
    const trintaDiasAtras = new Date();
    trintaDiasAtras.setDate(hoje.getDate() - 30);

    const dataInicioStr = trintaDiasAtras.toISOString().split('T')[0];
    const dataFimStr = hoje.toISOString().split('T')[0];

    setDataInicio(dataInicioStr);
    setDataFim(dataFimStr);

    // Notifica o componente pai com os valores iniciais
    const filtrosIniciais = {
      dataInicio: trintaDiasAtras,
      dataFim: hoje,
      municipio: null
    };
    
    console.log('Filtros: Definindo valores iniciais:', filtrosIniciais);
    
    // Garante que o callback seja chamado
    if (onFiltrosChange) {
      onFiltrosChange(filtrosIniciais);
    } else {
      console.error('onFiltrosChange não está definido!');
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Função chamada quando qualquer filtro é alterado
  const handleChange = () => {
    const filtros = {
      dataInicio: dataInicio ? new Date(dataInicio) : null,
      dataFim: dataFim ? new Date(dataFim) : null,
      municipio: municipio || null
    };

    // Validação básica: data início não pode ser maior que data fim
    if (filtros.dataInicio && filtros.dataFim && filtros.dataInicio > filtros.dataFim) {
      alert('A data de início deve ser anterior à data de fim');
      return;
    }

    // Notifica o componente pai sobre a mudança nos filtros
    onFiltrosChange(filtros);
  };

  return (
    <div className="filtros-container">
      <h3>Filtros de Pesquisa</h3>
      <div className="filtros-grid">
        <div className="filtro-item">
          <label htmlFor="dataInicio">Data Início:</label>
          <input
            type="date"
            id="dataInicio"
            value={dataInicio}
            onChange={(e) => {
              setDataInicio(e.target.value);
              handleChange();
            }}
          />
        </div>

        <div className="filtro-item">
          <label htmlFor="dataFim">Data Fim:</label>
          <input
            type="date"
            id="dataFim"
            value={dataFim}
            onChange={(e) => {
              setDataFim(e.target.value);
              handleChange();
            }}
          />
        </div>

        <div className="filtro-item">
          <label htmlFor="municipio">Município (opcional):</label>
          <input
            type="text"
            id="municipio"
            value={municipio}
            onChange={(e) => {
              setMunicipio(e.target.value);
              handleChange();
            }}
            placeholder="Digite o nome do município"
            list="municipios-list"
          />
          {municipios.length > 0 && (
            <datalist id="municipios-list">
              {municipios.map((mun, index) => (
                <option key={index} value={mun} />
              ))}
            </datalist>
          )}
        </div>
      </div>
    </div>
  );
}

export default Filtros;

