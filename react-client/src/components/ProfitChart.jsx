import { useState, useEffect, useCallback } from 'react';
import { Bar } from 'react-chartjs-2';
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend } from 'chart.js';
import { Printer, FileSpreadsheet, Eye, EyeOff, DatabaseBackup, DatabaseZap } from 'lucide-react';

import { useReward } from 'partycles';

ChartJS.register(CategoryScale, LinearScale, BarElement, ArcElement, Title, Tooltip, Legend);

// --- Configuration ---
const BASE_URL = 'http://localhost:5292';

const labelMap = {
  profit: 'Profit',
  damages: 'Maintenance Cost',
  staffing: 'Demand vs Staffing Capacity',
  collectionrate: 'Customer Retention Rate',
  payroll_rollup: 'Payroll (ROLLUP)',
  invoice_cube: 'Invoice Revenue (CUBE)',
  invoice_grouping: 'Invoice Revenue (GROUPING)',
  shift_groupingsets: 'Shift Breakdown (GROUPING SETS)',
};

const ProfitChart = () => {
  const [data, setData] = useState([]);
  const [metric, setMetric] = useState('profit');
  const [showTable, setShowTable] = useState(false);
  const [showChart, setShowChart] = useState(true);
  const [loading, setLoading] = useState(false);
  const [isDbEmpty, setIsDbEmpty] = useState(true);
  const [theme, setTheme] = useState('light');

  // --- Initialize confetti effect ---
  const { reward, isAnimating } = useReward('rewardId', 'confetti');

  // --- Detect system theme ---
  useEffect(() => {
    const mq = window.matchMedia('(prefers-color-scheme: dark)');
    const updateTheme = () => setTheme(mq.matches ? 'dark' : 'light');
    updateTheme();
    mq.addEventListener('change', updateTheme);
    return () => mq.removeEventListener('change', updateTheme);
  }, []);

  // --- Common reusable fetch function ---
  const fetchMetricData = useCallback(async (metricName = metric) => {
    try {
      setLoading(true);
      // Use the full backend URL
      const res = await fetch(`${BASE_URL}/api/reports/analytics?metric=${metricName}`);
      if (!res.ok) {
        throw new Error(`HTTP error! status: ${res.status}`);
      }
      const jsonData = await res.json();
      setData(jsonData);
      setIsDbEmpty(jsonData.length === 0);
    } catch (err) {
      console.error('Failed to load data:', err);
      setData([]);
      setIsDbEmpty(true);
    } finally {
      setLoading(false);
    }
  }, [metric]);

  useEffect(() => {
    fetchMetricData();
  }, [metric, fetchMetricData]);

  // --- Show table automatically for specific metrics ---
  useEffect(() => {
    setShowTable(true);
    if (isTableOnlyMetric(metric)) {
      setShowChart(false);
    } else {
      setShowChart(true);
    }
  }, [metric]);


  const labelKey = data.length ? Object.keys(data[0])[0] : 'label';
  const valueKey =
    data.length && Object.keys(data[0]).find(k => k.toLowerCase().includes('metric'))
      ? Object.keys(data[0]).find(k => k.toLowerCase().includes('metric'))
      : data.length ? Object.keys(data[0])[1] : 'metricValue';

  const chartData = {
    labels: data.map(d => d[labelKey]),
    datasets: [
      {
        label: labelMap[metric],
        data: data.map(d => d[valueKey]),
        backgroundColor: theme === 'dark' ? 'rgba(255, 137, 2, 0.6)' : 'rgba(54, 162, 235, 0.6)',
        borderRadius: 6,
      }
    ]
  };

  const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { labels: { color: theme === "dark" ? "#ddd" : "#222" } },
          title: {
            display: true,
            text: `${labelMap[metric]} Analysis`,
            color: theme === "dark" ? "#fff" : "#000"
          }
        },
        scales: {
          x: { ticks: { color: theme === "dark" ? "#ccc" : "#333" } },
          y: { ticks: { color: theme === "dark" ? "#ccc" : "#333" } }
        }
      };

  const handleDownloadExcel = async () => {
    setLoading(true);
    try {
      const response = await fetch(`${BASE_URL}/api/reports/export?metric=${metric}`);
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      const blob = await response.blob();
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `${metric}-analytics.xlsx`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      console.error('Download failed', err);
      alert('Download failed. Please check if the backend is running.');
    } finally {
      setLoading(false);
    }
  };

  const handleEtlClick = async () => {
    const endpoint = isDbEmpty
      ? `${BASE_URL}/api/etl/run`
      : `${BASE_URL}/api/etl/purge`;

    try {
      const response = await fetch(endpoint, { method: 'GET' });
      if (!response.ok) {
        throw new Error(`HTTP error! status: ${response.status}`);
      }
      await fetchMetricData();
    } catch (err) {
      console.error('ETL action failed:', err);
      alert('ETL action failed. Please check if the backend is running.');
    }
  };

  const isTableOnlyMetric = (metric) => 
  ["payroll_rollup", "invoice_cube", "invoice_grouping", "shift_groupingsets"].includes(metric);

  return (
    <div className={`dashboard ${theme}`} data-theme={theme}>
      <header>
        <h2>Analysis</h2>
        <div className="controls">
          <select value={metric} onChange={(e) => {
            setMetric(e.target.value);
            reward();
          }}>
            <option value="profit">Profit By Service</option>
            <option value="damages">Maintenance Cost</option>
            <option value="staffing">Demand vs Staffing Capacity</option>
            <option value="collectionrate">Customer Retention</option>
            <option value="payroll_rollup">Payroll (ROLLUP)</option>
            <option value="invoice_cube">Invoice Revenue (CUBE)</option>
            <option value="invoice_grouping">Invoice Revenue (GROUPING)</option>
            <option value="shift_groupingsets">Shift Breakdown (GROUPING SETS)</option>
          </select>

          <div className="button-group">
            <button onClick={() => window.print()}>
              <Printer size={16} /> Print
            </button>
            <button onClick={handleDownloadExcel} disabled={loading}>
              <FileSpreadsheet size={16} />
              {loading ? 'Preparing…' : 'Download Excel'}
            </button>
            <button 
              onClick={() => setShowChart(c => !c)} 
              disabled={isTableOnlyMetric(metric)}
            >
              {showChart ? <><EyeOff size={16} /> Hide Chart</> : <><Eye size={16} /> Show Chart</>}
            </button>
            <button onClick={() => setShowTable(p => !p)}>
              {showTable ? <><EyeOff size={16} /> Hide Table</> : <><Eye size={16} /> Show Table</>}
            </button>
            <button onClick={handleEtlClick}>
              {isDbEmpty ? <><DatabaseZap size={16} /> Run ETL</> : <><DatabaseBackup size={16} /> Purge DB</>}
            </button>
          </div>
        </div>
      </header>

      {showChart && !isTableOnlyMetric(metric) && (
        <div id='rewardId' className="chart-wrapper">
          {loading ? (
            <div className="loading">Loading data...</div>
          ) : data.length > 0 ? (
            <Bar data={chartData} options={chartOptions} />
          ) : (
            <div className="no-data">
              {isDbEmpty ? 'No data available. Run ETL to populate the database.' : 'No data found for this metric.'}
            </div>
          )}
        </div>
      )}

      {showTable && data.length > 0 && (
        <div className="table-container">
          <h3>{labelMap[metric]} Data</h3>
          <table>
            <thead>
              <tr>
                {Object.keys(data[0]).map((key) => (
                  <th key={key}>{key.charAt(0).toUpperCase() + key.slice(1)}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {data.map((row, i) => (
                <tr key={i}>
                  {Object.values(row).map((val, j) => (
                    <td key={j}>{val}</td>
                  ))}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};

export default ProfitChart;