const API_URL = '/api';

// --- UI Logic (Tabs & Logs) ---
document.querySelectorAll('.tab').forEach(tab => {
    tab.addEventListener('click', () => {
        document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(c => {
            c.hidden = true;
            c.classList.remove('active');
        });
        tab.classList.add('active');
        const target = document.getElementById(tab.dataset.tabTarget);
        target.hidden = false;
        target.classList.add('active');
    });
});

const logger = document.getElementById('logger');
function log(method, url, status, responseData) {
    const isError = status >= 400;
    const logItem = document.createElement('article');
    logItem.className = `log-line ${isError ? 'is-error' : ''}`;
    
    let formattedResponse = typeof responseData === 'object' ? JSON.stringify(responseData, null, 2) : responseData;
    
    const timeStr = new Date().toLocaleTimeString();
    logItem.innerHTML = `
        <p class="log-meta">
            <span>[${timeStr}]</span>
            <strong>${method}</strong>
            <span>${url}</span>
            <span>Status: ${status}</span>
        </p>
        <pre class="log-payload">${formattedResponse}</pre>
    `;
    
    const emptyLog = document.querySelector('.empty-log');
    if (emptyLog) emptyLog.remove();
    
    logger.prepend(logItem);
}

document.querySelector('[data-action="clear-logs"]').addEventListener('click', () => {
    logger.innerHTML = '<p class="empty-log">Awaiting commands.</p>';
});

function updateStatus(stepId, text, isError = false) {
    const el = document.getElementById(stepId);
    if (!el) return;
    el.innerText = text;
    el.style.backgroundColor = isError ? 'var(--color-danger)' : 'var(--color-success)';
    el.style.color = '#fff';
}

function updateStateDisplay(base, allowances, deductions, net) {
    document.getElementById('var-base').innerText = base;
    document.getElementById('var-allowances').innerText = allowances;
    document.getElementById('var-deductions').innerText = deductions;
    document.getElementById('var-net').innerText = net;
}

// --- API Calls ---
async function fetchApi(endpoint, method = 'GET', body = null) {
    const options = {
        method,
        headers: { 'Content-Type': 'application/json' }
    };
    if (body) options.body = JSON.stringify(body);
    
    try {
        const res = await fetch(`${API_URL}${endpoint}`, options);
        let data;
        const text = await res.text();
        try { data = text ? JSON.parse(text) : null; } catch(e) { data = text; }
        
        log(method, endpoint, res.status, data);
        return { status: res.status, data };
    } catch (e) {
        log(method, endpoint, 500, e.toString());
        return { status: 500, data: null };
    }
}

// --- Action Bindings ---
document.querySelector('[data-action="update-config"]').addEventListener('click', async () => {
    const empId = document.getElementById('var-emp-input').value;
    if (!empId) return alert('Please enter Employee ID (UUID)');
    
    const payload = {
        employeeId: empId,
        baseSalary: parseFloat(document.getElementById('configBase').value),
        mealAllowance: parseFloat(document.getElementById('configMeal').value),
        transportAllowance: parseFloat(document.getElementById('configTransport').value),
        insuranceDeduction: parseFloat(document.getElementById('configInsurance').value),
        otherDeductions: parseFloat(document.getElementById('configOther').value)
    };
    
    const res = await fetchApi('/SalaryConfigs', 'POST', payload);
    if (res.status === 200 || res.status === 201 || res.status === 204) {
        updateStatus('status1', 'Success');
    } else {
        updateStatus('status1', 'Error', true);
    }
});

document.querySelector('[data-action="get-config"]').addEventListener('click', async () => {
    const empId = document.getElementById('var-emp-input').value;
    if (!empId) return alert('Please enter Employee ID (UUID)');
    
    const res = await fetchApi(`/SalaryConfigs/${empId}`);
    if (res.status === 200 && res.data) {
        updateStatus('status2', 'Fetched');
        const d = res.data;
        updateStateDisplay(
            d.baseSalary, 
            d.mealAllowance + d.transportAllowance, 
            d.insuranceDeduction + d.otherDeductions, 
            '-'
        );
    } else {
        updateStatus('status2', 'Error', true);
    }
});

document.querySelector('[data-action="calc-payroll"]').addEventListener('click', async () => {
    const month = document.getElementById('var-month-input').value;
    const year = document.getElementById('var-year-input').value;
    const empId = document.getElementById('var-emp-input').value;
    
    const payload = {
        month: parseInt(month),
        year: parseInt(year),
        employeeId: empId || null
    };
    
    const res = await fetchApi('/Payrolls/calculate', 'POST', payload);
    if (res.status === 200 || res.status === 204) {
        updateStatus('status3', 'Calculated');
    } else {
        updateStatus('status3', 'Error', true);
    }
});

document.querySelector('[data-action="sim-event"]').addEventListener('click', async () => {
    const month = document.getElementById('var-month-input').value;
    const year = document.getElementById('var-year-input').value;
    const empId = document.getElementById('var-emp-input').value;
    
    const payload = {
        month: parseInt(month),
        year: parseInt(year),
        employeeId: empId || null
    };
    
    const res = await fetchApi('/Payrolls/simulate-event', 'POST', payload);
    if (res.status === 202) {
        updateStatus('status3', 'Event Sent');
        alert("Fired 'attendance.monthly.closed' event to RabbitMQ. The N3 Consumer will run in background to calculate payroll.");
    } else {
        updateStatus('status3', 'Error', true);
    }
});

document.querySelector('[data-action="get-payrolls"]').addEventListener('click', async () => {
    const month = document.getElementById('var-month-input').value;
    const year = document.getElementById('var-year-input').value;
    const empId = document.getElementById('var-emp-input').value;
    
    let url = `/Payrolls?`;
    if (month) url += `month=${month}&`;
    if (year) url += `year=${year}&`;
    if (empId) url += `employeeId=${empId}`;
    
    const res = await fetchApi(url);
    if (res.status === 200) {
        updateStatus('status4', 'Success');
        if (res.data && res.data.length > 0) {
            const p = res.data[res.data.length - 1]; // get the last one to show
            updateStateDisplay(p.baseSalary, p.totalAllowances, p.totalDeductions, p.netSalary);
        }
    } else {
        updateStatus('status4', 'Error', true);
    }
});

document.querySelector('[data-action="get-dashboard"]').addEventListener('click', async () => {
    const month = document.getElementById('var-month-input').value;
    const year = document.getElementById('var-year-input').value;
    
    if (!month || !year) return alert('Please enter Month and Year');
    
    await fetchApi(`/Reports/dashboard?month=${month}&year=${year}`);
});
