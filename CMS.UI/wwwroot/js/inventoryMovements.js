// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/inventoryMovements.js
// PROPÓSITO: Lógica cliente para la pantalla de Inventory Movements
// DESCRIPCIÓN: Gestiona la visualización, creación y flujo completo de movimientos
//              de inventario incluyendo el modo de tránsito (vehículo → bodegas).
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-13
// ================================================================================

'use strict';

// ============================================================
// CONFIGURACIÓN GLOBAL
// ============================================================

const INV = {
    api:       window.INV_API   || '',
    token:     window.INV_TOKEN || '',
    page:      1,
    pageSize:  20,
    totalPages: 1,
    // Filtros activos
    filters:   { search: '', type: '', status: '', dateFrom: '', dateTo: '' },
    // Datos cacheados
    warehouses: [],
    items:      [],
    // Estado del modal
    editingId:  null,
    lines:      [],        // líneas editables [{...}]
    _groups:    [],        // grupos de destino en modo tránsito [{destId, sealDest, departureTime, arrivalTime, odometerOut}]
    // Movimiento actual en detalle
    currentTxn: null,
    cancelTargetId: null,
};

// Constantes de tipo y estado
const MOVEMENT_TYPES = {
    Transfer:         { label: 'Traslado',        icon: '📦', css: 'tp-Transfer'         },
    TransitTransfer:  { label: 'Tránsito',         icon: '🚛', css: 'tp-TransitTransfer'  },
    PurchaseReceipt:  { label: 'Entrada Compra',   icon: '🟢', css: 'tp-PurchaseReceipt'  },
    SaleIssue:        { label: 'Salida Venta',     icon: '🔴', css: 'tp-SaleIssue'        },
    AdjustmentIn:     { label: 'Ajuste (+)',       icon: '➕', css: 'tp-AdjustmentIn'     },
    AdjustmentOut:    { label: 'Ajuste (-)',       icon: '➖', css: 'tp-AdjustmentOut'    },
    CustomerReturn:   { label: 'Dev. Cliente',     icon: '↩️', css: 'tp-CustomerReturn'   },
    SupplierReturn:   { label: 'Dev. Proveedor',   icon: '↪️', css: 'tp-SupplierReturn'   },
    WriteOff:         { label: 'Merma',            icon: '🗑️', css: 'tp-WriteOff'         },
    PhysicalCount:    { label: 'Conteo Físico',    icon: '📋', css: 'tp-PhysicalCount'    },
};

const STATUS_META = {
    Draft:             { label: 'Borrador',       icon: '📝', css: 'st-Draft'             },
    Confirmed:         { label: 'Confirmado',     icon: '✔️', css: 'st-Confirmed'         },
    InTransit:         { label: 'En Tránsito',    icon: '🚛', css: 'st-InTransit'         },
    PartiallyReceived: { label: 'Parcial',        icon: '⏳', css: 'st-PartiallyReceived' },
    Completed:         { label: 'Completado',     icon: '✅', css: 'st-Completed'         },
    Cancelled:         { label: 'Cancelado',      icon: '❌', css: 'st-Cancelled'         },
};

// ============================================================
// FETCH HELPER
// ============================================================

async function invFetch(path, options = {}) {
    const url = `${INV.api}${path}`;
    const opts = {
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${INV.token}`,
        },
        ...options,
    };
    if (opts.body && typeof opts.body !== 'string') opts.body = JSON.stringify(opts.body);
    const res = await fetch(url, opts);
    if (!res.ok) {
        let msg = `HTTP ${res.status}`;
        try { const d = await res.json(); msg = d.error || d.title || msg; } catch {}
        throw new Error(msg);
    }
    return res.json();
}

// ============================================================
// INIT
// ============================================================

document.addEventListener('DOMContentLoaded', () => {
    loadWarehouses();
    loadMovements();
    bindEvents();
});

// ============================================================
// CARGAR BODEGAS (caché)
// ============================================================

async function loadWarehouses() {
    try {
        const data = await invFetch('/api/warehouse?isActive=true&pageSize=200');
        INV.warehouses = data.items || data || [];
    } catch (e) {
        console.warn('No se pudieron cargar bodegas:', e);
    }
}

// ============================================================
// LISTAR MOVIMIENTOS
// ============================================================

async function loadMovements(page = 1) {
    INV.page = page;
    const tb = document.getElementById('invTableBody');
    tb.innerHTML = '<tr><td colspan="8" class="text-center py-4" style="color:#cbd5e1;">Cargando…</td></tr>';

    try {
        const params = new URLSearchParams({
            page,
            pageSize: INV.pageSize,
        });
        if (INV.filters.search)   params.set('search',      INV.filters.search);
        if (INV.filters.type)     params.set('movementType', INV.filters.type);
        if (INV.filters.status)   params.set('status',       INV.filters.status);
        if (INV.filters.dateFrom) params.set('dateFrom',     INV.filters.dateFrom);
        if (INV.filters.dateTo)   params.set('dateTo',       INV.filters.dateTo);

        const data = await invFetch(`/api/inventorytransaction?${params}`);
        const items = data.items || [];
        INV.totalPages = data.totalPages || 1;

        renderTable(items);
        renderPagination(data.totalCount || 0, data.page, data.totalPages);
        updateKpis(items, data.totalCount);
    } catch (e) {
        tb.innerHTML = `<tr><td colspan="8" class="text-center py-4 text-danger">${e.message}</td></tr>`;
    }
}

// ============================================================
// RENDER TABLA
// ============================================================

function renderTable(items) {
    const tb = document.getElementById('invTableBody');
    if (!items.length) {
        tb.innerHTML = '<tr><td colspan="8" class="text-center py-4 text-muted">No se encontraron movimientos.</td></tr>';
        return;
    }

    tb.innerHTML = items.map(t => {
        const typeM = MOVEMENT_TYPES[t.movementType] || { label: t.movementType, icon: '📦', css: '' };
        const statM = STATUS_META[t.status] || { label: t.status, icon: '', css: '' };
        const originName = warehouseName(t.idWarehouseOrigin);
        const destName   = t.idWarehouseDest ? warehouseName(t.idWarehouseDest) : '—';

        return `<tr onclick="openDetail(${t.id})" title="Ver detalle">
            <td>
                <span class="fw-semibold text-white">${t.transactionNumber}</span>
                ${t.isTransitTransfer ? '<span class="ms-1 badge-type tp-TransitTransfer" style="font-size:.65rem;padding:.1rem .3rem;border-radius:4px;"><i class="bi bi-truck"></i></span>' : ''}
            </td>
            <td><span class="badge-type ${typeM.css}">${typeM.icon} ${typeM.label}</span></td>
            <td><span class="badge-status ${statM.css}">${statM.icon} ${statM.label}</span></td>
            <td style="color:#cbd5e1;">${t.transactionDate || '—'}</td>
            <td class="text-light">${originName}</td>
            <td class="text-light">${destName}</td>
            <td class="hide-mobile" style="color:#94a3b8;">${t.reference || '—'}</td>
            <td onclick="event.stopPropagation()">
                ${actionButtons(t)}
            </td>
        </tr>`;
    }).join('');
}

function actionButtons(t) {
    const btns = [];
    if (t.status === 'Draft') {
        btns.push(`<button class="btn btn-sm btn-outline-warning me-1 btn-inv" onclick="openEdit(${t.id})" title="Editar"><i class="bi bi-pencil"></i></button>`);
        btns.push(`<button class="btn btn-sm btn-outline-info btn-inv" onclick="confirmTxn(${t.id})" title="Confirmar"><i class="bi bi-check2-circle"></i></button>`);
    }
    if (t.status === 'InTransit' || t.status === 'PartiallyReceived') {
        btns.push(`<button class="btn btn-sm btn-outline-success btn-inv" onclick="openReceive(${t.id})" title="Recibir"><i class="bi bi-box-arrow-in-down"></i></button>`);
    }
    if (t.status === 'Confirmed') {
        btns.push(`<button class="btn btn-sm btn-outline-success btn-inv" onclick="completeTxn(${t.id})" title="Completar"><i class="bi bi-check-all"></i></button>`);
    }
    if (!['Completed','Cancelled'].includes(t.status)) {
        btns.push(`<button class="btn btn-sm btn-outline-danger btn-inv" onclick="openCancel(${t.id})" title="Cancelar"><i class="bi bi-x-circle"></i></button>`);
    }
    return btns.join('');
}

// ============================================================
// KPIs
// ============================================================

function updateKpis(items, total) {
    document.getElementById('kpiTotal').textContent     = total;
    document.getElementById('kpiDraft').textContent     = items.filter(i => i.status === 'Draft').length;
    document.getElementById('kpiTransit').textContent   = items.filter(i => i.status === 'InTransit').length;
    document.getElementById('kpiPartial').textContent   = items.filter(i => i.status === 'PartiallyReceived').length;
    document.getElementById('kpiCompleted').textContent = items.filter(i => i.status === 'Completed').length;
}

// ============================================================
// PAGINACIÓN
// ============================================================

function renderPagination(total, page, totalPages) {
    document.getElementById('pageInfo').textContent =
        `Mostrando página ${page} de ${totalPages} (${total} registros)`;

    const ul = document.getElementById('pagination');
    ul.innerHTML = '';

    const addPage = (label, p, disabled = false, active = false) => {
        const li = document.createElement('li');
        li.className = `page-item${disabled ? ' disabled' : ''}${active ? ' active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#">${label}</a>`;
        if (!disabled) li.addEventListener('click', e => { e.preventDefault(); loadMovements(p); });
        ul.appendChild(li);
    };

    addPage('‹', page - 1, page <= 1);
    const start = Math.max(1, page - 2), end = Math.min(totalPages, page + 2);
    for (let p = start; p <= end; p++) addPage(p, p, false, p === page);
    addPage('›', page + 1, page >= totalPages);
}

// ============================================================
// ABRIR MODAL NUEVO
// ============================================================

async function openCreate() {
    INV.editingId = null;
    INV.lines = [];

    document.getElementById('invModalTitle').innerHTML =
        '<i class="bi bi-plus-circle me-2 text-warning"></i>Nuevo Movimiento';
    document.getElementById('modalAlert').classList.add('d-none');

    // Número automático
    try {
        const { transactionNumber } = await invFetch('/api/inventorytransaction/next-number');
        document.getElementById('fldNumber').value = transactionNumber;
    } catch { document.getElementById('fldNumber').value = ''; }

    const today = new Date().toISOString().slice(0, 10);
    document.getElementById('fldDate').value    = today;
    document.getElementById('fldDate').min      = today;
    document.getElementById('fldType').value    = 'Transfer';
    document.getElementById('fldOrigin').value  = '';
    document.getElementById('fldDest').value    = '';
    document.getElementById('fldArrival').value = today;
    document.getElementById('fldArrival').min   = today;
    document.getElementById('fldReference').value = '';
    document.getElementById('fldNotes').value   = '';
    document.getElementById('fldSecuritySeal').value = '';
    document.getElementById('sealError').classList.add('d-none');
    document.getElementById('fldDepartureTime').value = '';
    document.getElementById('fldArrivalTime').value = '';
    document.getElementById('fldOdometerOut').value = '';

    INV._groups = [{ destId: null, sealDest: '', departureTime: '', arrivalTime: '', odometerOut: '' }];
    populateWarehouseSelects();
    updateTransitUI();
    renderLines();

    document.getElementById('btnSaveConfirm').classList.remove('d-none');
    bootstrap.Modal.getOrCreateInstance(document.getElementById('invModal')).show();
}

// ============================================================
// ABRIR MODAL EDITAR
// ============================================================

async function openEdit(id) {
    INV.editingId = id;
    INV.lines = [];

    document.getElementById('invModalTitle').innerHTML =
        '<i class="bi bi-pencil me-2 text-warning"></i>Editar Movimiento';
    document.getElementById('modalAlert').classList.add('d-none');

    try {
        const txn = await invFetch(`/api/inventorytransaction/${id}`);
        const lines = txn.lines || await invFetch(`/api/inventorytransaction/${id}/lines`);

        document.getElementById('fldNumber').value    = txn.transactionNumber;
        document.getElementById('fldDate').value      = txn.transactionDate;
        document.getElementById('fldDate').min        = '';
        document.getElementById('fldArrival').value   = txn.expectedArrivalDate || '';
        document.getElementById('fldArrival').min     = txn.transactionDate || '';
        document.getElementById('fldType').value      = txn.movementType;
        document.getElementById('fldReference').value = txn.reference || '';
        document.getElementById('fldNotes').value     = txn.notes || '';
        document.getElementById('fldSecuritySeal').value = txn.securitySeal || '';
        document.getElementById('sealError').classList.add('d-none');
        document.getElementById('fldDepartureTime').value = txn.departureTime || '';
        document.getElementById('fldArrivalTime').value   = txn.arrivalTime   || '';
        document.getElementById('fldOdometerOut').value   = txn.odometerOut   != null ? txn.odometerOut : '';

        // Guardar IDs a restaurar ANTES de que updateTransitUI() reconstruya los selects
        const savedDestId   = txn.idWarehouseDest || '';
        const savedOriginId = txn.idWarehouseOrigin || '';

        INV.lines = (lines || []).map(l => ({
            id:                   l.id || 0,
            idItem:               l.idItem,
            itemCode:             l.itemCode,
            itemName:             l.itemName,
            qtyRequested:         l.qtyRequested,
            idWarehouseDestLine:   l.idWarehouseDestLine || null,
            idUnitOfMeasure:      l.idUnitOfMeasure || null,
            unitOfMeasureCode:    l.unitOfMeasureCode || '',
            unitCost:             l.unitCost || null,
            lotNumber:            l.lotNumber || '',
            notes:                l.notes || '',
        }));

        // Reconstruir grupos a partir de los destinos únicos de las líneas
        const isTransitMode = txn.movementType === 'TransitTransfer';
        if (isTransitMode && INV.lines.length > 0) {
            const seenDests = [];
            INV.lines.forEach(l => {
                const d = l.idWarehouseDestLine || null;
                if (!seenDests.includes(d)) seenDests.push(d);
            });
            // Tomar campos del grupo desde la primera línea de ese destino
            INV._groups = seenDests.map(d => {
                const firstLine = INV.lines.find(l => l.idWarehouseDestLine === d) || {};
                return {
                    destId:        d,
                    sealDest:      firstLine.destSecuritySeal || '',
                    departureTime: firstLine.departureTime    || '',
                    arrivalTime:   firstLine.arrivalTime      || '',
                    odometerOut:   firstLine.odometerOut      != null ? firstLine.odometerOut : '',
                };
            });
            if (!INV._groups.length) INV._groups = [{ destId: null, sealDest: '', departureTime: '', arrivalTime: '', odometerOut: '' }];
        } else {
            INV._groups = [{ destId: null, sealDest: '', departureTime: '', arrivalTime: '', odometerOut: '' }];
        }

        populateWarehouseSelects();
        document.getElementById('fldOrigin').value = savedOriginId;

        // updateTransitUI reconstruye fldDest — por eso restauramos después
        updateTransitUI();

        // Restaurar Bodega Tránsito (fldDest) después de que updateTransitUI lo reconstruyó
        if (savedDestId) document.getElementById('fldDest').value = savedDestId;

        renderLines();

        document.getElementById('btnSaveConfirm').classList.remove('d-none');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('invModal')).show();
    } catch (e) {
        showAlert(e.message, 'danger');
    }
}

// ============================================================
// POBLAR SELECTS DE BODEGAS
// ============================================================

function populateWarehouseSelects() {
    const type = document.getElementById('fldType').value;
    const isTransit = type === 'TransitTransfer';

    // Origen: excluir bodegas tipo Transit cuando el movimiento es TransitTransfer
    const originWhs = isTransit
        ? INV.warehouses.filter(w => w.warehouseType !== 'Transit')
        : INV.warehouses;

    const originOpts = originWhs.map(w =>
        `<option value="${w.id}">${w.code} — ${w.name}</option>`
    ).join('');

    const elOrigin = document.getElementById('fldOrigin');
    const curOrigin = elOrigin.value;
    elOrigin.innerHTML = '<option value="">— seleccione —</option>' + originOpts;
    if (curOrigin) elOrigin.value = curOrigin;
}

// ============================================================
// UI DINÁMICA SEGÚN TIPO DE MOVIMIENTO
// ============================================================

function updateTransitUI() {
    const type      = document.getElementById('fldType').value;
    const isTransit = type === 'TransitTransfer';
    const needsDest = ['Transfer', 'TransitTransfer', 'CustomerReturn'].includes(type);
    const banner    = document.getElementById('transitBanner');
    const destGroup = document.getElementById('destGroup');
    const destLabel = document.getElementById('destLabel');
    const destHint  = document.getElementById('destHint');
    const sealGroup = document.getElementById('sealGroup');
    const finalDestGroup = document.getElementById('finalDestGroup');

    banner.classList.toggle('d-none', !isTransit);
    destGroup.classList.toggle('d-none', !needsDest);
    sealGroup.classList.toggle('d-none', !isTransit);
    const transitTimeKmGroup = document.getElementById('transitTimeKmGroup');
    if (transitTimeKmGroup) transitTimeKmGroup.classList.toggle('d-none', !isTransit);

    // Bloquear Hora Llegada del encabezado en tránsito (se asigna al confirmar la última entrega)
    const arrTimeEl = document.getElementById('fldArrivalTime');
    if (arrTimeEl) {
        arrTimeEl.disabled = isTransit;
        arrTimeEl.title    = isTransit ? 'Se determina automáticamente al confirmar la última entrega' : '';
        if (isTransit) arrTimeEl.value = '';
    }

    // En modo tránsito el selector global de destino se reemplaza por grupos inline
    finalDestGroup.classList.add('d-none');
    // El botón global Agregar Artículo solo aplica en modo no-tránsito
    const btnAdd = document.getElementById('btnAddLine');
    if (btnAdd) btnAdd.classList.toggle('d-none', isTransit);

    // Repoblar origen (excluir transit si es TransitTransfer)
    populateWarehouseSelects();

    if (isTransit) {
        // Solo bodegas de tránsito en el select de destino (vehículo intermediario)
        const opts = INV.warehouses
            .filter(w => w.warehouseType === 'Transit')
            .map(w => `<option value="${w.id}">🚛 ${w.code} — ${w.name}</option>`)
            .join('');

        document.getElementById('fldDest').innerHTML =
            '<option value="">— seleccione vehículo/tránsito —</option>' + opts;

        destLabel.textContent = 'Bodega Tránsito (Vehículo) *';
        destHint.textContent  = 'La bodega tipo Transit actúa como vehículo intermediario.';
    } else {
        // Para tipos no-transit: poblar destino con todas las bodegas excepto la misma que el origen
        const originId = parseInt(document.getElementById('fldOrigin').value) || 0;
        const destWhs  = INV.warehouses.filter(w => w.id !== originId);
        const destOpts = destWhs.map(w =>
            `<option value="${w.id}">${w.code} — ${w.name}</option>`
        ).join('');
        document.getElementById('fldDest').innerHTML =
            '<option value="">— seleccione —</option>' + destOpts;

        destLabel.textContent = 'Bodega Destino';
        destHint.textContent  = '';
    }

    // Re-renderizar líneas
    renderLines();
}

// ============================================================
// HELPERS DE GRUPOS (modo tránsito)
// ============================================================

// Retorna array de IDs de bodegas destino ya usadas en los grupos actuales
function getUsedDestIds() {
    return INV._groups
        .map(g => g.destId)
        .filter(id => id && id > 0);
}

// Opciones disponibles para un selector de grupo (excluye origen, tránsito y otros grupos)
function buildDestOpts(excludeGroupIdx) {
    const originId = parseInt(document.getElementById('fldOrigin').value) || 0;
    const otherDests = INV._groups
        .filter((_, i) => i !== excludeGroupIdx)
        .map(g => g.destId)
        .filter(id => id && id > 0);

    return INV.warehouses.filter(w =>
        w.warehouseType !== 'Transit' &&
        w.id !== originId &&
        !otherDests.includes(w.id)
    );
}

// El usuario cambia el selector de destino de un grupo
function changeGroupDest(groupIdx) {
    const sel = document.getElementById(`grpDest${groupIdx}`);
    if (!sel) return;
    const newDestId = parseInt(sel.value) || null;
    const oldDestId = INV._groups[groupIdx].destId;
    INV._groups[groupIdx].destId = newDestId;
    // Actualizar todas las líneas que pertenecían a este grupo
    INV.lines.forEach(l => {
        if (l.idWarehouseDestLine === oldDestId) l.idWarehouseDestLine = newDestId;
    });
    renderLines();
}

// ============================================================
// RENDER LÍNEAS EDITABLES
// ============================================================

function renderLines() {
    const container = document.getElementById('linesContainer');
    const empty     = document.getElementById('linesEmpty');
    const isTransit = document.getElementById('fldType').value === 'TransitTransfer';

    // Ocultar/mostrar el botón global "Agregar Artículo" y el bloque finalDestGroup del header
    const btnAddLine    = document.getElementById('btnAddLine');
    const addLineHeader = document.getElementById('addLineHeader');
    const finalDestGroup = document.getElementById('finalDestGroup');
    if (btnAddLine)    btnAddLine.style.display    = isTransit ? 'none' : '';
    if (addLineHeader) addLineHeader.style.display = isTransit ? 'none' : '';
    if (finalDestGroup) finalDestGroup.style.display = 'none'; // siempre oculto — grupos inline

    if (isTransit) {
        if (empty) empty.style.display = 'none';
        renderTransitGroups(container);
        return;
    }

    if (!INV.lines.length) {
        container.innerHTML = '';
        if (empty) empty.style.display = '';
        return;
    }
    if (empty) empty.style.display = 'none';

    container.innerHTML = INV.lines.map((l, idx) => _buildLineHtml(l, idx, false)).join('');
    _bindLineEvents(container);
}

// Construye el HTML de una línea (usada tanto en modo normal como en grupos transit)
function _buildLineHtml(l, idx, isTransit) {
    return `<div class="line-row${isTransit ? ' transit' : ''}" id="lineRow${idx}">
        <div class="row g-2">
            <div class="col-12 col-md-4">
                <label class="form-label" style="font-size:.72rem;">Artículo *</label>
                <div class="input-group input-group-sm" style="position:relative;">
                    <input type="text" class="form-control line-item-search" data-idx="${idx}"
                           value="${l.itemCode ? l.itemCode + ' — ' + l.itemName : ''}"
                           placeholder="Código o nombre…" autocomplete="off">
                    <input type="hidden" class="line-item-id"   data-idx="${idx}" value="${l.idItem || ''}">
                    <input type="hidden" class="line-item-code" data-idx="${idx}" value="${l.itemCode || ''}">
                    <input type="hidden" class="line-item-name" data-idx="${idx}" value="${l.itemName || ''}">
                </div>
                <div class="item-suggestions" id="sugg${idx}" style="display:none;position:absolute;z-index:9999;width:calc(100% - 1.5rem);"></div>
            </div>
            <div class="col-4 col-md-2">
                <label class="form-label" style="font-size:.72rem;">Cantidad *</label>
                <input type="number" class="form-control form-control-sm line-qty" data-idx="${idx}"
                       value="${l.qtyRequested || ''}" min="0.0001" step="0.0001">
            </div>
            <div class="col-4 col-md-2">
                <label class="form-label" style="font-size:.72rem;">Costo Unit.</label>
                <input type="number" class="form-control form-control-sm line-cost" data-idx="${idx}"
                       value="${l.unitCost || ''}" min="0" step="0.01">
            </div>
            <div class="col-4 col-md-2">
                <label class="form-label" style="font-size:.72rem;">Lote</label>
                <input type="text" class="form-control form-control-sm line-lot" data-idx="${idx}"
                       value="${l.lotNumber || ''}">
            </div>
            <div class="col-12 col-md-2 d-flex align-items-end">
                <button type="button" class="btn btn-sm btn-outline-danger w-100 btn-remove-line"
                        data-idx="${idx}"><i class="bi bi-trash3"></i></button>
            </div>
        </div>
    </div>`;
}

// Bind todos los eventos de líneas dentro de un container dado
function _bindLineEvents(container) {
    container.querySelectorAll('.line-item-search').forEach(inp => {
        inp.addEventListener('input', onItemSearch);
        inp.addEventListener('keydown', e => {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            const idx  = +inp.dataset.idx;
            const sugg = document.getElementById(`sugg${idx}`);
            const first = sugg && sugg.querySelector('.sugg-item');
            if (first) {
                const { id, code, name } = first.dataset;
                _applyItemSuggestion(idx, inp, id, code, name);
            }
        });
        inp.addEventListener('blur', () => setTimeout(() => {
            const idx   = +inp.dataset.idx;
            const term  = inp.value.trim().toLowerCase();
            const cache = _itemSuggestionsCache[idx] || [];
            if (!INV.lines[idx]) return;
            if (!INV.lines[idx].idItem && cache.length) {
                const exact = cache.find(it =>
                    it.code.toLowerCase() === term ||
                    `${it.code} — ${it.name}`.toLowerCase() === term
                );
                const match = exact || (cache.length === 1 ? cache[0] : null);
                if (match) {
                    _applyItemSuggestion(idx, inp, match.id, match.code, match.name, match.costPrice);
                    return;
                }
            }
            const sugg = document.getElementById(`sugg${idx}`);
            if (sugg) sugg.style.display = 'none';
        }, 200));
    });

    container.querySelectorAll('.line-qty').forEach(inp =>
        inp.addEventListener('change', e => {
            const idx = +e.target.dataset.idx;
            INV.lines[idx].qtyRequested = parseFloat(e.target.value) || 0;
        }));

    container.querySelectorAll('.line-cost').forEach(inp =>
        inp.addEventListener('change', e => {
            const idx = +e.target.dataset.idx;
            INV.lines[idx].unitCost = parseFloat(e.target.value) || null;
        }));

    container.querySelectorAll('.line-lot').forEach(inp =>
        inp.addEventListener('change', e => {
            const idx = +e.target.dataset.idx;
            INV.lines[idx].lotNumber = e.target.value;
        }));

    container.querySelectorAll('.btn-remove-line').forEach(btn =>
        btn.addEventListener('click', e => {
            const idx = +e.currentTarget.dataset.idx;
            const groupDestId = INV.lines[idx] ? INV.lines[idx].idWarehouseDestLine : null;
            INV.lines.splice(idx, 1);
            // Si ese grupo queda sin líneas, quitar el grupo también
            if (document.getElementById('fldType').value === 'TransitTransfer') {
                const groupIdx = INV._groups.findIndex(g => g.destId === groupDestId);
                if (groupIdx >= 0 && !INV.lines.some(l => l.idWarehouseDestLine === groupDestId)) {
                    // Solo eliminar el grupo si hay más de uno
                    if (INV._groups.length > 1) INV._groups.splice(groupIdx, 1);
                }
            }
            renderLines();
        }));
}

// ============================================================
// RENDER GRUPOS DE DESTINO (modo TransitTransfer)
// ============================================================

function renderTransitGroups(container) {
    // Calcular todos los destIds usados por otros grupos (para exclusión)
    const originId = parseInt(document.getElementById('fldOrigin').value) || 0;

    // Sincronizar primer grupo con los valores del encabezado (Sello, Hora Salida, Km Salida)
    if (INV._groups.length > 0) {
        INV._groups[0].sealDest      = document.getElementById('fldSecuritySeal').value.trim();
        INV._groups[0].departureTime = document.getElementById('fldDepartureTime').value;
        INV._groups[0].odometerOut   = document.getElementById('fldOdometerOut').value;
    }

    let html = '';

    INV._groups.forEach((group, gIdx) => {
        const isFirstGroup = gIdx === 0;
        const groupLines = INV.lines.filter(l => l.idWarehouseDestLine === group.destId);
        // Índices reales dentro de INV.lines para este grupo
        const groupLineIdxs = INV.lines
            .map((l, i) => ({ l, i }))
            .filter(({ l }) => l.idWarehouseDestLine === group.destId)
            .map(({ i }) => i);

        // Opciones para este selector: excluir origen, tránsito, y destinos usados en otros grupos
        const otherDests = INV._groups
            .filter((_, i) => i !== gIdx)
            .map(g => g.destId)
            .filter(id => id && id > 0);

        const availableWhs = INV.warehouses.filter(w =>
            w.warehouseType !== 'Transit' &&
            w.id !== originId &&
            !otherDests.includes(w.id)
        );

        const destOpts = availableWhs.map(w =>
            `<option value="${w.id}" ${group.destId === w.id ? 'selected' : ''}>${w.code} — ${w.name}</option>`
        ).join('');

        // Badges de bodegas usadas en el movimiento (todos los grupos)
        const usedBadges = INV._groups
            .filter(g => g.destId)
            .map(g => {
                const w = INV.warehouses.find(x => x.id === g.destId);
                const isCurrentGroup = g.destId === group.destId;
                return w
                    ? `<span class="badge" style="background:${isCurrentGroup ? '#2d1b6e' : '#1d3557'};color:${isCurrentGroup ? '#c4b5fd' : '#93c5fd'};font-size:.72rem;">${w.code}</span>`
                    : '';
            }).join(' ');

        const linesHtml = groupLineIdxs.map(idx => _buildLineHtml(INV.lines[idx], idx, true)).join('');

        const canRemoveGroup = INV._groups.length > 1;

        html += `<div class="transit-group-block mb-3" id="grpBlock${gIdx}"
                      style="background:#111827;border:1px solid var(--inv-orange);border-radius:10px;padding:1rem;">
            <div class="row g-2 align-items-end mb-2">
                <div class="col-12 col-md-6">
                    <label class="form-label text-warning fw-semibold" style="font-size:.8rem;">
                        <i class="bi bi-geo-alt me-1"></i>Bodega Destino Final *
                    </label>
                    <select id="grpDest${gIdx}" class="form-select grp-dest-sel" data-gidx="${gIdx}">
                        <option value="">— seleccione bodega destino —</option>
                        ${destOpts}
                    </select>
                    <small style="color:#cbd5e1!important;">Las bodegas ya usadas en este movimiento no aparecen.</small>
                </div>
                <div class="col-12 col-md-4">
                    <div class="d-flex flex-wrap gap-1 align-items-center mt-2">
                        <small class="text-muted" style="font-size:.72rem;">Bodegas usadas en este movimiento:</small>
                        <span>${usedBadges || '<span class="text-muted fst-italic" style="font-size:.72rem;">—</span>'}</span>
                    </div>
                </div>
                ${canRemoveGroup ? `<div class="col-12 col-md-2 d-flex align-items-end">
                    <button type="button" class="btn btn-sm btn-outline-danger w-100 btn-remove-group" data-gidx="${gIdx}">
                        <i class="bi bi-x-circle me-1"></i>Quitar
                    </button>
                </div>` : ''}
            </div>
            <!-- Sello + Hora/Km por destino -->
            <div class="row g-2 mb-2">
                <div class="col-12 col-md-4">
                    <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                        <i class="bi bi-shield-lock me-1 text-warning"></i>Sello Destino
                    </label>
                    <input type="text" id="grpSeal${gIdx}" class="form-control grp-seal" data-gidx="${gIdx}"
                           placeholder="Sello bodega destino…" maxlength="50" autocomplete="off"
                           value="${group.sealDest || ''}" ${isFirstGroup ? 'readonly style="opacity:.75;cursor:default;"' : ''}>
                </div>
                <div class="col-6 col-md-2">
                    <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                        <i class="bi bi-clock me-1 text-warning"></i>Hora Salida
                    </label>
                    <input type="time" id="grpDeptTime${gIdx}" class="form-control grp-dept-time" data-gidx="${gIdx}"
                           step="60" value="${group.departureTime || ''}" ${isFirstGroup ? 'readonly style="opacity:.75;cursor:default;"' : ''}>
                </div>
                <div class="col-6 col-md-2">
                    <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                        <i class="bi bi-clock-fill me-1 text-info"></i>Hora Llegada
                    </label>
                    <input type="time" id="grpArrTime${gIdx}" class="form-control grp-arr-time" data-gidx="${gIdx}"
                           step="60" value="${group.arrivalTime || ''}">
                </div>
                <div class="col-6 col-md-2">
                    <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                        <i class="bi bi-speedometer me-1 text-warning"></i>Km Salida
                    </label>
                    <input type="number" id="grpOdoOut${gIdx}" class="form-control grp-odo-out" data-gidx="${gIdx}"
                           placeholder="0" min="0" step="1" value="${group.odometerOut ?? ''}" ${isFirstGroup ? 'readonly style="opacity:.75;cursor:default;"' : ''}>
                </div>
                ${isFirstGroup ? '<div class="col-12 mt-1"><small style="color:#94a3b8;font-size:.7rem;"><i class="bi bi-info-circle me-1 text-info"></i>Sello Destino, Hora Salida y Km Salida se sincronizan automáticamente del encabezado.</small></div>' : ''}
            </div>
            <!-- Artículos de este grupo -->
            <div class="d-flex justify-content-between align-items-center mb-2">
                <span class="fw-semibold text-light" style="font-size:.85rem;"><i class="bi bi-list-ul me-1"></i>Artículos</span>
                <button type="button" class="btn btn-sm btn-outline-warning btn-add-group-line" data-gidx="${gIdx}">
                    <i class="bi bi-plus me-1"></i>Agregar Artículo
                </button>
            </div>
            <div id="grpLines${gIdx}">
                ${linesHtml || '<div class="text-center py-2" style="font-size:.82rem;color:#94a3b8;"><i class="bi bi-inbox me-1"></i>Sin artículos. Haga clic en "Agregar Artículo".</div>'}
            </div>
        </div>`;
    });

    // Botón para agregar nueva bodega destino
    html += `<div class="text-end mt-2">
        <button type="button" class="btn btn-sm btn-outline-info" id="btnAddDestGroup">
            <i class="bi bi-plus-circle me-1"></i>Agregar otra Bodega Destino
        </button>
    </div>`;

    container.innerHTML = html;

    // Bind: cambio de selector de destino por grupo
    container.querySelectorAll('.grp-dest-sel').forEach(sel => {
        sel.addEventListener('change', () => changeGroupDest(+sel.dataset.gidx));
    });

    // Bind: sello, hora y km por grupo (actualiza INV._groups en tiempo real)
    container.querySelectorAll('.grp-seal').forEach(inp => {
        inp.addEventListener('input', () => { if (INV._groups[+inp.dataset.gidx]) INV._groups[+inp.dataset.gidx].sealDest = inp.value; });
    });
    container.querySelectorAll('.grp-dept-time').forEach(inp => {
        inp.addEventListener('change', () => { if (INV._groups[+inp.dataset.gidx]) INV._groups[+inp.dataset.gidx].departureTime = inp.value; });
    });
    container.querySelectorAll('.grp-arr-time').forEach(inp => {
        inp.addEventListener('change', () => { if (INV._groups[+inp.dataset.gidx]) INV._groups[+inp.dataset.gidx].arrivalTime = inp.value; });
    });
    container.querySelectorAll('.grp-odo-out').forEach(inp => {
        inp.addEventListener('input', () => { if (INV._groups[+inp.dataset.gidx]) INV._groups[+inp.dataset.gidx].odometerOut = inp.value; });
    });

    // Bind: agregar artículo a un grupo específico
    container.querySelectorAll('.btn-add-group-line').forEach(btn => {
        btn.addEventListener('click', () => {
            const gIdx = +btn.dataset.gidx;
            const destId = INV._groups[gIdx] ? INV._groups[gIdx].destId : null;
            INV.lines.push({
                idItem: 0, itemCode: '', itemName: '', qtyRequested: 1,
                idWarehouseDestLine: destId,
                idUnitOfMeasure: null, unitOfMeasureCode: '',
                unitCost: null, lotNumber: '', notes: '',
            });
            renderLines();
            // Scroll al fondo del modal
            const modalBody = document.querySelector('#invModal .modal-body');
            if (modalBody) setTimeout(() => modalBody.scrollTo({ top: modalBody.scrollHeight, behavior: 'smooth' }), 50);
        });
    });

    // Bind: quitar grupo
    container.querySelectorAll('.btn-remove-group').forEach(btn => {
        btn.addEventListener('click', () => {
            const gIdx = +btn.dataset.gidx;
            if (INV._groups.length <= 1) return;
            const removedDest = INV._groups[gIdx].destId;
            // Quitar las líneas de este grupo
            INV.lines = INV.lines.filter(l => l.idWarehouseDestLine !== removedDest);
            INV._groups.splice(gIdx, 1);
            renderLines();
        });
    });

    // Bind: agregar nueva bodega destino
    const btnAddDestGroup = container.querySelector('#btnAddDestGroup');
    if (btnAddDestGroup) {
        btnAddDestGroup.addEventListener('click', () => {
            const hasUnselected = INV._groups.some(g => !g.destId);
            if (hasUnselected) {
                showModalAlert('Seleccione una bodega destino en los grupos existentes antes de agregar uno nuevo.', 'info');
                return;
            }
            const available = buildDestOpts(-1);
            if (available.length === 0) {
                showModalAlert('No hay más bodegas destino disponibles. Todas las bodegas ya han sido asignadas en este movimiento.', 'info');
                return;
            }
            INV._groups.push({ destId: null, sealDest: '', departureTime: '', arrivalTime: '', odometerOut: '' });
            renderLines();
            const modalBody = document.querySelector('#invModal .modal-body');
            if (modalBody) setTimeout(() => modalBody.scrollTo({ top: modalBody.scrollHeight, behavior: 'smooth' }), 50);
        });
    }

    // Bind eventos de líneas dentro de los grupos
    _bindLineEvents(container);
}

// ============================================================
// BÚSQUEDA DE ARTÍCULOS (autocompletado)
// ============================================================

let _itemSearchTimeout = null;

// Caché de sugerencias por índice de línea
const _itemSuggestionsCache = {};

function _applyItemSuggestion(idx, inp, id, code, name, costPrice) {
    INV.lines[idx].idItem   = parseInt(id);
    INV.lines[idx].itemCode = code;
    INV.lines[idx].itemName = name;
    if (costPrice != null && costPrice !== '') {
        INV.lines[idx].unitCost = parseFloat(costPrice) || null;
        // Actualizar el input de costo en el DOM si ya está renderizado
        const costEl = document.querySelector(`.line-cost[data-idx="${idx}"]`);
        if (costEl && INV.lines[idx].unitCost != null) costEl.value = INV.lines[idx].unitCost;
    }
    inp.value = `${code} — ${name}`;
    const sugg = document.getElementById(`sugg${idx}`);
    if (sugg) sugg.style.display = 'none';
}

async function onItemSearch(e) {
    const inp  = e.target;
    const idx  = +inp.dataset.idx;
    const term = inp.value.trim();

    // Limpiar idItem al modificar el campo
    INV.lines[idx].idItem   = 0;
    INV.lines[idx].itemCode = '';
    INV.lines[idx].itemName = '';
    _itemSuggestionsCache[idx] = [];

    clearTimeout(_itemSearchTimeout);
    if (term.length < 2) {
        const s = document.getElementById(`sugg${idx}`);
        if (s) s.style.display = 'none';
        return;
    }

    _itemSearchTimeout = setTimeout(async () => {
        try {
            const data = await invFetch(`/api/item?search=${encodeURIComponent(term)}&pageSize=8&isActive=true`);
            const items = data.items || data || [];
            _itemSuggestionsCache[idx] = items;
            renderItemSuggestions(idx, items, inp);
            // Auto-seleccionar si hay resultado único exacto por código
            if (items.length === 1) {
                const it = items[0];
                if (it.code.toLowerCase() === term.toLowerCase() || term.toLowerCase() === `${it.code} — ${it.name}`.toLowerCase()) {
                    _applyItemSuggestion(idx, inp, it.id, it.code, it.name, it.costPrice);
                }
            }
        } catch {}
    }, 300);
}

function renderItemSuggestions(idx, items, inp) {
    let sugg = document.getElementById(`sugg${idx}`);
    if (!sugg) return;

    if (!items.length) { sugg.style.display = 'none'; return; }

    sugg.style.cssText = `
        display:block; position:absolute; z-index:9999;
        background:#1a2340; border:1px solid #2a3a5c; border-radius:8px;
        max-height:200px; overflow-y:auto; width:100%;
    `;

    sugg.innerHTML = items.map(it =>
        `<div class="sugg-item p-2" style="cursor:pointer;border-bottom:1px solid #2a3a5c;font-size:.82rem;"
              data-id="${it.id}" data-code="${it.code}" data-name="${it.name}" data-cost="${it.costPrice ?? ''}">
            <strong>${it.code}</strong> — ${it.name}
        </div>`
    ).join('');

    sugg.querySelectorAll('.sugg-item').forEach(el => {
        el.addEventListener('mousedown', ev => {
            ev.preventDefault();
            const { id, code, name, cost } = ev.currentTarget.dataset;
            _applyItemSuggestion(idx, inp, id, code, name, cost);
        });
        el.addEventListener('mouseenter', () => el.style.background = '#1e2d4a');
        el.addEventListener('mouseleave', () => el.style.background = 'transparent');
    });
}

// ============================================================
// GUARDAR MOVIMIENTO
// ============================================================

async function saveMovement(autoConfirm = false) {
    const number    = document.getElementById('fldNumber').value.trim();
    const date      = document.getElementById('fldDate').value;
    const type      = document.getElementById('fldType').value;
    const originId  = parseInt(document.getElementById('fldOrigin').value) || 0;
    const destId    = parseInt(document.getElementById('fldDest').value)   || null;
    const arrival   = document.getElementById('fldArrival').value   || null;
    const reference = document.getElementById('fldReference').value.trim() || null;
    const notes     = document.getElementById('fldNotes').value.trim()     || null;
    const isTransit = type === 'TransitTransfer';
    const securitySeal = isTransit
        ? (document.getElementById('fldSecuritySeal').value.trim() || null)
        : null;

    // Campos de tramo Origen→Tránsito
    const departureTime  = isTransit ? (document.getElementById('fldDepartureTime').value || null) : null;
    const arrivalTime    = isTransit ? (document.getElementById('fldArrivalTime').value   || null) : null;
    const odometerRaw    = document.getElementById('fldOdometerOut').value.trim();
    const odometerOut    = isTransit ? (odometerRaw !== '' ? parseFloat(odometerRaw) : null) : null;

    // Auto-rellenar fecha esperada si está vacía
    const fldArrivalEl = document.getElementById('fldArrival');
    if (!arrival) {
        fldArrivalEl.value = date;
    }
    const effectiveArrival = fldArrivalEl.value || date;
    if (date && effectiveArrival < date) {
        fldArrivalEl.focus();
        return showModalAlert('La Fecha Esperada de Llegada no puede ser anterior a la Fecha del movimiento.', 'warning');
    }

    if (!originId) return showModalAlert('Seleccione la Bodega Origen.', 'warning');
    if (isTransit && !destId) return showModalAlert('Seleccione la Bodega de Tránsito (Vehículo).', 'warning');
    if (isTransit && !securitySeal) return showModalAlert('Ingrese el Sello de Seguridad.', 'warning');
    if (isTransit && !departureTime) return showModalAlert('La Hora de Salida del encabezado es obligatoria.', 'warning');
    if (isTransit && odometerOut === null) return showModalAlert('El Kilometraje de Salida es obligatorio.', 'warning');
    if (isTransit) {
        // Validar que cada grupo tenga bodega destino asignada
        const missingDest = INV._groups.some(g => !g.destId);
        if (missingDest) return showModalAlert('Seleccione la Bodega Destino Final en todos los grupos de artículos.', 'warning');
        // Validar que ningún grupo esté vacío de artículos
        for (const [gIdx, group] of INV._groups.entries()) {
            const hasLines = INV.lines.some(l => l.idWarehouseDestLine === group.destId);
            if (!hasLines) {
                const w = INV.warehouses.find(x => x.id === group.destId);
                const wName = w ? `${w.code} — ${w.name}` : `Grupo ${gIdx + 1}`;
                return showModalAlert(
                    `La bodega destino "${wName}" no tiene artículos asignados. Agregue artículos o elimine la bodega del movimiento.`,
                    'warning'
                );
            }
        }
    }
    if (!INV.lines.length) return showModalAlert('Agregue al menos un artículo.', 'warning');

    // Verificar unicidad del sello de seguridad principal
    if (isTransit && securitySeal) {
        try {
            const excludeId = INV.editingId || undefined;
            const sealUrl   = `/api/inventorytransaction/check-seal?seal=${encodeURIComponent(securitySeal)}`
                + (excludeId ? `&excludeId=${excludeId}` : '');
            const { exists } = await invFetch(sealUrl);
            if (exists) {
                document.getElementById('sealError').classList.remove('d-none');
                return showModalAlert('El Sello de Seguridad ya está en uso en otra transacción.', 'danger');
            }
            document.getElementById('sealError').classList.add('d-none');
        } catch {}
    }

    // Recolectar valores de líneas desde el DOM y propagar campos de grupo
    const container = document.getElementById('linesContainer');
    INV.lines.forEach((l, idx) => {
        const qtyEl  = container.querySelector(`.line-qty[data-idx="${idx}"]`);
        const costEl = container.querySelector(`.line-cost[data-idx="${idx}"]`);
        const lotEl  = container.querySelector(`.line-lot[data-idx="${idx}"]`);
        if (qtyEl)  l.qtyRequested = parseFloat(qtyEl.value)  || 0;
        if (costEl) l.unitCost     = parseFloat(costEl.value) || null;
        if (lotEl)  l.lotNumber    = lotEl.value || null;

        // Copiar campos del grupo (sello, hora, km) a cada línea del grupo
        if (isTransit) {
            const grp = INV._groups.find(g => g.destId === l.idWarehouseDestLine);
            if (grp) {
                l.destSecuritySeal = grp.sealDest  || null;
                l.departureTime    = grp.departureTime || null;
                l.arrivalTime      = grp.arrivalTime   || null;
                l.odometerOut      = grp.odometerOut !== '' ? (parseFloat(grp.odometerOut) || null) : null;
            }
        }
    });

    for (const l of INV.lines) {
        if (!l.idItem)       return showModalAlert('Una línea no tiene artículo asignado.', 'warning');
        if (!l.qtyRequested) return showModalAlert(`La cantidad de "${l.itemCode}" debe ser mayor a 0.`, 'warning');
    }

    const payload = {
        transactionNumber: number || undefined,
        movementType:      type,
        idWarehouseOrigin: originId,
        idWarehouseDest:   destId,
        reference,
        notes,
        securitySeal,
        departureTime,
        arrivalTime,
        odometerOut,
        transactionDate:      date,
        expectedArrivalDate:  effectiveArrival,
        isTransitTransfer:    isTransit,
        lines: INV.lines.map(l => ({
            idItem:              l.idItem,
            itemCode:            l.itemCode,
            itemName:            l.itemName,
            qtyRequested:        l.qtyRequested,
            idWarehouseDestLine: l.idWarehouseDestLine || null,
            idUnitOfMeasure:     l.idUnitOfMeasure || null,
            unitOfMeasureCode:   l.unitOfMeasureCode || null,
            unitCost:            l.unitCost || null,
            lotNumber:           l.lotNumber || null,
            destSecuritySeal:    l.destSecuritySeal || null,
            departureTime:       l.departureTime || null,
            arrivalTime:         l.arrivalTime   || null,
            odometerOut:         l.odometerOut   != null ? l.odometerOut : null,
        })),
    };

    try {
        let txn;
        if (INV.editingId) {
            await invFetch(`/api/inventorytransaction/${INV.editingId}`, { method: 'PUT', body: payload });
            await invFetch(`/api/inventorytransaction/${INV.editingId}/lines`, { method: 'PUT', body: payload.lines });
            txn = { id: INV.editingId };
        } else {
            txn = await invFetch('/api/inventorytransaction', { method: 'POST', body: payload });
        }

        if (autoConfirm && txn.id) {
            await invFetch(`/api/inventorytransaction/${txn.id}/confirm`, { method: 'PATCH' });
        }

        bootstrap.Modal.getInstance(document.getElementById('invModal')).hide();
        showAlert('Movimiento guardado correctamente.', 'success');
        loadMovements(INV.page);
    } catch (e) {
        showModalAlert(e.message, 'danger');
    }
}

// ============================================================
// RECIBIR MOVIMIENTO — pantalla bodega por bodega
// ============================================================

async function openReceive(id) {
    INV.currentTxn = null;
    const body   = document.getElementById('receiveModalBody');
    const footer = document.getElementById('receiveModalFooter');
    body.innerHTML   = '<div class="text-center py-4" style="color:#cbd5e1;">Cargando…</div>';
    footer.innerHTML = '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>';
    document.getElementById('receiveModalTitle').innerHTML =
        '<i class="bi bi-box-arrow-in-down me-2 text-success"></i>Recibir Movimiento';

    bootstrap.Modal.getOrCreateInstance(document.getElementById('receiveModal')).show();

    try {
        const txn = await invFetch(`/api/inventorytransaction/${id}`);
        const lines = txn.lines || await invFetch(`/api/inventorytransaction/${id}/lines`);
        txn.lines = lines;
        INV.currentTxn = txn;

        document.getElementById('receiveModalTitle').innerHTML =
            `<i class="bi bi-box-arrow-in-down me-2 text-success"></i>${txn.transactionNumber} — ${statusBadge(txn.status)}`;

        body.innerHTML = renderReceiveBody(txn);
        renderReceiveFooter(txn, footer);
        _bindReceiveBodyEvents(body);

    } catch (e) {
        body.innerHTML = `<div class="text-danger text-center py-4">${e.message}</div>`;
    }
}

function _bindReceiveBodyEvents(container) {
    // Per-line checkbox → set qty to dispatched
    container.querySelectorAll('.rcv-line-chk').forEach(chk => {
        chk.addEventListener('change', () => {
            const lineId    = chk.dataset.lineId;
            const dispatched = parseFloat(chk.dataset.dispatched) || 0;
            const qtyInput   = document.getElementById(`rcv-qty-${lineId}`);
            if (qtyInput) {
                qtyInput.value = chk.checked ? dispatched : '';
                qtyInput.style.borderColor = chk.checked ? '#22c55e' : '#2a3a5c';
            }
        });
    });

    // Mark-all checkbox → check all line checkboxes and fill qtys
    container.querySelectorAll('.rcv-mark-all-chk').forEach(allChk => {
        allChk.addEventListener('change', () => {
            const groupChks = container.querySelectorAll('.rcv-line-chk');
            groupChks.forEach(chk => {
                if (chk.checked !== allChk.checked) {
                    chk.checked = allChk.checked;
                    chk.dispatchEvent(new Event('change'));
                }
            });
        });
    });

    // qty input manual edit → uncheck checkbox if value differs from dispatched
    container.querySelectorAll('.rcv-qty-input').forEach(inp => {
        inp.addEventListener('input', () => {
            const dispatched = parseFloat(inp.dataset.dispatched) || 0;
            const lineId = inp.id.replace('rcv-qty-', '');
            const chk = container.querySelector(`.rcv-line-chk[data-line-id="${lineId}"]`);
            if (chk) {
                const newVal = parseFloat(inp.value);
                chk.checked = !isNaN(newVal) && newVal === dispatched;
            }
        });
    });

    // Km Salida: validate > previous km on input
    container.querySelectorAll('[id^="rcvOdoOut"]').forEach(odoInput => {
        const gIdx = odoInput.id.replace('rcvOdoOut', '');
        const errEl = document.getElementById(`rcvOdoOutErr${gIdx}`);
        odoInput.addEventListener('input', () => {
            const prevKm = parseFloat(odoInput.dataset.prevKm);
            const newKm  = parseFloat(odoInput.value);
            const isInvalid = !isNaN(prevKm) && prevKm > 0 && !isNaN(newKm) && newKm <= prevKm;
            if (errEl) errEl.style.display = isInvalid ? '' : 'none';
            odoInput.style.borderColor = isInvalid ? '#ef4444' : '';
        });
    });

    // Nuevo Sello Destino: real-time uniqueness check
    let _sealCheckTimer = null;
    container.querySelectorAll('[id^="rcvNextSeal"]').forEach(sealInput => {
        const gIdx  = sealInput.id.replace('rcvNextSeal', '');
        const errEl = document.getElementById(`rcvNextSealErr${gIdx}`);
        const okEl  = document.getElementById(`rcvNextSealOk${gIdx}`);
        const txnId = sealInput.dataset.txnId;

        sealInput.addEventListener('input', () => {
            clearTimeout(_sealCheckTimer);
            const seal = sealInput.value.trim();
            if (errEl) errEl.style.display = 'none';
            if (okEl)  okEl.style.display  = 'none';
            sealInput.style.borderColor = '';
            if (!seal) return;

            _sealCheckTimer = setTimeout(async () => {
                try {
                    // No excludeId: new dest seal must be globally unique — cannot match
                    // any existing seal anywhere, not even on the current transaction header
                    const url = `/api/inventorytransaction/check-any-seal?seal=${encodeURIComponent(seal)}`;
                    const { exists } = await invFetch(url);
                    if (errEl) errEl.style.display = exists ? '' : 'none';
                    if (okEl)  okEl.style.display  = exists ? 'none' : '';
                    sealInput.style.borderColor = exists ? '#ef4444' : '#22c55e';
                } catch {}
            }, 450);
        });
    });
}

function _getReceiveDestGroups(txn) {
    // Agrupa las líneas por bodega destino, en el orden en que aparecen
    const lines = txn.lines || [];
    const ordered = [];
    const seen = new Set();
    for (const l of lines) {
        const destId = l.idWarehouseDestLine || 0;
        if (!seen.has(destId)) { seen.add(destId); ordered.push(destId); }
    }
    return ordered.map(destId => ({
        destId,
        lines: lines.filter(l => (l.idWarehouseDestLine || 0) === destId),
    }));
}

function _activeReceiveGroupIdx(groups) {
    // El primer grupo que tenga al menos una línea NO recibida
    return groups.findIndex(g => g.lines.some(l => l.lineStatus !== 'Received' && l.lineStatus !== 'Cancelled'));
}

function renderReceiveBody(txn) {
    const typeM = MOVEMENT_TYPES[txn.movementType] || { label: txn.movementType, icon: '📦', css: '' };

    const infoRow = (label, val) =>
        `<div class="col-6 col-md-3 mb-2">
            <div class="text-muted" style="font-size:.72rem;">${label}</div>
            <div class="text-white">${val || '—'}</div>
        </div>`;

    const headerBlock = `
        <div class="row mb-3">
            ${infoRow('Número', `<strong>${txn.transactionNumber}</strong>`)}
            ${infoRow('Tipo', `<span class="badge-type ${typeM.css}">${typeM.icon} ${typeM.label}</span>`)}
            ${infoRow('Estado', statusBadge(txn.status))}
            ${infoRow('Fecha', txn.transactionDate)}
            ${infoRow('Origen', warehouseName(txn.idWarehouseOrigin))}
            ${infoRow('Vehículo / Tránsito', txn.idWarehouseDest ? warehouseName(txn.idWarehouseDest) : '—')}
            ${infoRow('Referencia', txn.reference)}
            ${infoRow('Sello Seguridad', txn.securitySeal || '—')}
        </div>
        <hr style="border-color:#2a3a5c;">`;

    // Para TransitTransfer: grupos bodega por bodega
    if (txn.isTransitTransfer) {
        const groups = _getReceiveDestGroups(txn);
        const activeIdx = _activeReceiveGroupIdx(groups);

        let groupsHtml = '';
        groups.forEach((g, gIdx) => {
            const isActive   = gIdx === activeIdx;
            const isReceived = g.lines.every(l => l.lineStatus === 'Received' || l.lineStatus === 'Cancelled');
            const isPending  = !isReceived && !isActive;

            const usedBadges = groups.filter(x => x.destId).map(x => {
                const w = INV.warehouses.find(wh => wh.id === x.destId);
                const isCur = x.destId === g.destId;
                return w ? `<span class="badge" style="background:${isCur ? '#2d1b6e' : '#1d3557'};color:${isCur ? '#c4b5fd' : '#93c5fd'};font-size:.72rem;">${w.code}</span>` : '';
            }).join(' ');

            // Primera línea del grupo para leer sello/hora previos
            const firstLine = g.lines[0] || {};

            // Previous km: from the last received group's line odometerOut, or from the header odometerOut
            let prevKm = txn.odometerOut || 0;
            if (gIdx > 0) {
                const prevGroup = groups[gIdx - 1];
                const prevGroupLine = prevGroup?.lines?.[0];
                if (prevGroupLine?.odometerOut) prevKm = prevGroupLine.odometerOut;
            }

            // Artículos en tabla — activos con checkbox + qty editable, recibidos en modo lectura
            const artRows = g.lines.map(l => {
                const ls = { Pending: '⏳', InTransit: '🚛', Received: '✅', Cancelled: '❌' }[l.lineStatus] || '';
                const alreadyDone = l.lineStatus === 'Received' || l.lineStatus === 'Cancelled';

                if (!isActive || alreadyDone) {
                    return `<tr style="opacity:${alreadyDone ? '.6' : '1'}">
                        <td style="width:36px;"></td>
                        <td><strong>${l.itemCode}</strong><br><small class="text-muted">${l.itemName}</small></td>
                        <td class="text-center">${l.qtyRequested}</td>
                        <td class="text-center">${l.qtyDispatched || 0}</td>
                        <td class="text-center text-success">${l.qtyReceived || 0}</td>
                        <td class="text-center">${ls} ${l.lineStatus}</td>
                    </tr>`;
                }

                const dispatched = l.qtyDispatched || l.qtyRequested || 0;
                return `<tr id="rcv-row-${l.id}">
                    <td class="text-center" style="width:36px;vertical-align:middle;">
                        <input type="checkbox" class="form-check-input rcv-line-chk" data-line-id="${l.id}"
                               data-dispatched="${dispatched}" style="width:1.1rem;height:1.1rem;cursor:pointer;"
                               title="Marcar como recibido con total despachado">
                    </td>
                    <td><strong>${l.itemCode}</strong><br><small class="text-muted">${l.itemName}</small></td>
                    <td class="text-center">${l.qtyRequested}</td>
                    <td class="text-center">${dispatched}</td>
                    <td class="text-center" style="min-width:90px;">
                        <input type="number" id="rcv-qty-${l.id}" class="form-control form-control-sm rcv-qty-input text-center"
                               value="${dispatched}" min="0" step="any" data-dispatched="${dispatched}"
                               style="background:#0d1117;color:#4ade80;border-color:#2a3a5c;width:90px;margin:auto;">
                    </td>
                    <td class="text-center">${ls} ${l.lineStatus}</td>
                </tr>`;
            }).join('');

            let borderColor = isActive ? 'var(--inv-green)' : (isReceived ? '#22c55e55' : '#2a3a5c');
            let headerColor = isActive ? '#22c55e' : (isReceived ? '#4ade80' : '#94a3b8');
            let statusTag   = isActive
                ? `<span class="badge bg-success ms-2" style="font-size:.72rem;">⬅ ACTIVO — RECIBIR</span>`
                : (isReceived
                    ? `<span class="badge" style="background:#14532d;color:#4ade80;font-size:.72rem;">✅ Recibido</span>`
                    : `<span class="badge" style="background:#1e3a5f;color:#60a5fa;font-size:.72rem;">⏳ Pendiente</span>`);

            // Campos de recepción (solo en grupo activo)
            const receiveFields = isActive ? `
                <div class="row g-2 mb-2 mt-1" id="rcv-fields-${gIdx}">
                    <div class="col-12 col-md-4">
                        <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                            <i class="bi bi-shield-lock me-1 text-warning"></i>Sello Destino
                            <span class="text-muted" style="font-size:.68rem;">(pre-cargado)</span>
                        </label>
                        <input type="text" id="rcvSeal${gIdx}" class="form-control form-control-sm"
                               value="${firstLine.destSecuritySeal || ''}" readonly
                               style="opacity:.75;cursor:default;" title="Sello pre-asignado para este tramo">
                    </div>
                    <div class="col-6 col-md-2">
                        <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                            <i class="bi bi-clock me-1 text-warning"></i>Hora Salida *
                        </label>
                        <input type="time" id="rcvDeptTime${gIdx}" class="form-control form-control-sm"
                               value="${firstLine.departureTime || ''}" step="60" required
                               placeholder="--:--">
                        <small style="color:#cbd5e1!important;">Obligatorio</small>
                    </div>
                    <div class="col-6 col-md-2">
                        <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                            <i class="bi bi-clock-fill me-1 text-info"></i>Hora Llegada *
                        </label>
                        <input type="time" id="rcvArrTime${gIdx}" class="form-control form-control-sm" step="60"
                               placeholder="--:--" required>
                        <small style="color:#cbd5e1!important;">Obligatorio para confirmar recepción</small>
                    </div>
                    <div class="col-6 col-md-2">
                        <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                            <i class="bi bi-speedometer me-1 text-warning"></i>Km Salida *
                        </label>
                        <input type="number" id="rcvOdoOut${gIdx}" class="form-control form-control-sm"
                               placeholder="0" min="0" step="1" required
                               data-prev-km="${prevKm}">
                        <small id="rcvOdoOutErr${gIdx}" style="color:#f87171!important;display:none;">Debe ser mayor al Km anterior</small>
                        <small class="rcvOdoHelp${gIdx}" style="color:#cbd5e1!important;">Km al salir hacia siguiente destino${prevKm > 0 ? ` (anterior: ${prevKm})` : ''}</small>
                    </div>
                </div>` : '';

            // Sello para la siguiente bodega (solo si hay un siguiente grupo, y solo en grupo activo)
            const nextGroup = groups[gIdx + 1];
            const nextSealField = (isActive && nextGroup) ? `
                <div class="row g-2 mb-2 mt-1" id="rcv-next-seal-${gIdx}">
                    <div class="col-12 col-md-5">
                        <label class="form-label" style="color:#fbbf24;font-size:.78rem;">
                            <i class="bi bi-shield-check me-1 text-warning"></i>Nuevo Sello Destino *
                            <span style="color:#94a3b8;font-size:.7rem;">→ para <strong>${warehouseName(nextGroup.destId)}</strong></span>
                        </label>
                        <div class="d-flex gap-2 align-items-center">
                            <div class="flex-fill">
                                <input type="text" id="rcvNextSeal${gIdx}" class="form-control form-control-sm"
                                       placeholder="Sello para la siguiente bodega…" maxlength="50" required
                                       data-txn-id="${txn.id}">
                                <small id="rcvNextSealErr${gIdx}" style="color:#f87171!important;display:none;">
                                    <i class="bi bi-exclamation-triangle me-1"></i>Este sello ya está en uso, debe ser único.
                                </small>
                                <small id="rcvNextSealOk${gIdx}" style="color:#4ade80!important;display:none;">
                                    <i class="bi bi-check-circle me-1"></i>Sello disponible.
                                </small>
                            </div>
                            <div class="form-check mb-0 text-nowrap" title="Marcar todas las líneas como recibidas con el total despachado">
                                <input class="form-check-input rcv-mark-all-chk" type="checkbox"
                                       id="rcvMarkAll${gIdx}" data-group="${gIdx}"
                                       style="width:1.1rem;height:1.1rem;cursor:pointer;">
                                <label class="form-check-label" for="rcvMarkAll${gIdx}"
                                       style="color:#cbd5e1;font-size:.78rem;cursor:pointer;">Marcar todas recibidas</label>
                            </div>
                        </div>
                        <small style="color:#cbd5e1!important;">Este sello se asignará al tramo hacia la siguiente bodega destino</small>
                    </div>
                </div>` : (isActive ? `
                <div class="row g-2 mb-2 mt-1">
                    <div class="col-12 col-md-5">
                        <div class="form-check" title="Marcar todas las líneas como recibidas con el total despachado">
                            <input class="form-check-input rcv-mark-all-chk" type="checkbox"
                                   id="rcvMarkAll${gIdx}" data-group="${gIdx}"
                                   style="width:1.1rem;height:1.1rem;cursor:pointer;">
                            <label class="form-check-label" for="rcvMarkAll${gIdx}"
                                   style="color:#cbd5e1;font-size:.78rem;cursor:pointer;">Marcar todas como recibidas (total despachado)</label>
                        </div>
                    </div>
                </div>` : '');

            groupsHtml += `
            <div class="transit-group-block mb-3"
                 style="background:#111827;border:1px solid ${borderColor};border-radius:10px;padding:1rem;">
                <!-- Encabezado del grupo -->
                <div class="row g-2 align-items-end mb-2">
                    <div class="col-12 col-md-6">
                        <label class="form-label fw-semibold" style="color:${headerColor};font-size:.8rem;">
                            <i class="bi bi-geo-alt me-1"></i>Bodega Destino Final ${statusTag}
                        </label>
                        <div class="form-control form-control-sm" style="background:#0d1117;color:#e2e8f0;pointer-events:none;">
                            ${warehouseName(g.destId)}
                        </div>
                        <small style="color:#cbd5e1!important;">Las bodegas ya usadas en este movimiento no aparecen.</small>
                    </div>
                    <div class="col-12 col-md-6">
                        <div class="d-flex flex-wrap gap-1 align-items-center mt-2">
                            <small class="text-muted" style="font-size:.72rem;">Bodegas usadas en este movimiento:</small>
                            <span>${usedBadges || '—'}</span>
                        </div>
                    </div>
                </div>
                ${receiveFields}
                ${nextSealField}
                <div class="d-flex justify-content-between align-items-center mb-2 mt-2">
                    <span class="fw-semibold text-light" style="font-size:.85rem;"><i class="bi bi-list-ul me-1"></i>Artículos</span>
                </div>
                <div class="table-responsive">
                    <table class="table table-sm" style="color:var(--inv-text);">
                        <thead style="background:#0d1117;">
                            <tr>
                                <th style="width:36px;"></th>
                                <th>Artículo</th>
                                <th class="text-center">Solicitado</th>
                                <th class="text-center">Despachado</th>
                                <th class="text-center">Recibido</th>
                                <th class="text-center">Estado</th>
                            </tr>
                        </thead>
                        <tbody>${artRows}</tbody>
                    </table>
                </div>
            </div>`;
        });

        return `${headerBlock}<div id="rcv-groups-container">${groupsHtml}</div>`;
    }

    // Para movimientos no-TransitTransfer: recepción simple
    const allLines = txn.lines || [];
    const canReceiveLines = allLines.filter(l => l.lineStatus !== 'Received' && l.lineStatus !== 'Cancelled');

    const simpleRows = allLines.map(l => {
        const ls = { Pending: '⏳', InTransit: '🚛', Received: '✅', Cancelled: '❌' }[l.lineStatus] || '';
        const alreadyDone = l.lineStatus === 'Received' || l.lineStatus === 'Cancelled';
        const dispatched = l.qtyDispatched || l.qtyRequested || 0;

        if (alreadyDone) {
            return `<tr style="opacity:.6">
                <td style="width:36px;"></td>
                <td><strong>${l.itemCode}</strong><br><small class="text-muted">${l.itemName}</small></td>
                <td class="text-center">${l.qtyRequested}</td>
                <td class="text-center">${dispatched}</td>
                <td class="text-center text-success">${l.qtyReceived || 0}</td>
                <td class="text-center">${ls} ${l.lineStatus}</td>
            </tr>`;
        }

        return `<tr id="rcv-row-${l.id}">
            <td class="text-center" style="width:36px;vertical-align:middle;">
                <input type="checkbox" class="form-check-input rcv-line-chk" data-line-id="${l.id}"
                       data-dispatched="${dispatched}" style="width:1.1rem;height:1.1rem;cursor:pointer;">
            </td>
            <td><strong>${l.itemCode}</strong><br><small class="text-muted">${l.itemName}</small></td>
            <td class="text-center">${l.qtyRequested}</td>
            <td class="text-center">${dispatched}</td>
            <td class="text-center" style="min-width:90px;">
                <input type="number" id="rcv-qty-${l.id}" class="form-control form-control-sm rcv-qty-input text-center"
                       value="${dispatched}" min="0" step="any" data-dispatched="${dispatched}"
                       style="background:#0d1117;color:#4ade80;border-color:#2a3a5c;width:90px;margin:auto;">
            </td>
            <td class="text-center">${ls} ${l.lineStatus}</td>
        </tr>`;
    }).join('');

    return `${headerBlock}
        <div class="row g-2 mb-3">
            <div class="col-6 col-md-3">
                <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                    <i class="bi bi-clock-fill me-1 text-info"></i>Hora Llegada *
                </label>
                <input type="time" id="rcvSimpleArrTime" class="form-control form-control-sm" step="60" required>
                <small style="color:#cbd5e1!important;">Obligatorio</small>
            </div>
            <div class="col-6 col-md-3">
                <label class="form-label" style="color:#94a3b8;font-size:.78rem;">
                    <i class="bi bi-speedometer me-1 text-warning"></i>Km Salida *
                </label>
                <input type="number" id="rcvSimpleOdoOut" class="form-control form-control-sm"
                       placeholder="0" min="0" step="1" required>
                <small style="color:#cbd5e1!important;">Obligatorio</small>
            </div>
        </div>
        <div class="table-responsive">
            <div class="d-flex justify-content-between align-items-center mb-2">
                <span class="fw-semibold text-light" style="font-size:.85rem;"><i class="bi bi-list-ul me-1"></i>Artículos</span>
                <div class="form-check mb-0" title="Marcar todas las líneas como recibidas con el total despachado">
                    <input class="form-check-input rcv-mark-all-chk" type="checkbox" id="rcvSimpleMarkAll"
                           style="width:1.1rem;height:1.1rem;cursor:pointer;">
                    <label class="form-check-label" for="rcvSimpleMarkAll"
                           style="color:#cbd5e1;font-size:.78rem;cursor:pointer;">Marcar todas recibidas</label>
                </div>
            </div>
            <table class="table table-sm" style="color:var(--inv-text);">
                <thead style="background:#0d1117;">
                    <tr>
                        <th style="width:36px;"></th>
                        <th>Artículo</th>
                        <th class="text-center">Solicitado</th>
                        <th class="text-center">Despachado</th>
                        <th class="text-center">Recibido</th>
                        <th class="text-center">Estado</th>
                    </tr>
                </thead>
                <tbody>${simpleRows}</tbody>
            </table>
        </div>`;
}

function renderReceiveFooter(txn, footer) {
    const btns = ['<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>'];

    const canReceive = txn.status === 'InTransit' || txn.status === 'PartiallyReceived';
    if (canReceive) {
        const groups = txn.isTransitTransfer ? _getReceiveDestGroups(txn) : null;
        const activeIdx = groups ? _activeReceiveGroupIdx(groups) : -1;

        if (txn.isTransitTransfer && activeIdx >= 0) {
            const nextGroup = groups[activeIdx + 1];
            btns.push(`<button type="button" class="btn btn-success" id="btnSubmitReceive"
                        onclick="submitReceive(${txn.id}, ${activeIdx}, ${nextGroup ? nextGroup.destId : 'null'})">
                        <i class="bi bi-box-arrow-in-down me-1"></i>Confirmar Recepción
                       </button>`);
        } else if (!txn.isTransitTransfer) {
            btns.push(`<button type="button" class="btn btn-success" id="btnSubmitReceive"
                        onclick="submitReceiveSimple(${txn.id})">
                        <i class="bi bi-box-arrow-in-down me-1"></i>Confirmar Recepción
                       </button>`);
        }
    }

    if (!['Completed', 'Cancelled'].includes(txn.status)) {
        btns.push(`<button type="button" class="btn btn-outline-danger" onclick="openCancelFromReceive(${txn.id})">
                    <i class="bi bi-x-circle me-1"></i>Cancelar
                   </button>`);
    }

    footer.innerHTML = btns.join('');
}

async function submitReceive(txnId, activeGroupIdx, nextWarehouseId) {
    const deptTimeEl = document.getElementById(`rcvDeptTime${activeGroupIdx}`);
    const arrTime    = document.getElementById(`rcvArrTime${activeGroupIdx}`)?.value?.trim();
    const odoOutEl   = document.getElementById(`rcvOdoOut${activeGroupIdx}`);
    const nextSealEl = document.getElementById(`rcvNextSeal${activeGroupIdx}`);

    const deptTime = deptTimeEl?.value?.trim();
    const odoOut   = odoOutEl?.value?.trim();
    const nextSeal = nextSealEl ? nextSealEl.value.trim() : null;

    // Required: hora llegada
    if (!arrTime) {
        showInfoDialog('La Hora de Llegada es obligatoria para confirmar la recepción.', 'warning');
        document.getElementById(`rcvArrTime${activeGroupIdx}`)?.focus();
        return;
    }
    // Required: hora salida
    if (!deptTime) {
        showInfoDialog('La Hora de Salida es obligatoria para confirmar la recepción.', 'warning');
        deptTimeEl?.focus();
        return;
    }
    // Hora Llegada must be strictly before Hora Salida
    if (arrTime >= deptTime) {
        showInfoDialog('La Hora de Llegada debe ser menor a la Hora de Salida.', 'warning');
        document.getElementById(`rcvArrTime${activeGroupIdx}`)?.focus();
        return;
    }
    // Required: km salida
    if (!odoOut || odoOut === '') {
        showInfoDialog('El Km de Salida es obligatorio para confirmar la recepción.', 'warning');
        odoOutEl?.focus();
        return;
    }
    const odoOutNum = parseFloat(odoOut);
    if (isNaN(odoOutNum) || odoOutNum < 0) {
        showInfoDialog('El Km de Salida debe ser un número válido mayor o igual a cero.', 'warning');
        odoOutEl?.focus();
        return;
    }
    // km must be strictly greater than previous
    const prevKm = odoOutEl ? parseFloat(odoOutEl.dataset.prevKm) : NaN;
    if (!isNaN(prevKm) && prevKm > 0 && odoOutNum <= prevKm) {
        showInfoDialog(`El Km de Salida (${odoOutNum}) debe ser mayor al kilometraje anterior (${prevKm}).`, 'warning');
        odoOutEl?.focus();
        return;
    }
    // Required: new seal when there is a next group
    if (nextSealEl && !nextSeal) {
        showInfoDialog('El Nuevo Sello Destino para la siguiente bodega es obligatorio.', 'warning');
        nextSealEl?.focus();
        return;
    }
    // Block if seal is already flagged as duplicate by real-time check
    if (nextSealEl && nextSeal) {
        const errEl = document.getElementById(`rcvNextSealErr${activeGroupIdx}`);
        if (errEl && errEl.style.display !== 'none') {
            showInfoDialog('El sello ingresado ya está en uso. Ingrese un sello único.', 'warning');
            nextSealEl?.focus();
            return;
        }
        // Final server-side check before submit — no excludeId: must be globally unique
        try {
            const url = `/api/inventorytransaction/check-any-seal?seal=${encodeURIComponent(nextSeal)}`;
            const { exists } = await invFetch(url);
            if (exists) {
                showInfoDialog(`El sello '${nextSeal}' ya está en uso. Debe ingresar un sello único.`, 'warning');
                nextSealEl?.focus();
                return;
            }
        } catch {}
    }

    // Obtain active group lines
    const txn = INV.currentTxn;
    const groups = _getReceiveDestGroups(txn);
    const activeGroup = groups[activeGroupIdx];
    if (!activeGroup) return;

    const pendingLines = activeGroup.lines.filter(l => l.lineStatus !== 'Received' && l.lineStatus !== 'Cancelled');
    if (!pendingLines.length) {
        showInfoDialog('No hay líneas pendientes de recepción en esta bodega.', 'info');
        return;
    }

    const lineIds  = pendingLines.map(l => l.id);
    const lineQtys = pendingLines.map(l => {
        const qtyInput = document.getElementById(`rcv-qty-${l.id}`);
        const qty = qtyInput ? parseFloat(qtyInput.value) : null;
        return { lineId: l.id, qty: (qty > 0 ? qty : (l.qtyDispatched || l.qtyRequested || 0)) };
    });

    const btn = document.getElementById('btnSubmitReceive');
    if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Procesando…'; }

    try {
        await invFetch(`/api/inventorytransaction/${txnId}/receive`, {
            method: 'PATCH',
            body: {
                lineIds,
                lineQtys,
                arrivalTime:     arrTime,
                departureTime:   deptTime,
                odometerOut:     odoOutNum,
                nextDestSeal:    nextSeal || null,
                nextWarehouseId: nextWarehouseId || null,
            },
        });
        bootstrap.Modal.getInstance(document.getElementById('receiveModal')).hide();
        showAlert('Recepción registrada correctamente.', 'success');
        loadMovements(INV.page);
    } catch (e) {
        if (btn) { btn.disabled = false; btn.innerHTML = '<i class="bi bi-box-arrow-in-down me-1"></i>Confirmar Recepción'; }
        showInfoDialog(e.message, 'danger');
    }
}

async function submitReceiveSimple(txnId) {
    const arrTime = document.getElementById('rcvSimpleArrTime')?.value?.trim();
    const odoOut  = document.getElementById('rcvSimpleOdoOut')?.value?.trim();

    if (!arrTime) {
        showInfoDialog('La Hora de Llegada es obligatoria para confirmar la recepción.', 'warning');
        document.getElementById('rcvSimpleArrTime')?.focus();
        return;
    }
    if (!odoOut || parseFloat(odoOut) < 0) {
        showInfoDialog('El Km de Salida es obligatorio para confirmar la recepción.', 'warning');
        return;
    }

    const txn = INV.currentTxn;
    const pendingLines = (txn.lines || []).filter(l => l.lineStatus !== 'Received' && l.lineStatus !== 'Cancelled');

    if (!pendingLines.length) {
        showInfoDialog('No hay líneas pendientes de recepción.', 'info');
        return;
    }

    const lineIds  = pendingLines.map(l => l.id);
    const lineQtys = pendingLines.map(l => {
        const qtyInput = document.getElementById(`rcv-qty-${l.id}`);
        const qty = qtyInput ? parseFloat(qtyInput.value) : null;
        return { lineId: l.id, qty: (qty > 0 ? qty : (l.qtyDispatched || l.qtyRequested || 0)) };
    });

    const btn = document.getElementById('btnSubmitReceive');
    if (btn) { btn.disabled = true; btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1"></span>Procesando…'; }

    try {
        await invFetch(`/api/inventorytransaction/${txnId}/receive`, {
            method: 'PATCH',
            body: {
                lineIds,
                lineQtys,
                arrivalTime:  arrTime,
                odometerOut:  parseFloat(odoOut),
            },
        });
        bootstrap.Modal.getInstance(document.getElementById('receiveModal')).hide();
        showAlert('Recepción registrada correctamente.', 'success');
        loadMovements(INV.page);
    } catch (e) {
        if (btn) { btn.disabled = false; btn.innerHTML = '<i class="bi bi-box-arrow-in-down me-1"></i>Confirmar Recepción'; }
        showInfoDialog(e.message, 'danger');
    }
}

function openCancelFromReceive(id) {
    const rcvModal = bootstrap.Modal.getInstance(document.getElementById('receiveModal'));
    if (rcvModal) rcvModal.hide();
    openCancel(id);
}

// ============================================================
// DETALLE / RECEPCIÓN
// ============================================================

async function openDetail(id) {
    INV.currentTxn = null;
    const body   = document.getElementById('detailBody');
    const footer = document.getElementById('detailFooter');
    body.innerHTML = '<div class="text-center py-4" style="color:#cbd5e1;">Cargando…</div>';
    footer.innerHTML = '<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>';
    document.getElementById('detailTitle').innerHTML =
        '<i class="bi bi-file-earmark-text me-2"></i>Detalle del Movimiento';

    bootstrap.Modal.getOrCreateInstance(document.getElementById('detailModal')).show();

    try {
        const txn = await invFetch(`/api/inventorytransaction/${id}`);
        INV.currentTxn = txn;

        const lines = txn.lines || await invFetch(`/api/inventorytransaction/${id}/lines`);
        txn.lines = lines;

        document.getElementById('detailTitle').innerHTML =
            `<i class="bi bi-file-earmark-text me-2"></i>${txn.transactionNumber} — ${statusBadge(txn.status)}`;

        body.innerHTML = renderDetailBody(txn);
        renderDetailFooter(txn, footer);

    } catch (e) {
        body.innerHTML = `<div class="text-danger text-center py-4">${e.message}</div>`;
    }
}

function renderDetailBody(txn) {
    const typeM = MOVEMENT_TYPES[txn.movementType] || { label: txn.movementType, icon: '📦', css: '' };

    const infoRow = (label, val) =>
        `<div class="col-6 col-md-3 mb-2">
            <div class="text-muted" style="font-size:.72rem;">${label}</div>
            <div class="text-white">${val || '—'}</div>
        </div>`;

    const header = `
        <div class="row mb-3">
            ${infoRow('Número', `<strong>${txn.transactionNumber}</strong>`)}
            ${infoRow('Tipo', `<span class="badge-type ${typeM.css}">${typeM.icon} ${typeM.label}</span>`)}
            ${infoRow('Estado', statusBadge(txn.status))}
            ${infoRow('Fecha', txn.transactionDate)}
            ${infoRow('Origen', warehouseName(txn.idWarehouseOrigin))}
            ${infoRow('Destino', txn.idWarehouseDest ? warehouseName(txn.idWarehouseDest) : '—')}
            ${infoRow('Referencia', txn.reference)}
            ${infoRow('Notas', txn.notes)}
        </div>`;

    const linesHtml = (txn.lines || []).map(l => {
        const lineStatM = {
            Pending:    { css: 'bg-secondary', icon: '⏳' },
            InTransit:  { css: 'bg-warning text-dark', icon: '🚛' },
            Received:   { css: 'bg-success', icon: '✅' },
            Rejected:   { css: 'bg-danger',  icon: '❌' },
            Cancelled:  { css: 'bg-secondary', icon: '❌' },
        }[l.lineStatus] || { css: 'bg-secondary', icon: '' };

        const destName = l.idWarehouseDestLine ? warehouseName(l.idWarehouseDestLine) : '(Según encabezado)';

        return `<tr>
            <td><strong>${l.itemCode}</strong><br><small class="text-muted">${l.itemName}</small></td>
            <td class="text-center">${l.qtyRequested}</td>
            <td class="text-center">${l.qtyDispatched || 0}</td>
            <td class="text-center text-success">${l.qtyReceived || 0}</td>
            <td>${txn.isTransitTransfer ? destName : '—'}</td>
            <td><span class="badge ${lineStatM.css}">${lineStatM.icon} ${l.lineStatus}</span></td>
        </tr>`;
    }).join('');

    return `
        ${header}
        <div class="table-responsive">
            <table class="table table-sm" style="color:var(--inv-text);">
                <thead style="background:#111827;">
                    <tr>
                        <th>Artículo</th>
                        <th class="text-center">Solicitado</th>
                        <th class="text-center">Despachado</th>
                        <th class="text-center">Recibido</th>
                        <th>Bodega Destino</th>
                        <th>Estado Línea</th>
                    </tr>
                </thead>
                <tbody>${linesHtml}</tbody>
            </table>
        </div>`;
}

function renderDetailFooter(txn, footer) {
    const btns = ['<button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cerrar</button>'];

    if (txn.status === 'InTransit' || txn.status === 'PartiallyReceived') {
        btns.push(`<button type="button" class="btn btn-success"
                   onclick="bootstrap.Modal.getInstance(document.getElementById('detailModal')).hide(); openReceive(${txn.id});">
                   <i class="bi bi-box-arrow-in-down me-1"></i>Recibir Movimiento
                 </button>`);
    }

    if (txn.status === 'Confirmed') {
        btns.push(`<button type="button" class="btn btn-success" onclick="completeTxn(${txn.id})">
                    <i class="bi bi-check-all me-1"></i>Completar Movimiento
                   </button>`);
    }

    if (!['Completed', 'Cancelled'].includes(txn.status)) {
        btns.push(`<button type="button" class="btn btn-outline-danger" onclick="openCancel(${txn.id})">
                    <i class="bi bi-x-circle me-1"></i>Cancelar
                   </button>`);
    }

    footer.innerHTML = btns.join('');
}

// ============================================================
// CONFIRMAR / COMPLETAR
// ============================================================

async function confirmTxn(id) {
    if (!confirm('¿Confirmar este movimiento? Esto despachará la mercancía desde la bodega origen.')) return;
    try {
        await invFetch(`/api/inventorytransaction/${id}/confirm`, { method: 'PATCH' });
        showAlert('Movimiento confirmado. Mercancía despachada.', 'success');
        loadMovements(INV.page);
    } catch (e) {
        showAlert(e.message, 'danger');
    }
}

async function completeTxn(id) {
    if (!confirm('¿Completar este movimiento? Se actualizarán los saldos de inventario.')) return;
    try {
        await invFetch(`/api/inventorytransaction/${id}/complete`, { method: 'PATCH' });
        const modal = bootstrap.Modal.getInstance(document.getElementById('detailModal'));
        if (modal) modal.hide();
        showAlert('Movimiento completado. Inventario actualizado.', 'success');
        loadMovements(INV.page);
    } catch (e) {
        showAlert(e.message, 'danger');
    }
}

// ============================================================
// CANCELAR
// ============================================================

function openCancel(id) {
    INV.cancelTargetId = id;
    document.getElementById('cancelReason').value = '';
    const detailModal = bootstrap.Modal.getInstance(document.getElementById('detailModal'));
    if (detailModal) detailModal.hide();
    bootstrap.Modal.getOrCreateInstance(document.getElementById('cancelModal')).show();
}

async function executeCancelTxn() {
    const reason = document.getElementById('cancelReason').value.trim();
    if (!reason) {
        alert('Ingrese el motivo de cancelación.');
        return;
    }
    try {
        await invFetch(`/api/inventorytransaction/${INV.cancelTargetId}/cancel`, {
            method: 'PATCH',
            body: { reason },
        });
        bootstrap.Modal.getInstance(document.getElementById('cancelModal')).hide();
        showAlert('Movimiento cancelado.', 'warning');
        loadMovements(INV.page);
    } catch (e) {
        showAlert(e.message, 'danger');
    }
}

// ============================================================
// BIND EVENTS
// ============================================================

function bindEvents() {
    // Nuevo movimiento
    document.getElementById('btnNewMovement').addEventListener('click', openCreate);

    // Guardar
    document.getElementById('btnSaveMovement').addEventListener('click', () => saveMovement(false));
    document.getElementById('btnSaveConfirm').addEventListener('click',  () => saveMovement(true));

    // Agregar línea (solo modo no-tránsito; en tránsito cada grupo tiene su propio botón)
    document.getElementById('btnAddLine').addEventListener('click', () => {
        INV.lines.push({
            idItem: 0, itemCode: '', itemName: '', qtyRequested: 1,
            idWarehouseDestLine: null,
            idUnitOfMeasure: null, unitOfMeasureCode: '',
            unitCost: null, lotNumber: '', notes: '',
        });
        renderLines();
        const modalBody = document.querySelector('#invModal .modal-body');
        if (modalBody) setTimeout(() => modalBody.scrollTo({ top: modalBody.scrollHeight, behavior: 'smooth' }), 50);
    });

    // Tipo de movimiento cambia → actualizar UI
    document.getElementById('fldType').addEventListener('change', updateTransitUI);

    // Cambio de Fecha → ajustar mínimo de Fecha y de Fecha Esperada Llegada
    document.getElementById('fldDate').addEventListener('change', () => {
        const dateVal   = document.getElementById('fldDate').value;
        const arrivalEl = document.getElementById('fldArrival');
        arrivalEl.min   = dateVal;
        if (!arrivalEl.value || arrivalEl.value < dateVal) {
            arrivalEl.value = dateVal;
        }
    });

    // Impedir seleccionar fecha menor a hoy en fldDate (tanto en crear como en editar)
    document.getElementById('fldDate').addEventListener('input', () => {
        const today  = new Date().toISOString().slice(0, 10);
        const dateEl = document.getElementById('fldDate');
        if (dateEl.value && dateEl.value < today) {
            dateEl.value = today;
            showInfoDialog('La fecha del movimiento no puede ser anterior al día de hoy.', 'warning');
        }
    });

    // Cambio de origen → actualizar opciones de destino
    document.getElementById('fldOrigin').addEventListener('change', () => {
        updateTransitUI();
    });

    // fldFinalDest queda oculto (reemplazado por grupos inline) — sin listener necesario

    // Sincronizar Sello / Hora Salida / Km Salida del encabezado con la primera bodega destino
    const syncHeaderToFirstGroup = () => {
        if (document.getElementById('fldType').value !== 'TransitTransfer') return;
        if (!INV._groups.length) return;
        const seal = document.getElementById('fldSecuritySeal').value.trim();
        const dept = document.getElementById('fldDepartureTime').value;
        const odo  = document.getElementById('fldOdometerOut').value;
        INV._groups[0].sealDest      = seal;
        INV._groups[0].departureTime = dept;
        INV._groups[0].odometerOut   = odo;
        const grpSeal = document.getElementById('grpSeal0');
        const grpDept = document.getElementById('grpDeptTime0');
        const grpOdo  = document.getElementById('grpOdoOut0');
        if (grpSeal) grpSeal.value = seal;
        if (grpDept) grpDept.value = dept;
        if (grpOdo)  grpOdo.value  = odo;
    };
    document.getElementById('fldSecuritySeal').addEventListener('input',  syncHeaderToFirstGroup);
    document.getElementById('fldDepartureTime').addEventListener('change', syncHeaderToFirstGroup);
    document.getElementById('fldOdometerOut').addEventListener('input',   syncHeaderToFirstGroup);

    // Validación del sello de seguridad en tiempo real
    let sealTimer = null;
    document.getElementById('fldSecuritySeal').addEventListener('input', () => {
        clearTimeout(sealTimer);
        const seal = document.getElementById('fldSecuritySeal').value.trim();
        document.getElementById('sealError').classList.add('d-none');
        if (!seal) return;
        sealTimer = setTimeout(async () => {
            try {
                const excludeId = INV.editingId || undefined;
                const url = `/api/inventorytransaction/check-seal?seal=${encodeURIComponent(seal)}`
                    + (excludeId ? `&excludeId=${excludeId}` : '');
                const { exists } = await invFetch(url);
                document.getElementById('sealError').classList.toggle('d-none', !exists);
            } catch {}
        }, 500);
    });

    // Filtros
    let filterTimer;
    const triggerFilter = () => {
        clearTimeout(filterTimer);
        filterTimer = setTimeout(() => {
            INV.filters.search   = document.getElementById('fSearch').value.trim();
            INV.filters.type     = document.getElementById('fType').value;
            INV.filters.status   = document.getElementById('fStatus').value;
            INV.filters.dateFrom = document.getElementById('fDateFrom').value;
            INV.filters.dateTo   = document.getElementById('fDateTo').value;
            loadMovements(1);
        }, 400);
    };

    ['fSearch', 'fType', 'fStatus', 'fDateFrom', 'fDateTo'].forEach(id =>
        document.getElementById(id).addEventListener('change', triggerFilter));
    document.getElementById('fSearch').addEventListener('input', triggerFilter);

    document.getElementById('btnClearFilters').addEventListener('click', () => {
        ['fSearch', 'fDateFrom', 'fDateTo'].forEach(id => document.getElementById(id).value = '');
        ['fType', 'fStatus'].forEach(id => document.getElementById(id).value = '');
        INV.filters = { search: '', type: '', status: '', dateFrom: '', dateTo: '' };
        loadMovements(1);
    });

    // KPI chips como filtros
    document.getElementById('kpiRow').querySelectorAll('.kpi-chip').forEach(chip => {
        chip.addEventListener('click', () => {
            document.getElementById('fStatus').value = chip.dataset.filterStatus || '';
            INV.filters.status = chip.dataset.filterStatus || '';
            document.querySelectorAll('.kpi-chip').forEach(c => c.classList.remove('active'));
            chip.classList.add('active');
            loadMovements(1);
        });
    });

    // Confirmar cancelación
    document.getElementById('btnConfirmCancel').addEventListener('click', executeCancelTxn);
}

// ============================================================
// HELPERS
// ============================================================

function warehouseName(id) {
    if (!id) return '—';
    const w = INV.warehouses.find(w => w.id === id || w.id === +id);
    return w ? `${w.code} — ${w.name}` : `#${id}`;
}

function statusBadge(status) {
    const m = STATUS_META[status] || { label: status, icon: '', css: 'st-Draft' };
    return `<span class="badge-status ${m.css}">${m.icon} ${m.label}</span>`;
}

function showAlert(msg, type = 'info') {
    const el = document.getElementById('invAlert');
    el.className = `inv-alert alert alert-${type}`;
    el.textContent = msg;
    el.classList.remove('d-none');
    setTimeout(() => el.classList.add('d-none'), 5000);
}

function showInfoDialog(msg, type = 'warning') {
    const existing = document.getElementById('_infoDialogOverlay');
    if (existing) existing.remove();

    const iconMap = {
        info:    { icon: 'bi-info-circle',          color: '#38bdf8', border: '#0ea5e9', title: 'Información' },
        warning: { icon: 'bi-exclamation-triangle', color: '#fbbf24', border: '#f59e0b', title: 'Atención'    },
        danger:  { icon: 'bi-x-circle',             color: '#f87171', border: '#ef4444', title: 'Error'       },
        success: { icon: 'bi-check-circle',         color: '#4ade80', border: '#22c55e', title: 'Éxito'       },
    };
    const m = iconMap[type] || iconMap.warning;

    const overlay = document.createElement('div');
    overlay.id = '_infoDialogOverlay';
    overlay.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,.6);z-index:9999;display:flex;align-items:center;justify-content:center;';

    const box = document.createElement('div');
    box.style.cssText = `background:#1e2d4a;border:1px solid ${m.border};border-radius:12px;padding:1.75rem 2rem;max-width:420px;width:90%;box-shadow:0 8px 32px rgba(0,0,0,.6);text-align:center;`;

    const iconEl = document.createElement('i');
    iconEl.className = `bi ${m.icon}`;
    iconEl.style.cssText = `font-size:2.2rem;color:${m.color};display:block;margin-bottom:.85rem;`;

    const titleEl = document.createElement('h6');
    titleEl.style.cssText = 'color:#ffffff;font-weight:700;margin-bottom:.6rem;font-size:1rem;';
    titleEl.textContent = m.title;

    const msgEl = document.createElement('p');
    msgEl.style.cssText = 'color:#cbd5e1;font-size:.9rem;margin-bottom:1.4rem;line-height:1.5;';
    msgEl.textContent = msg;

    const btn = document.createElement('button');
    btn.className = 'btn btn-primary btn-sm px-4';
    btn.innerHTML = '<i class="bi bi-check me-1"></i>Aceptar';
    btn.addEventListener('click', () => overlay.remove());

    box.appendChild(iconEl);
    box.appendChild(titleEl);
    box.appendChild(msgEl);
    box.appendChild(btn);
    overlay.appendChild(box);
    document.body.appendChild(overlay);
    setTimeout(() => btn.focus(), 50);
    overlay.addEventListener('click', e => { if (e.target === overlay) overlay.remove(); });
}

function showModalAlert(msg, type = 'warning') {
    showInfoDialog(msg, type);
}
