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
import { useState, useEffect } from 'react';
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

function Mapa() {
  // Estados para armazenar dados e controle da UI
  const [queimadas, setQueimadas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [filtros, setFiltros] = useState(null);

  // Coordenadas do centro de Goiás (Goiânia)
  const centroGoias = [-16.6864, -49.2643];
  const zoomInicial = 7;
  const limiteAvisoMarcadores = 8000;

  const clusterConfiguracao = {
    chunkedLoading: true,
    spiderfyOnMaxZoom: true,
    showCoverageOnHover: false,
    removeOutsideVisibleBounds: true,
    disableClusteringAtZoom: 12
  };

  // Função para carregar dados de queimadas do backend
  const carregarDados = async (filtrosAtuais) => {
    if (!filtrosAtuais) return;

    setLoading(true);
    setError(null);

    try {
      // Chama o Web Service do backend para obter queimadas
      const dados = await obterQueimadas(
        filtrosAtuais.dataInicio,
        filtrosAtuais.dataFim,
        filtrosAtuais.municipio
      );

      setQueimadas(dados);
    } catch (err) {
      console.error('Erro ao carregar queimadas:', err);
      setError(err.message || 'Erro ao carregar dados do servidor');
    } finally {
      setLoading(false);
    }
  };

  // Callback quando os filtros são alterados
  const handleFiltrosChange = (novosFiltros) => {
    setFiltros(novosFiltros);
    carregarDados(novosFiltros);
  };

  // Carrega dados iniciais quando o componente é montado
  useEffect(() => {
    // Os dados iniciais serão carregados quando o componente Filtros definir os valores padrão
  }, []);

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

      {loading && queimadas.length === 0 ? (
        <div className="loading">Carregando mapa e dados...</div>
      ) : (
        <div className="mapa-wrapper">
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
              {queimadas.map((queimada) => (
                <Marker
                  key={queimada.id}
                  position={[Number(queimada.latitude), Number(queimada.longitude)]}
                  icon={iconeQueimada}
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
              ))}
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
        </div>
      )}
    </div>
  );
}

export default Mapa;

