const chartRegistry = new Map();

const centerTextPlugin = {
    id: 'centerText',
    afterDraw(chart) {
        const opts = chart.config.options.plugins?.centerText;
        if (!opts?.text) return;

        const { ctx, chartArea: { left, top, width, height } } = chart;
        const centerX = left + width / 2;
        const centerY = top + height / 2;

        ctx.save();
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';

        ctx.font = opts.font || 'bold 22px sans-serif';
        ctx.fillStyle = opts.color || '#212529';
        ctx.fillText(opts.text, centerX, centerY - (opts.subtext ? 10 : 0));

        if (opts.subtext) {
            ctx.font = opts.subFont || '12px sans-serif';
            ctx.fillStyle = opts.subColor || '#6c757d';
            ctx.fillText(opts.subtext, centerX, centerY + 12);
        }

        ctx.restore();
    }
};

Chart.register(centerTextPlugin);

export function renderChart(canvasId, config) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) {
        console.warn(`progressChart: canvas '${canvasId}' not found.`);
        return;
    }

    const existing = chartRegistry.get(canvasId);
    if (existing) {
        existing.data = config.data;
        existing.options = config.options;
        existing.config.type = config.type;
        existing.update();
        return;
    }

    chartRegistry.set(canvasId, new Chart(canvas.getContext('2d'), config));
}

export function renderDoughnutChart(canvasId, config) {
    config.options = config.options || {};
    config.options.plugins = config.options.plugins || {};
    config.options.plugins.tooltip = {
        callbacks: {
            label: (ctx) => {
                const kcal = ctx.parsed;
                const total = ctx.dataset.data.reduce((a, b) => a + b, 0);
                const pct = total > 0 ? Math.round((kcal / total) * 100) : 0;
                return `${ctx.label}: ${kcal} kcal (${pct}%)`;
            }
        }
    };

    renderChart(canvasId, config);
}

export function destroyChart(canvasId) {
    const existing = chartRegistry.get(canvasId);
    if (existing) {
        existing.destroy();
        chartRegistry.delete(canvasId);
    }
}