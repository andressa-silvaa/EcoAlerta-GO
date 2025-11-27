import { useState, useCallback } from 'react';
import Filtros from '../components/Filtros';
import EstatisticasCard from '../components/EstatisticasCard';
import LineChart from '../components/charts/LineChart';
import BarChart from '../components/charts/BarChart';
import LoadingBanner from '../components/LoadingBanner';
import ErrorMessage from '../components/ErrorMessage';
import EmptyState from '../components/EmptyState';
import useDashboardData from '../hooks/useDashboardData';
import { formatNumber } from '../utils/dateUtils';
import { CHART_COLORS } from '../constants/appConfig';
import './Dashboard.css';

const StatisticsCards = ({ resumo }) => (
  <div className="cards-grid">
    <EstatisticasCard
      titulo="Total de Focos"
      valor={formatNumber(resumo.totalFocos)}
      icone="🔥"
      cor={CHART_COLORS.PRIMARY}
    />
    <EstatisticasCard
      titulo="Municípios Afetados"
      valor={resumo.totalMunicipiosAfetados}
      icone="📍"
      cor={CHART_COLORS.SECONDARY}
    />
    <EstatisticasCard
      titulo="Média por Dia"
      valor={formatNumber(resumo.mediaFocosPorDia, 1)}
      icone="📊"
      cor={CHART_COLORS.TERTIARY}
    />
    {resumo.dataComMaisFocos && (
      <EstatisticasCard
        titulo="Dia com Mais Focos"
        valor={resumo.focosNaDataMaxima}
        icone="⚠️"
        cor={CHART_COLORS.QUATERNARY}
      />
    )}
  </div>
);

const ChartsSection = ({ focosPorDia, estatisticasMunicipios }) => (
  <div className="graficos-container">
    <LineChart data={focosPorDia} />
    <BarChart data={estatisticasMunicipios} />
  </div>
);

const DashboardContent = ({ resumo, focosPorDia, estatisticasMunicipios, status }) => {
  if (resumo === null || status !== 'success') return null;

  if (resumo.totalFocos === 0) {
    return (
      <EmptyState
        icon="📊"
        title="Nenhum dado encontrado"
        message="Não há focos de queimadas registrados para os filtros selecionados."
      />
    );
  }

  return (
    <div style={{ opacity: status === 'loading' ? 0.5 : 1, transition: 'opacity 0.3s ease' }}>
      <StatisticsCards resumo={resumo} />
      <ChartsSection focosPorDia={focosPorDia} estatisticasMunicipios={estatisticasMunicipios} />
    </div>
  );
};

function Dashboard() {
  const [filtros, setFiltros] = useState(null);
  const handleFiltrosChange = useCallback((novosFiltros) => {
    setFiltros(novosFiltros);
  }, []);

  const { resumo, estatisticasMunicipios, focosPorDia, status, error } = useDashboardData(filtros);

  return (
    <div className="dashboard-container">
      <h1>Monitoramento de Queimadas em Goiás</h1>
      <p className="subtitulo">Sistema de gestão ambiental para monitoramento de focos de queimadas</p>

      <Filtros onFiltrosChange={handleFiltrosChange} />

      {status === 'loading' && (
        <LoadingBanner message="Carregando estatísticas de queimadas... Aguarde alguns segundos." />
      )}

      {error && <ErrorMessage error={error} />}

      <DashboardContent
        resumo={resumo}
        focosPorDia={focosPorDia}
        estatisticasMunicipios={estatisticasMunicipios}
        status={status}
      />
    </div>
  );
}

export default Dashboard;

