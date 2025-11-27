import { useState, useMemo, useCallback, memo } from 'react';
import Filtros from '../components/Filtros';
import EstatisticasCard from '../components/EstatisticasCard';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
  LineChart,
  Line,
} from 'recharts';
import useDashboardData from '../hooks/useDashboardData';
import './Dashboard.css';

// Componente de gráfico de linha memoizado
const GraficoLinha = memo(({ data }) => (
  <div className="grafico-card">
    <h3>Focos por Dia</h3>
    <ResponsiveContainer width="100%" height={300}>
      <LineChart data={data}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="data" />
        <YAxis />
        <Tooltip />
        <Legend />
        <Line type="monotone" dataKey="total" stroke="#e74c3c" strokeWidth={3} name="Focos" />
      </LineChart>
    </ResponsiveContainer>
  </div>
));

// Componente de gráfico de barras memoizado
const GraficoBarras = memo(({ data, eixoMunicipioAltura, legendaOffset, graficoAltura }) => {
  const quantidade = data.length;
  const titulo = quantidade === 1 
    ? 'Município com Focos de Queimadas' 
    : `Top ${Math.min(quantidade, 10)} Municípios com Mais Focos`;
  
  return (
    <div className="grafico-card">
      <h3>{titulo}</h3>
      <ResponsiveContainer width="100%" height={graficoAltura}>
        <BarChart
          data={data}
          margin={{ top: 5, right: 30, left: 10, bottom: 20 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="municipio"
            angle={-45}
            textAnchor="end"
            interval={0}
            height={eixoMunicipioAltura}
          />
          <YAxis />
          <Tooltip />
          <Legend
            verticalAlign="bottom"
            wrapperStyle={{ paddingTop: legendaOffset }}
          />
          <Bar dataKey="totalFocos" fill="#e67e22" name="Total de Focos" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
});

function Dashboard() {
  const [filtros, setFiltros] = useState(null);
  const handleFiltrosChange = useCallback((novosFiltros) => {
    setFiltros(novosFiltros);
  }, []);

  const { resumo, estatisticasMunicipios, focosPorDia, status, error } = useDashboardData(filtros);

  const { eixoMunicipioAltura, legendaOffset, graficoAltura } = useMemo(() => {
    const longest = estatisticasMunicipios.reduce(
      (max, municipio) => Math.max(max, (municipio.municipio || '').length),
      0
    );

    const eixoBase = Math.min(Math.max(longest * 6, 80), 220);
    const legenda = Math.min(Math.max(longest * 0.8, 12), 80);
    const graficoBase = 260;
    const eixoExtra = Math.max(0, eixoBase - 100);
    const altura = Math.min(graficoBase + eixoExtra + legenda, 420);

    return {
      eixoMunicipioAltura: eixoBase,
      legendaOffset: legenda,
      graficoAltura: altura,
    };
  }, [estatisticasMunicipios]);

  return (
    <div className="dashboard-container">
      <h1>Monitoramento de Queimadas em Goiás</h1>
      <p className="subtitulo">Sistema de gestão ambiental para monitoramento de focos de queimadas</p>

      <Filtros onFiltrosChange={handleFiltrosChange} />

      {status === 'loading' && (
        <div className="loading-banner">
          <div className="loading-spinner"></div>
          <span>Carregando estatísticas de queimadas... Aguarde alguns segundos.</span>
        </div>
      )}

      {error && (
        <div className="error-message">
          <div>
            <strong>Erro ao carregar dados:</strong> {error}
            <br />
            <small style={{ marginTop: '10px', display: 'block' }}>
              Verifique se o backend está rodando em {import.meta.env.VITE_API_BASE_URL || 'http://localhost:5285'}
            </small>
          </div>
        </div>
      )}

      {resumo !== null && status === 'success' && (
        <div 
          style={{ 
            opacity: status === 'loading' ? 0.5 : 1,
            transition: 'opacity 0.3s ease'
          }}
        >
          {resumo.totalFocos > 0 ? (
            <>
              <div className="cards-grid">
                <EstatisticasCard
                  titulo="Total de Focos"
                  valor={resumo.totalFocos.toLocaleString('pt-BR')}
                  icone="🔥"
                  cor="#e74c3c"
                />
                <EstatisticasCard
                  titulo="Municípios Afetados"
                  valor={resumo.totalMunicipiosAfetados}
                  icone="📍"
                  cor="#e67e22"
                />
                <EstatisticasCard
                  titulo="Média por Dia"
                  valor={resumo.mediaFocosPorDia.toFixed(1)}
                  icone="📊"
                  cor="#f39c12"
                />
                {resumo.dataComMaisFocos && (
                  <EstatisticasCard
                    titulo="Dia com Mais Focos"
                    valor={resumo.focosNaDataMaxima}
                    icone="⚠️"
                    cor="#d35400"
                  />
                )}
              </div>

              <div className="graficos-container">
                <GraficoLinha data={focosPorDia} />
                <GraficoBarras 
                  data={estatisticasMunicipios}
                  eixoMunicipioAltura={eixoMunicipioAltura}
                  legendaOffset={legendaOffset}
                  graficoAltura={graficoAltura}
                />
              </div>
            </>
          ) : (
            <div className="sem-dados">
              <div className="sem-dados-icone">📊</div>
              <h3>Nenhum dado encontrado</h3>
              <p>Não há focos de queimadas registrados para os filtros selecionados.</p>
            </div>
          )}
        </div>
      )}
    </div>
  );
}

export default Dashboard;

