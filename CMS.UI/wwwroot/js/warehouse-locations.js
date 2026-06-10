/**
 * warehouse-locations.js
 * Gestión de Locations del módulo Warehouse & Distribution.
 * Consume /api/location filtrando por tipo WAREHOUSE.
 * Schema: sinai.location (sin code/name/description/phone/email/notes)
 */

'use strict';

// ── Configuración ──────────────────────────────────────────────
const WLOC_CFG = {
    apiBase: '',
    token: '',
    warehouseTypeCode: 'WAREHOUSE',
    companyCountryId: 0
};

const WLOC = (() => {

    let _state = {
        items: [],
        locationTypes: [],
        page: 1,
        pageSize: 25,
        total: 0,
        editId: null,
        filters: { search: '', locationType: '', activeOnly: 'true' }
    };

    const api = (path, method = 'GET', body = null) => {
        const opts = {
            method,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${WLOC_CFG.token}`
            }
        };
        if (body) opts.body = JSON.stringify(body);
        return fetch(WLOC_CFG.apiBase + path, opts).then(r => r.json());
    };

    const showAlert = (msg, type = 'danger') => {
        const el = document.getElementById('pageAlert');
        el.className = `alert alert-${type} alert-dismissible fade show`;
        el.innerHTML = `${msg}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
        el.classList.remove('d-none');
        setTimeout(() => el.classList.add('d-none'), 6000);
    };

    const setSpinner = show => document.getElementById('wlocSpinner').classList.toggle('d-none', !show);

    // ── Inicialización ─────────────────────────────────────────
    const init = async () => {
        try {
            const types = await api('/api/locationtype');
            if (types && Array.isArray(types)) {
                _state.locationTypes = types.filter(t => t.isActive);
                const selFilter = document.getElementById('wlocLocType');
                if (selFilter) {
                    selFilter.innerHTML = '<option value="">Todos los tipos</option>' +
                        _state.locationTypes.map(t =>
                            `<option value="${t.id}">${t.name}</option>`).join('');
                }
            }
        } catch { /* continuar */ }

        await loadCountries();
        await load();
    };

    // ── Carga de datos ─────────────────────────────────────────
    const load = async () => {
        setSpinner(true);
        try {
            const params = new URLSearchParams({ page: _state.page, pageSize: _state.pageSize });
            if (_state.filters.locationType) params.set('locationTypeId', _state.filters.locationType);
            if (_state.filters.search) params.set('search', _state.filters.search);
            if (_state.filters.activeOnly !== '') params.set('isActive', _state.filters.activeOnly);
            const data = await api(`/api/location?${params}`);
            _state.items = data.items || data || [];
            _state.total = data.total || _state.items.length;
            render();
        } catch (e) {
            showAlert('Error cargando ubicaciones: ' + e.message);
        } finally {
            setSpinner(false);
        }
    };

    // ── Renderizado ────────────────────────────────────────────
    const render = () => {
        document.getElementById('wlocTotal').textContent = _state.total;
        const container = document.getElementById('wlocContainer');
        const empty = document.getElementById('wlocEmpty');

        if (!_state.items.length) {
            container.innerHTML = '';
            empty.classList.remove('d-none');
            document.getElementById('wlocPager').innerHTML = '';
            return;
        }
        empty.classList.add('d-none');

        const rows = _state.items.map(loc => `
            <tr class="loc-row" onclick="WLOC.openEdit(${loc.id})">
                <td><span class="badge" style="background:${loc.locationTypeColor||'#6366f1'};font-size:.75rem;">
                    <i class="bi ${loc.locationTypeIcon||'bi-geo-alt'} me-1"></i>${loc.locationTypeName||''}
                </span></td>
                <td style="color:#e2e8f0;font-size:.875rem;">${loc.address || '—'}</td>
                <td style="color:#e2e8f0;font-size:.875rem;">${loc.postalCode || '—'}</td>
                <td>
                    <span class="badge ${loc.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${loc.isActive ? 'Activa' : 'Inactiva'}
                    </span>
                </td>
                <td class="text-end" onclick="event.stopPropagation()">
                    <button class="btn btn-sm btn-outline-primary me-1" onclick="WLOC.openEdit(${loc.id})" title="Editar">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm ${loc.isActive ? 'btn-outline-warning' : 'btn-outline-success'} me-1"
                            onclick="WLOC.toggleStatus(${loc.id}, ${loc.isActive})"
                            title="${loc.isActive ? 'Desactivar' : 'Activar'}">
                        <i class="bi bi-toggle-${loc.isActive ? 'on' : 'off'}"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger"
                            onclick="WLOC.confirmDelete(${loc.id})"
                            title="Eliminar">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>`).join('');

        container.innerHTML = `
            <div class="table-responsive">
                <table class="table table-dark table-hover align-middle mb-0">
                    <thead>
                        <tr>
                            <th style="width:160px">Tipo</th>
                            <th>Dirección</th>
                            <th style="width:110px">C.P.</th>
                            <th style="width:90px">Estado</th>
                            <th style="width:130px" class="text-end">Acciones</th>
                        </tr>
                    </thead>
                    <tbody>${rows}</tbody>
                </table>
            </div>`;

        renderPager();
    };

    const renderPager = () => {
        const totalPages = Math.ceil(_state.total / _state.pageSize);
        const pager = document.getElementById('wlocPager');
        if (totalPages <= 1) { pager.innerHTML = ''; return; }
        let html = '';
        const mkLi = (label, page, disabled = false, active = false) =>
            `<li class="page-item${disabled ? ' disabled' : ''}${active ? ' active' : ''}">
                <a class="page-link" href="#" onclick="WLOC.goPage(${page});return false;">${label}</a>
            </li>`;
        html += mkLi('«', 1, _state.page === 1);
        html += mkLi('‹', _state.page - 1, _state.page === 1);
        const start = Math.max(1, _state.page - 2);
        const end   = Math.min(totalPages, _state.page + 2);
        for (let p = start; p <= end; p++) html += mkLi(p, p, false, p === _state.page);
        html += mkLi('›', _state.page + 1, _state.page === totalPages);
        html += mkLi('»', totalPages, _state.page === totalPages);
        pager.innerHTML = html;
    };

    // ── Geografía ──────────────────────────────────────────────
    const loadCountries = async () => {
        try {
            const list = await api('/api/location/geo/countries');
            const sel = document.getElementById('fWlocCountry');
            if (!sel) return;
            sel.innerHTML = '<option value="">-- Seleccione pa\u00eds --</option>' +
                (list || []).map(c => `<option value="${c.id}">${c.name}</option>`).join('');
            // Pre-seleccionar el pa\u00eds de la compa\u00f1\u00eda
            if (WLOC_CFG.companyCountryId) {
                sel.value = WLOC_CFG.companyCountryId;
                if (sel.value) await loadProvinces(WLOC_CFG.companyCountryId);
            }
        } catch { /* ignore */ }
    };

    const loadProvinces = async (idCountry) => {
        const sel = document.getElementById('fWlocProvince');
        if (!sel) return;
        sel.innerHTML = '<option value="">-- Seleccione --</option>';
        resetSelect('fWlocCanton', '-- Seleccione --');
        resetSelect('fWlocDistrict', '-- Seleccione --');
        resetSelect('fWlocNeighborhood', '-- Seleccione --');
        document.getElementById('fWlocPostalCode').value = '';
        if (!idCountry) return;
        try {
            const list = await api(`/api/location/geo/provinces?idCountry=${idCountry}`);
            sel.innerHTML = '<option value="">-- Seleccione --</option>' +
                (list || []).map(p => `<option value="${p.id}">${p.name}</option>`).join('');
        } catch { /* ignore */ }
    };

    const loadCantons = async (idProvince) => {
        const sel = document.getElementById('fWlocCanton');
        if (!sel) return;
        sel.innerHTML = '<option value="">-- Seleccione --</option>';
        resetSelect('fWlocDistrict', '-- Seleccione --');
        resetSelect('fWlocNeighborhood', '-- Seleccione --');
        document.getElementById('fWlocPostalCode').value = '';
        if (!idProvince) return;
        try {
            const list = await api(`/api/location/geo/cantons?idProvince=${idProvince}`);
            sel.innerHTML = '<option value="">-- Seleccione --</option>' +
                (list || []).map(c => `<option value="${c.id}">${c.name}</option>`).join('');
        } catch { /* ignore */ }
    };

    const loadDistricts = async (idCanton) => {
        const sel = document.getElementById('fWlocDistrict');
        if (!sel) return;
        sel.innerHTML = '<option value="">-- Seleccione --</option>';
        resetSelect('fWlocNeighborhood', '-- Seleccione --');
        document.getElementById('fWlocPostalCode').value = '';
        if (!idCanton) return;
        try {
            const list = await api(`/api/location/geo/districts?idCanton=${idCanton}`);
            sel.innerHTML = '<option value="">-- Seleccione --</option>' +
                (list || []).map(d => `<option value="${d.id}">${d.name}</option>`).join('');
        } catch { /* ignore */ }
    };

    const loadNeighborhoods = async (idDistrict) => {
        const sel = document.getElementById('fWlocNeighborhood');
        if (!sel) return;
        sel.innerHTML = '<option value="">-- Seleccione --</option>';
        document.getElementById('fWlocPostalCode').value = '';
        if (!idDistrict) return;
        try {
            const list = await api(`/api/location/geo/neighborhoods?idDistrict=${idDistrict}`);
            sel.innerHTML = '<option value="">-- Seleccione --</option>' +
                (list || []).map(n => `<option value="${n.id}" data-postal="${n.postalCode || ''}">${n.name}</option>`).join('');
        } catch { /* ignore */ }
    };

    const resetSelect = (id, placeholder) => {
        const el = document.getElementById(id);
        if (el) el.innerHTML = `<option value="">${placeholder}</option>`;
    };

    // ── Filtros ────────────────────────────────────────────────
    const applyFilters = () => {
        _state.filters.search       = document.getElementById('wlocSearch').value.trim();
        _state.filters.locationType = document.getElementById('wlocLocType')?.value || '';
        _state.filters.activeOnly   = document.getElementById('wlocStatus').value;
        _state.page = 1;
        load();
    };

    const clearFilters = () => {
        document.getElementById('wlocSearch').value = '';
        const lt = document.getElementById('wlocLocType');
        if (lt) lt.value = '';
        document.getElementById('wlocStatus').value = 'true';
        _state.filters = { search: '', locationType: '', activeOnly: 'true' };
        _state.page = 1;
        load();
    };

    const goPage = p => { _state.page = p; load(); };

    // ── Modal ──────────────────────────────────────────────────
    const resetForm = () => {
        ['fWlocAddress', 'fWlocAddress2', 'fWlocPostalCode', 'fWlocLat', 'fWlocLon'].forEach(id => {
            const el = document.getElementById(id);
            if (el) { el.value = ''; el.classList.remove('is-invalid', 'is-valid'); }
        });
        ['fWlocCountry', 'fWlocProvince', 'fWlocCanton', 'fWlocDistrict', 'fWlocNeighborhood'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.classList.remove('is-invalid', 'is-valid');
        });
        document.getElementById('fWlocIsActive').checked = true;
        const selType = document.getElementById('fWlocLocType');
        if (selType) {
            selType.innerHTML = '<option value="">-- Seleccione tipo --</option>' +
                _state.locationTypes.map(t =>
                    `<option value="${t.id}">${t.name}</option>`).join('');
        }
        loadCountries();
    };

    const openCreate = () => {
        _state.editId = null;
        resetForm();
        document.getElementById('wlocModalTitle').textContent = 'Nueva Ubicación';
        document.getElementById('wlocModalSubtitle').textContent = 'Complete los datos de la ubicación.';
        bootstrap.Modal.getOrCreateInstance(document.getElementById('wlocModal')).show();
    };

    const openEdit = async id => {
        resetForm();
        try {
            const loc = await api(`/api/location/${id}`);
            _state.editId = id;

            const selType = document.getElementById('fWlocLocType');
            if (selType && loc.idLocationType) selType.value = loc.idLocationType;

            document.getElementById('fWlocAddress').value    = loc.address    || '';
            document.getElementById('fWlocAddress2').value   = loc.address2   || '';
            document.getElementById('fWlocPostalCode').value = loc.postalCode  || '';
            document.getElementById('fWlocLat').value        = loc.gpsLatitude  != null ? loc.gpsLatitude  : '';
            document.getElementById('fWlocLon').value        = loc.gpsLongitude != null ? loc.gpsLongitude : '';
            document.getElementById('fWlocIsActive').checked = loc.isActive !== false;

            // Cascada geográfica
            if (loc.idCountry) {
                const selC = document.getElementById('fWlocCountry');
                if (selC) { selC.value = loc.idCountry; await loadProvinces(loc.idCountry); }
            }
            if (loc.idProvince) {
                const selP = document.getElementById('fWlocProvince');
                if (selP) { selP.value = loc.idProvince; await loadCantons(loc.idProvince); }
            }
            if (loc.idCanton) {
                const selCa = document.getElementById('fWlocCanton');
                if (selCa) { selCa.value = loc.idCanton; await loadDistricts(loc.idCanton); }
            }
            if (loc.idDistrict) {
                const selD = document.getElementById('fWlocDistrict');
                if (selD) { selD.value = loc.idDistrict; await loadNeighborhoods(loc.idDistrict); }
            }
            if (loc.idNeighborhood) {
                const selN = document.getElementById('fWlocNeighborhood');
                if (selN) {
                    selN.value = loc.idNeighborhood;
                    // Postal code ya viene en loc.postalCode
                    const cpEl = document.getElementById('fWlocPostalCode');
                    if (cpEl && loc.postalCode) cpEl.value = loc.postalCode;
                }
            }

            document.getElementById('wlocModalTitle').textContent    = 'Editar Ubicación';
            document.getElementById('wlocModalSubtitle').textContent = `ID: ${loc.id}`;
            bootstrap.Modal.getOrCreateInstance(document.getElementById('wlocModal')).show();
        } catch {
            showAlert('Error al cargar la ubicación.');
        }
    };

    const save = async () => {
        let valid = true;
        const mark = (id, ok) => {
            const el = document.getElementById(id);
            if (!el) return;
            el.classList.toggle('is-invalid', !ok);
            el.classList.toggle('is-valid', ok);
        };

        const selType = document.getElementById('fWlocLocType');
        const idLocationType = parseInt(selType?.value) || 0;
        if (!idLocationType) { selType?.classList.add('is-invalid'); valid = false; }
        else selType?.classList.remove('is-invalid');

        const address = document.getElementById('fWlocAddress').value.trim();
        if (address.length < 4) { mark('fWlocAddress', false); valid = false; }
        else mark('fWlocAddress', true);

        const idCountry      = parseInt(document.getElementById('fWlocCountry')?.value)      || 0;
        const idProvince     = parseInt(document.getElementById('fWlocProvince')?.value)     || 0;
        const idCanton       = parseInt(document.getElementById('fWlocCanton')?.value)       || 0;
        const idDistrict     = parseInt(document.getElementById('fWlocDistrict')?.value)     || 0;
        const idNeighborhood = parseInt(document.getElementById('fWlocNeighborhood')?.value) || 0;

        mark('fWlocCountry',      idCountry > 0);      if (!idCountry)      valid = false;
        mark('fWlocProvince',     idProvince > 0);     if (!idProvince)     valid = false;
        mark('fWlocCanton',       idCanton > 0);       if (!idCanton)       valid = false;
        mark('fWlocDistrict',     idDistrict > 0);     if (!idDistrict)     valid = false;
        mark('fWlocNeighborhood', idNeighborhood > 0); if (!idNeighborhood) valid = false;

        if (!valid) {
            // Mostrar alerta en la parte superior del modal
            const modalBody = document.querySelector('#wlocModal .modal-body');
            let alertEl = document.getElementById('wlocFormAlert');
            if (!alertEl) {
                alertEl = document.createElement('div');
                alertEl.id = 'wlocFormAlert';
                modalBody.insertBefore(alertEl, modalBody.firstChild);
            }
            alertEl.className = 'alert alert-warning d-flex align-items-center gap-2 mb-3';
            alertEl.innerHTML = '<i class="bi bi-exclamation-triangle-fill fs-5"></i><span>Hay campos obligatorios sin completar. Por favor revise el formulario.</span>';

            // Scroll autom\u00e1tico al primer campo inv\u00e1lido
            const firstInvalid = document.querySelector('#wlocForm .is-invalid');
            if (firstInvalid) {
                firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                firstInvalid.focus();
            }
            return;
        }

        // Limpiar alerta si existe
        const alertEl = document.getElementById('wlocFormAlert');
        if (alertEl) alertEl.remove();

        const payload = {
            idLocationType,
            idCountry:      idCountry      || null,
            idProvince:     idProvince     || null,
            idCanton:       idCanton       || null,
            idDistrict:     idDistrict     || null,
            idNeighborhood: idNeighborhood || null,
            address:        address || null,
            address2:       document.getElementById('fWlocAddress2').value.trim() || null,
            postalCode:     document.getElementById('fWlocPostalCode').value.trim() || null,
            gpsLatitude:    parseFloat(document.getElementById('fWlocLat').value)  || null,
            gpsLongitude:   parseFloat(document.getElementById('fWlocLon').value)  || null,
            isActive:       document.getElementById('fWlocIsActive').checked
        };

        try {
            if (_state.editId) await api(`/api/location/${_state.editId}`, 'PUT', payload);
            else               await api('/api/location', 'POST', payload);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('wlocModal')).hide();
            showAlert('Ubicación guardada correctamente.', 'success');
            await load();
        } catch (e) {
            showAlert('Error guardando ubicación: ' + e.message);
        }
    };

    const toggleStatus = async (id, isActive) => {
        if (isActive) {
            if (!confirm('\u00bfEst\u00e1 seguro de que desea INACTIVAR esta ubicaci\u00f3n?\nNo estar\u00e1 disponible hasta que sea activada nuevamente.')) return;
        }
        const action = isActive ? 'deactivate' : 'activate';
        try {
            await api(`/api/location/${id}/${action}`, 'PATCH');
            showAlert(`Ubicación ${isActive ? 'desactivada' : 'activada'} correctamente.`, 'success');
            await load();
        } catch {
            showAlert('Error cambiando estado.');
        }
    };

    const confirmDelete = (id) => {
        if (!confirm('¿Eliminar esta ubicación?\nEsta acción no se puede deshacer.')) return;
        api(`/api/location/${id}`, 'DELETE')
            .then(() => { showAlert('Ubicación eliminada.', 'success'); load(); })
            .catch(() => showAlert('No se pudo eliminar la ubicación.'));
    };

    // ── Iniciar al cargar ──────────────────────────────────────
    document.addEventListener('DOMContentLoaded', init);

    return { load, applyFilters, clearFilters, goPage, openCreate, openEdit, save,
             toggleStatus, confirmDelete, loadProvinces, loadCantons, loadDistricts, loadNeighborhoods };

})();
