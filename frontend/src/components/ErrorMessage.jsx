import { API_BASE_URL } from '../constants/appConfig';

const ErrorMessage = ({ error }) => (
  <div className="error-message">
    <div>
      <strong>Erro ao carregar dados:</strong> {error}
      <br />
      <small style={{ marginTop: '10px', display: 'block' }}>
        Verifique se o backend está rodando em {API_BASE_URL}
      </small>
    </div>
  </div>
);

export default ErrorMessage;

