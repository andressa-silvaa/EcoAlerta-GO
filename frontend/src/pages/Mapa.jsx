/**
 * Página de Mapa Interativo - Exibição geográfica dos focos de queimadas.
 * 
 * Esta página utiliza a biblioteca React-Leaflet para exibir um mapa
 * centrado no estado de Goiás com marcadores representando os focos
 * de queimadas detectados.
 * 
 * Funcionalidades:
 * - Mapa interativo centrado em Goiás
 * - Marcadores para cada foco de queimada
 * - Popup com informações detalhadas ao clicar no marcador
 * - Filtros integrados para atualizar o mapa dinamicamente
 */
import { useState, useCallback, useEffect, useMemo, memo } from 'react';
import { MapContainer, TileLayer, Marker, Popup, useMap } from 'react-leaflet';
import MarkerClusterGroup from 'react-leaflet-cluster';
import L from 'leaflet';
import { obterQueimadas } from '../services/api';
import Filtros from '../components/Filtros';
import 'leaflet/dist/leaflet.css';
import './Mapa.css';

// Fix para o ícone padrão do Leaflet (problema conhecido com webpack/vite)
// Usando ícone vermelho personalizado diretamente
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
});

// Configuração do ícone personalizado para marcadores de queimadas (vermelho)
const iconeQueimada = L.icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
  iconRetinaUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41]
});

/**
 * Componente auxiliar para ajustar o centro do mapa quando os filtros mudam.
 * React-Leaflet requer que mudanças de view sejam feitas através deste hook.
 * Este componente DEVE ser usado dentro do MapContainer.
 */
function AjustarVisualizacao({ center, zoom }) {
  const map = useMap();
  useEffect(() => {
    map.setView(center, zoom);
  }, [map, center, zoom]);
  return null;
}

/**
 * Componente de marcador individual otimizado com React.memo
 * Evita re-renderizações desnecessárias
 */
const MarcadorQueimada = memo(({ queimada, icon }) => (
  <Marker
    position={[Number(queimada.latitude), Number(queimada.longitude)]}
    icon={icon}
  >
    <Popup>
      <div className="popup-content">
        <h4>🔥 Foco de Queimada</h4>
        <p><strong>Município:</strong> {queimada.municipio}</p>
        <p><strong>Estado:</strong> {queimada.estado}</p>
        <p><strong>Data/Hora:</strong> {new Date(queimada.dataHora).toLocaleString('pt-BR')}</p>
        {queimada.intensidade && (
          <p><strong>Intensidade:</strong> {Number(queimada.intensidade).toFixed(2)}</p>
        )}
        {queimada.fonteSatelite && (
          <p><strong>Fonte:</strong> {queimada.fonteSatelite}</p>
        )}
        <p className="coordenadas">
          <strong>Coordenadas:</strong> {Number(queimada.latitude).toFixed(4)}, {Number(queimada.longitude).toFixed(4)}
        </p>
      </div>
    </Popup>
  </Marker>
), (prevProps, nextProps) => {
  // Comparação customizada para evitar re-renders desnecessários
  return prevProps.queimada.id === nextProps.queimada.id;
});

function Mapa() {
  const [queimadas, setQueimadas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Coordenadas do centro de Goiás (Goiânia) - memoizado
  const centroGoias = useMemo(() => [-16.6864, -49.2643], []);
  const zoomInicial = 7;
  const limiteAvisoMarcadores = 8000;

  // Configuração otimizada do cluster - memoizado
  const clusterConfiguracao = useMemo(() => ({
    chunkedLoading: true,
    spiderfyOnMaxZoom: false, // Desativa spider para melhor performance
    showCoverageOnHover: false,
    removeOutsideVisibleBounds: true,
    disableClusteringAtZoom: 13,
    maxClusterRadius: 60, // Reduzido para agrupar mais
    animate: false, // Desativa animações para melhor performance
    animateAddingMarkers: false,
    spiderLegPolylineOptions: { weight: 0 }, // Remove linhas do spider
    zoomToBoundsOnClick: true
  }), []);

  const carregarDados = useCallback(async (filtrosAtuais) => {
    if (!filtrosAtuais) return;

    console.log('🔍 Iniciando busca com filtros:', filtrosAtuais);
    const startTime = performance.now();

    setLoading(true);
    setError(null);

    try {
      console.log('📡 Fazendo requisição para API...');
      const dados = await obterQueimadas(
        filtrosAtuais.dataInicio,
        filtrosAtuais.dataFim,
        filtrosAtuais.municipio
      );

      const endTime = performance.now();
      const tempoDecorrido = ((endTime - startTime) / 1000).toFixed(2);
      console.log(`✅ Dados recebidos: ${dados.length} focos em ${tempoDecorrido}s`);

      // Garante mínimo de 300ms de loading para feedback visual
      const tempoMinimo = 300;
      const tempoRestante = Math.max(0, tempoMinimo - (endTime - startTime));
      
      await new Promise(resolve => setTimeout(resolve, tempoRestante));

      setQueimadas(dados);
    } catch (err) {
      const endTime = performance.now();
      const tempoDecorrido = ((endTime - startTime) / 1000).toFixed(2);
      console.error(`❌ Erro após ${tempoDecorrido}s:`, err);
      setError(err.message || 'Erro ao carregar dados do servidor');
    } finally {
      setLoading(false);
    }
  }, []);

  const handleFiltrosChange = useCallback((novosFiltros) => {
    carregarDados(novosFiltros);
  }, [carregarDados]);

  // Renderiza marcadores de forma otimizada com componente memoizado
  const marcadores = useMemo(() => {
    console.log(`🗺️ Renderizando ${queimadas.length} marcadores`);
    return queimadas.map((queimada) => (
      <MarcadorQueimada key={queimada.id} queimada={queimada} icon={iconeQueimada} />
    ));
  }, [queimadas]);

  return (
    <div className="mapa-container">
      <h1>Mapa de Focos de Queimadas em Goiás</h1>
      <p className="subtitulo">Visualização geográfica dos focos detectados</p>

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

      {loading && (
        <div className="loading-banner">
          <div className="loading-spinner"></div>
          <span>Carregando focos de queimadas... Aguarde alguns segundos.</span>
        </div>
      )}

      <div className="mapa-wrapper" style={{ 
        opacity: loading ? 0.5 : 1,
        transition: 'opacity 0.3s'
      }}>
        {queimadas.length > 0 ? (
          <>
            <MapContainer
              center={centroGoias}
              zoom={zoomInicial}
              style={{ height: '600px', width: '100%', borderRadius: '8px' }}
              scrollWheelZoom={true}
            >
              {/* Componente para ajustar a visualização do mapa */}
              <AjustarVisualizacao center={centroGoias} zoom={zoomInicial} />
              
              {/* Camada de tiles do OpenStreetMap */}
              <TileLayer
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              />

              <MarkerClusterGroup {...clusterConfiguracao}>
                {marcadores}
              </MarkerClusterGroup>
            </MapContainer>

            <div className="mapa-info">
              <p>
                <strong>Total de focos exibidos:</strong> {queimadas.length.toLocaleString('pt-BR')}
              </p>
              {queimadas.length > limiteAvisoMarcadores && (
                <p className="info-warning">
                  Exibindo {queimadas.length.toLocaleString('pt-BR')} focos de maneira agrupada para manter o desempenho.
                  Aproxime o mapa para expandir os clusters e ver detalhes específicos.
                </p>
              )}
              <p className="info-text">
                Clique nos marcadores vermelhos para ver detalhes de cada foco de queimada.
              </p>
            </div>
          </>
        ) : !loading && (
          <div className="sem-dados">
            <div className="sem-dados-icone">📊</div>
            <h3>Nenhum dado para exibir</h3>
            <p>Aguardando aplicação dos filtros ou não há focos no período selecionado.</p>
          </div>
        )}
      </div>
    </div>
  );
}

export default Mapa;

