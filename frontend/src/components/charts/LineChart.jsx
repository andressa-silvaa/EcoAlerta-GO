import { memo } from 'react';
import {
  LineChart as RechartsLineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { CHART_COLORS } from '../../constants/appConfig';

const CHART_HEIGHT = 300;

const LineChart = memo(({ data, title = 'Focos por Dia' }) => (
  <div className="grafico-card">
    <h3>{title}</h3>
    <ResponsiveContainer width="100%" height={CHART_HEIGHT}>
      <RechartsLineChart data={data}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="data" />
        <YAxis />
        <Tooltip />
        <Legend />
        <Line
          type="monotone"
          dataKey="total"
          stroke={CHART_COLORS.PRIMARY}
          strokeWidth={3}
          name="Focos"
        />
      </RechartsLineChart>
    </ResponsiveContainer>
  </div>
));

LineChart.displayName = 'LineChart';

export default LineChart;

