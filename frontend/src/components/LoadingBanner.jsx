const LoadingBanner = ({ message = 'Carregando dados...' }) => (
  <div className="loading-banner">
    <div className="loading-spinner"></div>
    <span>{message}</span>
  </div>
);

export default LoadingBanner;

