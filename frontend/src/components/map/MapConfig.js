import L from 'leaflet';

// Fix for default Leaflet icon (known issue with webpack/vite)
delete L.Icon.Default.prototype._getIconUrl;
L.Icon.Default.mergeOptions({
  iconRetinaUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon-2x.png',
  iconUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-icon.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
});

// Custom red icon for fire markers
export const fireIcon = L.icon({
  iconUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-red.png',
  iconRetinaUrl: 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png',
  shadowUrl: 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

export const clusterConfig = {
  chunkedLoading: true,
  spiderfyOnMaxZoom: false,
  showCoverageOnHover: false,
  removeOutsideVisibleBounds: true,
  disableClusteringAtZoom: 13,
  maxClusterRadius: 60,
  animate: false,
  animateAddingMarkers: false,
  spiderLegPolylineOptions: { weight: 0 },
  zoomToBoundsOnClick: true,
};

