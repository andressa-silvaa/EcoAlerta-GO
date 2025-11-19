/**
 * Página de Dashboard - Visão geral das estatísticas de queimadas.
 * 
 * Esta página exibe:
 * - Cards com métricas principais (total de focos, municípios afetados, etc.)
 * - Gráficos de focos por dia e por município
 * - Filtros para ajustar o período de análise
 * 
 * Os dados são obtidos dos Web Services do backend através dos serviços de API.
 */
import { useState, useEffect, useCallback } from 'react';
import { obterResumoEstatisticas, obterEstatisticasPorMunicipio, obterQueimadas } from '../services/api';
import Filtros from '../components/Filtros';
import EstatisticasCard from '../components/EstatisticasCard';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, LineChart, Line } from 'recharts';
import './Dashboard.css';

function Dashboard() {
  // Estados para armazenar os dados
  const [resumo, setResumo] = useState(null);
  const [estatisticasMunicipios, setEstatisticasMunicipios] = useState([]);
  const [focosPorDia, setFocosPorDia] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filtros, setFiltros] = useState(null);

  // Função para carregar todos os dados do dashboard
  const carregarDados = useCallback(async (filtrosAtuais) => {
    if (!filtrosAtuais) {
      console.log('Filtros não fornecidos, aguardando...');
      return;
    }

    console.log('Iniciando carregamento de dados com filtros:', filtrosAtuais);
    setLoading(true);
    setError(null);

    try {
      console.log('Fazendo requisições à API...');
      // Carrega dados em paralelo para melhor performance
      const [resumoData, municipiosData, queimadasData] = await Promise.all([
        obterResumoEstatisticas(filtrosAtuais.dataInicio, filtrosAtuais.dataFim),
        obterEstatisticasPorMunicipio(filtrosAtuais.dataInicio, filtrosAtuais.dataFim),
        obterQueimadas(filtrosAtuais.dataInicio, filtrosAtuais.dataFim, filtrosAtuais.municipio)
      ]);

      console.log('Dados recebidos:', { resumoData, municipiosData, queimadasData });

      setResumo(resumoData);
      setEstatisticasMunicipios(municipiosData.slice(0, 10)); // Top 10 municípios

      // Agrupa focos por dia para o gráfico
      const focosPorDiaMap = {};
      queimadasData.forEach(queimada => {
        const data = new Date(queimada.dataHora).toLocaleDateString('pt-BR');
        focosPorDiaMap[data] = (focosPorDiaMap[data] || 0) + 1;
      });

      const focosPorDiaArray = Object.entries(focosPorDiaMap)
        .map(([data, total]) => ({ data, total }))
        .sort((a, b) => new Date(a.data.split('/').reverse().join('-')) - new Date(b.data.split('/').reverse().join('-')));

      setFocosPorDia(focosPorDiaArray);
      console.log('Dados carregados com sucesso!');
    } catch (err) {
      console.error('Erro ao carregar dados:', err);
      console.error('Detalhes do erro:', {
        message: err.message,
        response: err.response,
        request: err.request
      });
      setError(err.message || 'Erro ao carregar dados do servidor. Verifique se o backend está rodando.');
    } finally {
      setLoading(false);
      console.log('Carregamento finalizado');
    }
  }, []);

  // Callback quando os filtros são alterados
  const handleFiltrosChange = (novosFiltros) => {
    console.log('Filtros alterados:', novosFiltros);
    setFiltros(novosFiltros);
    carregarDados(novosFiltros);
  };

  // Carrega dados iniciais quando o componente é montado
  useEffect(() => {
    // Se após 1 segundo não houver filtros definidos, carrega com valores padrão
    const timer = setTimeout(() => {
      if (!filtros) {
        console.log('Carregando dados iniciais (fallback)...');
        const hoje = new Date();
        const trintaDiasAtras = new Date();
        trintaDiasAtras.setDate(hoje.getDate() - 30);
        
        const filtrosIniciais = {
          dataInicio: trintaDiasAtras,
          dataFim: hoje,
          municipio: null
        };
        
        setFiltros(filtrosIniciais);
        carregarDados(filtrosIniciais);
      }
    }, 1000);

    return () => clearTimeout(timer);
  }, [filtros, carregarDados]);

  const longestMunicipioLength = estatisticasMunicipios.reduce(
    (max, municipio) => Math.max(max, (municipio.municipio || '').length),
    0
  );
  const eixoMunicipioAltura = Math.min(Math.max(longestMunicipioLength * 6, 80), 220);
  const legendaOffset = Math.min(Math.max(longestMunicipioLength * 0.8, 12), 80);
  const eixoExtraAltura = Math.max(0, eixoMunicipioAltura - 100);
  const graficoAlturaBase = 260;
  const graficoAltura = Math.min(graficoAlturaBase + eixoExtraAltura + legendaOffset, 420);

  if (loading && !resumo && !error) {
    return (
      <div className="dashboard-container">
        <h1>Monitoramento de Queimadas em Goiás</h1>
        <p className="subtitulo">Sistema de gestão ambiental para monitoramento de focos de queimadas</p>
        <div className="loading">Carregando dados...</div>
      </div>
    );
  }

  return (
    <div className="dashboard-container">
      <h1>Monitoramento de Queimadas em Goiás</h1>
      <p className="subtitulo">Sistema de gestão ambiental para monitoramento de focos de queimadas</p>

      <Filtros onFiltrosChange={handleFiltrosChange} />

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

      {resumo && (
        <>
          {/* Cards de Estatísticas */}
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

          {/* Gráficos */}
          <div className="graficos-container">
            <div className="grafico-card">
              <h3>Focos por Dia</h3>
              <ResponsiveContainer width="100%" height={300}>
                <LineChart data={focosPorDia}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="data" />
                  <YAxis />
                  <Tooltip />
                  <Legend />
                  <Line type="monotone" dataKey="total" stroke="#e74c3c" strokeWidth={3} name="Focos" />
                </LineChart>
              </ResponsiveContainer>
            </div>

            <div className="grafico-card">
              <h3>Top 10 Municípios com Mais Focos</h3>
              <ResponsiveContainer width="100%" height={graficoAltura}>
                <BarChart
                  data={estatisticasMunicipios}
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
          </div>
        </>
      )}
    </div>
  );
}

export default Dashboard;

