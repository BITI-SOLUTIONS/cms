// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/stockTransfers.js
// PROPÓSITO: Lógica cliente para el módulo Stock Transfers
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-12
// ================================================================================

'use strict';

// ── Estado global ──────────────────────────────────────────────────────────────
let st = {
    apiBase: '',
    apiToken: '',
    page: 1,
    pageSize: 10,
    totalPages: 1,
    totalCount: 0,
    transfers: [],
    warehouses: [],
    items: [],
    users: [],
    editingId: null,
    lines: [],           // líneas en el formulario activo
    completeLines: [],   // líneas en el modal de completar
    nextLineNum: 1,
    filter: { search: '', status: '', warehouseOrigin: '', warehouseDest: '', dateFrom: '', dateTo: '' }
};

// ── Init ──────────────────────────────────────────────────────────────────────
function initStockTransfers(apiBase, apiToken) {
    st.apiBase = apiBase.replace(/\/$/, '');
    st.apiToken = apiToken;

    bindEvents();
    Promise.all([
        loadWarehouses(),
        loadUsers()
    ]).then(() => {
        loadTransfers();
    });
}

function bindEvents() {
    // Filtros con debounce
    let searchTimer;
    document.getElementById('stSearch')?.addEventListener('input', e => {
        clearTimeout(searchTimer);
        searchTimer = setTimeout(() => { st.filter.search = e.target.value; st.page = 1; loadTransfers(); }, 400);
    });
    document.getElementById('stFilterStatus')?.addEventListener('change', e => { st.filter.status = e.target.value; st.page = 1; loadTransfers(); });
    document.getElementById('stFilterOrigin')?.addEventListener('change', e => { st.filter.warehouseOrigin = e.target.value; st.page = 1; loadTransfers(); });
    document.getElementById('stFilterDest')?.addEventListener('change', e => { st.filter.warehouseDest = e.target.value; st.page = 1; loadTransfers(); });
    document.getElementById('stDateFrom')?.addEventListener('change', e => { st.filter.dateFrom = e.target.value; st.page = 1; loadTransfers(); });
    document.getElementById('stDateTo')?.addEventListener('change', e => { st.filter.dateTo = e.target.value; st.page = 1; loadTransfers(); });

    // Modal líneas – agregar ítem buscando
    document.getElementById('stItemSearch')?.addEventListener('input', debounce(searchItems, 350));
    document.getElementById('stAddLineBtn')?.addEventListener('click', addLineFromForm);
}

// ── API Helpers ───────────────────────────────────────────────────────────────
async function apiFetch(path, options = {}) {
    const headers = {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${st.apiToken}`,
        ...(options.headers || {})
    };
    const res = await fetch(`${st.apiBase}${path}`, { ...options, headers });
    if (!res.ok) {
        const err = await res.json().catch(() => ({ error: res.statusText }));
        throw new Error(err.error || err.detail || res.statusText);
    }
    return res.status === 204 ? null : res.json();
}

// ── Load Warehouses ───────────────────────────────────────────────────────────
async function loadWarehouses() {
    try {
        const data = await apiFetch('/api/warehouse?pageSize=200&isActive=true');
        st.warehouses = data.items || [];
        populateWarehouseSelects();
    } catch (e) {
        console.warn('No se cargaron bodegas:', e.message);
    }
}

function populateWarehouseSelects() {
    const opts = `<option value="">-- Seleccionar --</option>` +
        st.warehouses.map(w => `<option value="${w.id}">${w.code} – ${w.name}</option>`).join('');

    ['stFilterOrigin', 'stFilterDest', 'stFormOrigin', 'stFormDest'].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.innerHTML = opts;
    });
}

// ── Load Users ────────────────────────────────────────────────────────────────
async function loadUsers() {
    try {
        const data = await apiFetch('/api/user/for-company');
        st.users = data || [];
    } catch (e) {
        console.warn('No se cargaron usuarios:', e.message);
    }
}

// ── Load Transfers ────────────────────────────────────────────────────────────
async function loadTransfers() {
    showLoader(true);
    try {
        const params = new URLSearchParams({
            page: st.page,
            pageSize: st.pageSize,
            ...(st.filter.search && { search: st.filter.search }),
            ...(st.filter.status && { status: st.filter.status }),
            ...(st.filter.warehouseOrigin && { warehouseOriginId: st.filter.warehouseOrigin }),
            ...(st.filter.warehouseDest && { warehouseDestId: st.filter.warehouseDest }),
            ...(st.filter.dateFrom && { dateFrom: st.filter.dateFrom }),
            ...(st.filter.dateTo && { dateTo: st.filter.dateTo }),
        });

        const data = await apiFetch(`/api/stocktransfer?${params}`);
        st.transfers = data.items || [];
        st.totalCount = data.totalCount || 0;
        st.totalPages = data.totalPages || 1;

        renderTable();
        renderPagination();
        updateCount();
    } catch (e) {
        showAlert('danger', `Error al cargar traslados: ${e.message}`);
    } finally {
        showLoader(false);
    }
}

// ── Render Table ──────────────────────────────────────────────────────────────
function renderTable() {
    const tbody = document.getElementById('stTableBody');
    if (!tbody) return;

    if (st.transfers.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="7" class="text-center py-5">
                    <i class="bi bi-inbox display-4 d-block mb-2" style="color:#475569;"></i>
                    <span style="color:#64748b;">No hay traslados que mostrar</span>
                </td>
            </tr>`;
        return;
    }

    tbody.innerHTML = st.transfers.map(t => `
        <tr class="st-row" data-id="${t.id}" onclick="openDetail(${t.id})">
            <td>
                <span class="fw-semibold text-white">${esc(t.transferNumber)}</span>
                ${t.reference ? `<br><small style="color:#94a3b8;">${esc(t.reference)}</small>` : ''}
            </td>
            <td>
                <div class="d-flex align-items-center gap-2">
                    <div>
                        <div style="color:#e2e8f0;font-size:.82rem;">${esc(t.originWarehouseName || '—')}</div>
                        <i class="bi bi-arrow-right" style="color:#6366f1;"></i>
                        <div style="color:#e2e8f0;font-size:.82rem;">${esc(t.destWarehouseName || '—')}</div>
                    </div>
                </div>
            </td>
            <td><span style="color:#94a3b8;">${formatDate(t.transferDate)}</span></td>
            <td><span style="color:#94a3b8;">${esc(t.requestedByName || '—')}</span></td>
            <td>${statusBadge(t.status)}</td>
            <td class="text-end">
                <div class="d-flex justify-content-end gap-2 action-btns">
                    ${actionButtons(t)}
                </div>
            </td>
        </tr>`).join('');
}

function statusBadge(status) {
    const map = {
        'Pending':    { color: '#f59e0b', bg: 'rgba(245,158,11,.15)',  icon: 'bi-clock',              label: 'Pendiente'   },
        'InProgress': { color: '#3b82f6', bg: 'rgba(59,130,246,.15)',   icon: 'bi-arrow-repeat',       label: 'En Tránsito' },
        'Completed':  { color: '#22c55e', bg: 'rgba(34,197,94,.15)',    icon: 'bi-check-circle',       label: 'Completado'  },
        'Cancelled':  { color: '#ef4444', bg: 'rgba(239,68,68,.15)',    icon: 'bi-x-circle',           label: 'Cancelado'   },
    };
    const s = map[status] || { color: '#94a3b8', bg: 'rgba(148,163,184,.15)', icon: 'bi-question-circle', label: status };
    return `<span style="display:inline-flex;align-items:center;gap:.3rem;padding:.25rem .6rem;border-radius:8px;font-size:.75rem;font-weight:600;background:${s.bg};color:${s.color};">
        <i class="bi ${s.icon}"></i>${s.label}</span>`;
}

function actionButtons(t) {
    let btns = `<button class="btn btn-sm" style="background:rgba(99,102,241,.15);color:#818cf8;border:1px solid rgba(99,102,241,.3);" onclick="event.stopPropagation();openDetail(${t.id})" title="Ver detalle"><i class="bi bi-eye"></i></button>`;
    if (t.status === 'Pending') {
        btns += `<button class="btn btn-sm" style="background:rgba(99,102,241,.15);color:#818cf8;border:1px solid rgba(99,102,241,.3);" onclick="event.stopPropagation();openEdit(${t.id})" title="Editar"><i class="bi bi-pencil"></i></button>`;
        btns += `<button class="btn btn-sm" style="background:rgba(34,197,94,.15);color:#22c55e;border:1px solid rgba(34,197,94,.3);" onclick="event.stopPropagation();approveTransfer(${t.id})" title="Aprobar"><i class="bi bi-check-lg"></i></button>`;
        btns += `<button class="btn btn-sm" style="background:rgba(239,68,68,.15);color:#ef4444;border:1px solid rgba(239,68,68,.3);" onclick="event.stopPropagation();promptCancel(${t.id})" title="Cancelar"><i class="bi bi-x-lg"></i></button>`;
    }
    if (t.status === 'InProgress') {
        btns += `<button class="btn btn-sm" style="background:rgba(34,197,94,.15);color:#22c55e;border:1px solid rgba(34,197,94,.3);" onclick="event.stopPropagation();openComplete(${t.id})" title="Completar recepción"><i class="bi bi-box-arrow-in-down"></i></button>`;
        btns += `<button class="btn btn-sm" style="background:rgba(239,68,68,.15);color:#ef4444;border:1px solid rgba(239,68,68,.3);" onclick="event.stopPropagation();promptCancel(${t.id})" title="Cancelar"><i class="bi bi-x-lg"></i></button>`;
    }
    return btns;
}

// ── Pagination ────────────────────────────────────────────────────────────────
function renderPagination() {
    const el = document.getElementById('stPagination');
    if (!el) return;
    if (st.totalPages <= 1) { el.innerHTML = ''; return; }

    let html = '';
    const addBtn = (page, label, disabled = false, active = false) => {
        html += `<li class="page-item${disabled ? ' disabled' : ''}${active ? ' active' : ''}">
            <button class="page-link" ${disabled ? '' : `onclick="goPage(${page})"`}>${label}</button></li>`;
    };

    addBtn(st.page - 1, '<i class="bi bi-chevron-left"></i>', st.page <= 1);
    const start = Math.max(1, st.page - 2), end = Math.min(st.totalPages, start + 4);
    for (let p = start; p <= end; p++) addBtn(p, p, false, p === st.page);
    addBtn(st.page + 1, '<i class="bi bi-chevron-right"></i>', st.page >= st.totalPages);

    el.innerHTML = html;
}

function goPage(p) { st.page = p; loadTransfers(); }

function updateCount() {
    const el = document.getElementById('stCount');
    if (el) el.textContent = `${st.totalCount} traslado${st.totalCount !== 1 ? 's' : ''}`;
    if (typeof updateKpiChips === 'function') updateKpiChips();
}

// ── Create / Edit Modal ───────────────────────────────────────────────────────
async function openCreate() {
    st.editingId = null;
    st.lines = [];
    st.nextLineNum = 1;
    resetForm();
    document.getElementById('stModalTitle').textContent = 'Nuevo Traslado';

    try {
        const data = await apiFetch('/api/stocktransfer/next-number');
        document.getElementById('stFormNumber').value = data.transferNumber;
    } catch (e) { console.warn('Error obteniendo número:', e.message); }

    renderLines();
    showModal('stModal');
}

async function openEdit(id) {
    st.editingId = id;
    try {
        const t = await apiFetch(`/api/stocktransfer/${id}`);
        document.getElementById('stModalTitle').textContent = `Editar — ${t.transferNumber}`;

        document.getElementById('stFormNumber').value = t.transferNumber;
        document.getElementById('stFormReference').value = t.reference || '';
        document.getElementById('stFormNotes').value = t.notes || '';
        document.getElementById('stFormDate').value = t.transferDate || '';
        document.getElementById('stFormExpected').value = t.expectedDate || '';

        await tick();
        document.getElementById('stFormOrigin').value = t.idWarehouseOrigin;
        document.getElementById('stFormDest').value = t.idWarehouseDest;

        st.lines = (t.lines || []).map(l => ({ ...l }));
        st.nextLineNum = st.lines.length + 1;
        renderLines();
        showModal('stModal');
    } catch (e) {
        showAlert('danger', `Error cargando traslado: ${e.message}`);
    }
}

function resetForm() {
    document.getElementById('stForm').reset();
    document.getElementById('stFormDate').value = todayStr();
    document.getElementById('stFormNumber').value = '';
    st.lines = [];
    st.nextLineNum = 1;
}

// ── Lines ─────────────────────────────────────────────────────────────────────
async function searchItems(e) {
    const term = e.target.value.trim();
    if (term.length < 2) { document.getElementById('stItemResults').innerHTML = ''; return; }
    try {
        const data = await apiFetch(`/api/item?search=${encodeURIComponent(term)}&pageSize=10&isActive=true`);
        renderItemResults(data.items || []);
    } catch (e) { console.warn('Error buscando items:', e.message); }
}

function renderItemResults(items) {
    const el = document.getElementById('stItemResults');
    if (!el) return;
    el.innerHTML = items.map(i => `
        <button type="button" class="list-group-item list-group-item-action"
            style="background:#1e293b;color:#e2e8f0;border-color:rgba(255,255,255,.08);"
            onclick="selectItem(${i.id},'${esc(i.code)}','${esc(i.name)}','${esc(i.unitOfMeasureCode||'')}',${i.idUnitOfMeasure||0})">
            <span class="fw-semibold">${esc(i.code)}</span>
            <small class="d-block" style="color:#94a3b8;">${esc(i.name)}</small>
        </button>`).join('');
}

function selectItem(id, code, name, uomCode, uomId) {
    document.getElementById('stLineItemId').value = id;
    document.getElementById('stLineItemCode').value = code;
    document.getElementById('stLineItemName').value = name;
    document.getElementById('stLineUomCode').value = uomCode;
    document.getElementById('stLineUomId').value = uomId;
    document.getElementById('stItemSearch').value = `${code} – ${name}`;
    document.getElementById('stItemResults').innerHTML = '';
    document.getElementById('stLineQty').focus();
}

function addLineFromForm() {
    const itemId = parseInt(document.getElementById('stLineItemId').value || '0');
    const itemCode = document.getElementById('stLineItemCode').value.trim();
    const itemName = document.getElementById('stLineItemName').value.trim();
    const qty = parseFloat((document.getElementById('stLineQty').value || '0').replace(',', '.'));
    const uomCode = document.getElementById('stLineUomCode').value;
    const uomId = parseInt(document.getElementById('stLineUomId').value || '0') || null;
    const lot = document.getElementById('stLineLot').value.trim();
    const notes = document.getElementById('stLineNotes').value.trim();

    if (!itemId || !itemCode || qty <= 0) {
        showAlert('warning', 'Seleccione un artículo y especifique una cantidad mayor a 0.');
        return;
    }

    st.lines.push({ id: 0, lineNumber: st.nextLineNum++, idItem: itemId, itemCode, itemName, qtyRequested: qty, qtyTransferred: 0, unitOfMeasureCode: uomCode, idUnitOfMeasure: uomId, lotNumber: lot || null, notes: notes || null });
    renderLines();
    clearLineForm();
}

function clearLineForm() {
    ['stItemSearch', 'stLineItemId', 'stLineItemCode', 'stLineItemName', 'stLineQty', 'stLineLot', 'stLineNotes', 'stLineUomCode', 'stLineUomId'].forEach(id => {
        const el = document.getElementById(id);
        if (el) el.value = '';
    });
    document.getElementById('stItemResults').innerHTML = '';
    document.getElementById('stItemSearch')?.focus();
}

function removeLine(idx) {
    st.lines.splice(idx, 1);
    st.lines.forEach((l, i) => l.lineNumber = i + 1);
    st.nextLineNum = st.lines.length + 1;
    renderLines();
}

function renderLines() {
    const el = document.getElementById('stLinesBody');
    if (!el) return;

    if (st.lines.length === 0) {
        el.innerHTML = `<tr><td colspan="4" class="text-center py-4" style="color:#475569;background:var(--st-surface1,#1e293b);">
            <i class="bi bi-box-seam me-2"></i>Sin artículos — use el buscador para agregar</td></tr>`;
        return;
    }

    el.innerHTML = st.lines.map((l, i) => `
        <tr>
            <td>
                <div class="fw-semibold" style="color:#e2e8f0;">${esc(l.itemCode)}</div>
                <div style="color:#94a3b8;font-size:.82rem;">${esc(l.itemName)}</div>
            </td>
            <td class="text-end">
                <span class="fw-bold" style="color:#e2e8f0;font-size:1rem;">${fmt(l.qtyRequested)}</span>
                ${l.unitOfMeasureCode ? `<small style="color:#64748b;"> ${esc(l.unitOfMeasureCode)}</small>` : ''}
            </td>
            <td style="color:#94a3b8;">${esc(l.lotNumber || '—')}</td>
            <td class="text-center">
                <button type="button" class="btn btn-sm" style="background:rgba(239,68,68,.15);color:#ef4444;border:1px solid rgba(239,68,68,.3);min-width:38px;min-height:38px;" onclick="removeLine(${i})" title="Quitar línea"><i class="bi bi-trash3"></i></button>
            </td>
        </tr>`).join('');

    const countEl = document.getElementById('stLinesCount');
    if (countEl) countEl.textContent = `${st.lines.length} artículo${st.lines.length !== 1 ? 's' : ''}`;
}

// ── Save ──────────────────────────────────────────────────────────────────────
async function saveTransfer() {
    const form = document.getElementById('stForm');
    if (!form.checkValidity()) { form.reportValidity(); return; }

    if (st.lines.length === 0) {
        showAlert('warning', 'Debe agregar al menos un artículo al traslado.');
        return;
    }

    const payload = {
        transferNumber: document.getElementById('stFormNumber').value.trim(),
        reference: document.getElementById('stFormReference').value.trim() || null,
        notes: document.getElementById('stFormNotes').value.trim() || null,
        idWarehouseOrigin: parseInt(document.getElementById('stFormOrigin').value),
        idWarehouseDest: parseInt(document.getElementById('stFormDest').value),
        transferDate: document.getElementById('stFormDate').value,
        expectedDate: document.getElementById('stFormExpected').value || null,
        requestedBy: 0,
        lines: st.lines.map(l => ({
            idItem: l.idItem,
            itemCode: l.itemCode,
            itemName: l.itemName,
            qtyRequested: l.qtyRequested,
            idUnitOfMeasure: l.idUnitOfMeasure || null,
            unitOfMeasureCode: l.unitOfMeasureCode || null,
            lotNumber: l.lotNumber || null,
            notes: l.notes || null
        }))
    };

    if (payload.idWarehouseOrigin === payload.idWarehouseDest) {
        showAlert('warning', 'La bodega de origen y destino no pueden ser la misma.');
        return;
    }

    setSavingState(true);
    try {
        if (st.editingId) {
            await apiFetch(`/api/stocktransfer/${st.editingId}`, { method: 'PUT', body: JSON.stringify(payload) });
            showAlert('success', 'Traslado actualizado correctamente.');
        } else {
            await apiFetch('/api/stocktransfer', { method: 'POST', body: JSON.stringify(payload) });
            showAlert('success', 'Traslado creado correctamente.');
        }
        hideModal('stModal');
        loadTransfers();
    } catch (e) {
        showAlert('danger', `Error al guardar: ${e.message}`);
    } finally {
        setSavingState(false);
    }
}

// ── Approve ───────────────────────────────────────────────────────────────────
async function approveTransfer(id) {
    if (!confirm('¿Aprobar este traslado y marcarlo como En Tránsito?')) return;
    try {
        await apiFetch(`/api/stocktransfer/${id}/approve`, { method: 'POST' });
        showAlert('success', 'Traslado aprobado — mercancía en tránsito.');
        loadTransfers();
    } catch (e) {
        showAlert('danger', `Error al aprobar: ${e.message}`);
    }
}

// ── Complete ──────────────────────────────────────────────────────────────────
async function openComplete(id) {
    try {
        const t = await apiFetch(`/api/stocktransfer/${id}`);
        document.getElementById('stCompleteTitle').textContent = `Completar — ${t.transferNumber}`;
        document.getElementById('stCompleteId').value = id;

        st.completeLines = (t.lines || []).map(l => ({ ...l, qtyTransferred: l.qtyRequested }));
        renderCompleteLines();
        showModal('stCompleteModal');
    } catch (e) {
        showAlert('danger', `Error cargando traslado: ${e.message}`);
    }
}

function renderCompleteLines() {
    const el = document.getElementById('stCompleteLinesBody');
    if (!el) return;

    el.innerHTML = st.completeLines.map((l, i) => `
        <tr>
            <td><span class="fw-semibold text-white">${esc(l.itemCode)}</span><br><small style="color:#94a3b8;">${esc(l.itemName)}</small></td>
            <td class="text-end" style="color:#94a3b8;">${fmt(l.qtyRequested)} ${esc(l.unitOfMeasureCode||'')}</td>
            <td>
                <input type="number" class="form-control form-control-sm text-end"
                    value="${l.qtyTransferred}" min="0" step="0.0001"
                    onchange="st.completeLines[${i}].qtyTransferred = parseFloat(this.value)||0"
                    style="width:100px;margin-left:auto;">
            </td>
        </tr>`).join('');
}

async function confirmComplete() {
    const id = parseInt(document.getElementById('stCompleteId').value);
    const payload = { lines: st.completeLines.map(l => ({ id: l.id, qtyTransferred: l.qtyTransferred })) };
    try {
        await apiFetch(`/api/stocktransfer/${id}/complete`, { method: 'POST', body: JSON.stringify(payload) });
        showAlert('success', 'Traslado completado — stock registrado en bodega destino.');
        hideModal('stCompleteModal');
        loadTransfers();
    } catch (e) {
        showAlert('danger', `Error al completar: ${e.message}`);
    }
}

// ── Cancel ────────────────────────────────────────────────────────────────────
function promptCancel(id) {
    document.getElementById('stCancelId').value = id;
    document.getElementById('stCancelReason').value = '';
    showModal('stCancelModal');
}

async function confirmCancel() {
    const id = parseInt(document.getElementById('stCancelId').value);
    const reason = document.getElementById('stCancelReason').value.trim();
    if (!reason) { document.getElementById('stCancelReason').focus(); return; }
    try {
        await apiFetch(`/api/stocktransfer/${id}/cancel`, { method: 'POST', body: JSON.stringify({ cancelReason: reason }) });
        showAlert('success', 'Traslado cancelado.');
        hideModal('stCancelModal');
        loadTransfers();
    } catch (e) {
        showAlert('danger', `Error al cancelar: ${e.message}`);
    }
}

// ── Detail ────────────────────────────────────────────────────────────────────
async function openDetail(id) {
    try {
        const t = await apiFetch(`/api/stocktransfer/${id}`);
        const el = document.getElementById('stDetailBody');
        if (!el) return;

        el.innerHTML = `
            <div class="row g-3 mb-3">
                <div class="col-6 col-md-3">
                    <div class="detail-label">Número</div>
                    <div class="detail-value fw-bold">${esc(t.transferNumber)}</div>
                </div>
                <div class="col-6 col-md-3">
                    <div class="detail-label">Estado</div>
                    <div>${statusBadge(t.status)}</div>
                </div>
                <div class="col-6 col-md-3">
                    <div class="detail-label">Fecha Traslado</div>
                    <div class="detail-value">${formatDate(t.transferDate)}</div>
                </div>
                <div class="col-6 col-md-3">
                    <div class="detail-label">Fecha Esperada</div>
                    <div class="detail-value">${formatDate(t.expectedDate) || '—'}</div>
                </div>
            </div>
            <div class="row g-3 mb-3">
                <div class="col-md-6">
                    <div class="detail-label">Bodega Origen</div>
                    <div class="detail-value"><i class="bi bi-box-seam me-1" style="color:#f59e0b;"></i>${esc(t.originWarehouseName||'—')}</div>
                </div>
                <div class="col-md-6">
                    <div class="detail-label">Bodega Destino</div>
                    <div class="detail-value"><i class="bi bi-box-arrow-in-down me-1" style="color:#22c55e;"></i>${esc(t.destWarehouseName||'—')}</div>
                </div>
            </div>
            <div class="row g-3 mb-3">
                <div class="col-md-4">
                    <div class="detail-label">Solicitado por</div>
                    <div class="detail-value">${esc(t.requestedByName||'—')}</div>
                </div>
                <div class="col-md-4">
                    <div class="detail-label">Aprobado por</div>
                    <div class="detail-value">${esc(t.approvedByName||'—')}</div>
                </div>
                <div class="col-md-4">
                    <div class="detail-label">Ejecutado por</div>
                    <div class="detail-value">${esc(t.executedByName||'—')}</div>
                </div>
            </div>
            ${t.reference ? `<div class="mb-3"><div class="detail-label">Referencia</div><div class="detail-value">${esc(t.reference)}</div></div>` : ''}
            ${t.notes ? `<div class="mb-3"><div class="detail-label">Notas</div><div class="detail-value">${esc(t.notes)}</div></div>` : ''}
            ${t.cancelReason ? `<div class="mb-3"><div class="detail-label">Motivo Cancelación</div><div class="detail-value text-danger">${esc(t.cancelReason)}</div></div>` : ''}

            <hr style="border-color:rgba(255,255,255,.1);">
            <h6 style="color:#818cf8;font-size:.8rem;text-transform:uppercase;letter-spacing:.06em;">
                <i class="bi bi-list-ul me-1"></i>Artículos
            </h6>
            <div class="table-responsive">
                <table class="table table-sm" style="color:#e2e8f0;">
                    <thead><tr style="color:#64748b;font-size:.78rem;">
                        <th>Artículo</th><th class="text-end">Solicitado</th><th class="text-end">Transferido</th><th>Lote</th>
                    </tr></thead>
                    <tbody>
                    ${(t.lines||[]).map(l => `
                        <tr>
                            <td><span class="fw-semibold">${esc(l.itemCode)}</span><br><small style="color:#94a3b8;">${esc(l.itemName)}</small></td>
                            <td class="text-end">${fmt(l.qtyRequested)} <small style="color:#64748b;">${esc(l.unitOfMeasureCode||'')}</small></td>
                            <td class="text-end">${fmt(l.qtyTransferred)} <small style="color:#64748b;">${esc(l.unitOfMeasureCode||'')}</small></td>
                            <td style="color:#94a3b8;">${esc(l.lotNumber||'—')}</td>
                        </tr>`).join('')}
                    </tbody>
                </table>
            </div>`;

        document.getElementById('stDetailActions').innerHTML = actionButtons(t);
        showModal('stDetailModal');
    } catch (e) {
        showAlert('danger', `Error cargando detalle: ${e.message}`);
    }
}

// ── Utilities ─────────────────────────────────────────────────────────────────
function showModal(id) {
    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById(id));
    modal.show();
}
function hideModal(id) {
    const modal = bootstrap.Modal.getInstance(document.getElementById(id));
    if (modal) modal.hide();
}
function showLoader(v) {
    const el = document.getElementById('stLoader');
    if (el) el.classList.toggle('d-none', !v);
}
function showAlert(type, msg) {
    const el = document.getElementById('stAlert');
    if (!el) return;
    el.className = `alert alert-${type} d-block`;
    el.innerHTML = msg;
    setTimeout(() => el.classList.add('d-none'), 5000);
}
function setSavingState(saving) {
    const btn = document.getElementById('stSaveBtn');
    if (!btn) return;
    btn.disabled = saving;
    btn.innerHTML = saving ? '<span class="spinner-border spinner-border-sm me-2"></span>Guardando…' : '<i class="bi bi-check-lg me-2"></i>Guardar';
}
function esc(s) { return String(s || '').replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;'); }
function fmt(n) { return Number(n || 0).toLocaleString('es-CR', { minimumFractionDigits: 0, maximumFractionDigits: 4 }); }
function formatDate(d) { if (!d) return ''; try { return new Date(d + 'T00:00:00').toLocaleDateString('es-CR'); } catch { return d; } }
function todayStr() { return new Date().toISOString().slice(0, 10); }
function debounce(fn, ms) { let t; return (...a) => { clearTimeout(t); t = setTimeout(() => fn(...a), ms); }; }
function tick() { return new Promise(r => setTimeout(r, 0)); }
