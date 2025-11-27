
import './EstatisticasCard.css';

function EstatisticasCard({ titulo, valor, icone, cor = '#4a90e2' }) {
  return (
    <div className="estatisticas-card" style={{ borderTopColor: cor }}>
      <div className="card-header">
        <span className="card-icone" style={{ color: cor }}>
          {icone}
        </span>
        <h4 className="card-titulo">{titulo}</h4>
      </div>
      <div className="card-valor" style={{ color: cor }}>
        {valor}
      </div>
    </div>
  );
}

export default EstatisticasCard;


