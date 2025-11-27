import { BrowserRouter as Router, Routes, Route, Link, useLocation } from 'react-router-dom';
import Dashboard from './pages/Dashboard';
import Mapa from './pages/Mapa';
import './App.css';

function Navegacao() {
  const location = useLocation();

  return (
    <nav className="navegacao">
      <div className="nav-container">
        <Link to="/" className="nav-logo">
          🔥 EcoAlerta GO
        </Link>
        <div className="nav-links">
          <Link 
            to="/" 
            className={location.pathname === '/' ? 'nav-link active' : 'nav-link'}
          >
            Dashboard
          </Link>
          <Link 
            to="/mapa" 
            className={location.pathname === '/mapa' ? 'nav-link active' : 'nav-link'}
          >
            Mapa
          </Link>
        </div>
      </div>
    </nav>
  );
}

function App() {
  return (
    <Router>
      <div className="app">
        <Navegacao />
        <main className="app-content">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/mapa" element={<Mapa />} />
          </Routes>
        </main>
        <footer className="app-footer">
          <p>Sistema de Monitoramento de Queimadas em Goiás - Trabalho Acadêmico APS</p>
          <p>Desenvolvido com Web Services (.NET Web API + React)</p>
        </footer>
      </div>
    </Router>
  );
}

export default App;
