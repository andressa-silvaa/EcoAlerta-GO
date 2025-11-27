import { useCallback, useEffect, useMemo } from 'react';
import { MapContainer, TileLayer, useMap } from 'react-leaflet';
import MarkerClusterGroup from 'react-leaflet-cluster';
import Filtros from '../components/Filtros';
import LoadingBanner from '../components/LoadingBanner';
import ErrorMessage from '../components/ErrorMessage';
import MapMarker from '../components/map/MapMarker';
import { fireIcon, clusterConfig } from '../components/map/MapConfig';
import { useMapData } from '../hooks/useMapData';
import { MAP_CONFIG } from '../constants/appConfig';
import { formatNumber } from '../utils/dateUtils';
import 'leaflet/dist/leaflet.css';
import './Mapa.css';

const MapViewController = ({ center, zoom }) => {
  const map = useMap();
  useEffect(() => {
    map.setView(center, zoom);
  }, [map, center, zoom]);
  return null;
};

const MapInfo = ({ count, warningLimit }) => (
  <div className="mapa-info">
    <p>
      <strong>Total de focos exibidos:</strong> {formatNumber(count)}
    </p>
    {count > warningLimit && (
      <p className="info-warning">
        Exibindo {formatNumber(count)} focos de maneira agrupada para manter o desempenho.
        Aproxime o mapa para expandir os clusters e ver detalhes específicos.
      </p>
    )}
    <p className="info-text">
      Clique nos marcadores vermelhos para ver detalhes de cada foco de queimada.
    </p>
  </div>
);

function Mapa() {
  const { queimadas, loading, error, loadData } = useMapData();

  const handleFiltrosChange = useCallback((novosFiltros) => {
    loadData(novosFiltros);
  }, [loadData]);

  const markers = useMemo(() =>
    queimadas.map((queimada) => (
      <MapMarker key={queimada.id} queimada={queimada} icon={fireIcon} />
    )),
    [queimadas]
  );

  return (
    <div className="mapa-container">
      <h1>Mapa de Focos de Queimadas em Goiás</h1>
      <p className="subtitulo">Visualização geográfica dos focos detectados</p>

      <Filtros onFiltrosChange={handleFiltrosChange} />

      {error && <ErrorMessage error={error} />}

      {loading && (
        <LoadingBanner message="Carregando focos de queimadas... Aguarde alguns segundos." />
      )}

      <div className="mapa-wrapper" style={{ opacity: loading ? 0.5 : 1, transition: 'opacity 0.3s' }}>
        {queimadas.length > 0 && (
          <>
            <MapContainer
              center={MAP_CONFIG.CENTER_GOIAS}
              zoom={MAP_CONFIG.INITIAL_ZOOM}
              style={{ height: '600px', width: '100%', borderRadius: '8px' }}
              scrollWheelZoom={true}
            >
              <MapViewController center={MAP_CONFIG.CENTER_GOIAS} zoom={MAP_CONFIG.INITIAL_ZOOM} />
              <TileLayer
                attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
              />
              <MarkerClusterGroup {...clusterConfig}>
                {markers}
              </MarkerClusterGroup>
            </MapContainer>

            <MapInfo count={queimadas.length} warningLimit={MAP_CONFIG.MARKER_WARNING_LIMIT} />
          </>
        )}
      </div>
    </div>
  );
}

export default Mapa;

