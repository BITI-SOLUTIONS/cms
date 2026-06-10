// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/fleetCatalogs.js
// PROPÓSITO: Lógica cliente para mantenimiento de catálogos Fleet Management
// CATÁLOGOS: TipoUnidad, EstadoUnidad, TipoCombustible, Marca, Modelo
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-14
// ================================================================================

const FC = (() => {
    const API   = () => window.FC_API   || '';
    const TOKEN = () => window.FC_TOKEN || '';

    const headers = () => ({
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${TOKEN()}`
    });

    /* ── Estado interno ─────────────────────────────────── */
    let _catalog  = '';        // 'unitType' | 'status' | 'fuel' | 'brand' | 'model'
    let _editId   = null;
    let _deleteId = null;
    let _brands     = [];
    let _countries  = [];
    let _unitTypes  = [];       // tipos de unidad para el select de modelos
    let _allModels  = [];       // copia completa para filtrado por marca

    /* ── Endpoints por catálogo ──────────────────────────── */
    const ENDPOINTS = {
        unitType : 'fleet-catalog/unit-types',
        status   : 'fleet-catalog/unit-statuses',
        fuel     : 'fleet-catalog/fuel-types',
        brand    : 'fleet-catalog/brands',
        model    : 'fleet-catalog/models',
    };

    const TITLES = {
        unitType : 'Tipo de Unidad',
        status   : 'Estado de Unidad',
        fuel     : 'Tipo de Combustible',
        brand    : 'Marca',
        model    : 'Modelo',
    };

    /* ── Fetch helper ────────────────────────────────────── */
    async function req(method, path, body) {
        const opts = { method, headers: headers() };
        if (body) opts.body = JSON.stringify(body);
        const r = await fetch(`${API()}/api/${path}`, opts);
        if (!r.ok) {
            const err = await r.json().catch(() => ({ message: r.statusText }));
            throw new Error(err.message || r.statusText);
        }
        if (r.status === 204) return null;
        return r.json();
    }

    /* ── Toast ───────────────────────────────────────────── */
    function toast(msg, isError = false) {
        const el = document.createElement('div');
        el.className = `alert alert-${isError ? 'danger' : 'success'} position-fixed bottom-0 end-0 m-3 py-2 px-3 shadow`;
        el.style.zIndex = 9999;
        el.innerHTML = `<i class="bi bi-${isError ? 'exclamation-triangle' : 'check-circle'} me-2"></i>${msg}`;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 3500);
    }

    /* ── Badge para estado activo ────────────────────────── */
    const activeBadge = v => v
        ? '<span class="badge bg-success">Sí</span>'
        : '<span class="badge bg-secondary">No</span>';

    /* ── CARGA ───────────────────────────────────────────── */
    async function load(catalog) {
        const ep    = ENDPOINTS[catalog];
        const url   = ep;   // sin filtro de isActive: la tabla muestra todos (activos e inactivos)
        const items = await req('GET', url).catch(e => {
            toast('Error cargando catálogo: ' + e.message, true); return [];
        });
        if (!items) return;

        if (catalog === 'model') {
            _allModels = items;
            _populateModelBrandFilter();
        }

        switch (catalog) {
            case 'unitType': renderUnitType(items);  break;
            case 'status'  : renderStatus(items);    break;
            case 'fuel'    : renderFuel(items);       break;
            case 'brand'   : renderBrand(items);      break;
            case 'model'   : renderModel(items);      break;
        }
    }

    function _populateModelBrandFilter() {
        const sel = document.getElementById('filterModelBrand');
        if (!sel) return;
        const current = sel.value;
        const seen = new Map();
        _allModels.forEach(m => {
            if (m.idVehicleBrand && m.brandName && !seen.has(m.idVehicleBrand))
                seen.set(m.idVehicleBrand, m.brandName);
        });
        const opts = [...seen.entries()]
            .sort((a, b) => a[1].localeCompare(b[1]))
            .map(([id, name]) => `<option value="${id}">${name}</option>`)
            .join('');
        sel.innerHTML = '<option value="">— Todas las marcas —</option>' + opts;
        if (current && sel.querySelector(`option[value="${current}"]`))
            sel.value = current;
    }

    function filterModels() {
        const brandId = parseInt(document.getElementById('filterModelBrand')?.value || '0');
        const filtered = brandId ? _allModels.filter(m => m.idVehicleBrand === brandId) : _allModels;
        renderModel(filtered);
    }

    async function loadAll() {
        // Cargar marcas, países y tipos de unidad para los combos
        [_brands, _countries, _unitTypes] = await Promise.all([
            req('GET', 'fleet-catalog/brands').catch(() => []),
            req('GET', 'fleet-catalog/countries').catch(() => []),
            req('GET', 'fleet-catalog/unit-types').catch(() => [])
        ]);
        _brands    = _brands    || [];
        _countries = _countries || [];
        _unitTypes = _unitTypes || [];
        await load('unitType');
        // Los demás se cargan al hacer click en el tab
        const tabCatalogMap = {
            'tabStatus' : 'status',
            'tabFuel'   : 'fuel',
            'tabBrand'  : 'brand',
            'tabModel'  : 'model',
        };
        document.querySelectorAll('#fleetTabs .nav-link').forEach(tab => {
            tab.addEventListener('shown.bs.tab', e => {
                // href puede ser "#tabStatus" — extraer el id sin "#"
                const href   = e.target.getAttribute('href') || '';
                const tabId  = href.replace('#', '');
                const cat    = tabCatalogMap[tabId];
                if (cat) load(cat);
            });
        });
    }

    /* ── RENDER helpers ──────────────────────────────────── */
    function actionBtns(catalog, row) {
        return `
            <button class="btn btn-xs btn-outline-info btn-sm py-0 px-1 me-1"
                    onclick='FC.openModal("${catalog}", ${JSON.stringify(row)})'>
                <i class="bi bi-pencil"></i>
            </button>
            <button class="btn btn-xs btn-outline-danger btn-sm py-0 px-1"
                    onclick='FC.openDelete("${catalog}", ${row.id}, "${(row.name||'').replace(/"/g,'&quot;')}")'>
                <i class="bi bi-trash"></i>
            </button>`;
    }

    function renderUnitType(items) {
        const body = document.getElementById('bodyUnitType');
        if (!items.length) { body.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin registros</td></tr>'; return; }
        body.innerHTML = items.map(r => `<tr>
            <td><code>${r.code}</code></td>
            <td>${r.name}</td>
            <td><small class="text-muted">${r.description || ''}</small></td>
            <td><i class="bi ${r.icon || ''}"></i> <small>${r.icon || ''}</small></td>
            <td class="text-center">${r.sortOrder}</td>
            <td class="text-center">${activeBadge(r.isActive)}</td>
            <td class="text-end">${actionBtns('unitType', r)}</td>
        </tr>`).join('');
    }

    function renderStatus(items) {
        const body = document.getElementById('bodyStatus');
        if (!items.length) { body.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin registros</td></tr>'; return; }
        body.innerHTML = items.map(r => `<tr>
            <td><code>${r.code}</code></td>
            <td>${r.name}</td>
            <td><span class="badge catalog-badge" style="background:${r.badgeColor||'#6b7280'}">${r.badgeColor||''}</span>
                <span class="color-preview ms-1" style="background:${r.badgeColor||'#6b7280'}"></span></td>
            <td><small class="text-muted">${r.description || ''}</small></td>
            <td class="text-center">${r.sortOrder}</td>
            <td class="text-center">${activeBadge(r.isActive)}</td>
            <td class="text-end">${actionBtns('status', r)}</td>
        </tr>`).join('');
    }

    function renderFuel(items) {
        const body = document.getElementById('bodyFuel');
        if (!items.length) { body.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin registros</td></tr>'; return; }
        body.innerHTML = items.map(r => `<tr>
            <td><code>${r.code}</code></td>
            <td>${r.name}</td>
            <td><small class="text-muted">${r.description || ''}</small></td>
            <td><i class="bi ${r.icon || ''}"></i> <small>${r.icon || ''}</small></td>
            <td class="text-center">${r.sortOrder}</td>
            <td class="text-center">${activeBadge(r.isActive)}</td>
            <td class="text-end">${actionBtns('fuel', r)}</td>
        </tr>`).join('');
    }

    function renderBrand(items) {
        const body = document.getElementById('bodyBrand');
        if (!items.length) { body.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin registros</td></tr>'; return; }
        body.innerHTML = items.map(r => `<tr>
            <td><code>${r.code}</code></td>
            <td>${r.name}</td>
            <td>${r.countryName || '<span class="text-muted">—</span>'}</td>
            <td><small class="text-muted">${r.description || ''}</small></td>
            <td class="text-center">${r.sortOrder}</td>
            <td class="text-center">${activeBadge(r.isActive)}</td>
            <td class="text-end">${actionBtns('brand', r)}</td>
        </tr>`).join('');
    }

    function renderModel(items) {
        const body = document.getElementById('bodyModel');
        if (!items.length) { body.innerHTML = '<tr><td colspan="7" class="text-center text-muted">Sin registros</td></tr>'; return; }
        body.innerHTML = items.map(r => `<tr>
            <td><code>${r.code}</code></td>
            <td>${r.brandName || '<span class="text-muted">—</span>'}</td>
            <td>${r.name}</td>
            <td><small>${r.unitTypeName || ''}</small></td>
            <td class="text-center">${r.sortOrder}</td>
            <td class="text-center">${activeBadge(r.isActive)}</td>
            <td class="text-end">${actionBtns('model', r)}</td>
        </tr>`).join('');
    }

    /* ── FORMS dinámicos ─────────────────────────────────── */
    function formBase(r) {
        return {
            code: r?.code || '',
            name: r?.name || '',
            description: r?.description || '',
            sortOrder: r?.sortOrder ?? 0,
            isActive: r?.isActive ?? true,
        };
    }

    function buildForm(catalog, r) {
        const d = formBase(r);
        const activeCheck = `
            <div class="mb-3 form-check form-switch">
                <input class="form-check-input" type="checkbox" id="fcActive" ${d.isActive ? 'checked' : ''}>
                <label class="form-check-label text-light" for="fcActive">Activo</label>
            </div>`;

        const base = `
            <div class="row g-2">
                <div class="col-md-4">
                    <label class="form-label text-light">Código *</label>
                    <input class="form-control bg-dark text-white border-secondary" id="fcCode" maxlength="30" value="${d.code}">
                </div>
                <div class="col-md-8">
                    <label class="form-label text-light">Nombre *</label>
                    <input class="form-control bg-dark text-white border-secondary" id="fcName" maxlength="100" value="${d.name}">
                </div>
                <div class="col-12">
                    <label class="form-label text-light">Descripción</label>
                    <input class="form-control bg-dark text-white border-secondary" id="fcDesc" maxlength="500" value="${d.description}">
                </div>`;

        if (catalog === 'status') {
            const bc = r?.badgeColor || '#3b82f6';
            return base + `
                <div class="col-md-6">
                    <label class="form-label text-light">Color del Badge</label>
                    <div class="d-flex gap-2 align-items-center">
                        <input type="color" class="form-control form-control-color border-secondary bg-dark"
                               id="fcBadgeColor" value="${bc}" style="width:60px;height:38px;">
                        <input class="form-control bg-dark text-white border-secondary" id="fcBadgeColorHex"
                               maxlength="20" value="${bc}" placeholder="#rrggbb"
                               oninput="document.getElementById('fcBadgeColor').value=this.value">
                    </div>
                    <small>El color se usa como fondo del badge de estado</small>
                </div>
                <div class="col-md-3">
                    <label class="form-label text-light">Icono Bootstrap</label>
                    <input class="form-control bg-dark text-white border-secondary" id="fcIcon" maxlength="60" value="${r?.icon||''}">
                </div>
                <div class="col-md-3">
                    <label class="form-label text-light">Orden</label>
                    <input type="number" class="form-control bg-dark text-white border-secondary" id="fcOrder" value="${d.sortOrder}">
                </div>
            </div>${activeCheck}`;
        }

        if (catalog === 'model') {
            const brandOpts = _brands.map(b =>
                `<option value="${b.id}" ${r?.idVehicleBrand === b.id ? 'selected' : ''}>${b.name}</option>`
            ).join('');
            const unitTypeOpts = _unitTypes.map(u =>
                `<option value="${u.id}" ${r?.idTransportUnitType === u.id ? 'selected' : ''}>${u.name}</option>`
            ).join('');
            return base + `
                <div class="col-md-6">
                    <label class="form-label text-light">Marca *</label>
                    <select class="form-select bg-dark text-white border-secondary" id="fcBrandId">
                        <option value="">Seleccione…</option>${brandOpts}
                    </select>
                </div>
                <div class="col-md-4">
                    <label class="form-label text-light">Tipo Unidad</label>
                    <select class="form-select bg-dark text-white border-secondary" id="fcUnitTypeId">
                        <option value="">— Sin tipo —</option>${unitTypeOpts}
                    </select>
                </div>
                <div class="col-md-2">
                    <label class="form-label text-light">Orden</label>
                    <input type="number" class="form-control bg-dark text-white border-secondary" id="fcOrder" value="${d.sortOrder}">
                </div>
            </div>${activeCheck}`;
        }

        // unitType, fuel, brand
        const extraBrand = catalog === 'brand' ? `
            <div class="col-md-6">
                <label class="form-label text-light">País de Origen *</label>
                <select class="form-select bg-dark text-white border-secondary" id="fcCountryId" required>
                    <option value="">— Seleccione un país —</option>
                    ${_countries.map(c =>
                        `<option value="${c.id}" ${r?.idCountry === c.id ? 'selected' : ''}>${c.name}</option>`
                    ).join('')}
                </select>
            </div>` : '';

        const extraIcon = catalog !== 'brand' ? `
            <div class="col-md-6">
                <label class="form-label text-light">Icono Bootstrap</label>
                <input class="form-control bg-dark text-white border-secondary" id="fcIcon" maxlength="60" value="${r?.icon||''}">
            </div>` : '';

        return base + extraBrand + extraIcon + `
                <div class="col-md-6">
                    <label class="form-label text-light">Orden</label>
                    <input type="number" class="form-control bg-dark text-white border-secondary" id="fcOrder" value="${d.sortOrder}">
                </div>
            </div>${activeCheck}`;
    }

    /* ── openModal ───────────────────────────────────────── */
    function openModal(catalog, row = null) {
        _catalog = catalog;
        _editId  = row?.id || null;

        document.getElementById('fcModalTitle').textContent =
            `${row ? 'Editar' : 'Nuevo'} ${TITLES[catalog]}`;
        document.getElementById('fcModalBody').innerHTML = buildForm(catalog, row);

        // Sincronizar color picker ↔ texto
        const picker = document.getElementById('fcBadgeColor');
        const hexIn  = document.getElementById('fcBadgeColorHex');
        if (picker && hexIn) {
            picker.addEventListener('input', () => { hexIn.value = picker.value; });
        }

        bootstrap.Modal.getOrCreateInstance(document.getElementById('fcModal')).show();
    }

    /* ── save ────────────────────────────────────────────── */
    async function save() {
        const code = document.getElementById('fcCode')?.value?.trim();
        const name = document.getElementById('fcName')?.value?.trim();
        if (!code || !name) { toast('Código y Nombre son obligatorios.', true); return; }

        const payload = {
            code,
            name,
            description : document.getElementById('fcDesc')?.value?.trim() || null,
            sortOrder   : parseInt(document.getElementById('fcOrder')?.value || '0'),
            isActive    : document.getElementById('fcActive')?.checked ?? true,
        };

        if (_catalog === 'status') {
            payload.badgeColor = document.getElementById('fcBadgeColorHex')?.value?.trim() || '#6b7280';
            payload.icon       = document.getElementById('fcIcon')?.value?.trim() || null;
        }
        if (_catalog === 'unitType' || _catalog === 'fuel') {
            payload.icon = document.getElementById('fcIcon')?.value?.trim() || null;
        }
        if (_catalog === 'brand') {
            const cId = document.getElementById('fcCountryId')?.value;
            if (!cId) { toast('El país de origen es obligatorio.', true); return; }
            payload.idCountry = parseInt(cId);
        }
        if (_catalog === 'model') {
            const bId = document.getElementById('fcBrandId')?.value;
            if (!bId) { toast('Debe seleccionar una marca.', true); return; }
            payload.idVehicleBrand = parseInt(bId);
            const utId = document.getElementById('fcUnitTypeId')?.value;
            payload.idTransportUnitType = utId ? parseInt(utId) : null;
        }

        const ep      = ENDPOINTS[_catalog];
        const method  = _editId ? 'PUT' : 'POST';
        const url     = _editId ? `${ep}/${_editId}` : ep;

        try {
            document.getElementById('fcBtnSave').disabled = true;
            await req(method, url, payload);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('fcModal')).hide();
            toast(`${TITLES[_catalog]} guardado correctamente.`);
            // Recargar catálogos de soporte si aplica
            if (_catalog === 'brand')    _brands    = await req('GET', 'fleet-catalog/brands').catch(()=>[]) || [];
            if (_catalog === 'unitType') _unitTypes = await req('GET', 'fleet-catalog/unit-types').catch(()=>[]) || [];
            await load(_catalog);
        } catch (e) {
            toast(e.message, true);
        } finally {
            document.getElementById('fcBtnSave').disabled = false;
        }
    }

    /* ── delete ──────────────────────────────────────────── */
    function openDelete(catalog, id, name) {
        _catalog  = catalog;
        _deleteId = id;
        document.getElementById('fcDeleteName').textContent = name;
        bootstrap.Modal.getOrCreateInstance(document.getElementById('fcDeleteModal')).show();
    }

    async function confirmDelete() {
        try {
            await req('DELETE', `${ENDPOINTS[_catalog]}/${_deleteId}`);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('fcDeleteModal')).hide();
            toast('Registro desactivado.');
            await load(_catalog);
        } catch (e) {
            toast(e.message, true);
        }
    }

    /* ── Init ────────────────────────────────────────────── */
    document.addEventListener('DOMContentLoaded', loadAll);

    return { openModal, save, openDelete, confirmDelete, load, filterModels };
})();
