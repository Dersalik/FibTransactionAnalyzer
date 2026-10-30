// Results report: currency switch, section tabs, counterparty list and Chart.js charts.
// Chart data comes from a JSON <script class="chart-data"> block inside each currency panel.

(function () {
    const COLORS = {
        ink: '#253247',
        muted: '#5B6678',
        grid: '#EEF0F4',
        accent: '#2E9C84',
        accentFill: 'rgba(63, 179, 154, 0.12)',
        inBar: '#2E9C84',
        outBar: '#F29A80',
        incomeBar: '#7FD1BE'
    };
    const COUNTERPARTY_PAGE = 20;

    const state = { currency: null, panel: 'overview' };
    const charts = new Map(); // currency -> { name: Chart }

    function formatter(decimals) {
        return new Intl.NumberFormat('en-US', { minimumFractionDigits: decimals, maximumFractionDigits: decimals });
    }

    function compact(value) {
        return new Intl.NumberFormat('en-US', { notation: 'compact', maximumFractionDigits: 1 }).format(value);
    }

    function baseOptions(data) {
        const fmt = formatter(data.decimals);
        return {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: {
                legend: { display: false },
                tooltip: {
                    backgroundColor: COLORS.ink,
                    padding: 12,
                    cornerRadius: 12,
                    callbacks: {
                        label: ctx => `${ctx.dataset.label}: ${fmt.format(ctx.parsed.y)} ${data.currency}`
                    }
                }
            },
            scales: {
                x: { grid: { display: false }, border: { display: false }, ticks: { maxRotation: 0, autoSkipPadding: 16 } },
                y: { grid: { color: COLORS.grid }, border: { display: false }, ticks: { callback: compact } }
            }
        };
    }

    const builders = {
        balance(canvas, data) {
            return new Chart(canvas, {
                type: 'line',
                data: {
                    labels: data.balance.labels,
                    datasets: [{
                        label: 'Balance',
                        data: data.balance.data,
                        borderColor: COLORS.accent,
                        backgroundColor: COLORS.accentFill,
                        borderWidth: 2.5,
                        fill: true,
                        tension: 0.25,
                        pointRadius: 0,
                        pointHoverRadius: 4
                    }]
                },
                options: baseOptions(data)
            });
        },

        cashflow(canvas, data) {
            const bar = { borderRadius: 999, borderSkipped: false, maxBarThickness: 18, categoryPercentage: 0.6, barPercentage: 0.9 };
            return new Chart(canvas, {
                type: 'bar',
                data: {
                    labels: data.monthly.labels,
                    datasets: [
                        { label: 'In', data: data.monthly.income, backgroundColor: COLORS.inBar, ...bar },
                        { label: 'Out', data: data.monthly.expenses, backgroundColor: COLORS.outBar, ...bar }
                    ]
                },
                options: baseOptions(data)
            });
        },

        types(canvas, data) {
            return new Chart(canvas, {
                type: 'doughnut',
                data: {
                    labels: data.types.labels,
                    datasets: [{ data: data.types.data, backgroundColor: data.types.colors, borderColor: '#FFFFFF', borderWidth: 2 }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    cutout: '68%',
                    plugins: {
                        legend: { display: false },
                        tooltip: { backgroundColor: COLORS.ink, padding: 12, cornerRadius: 12, callbacks: { label: ctx => `${ctx.label}: ${ctx.parsed} transactions` } }
                    }
                }
            });
        },

        income(canvas, data) {
            const average = data.monthly.labels.map(() => data.averageIncome);
            return new Chart(canvas, {
                type: 'bar',
                data: {
                    labels: data.monthly.labels,
                    datasets: [
                        { label: 'Income', data: data.monthly.income, backgroundColor: COLORS.incomeBar, borderRadius: 14, borderSkipped: 'bottom', maxBarThickness: 48 },
                        { label: 'Average', data: average, type: 'line', borderColor: COLORS.muted, borderDash: [6, 6], borderWidth: 2, pointRadius: 0 }
                    ]
                },
                options: baseOptions(data)
            });
        },

        yearly(canvas, data) {
            return new Chart(canvas, {
                type: 'bar',
                data: {
                    labels: data.yearly.labels,
                    datasets: [{
                        label: 'Net income',
                        data: data.yearly.data,
                        backgroundColor: ctx => (ctx.parsed && ctx.parsed.y < 0 ? COLORS.outBar : COLORS.inBar),
                        borderRadius: 18,
                        borderSkipped: false,
                        maxBarThickness: 96
                    }]
                },
                options: baseOptions(data)
            });
        }
    };

    // Charts are created the first time their panel becomes visible, so they size correctly.
    function renderCharts(currencyPanel, panel) {
        if (typeof Chart === 'undefined') return;
        Chart.defaults.font.family = "'Poppins', system-ui, sans-serif";
        Chart.defaults.color = COLORS.muted;

        const currency = currencyPanel.dataset.currency;
        const data = JSON.parse(currencyPanel.querySelector('.chart-data').textContent);
        const created = charts.get(currency) || {};
        panel.querySelectorAll('canvas[data-chart]').forEach(canvas => {
            const name = canvas.dataset.chart;
            if (!created[name] && builders[name]) created[name] = builders[name](canvas, data);
        });
        charts.set(currency, created);
    }

    function show() {
        document.querySelectorAll('.currency-panel').forEach(cp => {
            const active = cp.dataset.currency === state.currency;
            cp.hidden = !active;
            cp.querySelectorAll('.report-panel').forEach(p => { p.hidden = p.dataset.panel !== state.panel; });
            if (active) renderCharts(cp, cp.querySelector(`.report-panel[data-panel="${state.panel}"]`));
        });
        document.querySelectorAll('[data-currency]').forEach(b => {
            if (b.tagName === 'BUTTON') b.setAttribute('aria-pressed', String(b.dataset.currency === state.currency));
        });
        document.querySelectorAll('.tabs [data-panel]').forEach(t => t.setAttribute('aria-selected', String(t.dataset.panel === state.panel)));
    }

    function goTo(panel) {
        state.panel = panel;
        show();
        document.querySelector('.tabs').scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    // ---------- Counterparties ----------

    function initCounterparties(card) {
        const body = card.querySelector('[data-cp-body]');
        const rows = Array.from(body.children);
        const search = card.querySelector('[data-cp-search]');
        const empty = card.querySelector('[data-cp-empty]');
        const more = card.querySelector('[data-cp-more]');
        const cp = { type: '', sort: 'net', query: '', expanded: false };

        function apply() {
            const key = cp.sort;
            rows.sort((a, b) => parseFloat(b.dataset[key]) - parseFloat(a.dataset[key]));
            rows.forEach(r => body.appendChild(r));

            let visible = 0;
            rows.forEach(r => {
                const matches = r.dataset.name.includes(cp.query) && (!cp.type || r.dataset.types.includes(`|${cp.type}|`));
                const show = matches && (cp.expanded || visible < COUNTERPARTY_PAGE);
                r.hidden = !show;
                if (matches) visible++;
                r.querySelectorAll('.type-figure').forEach(f => { f.hidden = f.dataset.type !== cp.type; });
            });
            empty.hidden = visible > 0;
            if (more) {
                more.hidden = visible <= COUNTERPARTY_PAGE;
                more.textContent = cp.expanded ? 'Show fewer' : `Show all ${visible} counterparties`;
            }
        }

        card.querySelectorAll('[data-cp-type]').forEach(b => b.addEventListener('click', () => {
            cp.type = b.dataset.cpType;
            card.querySelectorAll('[data-cp-type]').forEach(x => x.setAttribute('aria-pressed', String(x === b)));
            apply();
        }));
        card.querySelectorAll('[data-cp-sort]').forEach(b => b.addEventListener('click', () => {
            cp.sort = b.dataset.cpSort;
            card.querySelectorAll('[data-cp-sort]').forEach(x => x.setAttribute('aria-pressed', String(x === b)));
            apply();
        }));
        search.addEventListener('input', () => { cp.query = search.value.trim().toLowerCase(); apply(); });
        if (more) more.addEventListener('click', () => { cp.expanded = !cp.expanded; apply(); });

        apply();
    }

    document.addEventListener('DOMContentLoaded', function () {
        const first = document.querySelector('.currency-panel');
        if (!first) return;
        state.currency = first.dataset.currency;

        document.querySelectorAll('.segmented button[data-currency]').forEach(b =>
            b.addEventListener('click', () => { state.currency = b.dataset.currency; show(); }));
        document.querySelectorAll('.tabs [data-panel]').forEach(t =>
            t.addEventListener('click', () => { state.panel = t.dataset.panel; show(); }));
        document.querySelectorAll('[data-goto]').forEach(b =>
            b.addEventListener('click', () => goTo(b.dataset.goto)));
        document.querySelectorAll('[data-action="print"]').forEach(b =>
            b.addEventListener('click', () => window.print()));
        document.querySelectorAll('[data-counterparties]').forEach(initCounterparties);

        show();
    });
})();
