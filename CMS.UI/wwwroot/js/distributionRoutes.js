// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/distributionRoutes.js
// PROPÓSITO: Lógica cliente para la pantalla de Rutas de Distribución
// DISEÑO: Touch-first, responsive, tema oscuro, optimizado para tablet.
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-10
// ================================================================================

'use strict';

// ── Estado global ────────────────────────────────────────────────────────────
const DR = {
    apiBase  : '',
    apiToken : '',
    routes   : [],
    total    : 0,
    page     : 1,
    pageSize : 12,
    filters  : { search: '', status: '', frequency: '', isActive: 'true' },
    editingId: null,          // null = crear, number = editar
    stops    : [],            // paradas en el formulario activo
    stopSeq  : 0,             // contador local para paradas nuevas (id temporal negativo)
    warehouses: [],
    users    : []
};

const DAYS = ['Lun','Mar','Mié','Jue','Vie','Sáb','Dom'];
const DAY_BITS = [1, 2, 4, 8, 16, 32, 64];

const STATUS_CFG = {
    Active    : { label:'Activa',     color:'#22c55e', icon:'bi-check-circle-fill' },
    Inactive  : { label:'Inactiva',   color:'#94a3b8', icon:'bi-pause-circle-fill' },
    InProgress: { label:'En ruta',    color:'#f59e0b', icon:'bi-truck'             }
};

const FREQ_CFG = {
    Daily    : { label:'Diaria',     icon:'bi-calendar-day'   },
    Weekly   : { label:'Semanal',    icon:'bi-calendar-week'  },
    BiWeekly : { label:'Quincenal',  icon:'bi-calendar2'      },
    Monthly  : { label:'Mensual',    icon:'bi-calendar-month' },
    OnDemand : { label:'Bajo demanda',icon:'bi-lightning'     }
};

// ── Init ─────────────────────────────────────────────────────────────────────
function initDistributionRoutes(apiBase, apiToken) {
    DR.apiBase  = apiBase;
    DR.apiToken = apiToken;
    bindEvents();
    Promise.all([loadWarehouses(), loadUsers()]).then(() => loadRoutes());
}

// ── HTTP ─────────────────────────────────────────────────────────────────────
async function drFetch(path, opts = {}) {
    const res = await fetch(DR.apiBase + path, {
        ...opts,
        headers: {
            'Content-Type' : 'application/json',
            'Authorization': `Bearer ${DR.apiToken}`,
            ...(opts.headers || {})
        }
    });
    if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        throw new Error(body.error || body.detail || `HTTP ${res.status}`);
    }
    return res.status === 204 ? null : res.json();
}

// ── Carga catálogos ───────────────────────────────────────────────────────────
async function loadWarehouses() {
    try {
        const data = await drFetch('/api/warehouse?isActive=true&pageSize=200');
        DR.warehouses = data.items || [];
        const sel = document.getElementById('fOriginWarehouse');
        if (!sel) return;
        sel.innerHTML = '<option value="">— Sin bodega origen —</option>';
        DR.warehouses.forEach(w => {
            sel.innerHTML += `<option value="${w.id}">${w.code} – ${w.name}</option>`;
        });
    } catch { /* no crítico */ }
}

async function loadUsers() {
    try {
        const data = await drFetch('/api/user/for-company?pageSize=200');
        DR.users = data.items || data || [];
        const sel = document.getElementById('fDriver');
        if (!sel) return;
        sel.innerHTML = '<option value="">— Sin conductor asignado —</option>';
        DR.users.forEach(u => {
            sel.innerHTML += `<option value="${u.id}">${u.firstName} ${u.lastName}</option>`;
        });
    } catch { /* no crítico */ }
}

// ── Carga rutas ───────────────────────────────────────────────────────────────
async function loadRoutes() {
    const p = new URLSearchParams({
        page    : DR.page,
        pageSize: DR.pageSize
    });
    if (DR.filters.search)    p.set('search',    DR.filters.search);
    if (DR.filters.status)    p.set('status',    DR.filters.status);
    if (DR.filters.frequency) p.set('frequency', DR.filters.frequency);
    if (DR.filters.isActive !== '') p.set('isActive', DR.filters.isActive);

    setLoading(true);
    try {
        const data = await drFetch(`/api/distributionroute?${p}`);
        DR.routes = data.items || [];
        DR.total  = data.totalCount || 0;
        renderGrid();
        renderPagination(data.totalPages || 1);
        updateKPIs();
    } catch (err) {
        showAlert('danger', `Error al cargar rutas: ${err.message}`);
    } finally {
        setLoading(false);
    }
}

// ── KPIs ──────────────────────────────────────────────────────────────────────
function updateKPIs() {
    const active   = DR.routes.filter(r => r.status === 'Active').length;
    const inProg   = DR.routes.filter(r => r.status === 'InProgress').length;
    const inactive = DR.routes.filter(r => r.status === 'Inactive' || !r.isActive).length;
    const stops    = DR.routes.reduce((s, r) => s + (r.stopCount || 0), 0);
    setText('kpiTotal',    DR.total);
    setText('kpiActive',   active);
    setText('kpiInProgress', inProg);
    setText('kpiStops',    stops);
}

// ── Render grid ───────────────────────────────────────────────────────────────
function renderGrid() {
    const container = document.getElementById('routesGrid');
    if (!container) return;

    if (DR.routes.length === 0) {
        container.innerHTML = `
            <div class="col-12 text-center py-5">
                <i class="bi bi-map" style="font-size:3rem;color:#334155;"></i>
                <p class="text-muted mt-3">No se encontraron rutas de distribución</p>
                <button class="btn btn-tech-primary mt-2" onclick="openCreate()">
                    <i class="bi bi-plus-lg me-2"></i>Crear Primera Ruta
                </button>
            </div>`;
        return;
    }

    container.innerHTML = DR.routes.map(r => routeCard(r)).join('');
}

function routeCard(r) {
    const st  = STATUS_CFG[r.status] || STATUS_CFG.Active;
    const fr  = FREQ_CFG[r.frequency] || FREQ_CFG.Daily;
    const days = daysLabel(r.operationDays);
    const opacity = r.isActive ? '1' : '0.55';

    return `
    <div class="col-12 col-sm-6 col-xl-4">
      <div class="dr-card" style="border-left:3px solid ${st.color};opacity:${opacity};" onclick="openDetail(${r.id})">
        <div class="d-flex justify-content-between align-items-start mb-2">
          <div>
            <span class="dr-code">${esc(r.code)}</span>
            <span class="dr-status-badge ms-2" style="background:${st.color}22;color:${st.color};">
              <i class="bi ${st.icon} me-1" style="font-size:.65rem;"></i>${st.label}
            </span>
          </div>
          <div class="dropdown" onclick="event.stopPropagation()">
            <button class="btn btn-sm btn-icon-ghost" data-bs-toggle="dropdown">
              <i class="bi bi-three-dots-vertical"></i>
            </button>
            <ul class="dropdown-menu dropdown-menu-dark dropdown-menu-end">
              <li><a class="dropdown-item" onclick="openEdit(${r.id})"><i class="bi bi-pencil me-2"></i>Editar</a></li>
              <li><a class="dropdown-item" onclick="openStops(${r.id},'${esc(r.name)}')"><i class="bi bi-signpost-split me-2"></i>Paradas</a></li>
              <li><hr class="dropdown-divider"></li>
              ${r.isActive
                ? `<li><a class="dropdown-item text-warning" onclick="deactivateRoute(${r.id})"><i class="bi bi-pause-circle me-2"></i>Desactivar</a></li>`
                : `<li><a class="dropdown-item text-success" onclick="activateRoute(${r.id})"><i class="bi bi-play-circle me-2"></i>Activar</a></li>`}
            </ul>
          </div>
        </div>

        <h6 class="dr-name mb-1">${esc(r.name)}</h6>
        ${r.description ? `<p class="dr-desc mb-2">${esc(r.description)}</p>` : ''}

        <div class="dr-meta-row">
          <span class="dr-meta-pill"><i class="bi ${fr.icon} me-1"></i>${fr.label}</span>
          ${r.departureTime ? `<span class="dr-meta-pill"><i class="bi bi-clock me-1"></i>${r.departureTime}</span>` : ''}
          ${r.estimatedDurationMinutes ? `<span class="dr-meta-pill"><i class="bi bi-hourglass me-1"></i>${fmtDuration(r.estimatedDurationMinutes)}</span>` : ''}
        </div>

        <div class="dr-meta-row mt-1">
          ${r.vehiclePlate ? `<span class="dr-meta-pill"><i class="bi bi-truck me-1"></i>${esc(r.vehiclePlate)}</span>` : ''}
          ${r.driverName   ? `<span class="dr-meta-pill"><i class="bi bi-person me-1"></i>${esc(r.driverName)}</span>` : ''}
        </div>

        <div class="d-flex justify-content-between align-items-center mt-3 pt-2" style="border-top:1px solid rgba(255,255,255,.07);">
          <span class="dr-days">${days}</span>
          <span class="dr-stops-badge">
            <i class="bi bi-signpost-split me-1"></i>${r.stopCount || 0} parada${r.stopCount !== 1 ? 's' : ''}
          </span>
        </div>
      </div>
    </div>`;
}

// ── Helpers visuales ──────────────────────────────────────────────────────────
function daysLabel(bits) {
    if (bits === 127) return '<span class="badge" style="background:#334155;color:#94a3b8;font-size:.7rem;">Todos los días</span>';
    if (bits === 31)  return '<span class="badge" style="background:#334155;color:#94a3b8;font-size:.7rem;">Lun – Vie</span>';
    return DAY_BITS.map((b, i) =>
        `<span class="dr-day-chip ${(bits & b) ? 'active' : ''}">${DAYS[i]}</span>`
    ).join('');
}

function fmtDuration(min) {
    if (!min) return '';
    const h = Math.floor(min / 60), m = min % 60;
    return h > 0 ? `${h}h${m > 0 ? m + 'm' : ''}` : `${m}m`;
}

// ── Paginación ────────────────────────────────────────────────────────────────
function renderPagination(totalPages) {
    const el = document.getElementById('pagination');
    if (!el) return;
    if (totalPages <= 1) { el.innerHTML = ''; return; }

    let html = `<li class="page-item ${DR.page===1?'disabled':''}">
        <a class="page-link" onclick="goPage(${DR.page-1})"><i class="bi bi-chevron-left"></i></a></li>`;
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || Math.abs(i - DR.page) <= 1) {
            html += `<li class="page-item ${i===DR.page?'active':''}">
                <a class="page-link" onclick="goPage(${i})">${i}</a></li>`;
        } else if (Math.abs(i - DR.page) === 2) {
            html += `<li class="page-item disabled"><a class="page-link">…</a></li>`;
        }
    }
    html += `<li class="page-item ${DR.page===totalPages?'disabled':''}">
        <a class="page-link" onclick="goPage(${DR.page+1})"><i class="bi bi-chevron-right"></i></a></li>`;
    el.innerHTML = html;
}

function goPage(p) { DR.page = p; loadRoutes(); }

// ── Eventos ───────────────────────────────────────────────────────────────────
function bindEvents() {
    // Búsqueda con debounce
    let searchTimer;
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', () => {
            clearTimeout(searchTimer);
            searchTimer = setTimeout(() => {
                DR.filters.search = searchInput.value.trim();
                DR.page = 1;
                loadRoutes();
            }, 350);
        });
    }

    document.getElementById('filterStatus')?.addEventListener('change', e => {
        DR.filters.status = e.target.value;
        DR.page = 1;
        loadRoutes();
    });

    document.getElementById('filterFrequency')?.addEventListener('change', e => {
        DR.filters.frequency = e.target.value;
        DR.page = 1;
        loadRoutes();
    });

    document.getElementById('filterActive')?.addEventListener('change', e => {
        DR.filters.isActive = e.target.value;
        DR.page = 1;
        loadRoutes();
    });

    // Botones de días en el formulario
    document.querySelectorAll('.day-toggle').forEach(btn => {
        btn.addEventListener('click', () => btn.classList.toggle('active'));
    });
}

// ── Filtros reset ─────────────────────────────────────────────────────────────
function clearFilters() {
    DR.filters = { search: '', status: '', frequency: '', isActive: 'true' };
    DR.page    = 1;
    document.getElementById('searchInput').value    = '';
    document.getElementById('filterStatus').value   = '';
    document.getElementById('filterFrequency').value = '';
    document.getElementById('filterActive').value   = 'true';
    loadRoutes();
}

// ── Abrir modal crear ─────────────────────────────────────────────────────────
function openCreate() {
    DR.editingId = null;
    DR.stops     = [];
    resetForm();
    document.getElementById('modalRouteTitle').textContent = 'Nueva Ruta de Distribución';
    document.getElementById('fStatus').value    = 'Active';
    document.getElementById('fFrequency').value = 'Daily';
    document.getElementById('fIsActive').checked = true;
    setDayBits(31); // Lun-Vie por defecto
    renderStopsInForm();
    new bootstrap.Modal(document.getElementById('routeModal')).show();
}

// ── Abrir modal editar ────────────────────────────────────────────────────────
async function openEdit(id) {
    DR.editingId = id;
    try {
        const r = await drFetch(`/api/distributionroute/${id}`);
        DR.stops = (r.stops || []).map(s => ({ ...s, _tmpId: s.id }));

        document.getElementById('modalRouteTitle').textContent = `Editar Ruta — ${r.code}`;
        document.getElementById('fCode').value             = r.code;
        document.getElementById('fName').value             = r.name;
        document.getElementById('fDescription').value      = r.description || '';
        document.getElementById('fStatus').value           = r.status;
        document.getElementById('fFrequency').value        = r.frequency;
        document.getElementById('fDeparture').value        = r.departureTime || '';
        document.getElementById('fDuration').value         = r.estimatedDurationMinutes || '';
        document.getElementById('fDistance').value         = r.estimatedDistanceKm || '';
        document.getElementById('fPlate').value            = r.vehiclePlate || '';
        document.getElementById('fVehicleDesc').value      = r.vehicleDescription || '';
        document.getElementById('fDriver').value           = r.driverUserId || '';
        document.getElementById('fOriginWarehouse').value  = r.idOriginWarehouse || '';
        document.getElementById('fMaxWeight').value        = r.maxWeightKg || '';
        document.getElementById('fMaxVolume').value        = r.maxVolumeM3 || '';
        document.getElementById('fReqSignature').checked   = r.requiresSignature;
        document.getElementById('fReqPhoto').checked       = r.requiresPhoto;
        document.getElementById('fAllowPartial').checked   = r.allowsPartialDelivery;
        document.getElementById('fIsActive').checked       = r.isActive;
        document.getElementById('fNotes').value            = r.notes || '';
        setDayBits(r.operationDays);
        renderStopsInForm();
        new bootstrap.Modal(document.getElementById('routeModal')).show();
    } catch (err) {
        showAlert('danger', `Error al cargar ruta: ${err.message}`);
    }
}

// ── Abrir detalle (solo lectura) ──────────────────────────────────────────────
async function openDetail(id) {
    try {
        const r = await drFetch(`/api/distributionroute/${id}`);
        renderDetail(r);
        new bootstrap.Modal(document.getElementById('detailModal')).show();
    } catch (err) {
        showAlert('danger', `Error al cargar detalle: ${err.message}`);
    }
}

function renderDetail(r) {
    const st = STATUS_CFG[r.status] || STATUS_CFG.Active;
    const fr = FREQ_CFG[r.frequency] || FREQ_CFG.Daily;
    const el = document.getElementById('detailBody');

    el.innerHTML = `
    <div class="row g-3">
      <!-- Cabecera -->
      <div class="col-12">
        <div class="d-flex align-items-center gap-3 mb-3">
          <div>
            <h5 class="text-white mb-0">${esc(r.name)}</h5>
            <span class="text-muted" style="font-size:.8rem;">${esc(r.code)}</span>
          </div>
          <span class="dr-status-badge" style="background:${st.color}22;color:${st.color};">
            <i class="bi ${st.icon} me-1"></i>${st.label}
          </span>
        </div>
        ${r.description ? `<p class="text-light small mb-0">${esc(r.description)}</p>` : ''}
      </div>

      <!-- Operación -->
      <div class="col-6 col-md-3">
        <div class="detail-block">
          <div class="detail-label">Frecuencia</div>
          <div class="detail-value"><i class="bi ${fr.icon} me-1"></i>${fr.label}</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="detail-block">
          <div class="detail-label">Salida</div>
          <div class="detail-value">${r.departureTime || '—'}</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="detail-block">
          <div class="detail-label">Duración est.</div>
          <div class="detail-value">${r.estimatedDurationMinutes ? fmtDuration(r.estimatedDurationMinutes) : '—'}</div>
        </div>
      </div>
      <div class="col-6 col-md-3">
        <div class="detail-block">
          <div class="detail-label">Distancia est.</div>
          <div class="detail-value">${r.estimatedDistanceKm ? r.estimatedDistanceKm + ' km' : '—'}</div>
        </div>
      </div>

      <!-- Días -->
      <div class="col-12">
        <div class="detail-label mb-1">Días de operación</div>
        <div>${daysLabel(r.operationDays)}</div>
      </div>

      <!-- Vehículo / Conductor -->
      <div class="col-6 col-md-4">
        <div class="detail-block">
          <div class="detail-label">Placa / Vehículo</div>
          <div class="detail-value">${r.vehiclePlate ? `<strong>${esc(r.vehiclePlate)}</strong> ${esc(r.vehicleDescription || '')}` : '—'}</div>
        </div>
      </div>
      <div class="col-6 col-md-4">
        <div class="detail-block">
          <div class="detail-label">Conductor</div>
          <div class="detail-value">${r.driverName || '—'}</div>
        </div>
      </div>
      <div class="col-6 col-md-4">
        <div class="detail-block">
          <div class="detail-label">Bodega origen</div>
          <div class="detail-value">${r.originWarehouseName || '—'}</div>
        </div>
      </div>

      <!-- Paradas -->
      <div class="col-12">
        <h6 class="form-section-title mt-2">
          <i class="bi bi-signpost-split me-2"></i>Paradas (${(r.stops || []).length})
        </h6>
        ${renderStopsReadOnly(r.stops || [])}
      </div>

      <!-- Notas -->
      ${r.notes ? `
      <div class="col-12">
        <div class="detail-label mb-1">Notas</div>
        <p class="text-light small mb-0">${esc(r.notes)}</p>
      </div>` : ''}
    </div>`;

    // Botones
    document.getElementById('detailEditBtn').onclick    = () => { bootstrap.Modal.getInstance(document.getElementById('detailModal')).hide(); openEdit(r.id); };
    document.getElementById('detailStopsBtn').onclick   = () => { bootstrap.Modal.getInstance(document.getElementById('detailModal')).hide(); openStops(r.id, r.name); };
    document.getElementById('detailToggleBtn').textContent = r.isActive ? 'Desactivar' : 'Activar';
    document.getElementById('detailToggleBtn').className   = r.isActive ? 'btn btn-warning' : 'btn btn-success';
    document.getElementById('detailToggleBtn').onclick     = () => r.isActive ? deactivateRoute(r.id) : activateRoute(r.id);
}

function renderStopsReadOnly(stops) {
    if (!stops.length) return '<p class="text-muted small">Sin paradas registradas</p>';
    return `<div class="stops-readonly-list">
        ${stops.map((s, i) => `
        <div class="stop-readonly-item">
          <span class="stop-num">${s.stopOrder || i+1}</span>
          <div class="stop-readonly-info">
            <div class="text-white" style="font-size:.875rem;">${esc(s.customerName || 'Sin nombre')}</div>
            <div class="text-muted" style="font-size:.75rem;">${esc(s.address || '')}${s.city ? ', '+esc(s.city) : ''}</div>
            ${s.timeWindowStart ? `<div style="font-size:.72rem;color:#94a3b8;"><i class="bi bi-clock me-1"></i>${s.timeWindowStart} – ${s.timeWindowEnd || '?'}</div>` : ''}
          </div>
          ${s.contactPhone ? `<a class="btn btn-sm btn-icon-ghost" href="tel:${esc(s.contactPhone)}"><i class="bi bi-telephone"></i></a>` : ''}
        </div>`).join('')}
    </div>`;
}

// ── Guardar ruta ──────────────────────────────────────────────────────────────
async function saveRoute() {
    const dto = buildRouteDto();
    if (!dto) return;

    const btn = document.getElementById('btnSaveRoute');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Guardando…';

    try {
        const isNew  = DR.editingId === null;
        const method = isNew ? 'POST' : 'PUT';
        const url    = isNew ? '/api/distributionroute' : `/api/distributionroute/${DR.editingId}`;

        const saved = await drFetch(url, { method, body: JSON.stringify(dto) });

        // Guardar paradas si hay
        if (DR.stops.length > 0) {
            const stopsDtos = DR.stops.map(s => ({
                customerName           : s.customerName || '',
                address                : s.address || '',
                city                   : s.city || '',
                gpsLatitude            : s.gpsLatitude || null,
                gpsLongitude           : s.gpsLongitude || null,
                contactName            : s.contactName || '',
                contactPhone           : s.contactPhone || '',
                timeWindowStart        : s.timeWindowStart || '',
                timeWindowEnd          : s.timeWindowEnd || '',
                estimatedServiceMinutes: s.estimatedServiceMinutes || null,
                notes                  : s.notes || ''
            }));
            await drFetch(`/api/distributionroute/${saved.id}/stops`, { method: 'POST', body: JSON.stringify(stopsDtos) });
        }

        bootstrap.Modal.getInstance(document.getElementById('routeModal'))?.hide();
        showAlert('success', `Ruta <strong>${saved.code}</strong> ${isNew ? 'creada' : 'actualizada'} exitosamente`);
        loadRoutes();
    } catch (err) {
        showAlert('danger', `Error al guardar: ${err.message}`);
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-check-lg me-2"></i>Guardar Ruta';
    }
}

function buildRouteDto() {
    const get  = id => document.getElementById(id)?.value?.trim() || '';
    const getN = id => { const v = document.getElementById(id)?.value; return v ? parseFloat(v) : null; };
    const getI = id => { const v = document.getElementById(id)?.value; return v ? parseInt(v) : null; };
    const chk  = id => document.getElementById(id)?.checked ?? false;

    const code = get('fCode');
    const name = get('fName');
    if (!code) { showAlert('warning', 'El código de la ruta es requerido'); return null; }
    if (!name) { showAlert('warning', 'El nombre de la ruta es requerido'); return null; }

    return {
        code                    : code.toUpperCase(),
        name,
        description             : get('fDescription') || null,
        status                  : get('fStatus') || 'Active',
        frequency               : get('fFrequency') || 'Daily',
        operationDays           : getDayBits(),
        departureTime           : get('fDeparture') || null,
        estimatedDurationMinutes: getI('fDuration'),
        estimatedDistanceKm     : getN('fDistance'),
        vehiclePlate            : get('fPlate') || null,
        vehicleDescription      : get('fVehicleDesc') || null,
        driverUserId            : getI('fDriver'),
        idOriginWarehouse       : getI('fOriginWarehouse'),
        maxWeightKg             : getN('fMaxWeight'),
        maxVolumeM3             : getN('fMaxVolume'),
        requiresSignature       : chk('fReqSignature'),
        requiresPhoto           : chk('fReqPhoto'),
        allowsPartialDelivery   : chk('fAllowPartial'),
        isActive                : chk('fIsActive'),
        notes                   : get('fNotes') || null
    };
}

// ── Días bitmask ──────────────────────────────────────────────────────────────
function getDayBits() {
    let bits = 0;
    document.querySelectorAll('.day-toggle').forEach((btn, i) => {
        if (btn.classList.contains('active')) bits |= DAY_BITS[i];
    });
    return bits;
}

function setDayBits(bits) {
    document.querySelectorAll('.day-toggle').forEach((btn, i) => {
        btn.classList.toggle('active', !!(bits & DAY_BITS[i]));
    });
}

// ── Paradas en formulario ─────────────────────────────────────────────────────
function openStops(routeId, routeName) {
    DR.editingId = routeId;
    document.getElementById('stopsRouteTitle').textContent = routeName || '';
    drFetch(`/api/distributionroute/${routeId}/stops`).then(stops => {
        DR.stops = stops.map(s => ({ ...s, _tmpId: s.id }));
        // Apuntar al contenedor standalone
        renderStopsStandalone();
        new bootstrap.Modal(document.getElementById('stopsModal')).show();
    }).catch(err => showAlert('danger', `Error al cargar paradas: ${err.message}`));
}

function addStopStandalone() {
    addStop();
    renderStopsStandalone();
}

function renderStopsStandalone() {
    const container = document.getElementById('stopsContainerStandalone');
    if (!container) return;
    _renderStopsTo(container);
}

function addStop() {
    DR.stops.push({
        _tmpId                 : --DR.stopSeq,
        customerName           : '',
        address                : '',
        city                   : '',
        contactName            : '',
        contactPhone           : '',
        timeWindowStart        : '',
        timeWindowEnd          : '',
        estimatedServiceMinutes: null,
        notes                  : ''
    });
    renderStopsInForm();
    // Scroll al último
    setTimeout(() => {
        const items = document.querySelectorAll('.stop-form-item');
        items[items.length - 1]?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    }, 100);
}

function removeStop(tmpId) {
    DR.stops = DR.stops.filter(s => s._tmpId !== tmpId);
    renderStopsInForm();
}

function moveStop(tmpId, dir) {
    const idx = DR.stops.findIndex(s => s._tmpId === tmpId);
    if (idx < 0) return;
    const newIdx = idx + dir;
    if (newIdx < 0 || newIdx >= DR.stops.length) return;
    [DR.stops[idx], DR.stops[newIdx]] = [DR.stops[newIdx], DR.stops[idx]];
    renderStopsInForm();
}

function renderStopsInForm() {
    // El contenedor puede estar en el modal de ruta (routeModal) o en el standalone (stopsModal)
    const container = document.getElementById('stopsContainer')
                   || document.getElementById('stopsContainerStandalone');
    if (!container) return;
    _renderStopsTo(container);
}

function _renderStopsTo(container) {
    if (DR.stops.length === 0) {
        container.innerHTML = `
            <div class="text-center py-4 text-muted">
                <i class="bi bi-signpost-split" style="font-size:2rem;opacity:.3;"></i>
                <p class="mt-2 small">Sin paradas. Use <strong>+ Agregar Parada</strong>.</p>
            </div>`;
        return;
    }

    container.innerHTML = DR.stops.map((s, i) => `
    <div class="stop-form-item" data-tmpid="${s._tmpId}">
      <div class="stop-form-header">
        <span class="stop-num">${i + 1}</span>
        <span class="text-white" style="font-size:.85rem;font-weight:500;">${esc(s.customerName || 'Nueva parada')}</span>
        <div class="d-flex gap-1 ms-auto">
          <button class="btn btn-sm btn-icon-ghost" onclick="moveStop(${s._tmpId},-1)" ${i===0?'disabled':''} title="Subir">
            <i class="bi bi-chevron-up"></i>
          </button>
          <button class="btn btn-sm btn-icon-ghost" onclick="moveStop(${s._tmpId},1)" ${i===DR.stops.length-1?'disabled':''} title="Bajar">
            <i class="bi bi-chevron-down"></i>
          </button>
          <button class="btn btn-sm btn-icon-ghost text-danger" onclick="removeStop(${s._tmpId})" title="Eliminar">
            <i class="bi bi-trash"></i>
          </button>
        </div>
      </div>
      <div class="stop-form-body">
        <div class="row g-2">
          <div class="col-12 col-md-6">
            <label class="form-label" style="font-size:.75rem;color:#94a3b8;">Cliente / Destinatario</label>
            <input type="text" class="form-control form-control-sm" value="${esc(s.customerName||'')}"
              oninput="updateStop(${s._tmpId},'customerName',this.value)" placeholder="Nombre del cliente…">
          </div>
          <div class="col-12 col-md-6">
            <label class="form-label" style="font-size:.75rem;color:#94a3b8;">Dirección</label>
            <input type="text" class="form-control form-control-sm" value="${esc(s.address||'')}"
              oninput="updateStop(${s._tmpId},'address',this.value)" placeholder="Dirección de entrega…">
          </div>
          <div class="col-6 col-md-4">
            <label class="form-label" style="font-size:.75rem;color:#94a3b8;">Ciudad</label>
            <input type="text" class="form-control form-control-sm" value="${esc(s.city||'')}"
              oninput="updateStop(${s._tmpId},'city',this.value)" placeholder="Ciudad">
          </div>
          <div class="col-6 col-md-4">
            <label class="form-label" style="font-size:.75rem;color:#94a3b8;">Contacto</label>
            <input type="text" class="form-control form-control-sm" value="${esc(s.contactName||'')}"
              oninput="updateStop(${s._tmpId},'contactName',this.value)" placeholder="Nombre contacto">
          </div>
          <div class="col-6 col-md-4">
            <label class="form-label" style="font-size:.75rem;color:#94a3b8;">Teléfono</label>
            <input type="tel" class="form-control form-control-sm" value="${esc(s.contactPhone||'')}"
              oninput="updateStop(${s._tmpId},'contactPhone',this.value)" placeholder="+506 …">
          </div>
          <div class="col-6 col-md-4">
            <label class="form-label" style="font-size:.75rem;color:#94a3b8;">Ventana: Desde</label>
            <input type="time" class="form-control form-control-sm" value="${s.timeWindowStart||''}"
              oninput="updateStop(${s._tmpId},'timeWindowStart',this.value)">
          </div>
          <div class="col-6 col-md-4">
            <label class="form-label" style="font-size:.75rem;color:#94a3b8;">Ventana: Hasta</label>
            <input type="time" class="form-control form-control-sm" value="${s.timeWindowEnd||''}"
              oninput="updateStop(${s._tmpId},'timeWindowEnd',this.value)">
          </div>
          <div class="col-6 col-md-4">
            <label class="form-label" style="font-size:.75rem;color:#94a3b8;">Servicio (min)</label>
            <input type="number" class="form-control form-control-sm" value="${s.estimatedServiceMinutes||''}"
              oninput="updateStop(${s._tmpId},'estimatedServiceMinutes',this.value?parseInt(this.value):null)" min="1" placeholder="15">
          </div>
        </div>
      </div>
    </div>`).join('');
}

function updateStop(tmpId, field, value) {
    const stop = DR.stops.find(s => s._tmpId === tmpId);
    if (stop) {
        stop[field] = value;
        // Re-renderizar solo el header para actualizar el nombre
        if (field === 'customerName') {
            const header = document.querySelector(`.stop-form-item[data-tmpid="${tmpId}"] .text-white`);
            if (header) header.textContent = value || 'Nueva parada';
        }
    }
}

async function saveStops() {
    if (DR.editingId === null) return;
    const btn = document.getElementById('btnSaveStops');
    btn.disabled = true;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Guardando…';

    try {
        const dtos = DR.stops.map(s => ({
            customerName           : s.customerName || '',
            address                : s.address || '',
            city                   : s.city || '',
            gpsLatitude            : s.gpsLatitude || null,
            gpsLongitude           : s.gpsLongitude || null,
            contactName            : s.contactName || '',
            contactPhone           : s.contactPhone || '',
            timeWindowStart        : s.timeWindowStart || '',
            timeWindowEnd          : s.timeWindowEnd || '',
            estimatedServiceMinutes: s.estimatedServiceMinutes || null,
            notes                  : s.notes || ''
        }));
        await drFetch(`/api/distributionroute/${DR.editingId}/stops`, { method: 'POST', body: JSON.stringify(dtos) });
        bootstrap.Modal.getInstance(document.getElementById('stopsModal'))?.hide();
        showAlert('success', 'Paradas guardadas exitosamente');
        loadRoutes();
    } catch (err) {
        showAlert('danger', `Error al guardar paradas: ${err.message}`);
    } finally {
        btn.disabled = false;
        btn.innerHTML = '<i class="bi bi-check-lg me-2"></i>Guardar Paradas';
    }
}

// ── Activar / Desactivar ──────────────────────────────────────────────────────
async function deactivateRoute(id) {
    if (!confirm('¿Desactivar esta ruta?')) return;
    try {
        await drFetch(`/api/distributionroute/${id}/deactivate`, { method: 'PATCH' });
        showAlert('success', 'Ruta desactivada');
        loadRoutes();
    } catch (err) {
        showAlert('danger', `Error: ${err.message}`);
    }
}

async function activateRoute(id) {
    try {
        await drFetch(`/api/distributionroute/${id}/activate`, { method: 'PATCH' });
        showAlert('success', 'Ruta activada');
        loadRoutes();
    } catch (err) {
        showAlert('danger', `Error: ${err.message}`);
    }
}

// ── Utils UI ──────────────────────────────────────────────────────────────────
function resetForm() {
    ['fCode','fName','fDescription','fDeparture','fDuration','fDistance',
     'fPlate','fVehicleDesc','fMaxWeight','fMaxVolume','fNotes']
        .forEach(id => { const el = document.getElementById(id); if (el) el.value = ''; });
    document.getElementById('fDriver').value           = '';
    document.getElementById('fOriginWarehouse').value  = '';
    document.getElementById('fReqSignature').checked   = false;
    document.getElementById('fReqPhoto').checked       = false;
    document.getElementById('fAllowPartial').checked   = true;
}

function setLoading(on) {
    const el = document.getElementById('loadingSpinner');
    if (el) el.style.display = on ? 'block' : 'none';
    const grid = document.getElementById('routesGrid');
    if (grid) grid.style.opacity = on ? '0.4' : '1';
}

function showAlert(type, msg) {
    const el = document.getElementById('pageAlert');
    if (!el) return;
    el.className = `alert alert-${type} alert-dismissible fade show`;
    el.innerHTML = `${msg}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
    el.classList.remove('d-none');
    if (type === 'success') setTimeout(() => el.classList.add('d-none'), 4000);
}

function setText(id, val) {
    const el = document.getElementById(id);
    if (el) el.textContent = val;
}

function esc(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
