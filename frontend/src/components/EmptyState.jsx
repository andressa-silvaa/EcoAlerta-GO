const EmptyState = ({
  icon = '📊',
  title = 'Nenhum dado encontrado',
  message = 'Não há dados para exibir com os filtros selecionados.',
}) => (
  <div className="sem-dados">
    <div className="sem-dados-icone">{icon}</div>
    <h3>{title}</h3>
    <p>{message}</p>
  </div>
);

export default EmptyState;

