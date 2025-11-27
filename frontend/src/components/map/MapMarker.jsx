import { memo } from 'react';
import { Marker, Popup } from 'react-leaflet';
import { formatBrazilianDateTime, formatNumber } from '../../utils/dateUtils';

const PopupContent = ({ queimada }) => (
  <div className="popup-content">
    <h4>🔥 Foco de Queimada</h4>
    <p><strong>Município:</strong> {queimada.municipio}</p>
    <p><strong>Estado:</strong> {queimada.estado}</p>
    <p><strong>Data/Hora:</strong> {formatBrazilianDateTime(queimada.dataHora)}</p>
    {queimada.intensidade && (
      <p><strong>Intensidade:</strong> {formatNumber(Number(queimada.intensidade), 2)}</p>
    )}
    {queimada.fonteSatelite && (
      <p><strong>Fonte:</strong> {queimada.fonteSatelite}</p>
    )}
    <p className="coordenadas">
      <strong>Coordenadas:</strong> {formatNumber(Number(queimada.latitude), 4)}, {formatNumber(Number(queimada.longitude), 4)}
    </p>
  </div>
);

const MapMarker = memo(({ queimada, icon }) => (
  <Marker
    position={[Number(queimada.latitude), Number(queimada.longitude)]}
    icon={icon}
  >
    <Popup>
      <PopupContent queimada={queimada} />
    </Popup>
  </Marker>
), (prevProps, nextProps) => prevProps.queimada.id === nextProps.queimada.id);

MapMarker.displayName = 'MapMarker';

export default MapMarker;

