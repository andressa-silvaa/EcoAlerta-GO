import { memo, useMemo } from 'react';
import {
  BarChart as RechartsBarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from 'recharts';
import { CHART_COLORS } from '../../constants/appConfig';

const calculateChartDimensions = (municipios) => {
  const longestNameLength = municipios.reduce(
    (max, item) => Math.max(max, (item.municipio || '').length),
    0
  );

  const baseAxisHeight = Math.min(Math.max(longestNameLength * 6, 80), 220);
  const legendOffset = Math.min(Math.max(longestNameLength * 0.8, 12), 80);
  const baseHeight = 260;
  const extraHeight = Math.max(0, baseAxisHeight - 100);
  const totalHeight = Math.min(baseHeight + extraHeight + legendOffset, 420);

  return {
    axisHeight: baseAxisHeight,
    legendOffset,
    chartHeight: totalHeight,
  };
};

const BarChart = memo(({ data }) => {
  const { axisHeight, legendOffset, chartHeight } = useMemo(
    () => calculateChartDimensions(data),
    [data]
  );

  const chartTitle = useMemo(() => {
    const count = data.length;
    return count === 1
      ? 'Município com Focos de Queimadas'
      : `Top ${Math.min(count, 10)} Municípios com Mais Focos`;
  }, [data.length]);

  return (
    <div className="grafico-card">
      <h3>{chartTitle}</h3>
      <ResponsiveContainer width="100%" height={chartHeight}>
        <RechartsBarChart
          data={data}
          margin={{ top: 5, right: 30, left: 10, bottom: 20 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis
            dataKey="municipio"
            angle={-45}
            textAnchor="end"
            interval={0}
            height={axisHeight}
          />
          <YAxis />
          <Tooltip />
          <Legend
            verticalAlign="bottom"
            wrapperStyle={{ paddingTop: legendOffset }}
          />
          <Bar dataKey="totalFocos" fill={CHART_COLORS.SECONDARY} name="Total de Focos" />
        </RechartsBarChart>
      </ResponsiveContainer>
    </div>
  );
});

BarChart.displayName = 'BarChart';

export default BarChart;

