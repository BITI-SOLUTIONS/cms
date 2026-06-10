// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/location.js
// PROPÃ“SITO: LÃ³gica cliente para Settings/Localization
// Schema: sinai.location (sin code/name/description/phone/email/notes)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// ================================================================================

'use strict';

const LOC_CFG = { apiBase: '', token: '', companyCountryId: 0 };

const LOC = (() => {
    let _editingId  = null;
    let _page       = 1;
    const _pageSize = 20;
    let _types      = [];

    const esc = s => !s ? '' : String(s)
        .replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');

    function apiFetch(path, opts = {}) {
        return fetch(LOC_CFG.apiBase + path, {
            ...opts,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + LOC_CFG.token,
                ...(opts.headers || {})
            }
        });
    }

    function showAlert(msg, type = 'danger') {
        const el = document.getElementById('pageAlert');
        el.className = `alert alert-${type} alert-dismissible fade show`;
        el.innerHTML = `${esc(msg)}<button type="button" class="btn-close" data-bs-dismiss="alert"></button>`;
        el.classList.remove('d-none');
        setTimeout(() => el.classList.add('d-none'), 6000);
    }

    async function loadTypes() {
        try {
            const res = await apiFetch('/api/locationtype?isActive=true');
            if (!res.ok) return;
            _types = await res.json();
            const filterSel = document.getElementById('locTypeFilter');
            const formSel   = document.getElementById('fLocType');
            [filterSel, formSel].forEach(sel => {
                if (!sel) return;
                const curVal = sel.value;
                while (sel.options.length > 1) sel.remove(1);
                for (const t of _types) sel.appendChild(new Option(t.name, t.id));
                if (curVal) sel.value = curVal;
            });
        } catch { /* silencioso */ }
    }

    async function load() {
        const search   = document.getElementById('locSearch').value.trim();
        const typeId   = document.getElementById('locTypeFilter').value;
        const isActive = document.getElementById('locStatus').value;
        const spinner  = document.getElementById('locSpinner');
        spinner.classList.remove('d-none');
        try {
            let url = `/api/location?page=${_page}&pageSize=${_pageSize}`;
            if (search)          url += `&search=${encodeURIComponent(search)}`;
            if (typeId)          url += `&locationTypeId=${typeId}`;
            if (isActive !== '') url += `&isActive=${isActive}`;
            const res = await apiFetch(url);
            if (!res.ok) throw new Error(await res.text());
            const data = await res.json();
            document.getElementById('locTotal').textContent = data.total;
            renderTable(data.items);
            renderPager(data.totalPages, data.page);
        } catch (e) {
            showAlert('Error al cargar localizaciones: ' + e.message);
        } finally {
            spinner.classList.add('d-none');
        }
    }

    function renderTable(items) {
        const container = document.getElementById('locContainer');
        const empty     = document.getElementById('locEmpty');
        if (!items.length) { container.innerHTML = ''; empty.classList.remove('d-none'); return; }
        empty.classList.add('d-none');

        const rows = items.map(x => {
            const color   = x.locationTypeColor || '#6366f1';
            const icon    = x.locationTypeIcon  || 'bi-geo-alt';
            const badgeCls = x.isActive ? 'text-success' : 'text-secondary';
            const badgeTxt = x.isActive ? 'Activa' : 'Inactiva';
            return `
            <tr>
              <td>
                <span class="rounded-2 d-inline-flex align-items-center justify-content-center"
                      style="width:30px;height:30px;background:${color}22;flex-shrink:0;vertical-align:middle;margin-right:.5rem;">
                  <i class="bi ${esc(icon)} small" style="color:${esc(color)};"></i>
                </span>
                <span class="badge" style="background:${color}33; color:${color}; font-size:.7rem;">
                  ${esc(x.locationTypeName || 'â€”')}
                </span>
              </td>
              <td class="text-muted small">${esc(x.address || 'â€”')}</td>
              <td class="text-muted small">${esc(x.postalCode || 'â€”')}</td>
              <td><small class="${badgeCls}"><i class="bi bi-circle-fill me-1" style="font-size:.45rem;"></i>${badgeTxt}</small></td>
              <td>
                <div class="d-flex gap-1">
                  <button class="btn btn-sm btn-outline-secondary py-0 px-2" onclick="LOC.openDetail(${x.id})" title="Ver"><i class="bi bi-eye"></i></button>
                  <button class="btn btn-sm btn-outline-primary py-0 px-2"   onclick="LOC.openEdit(${x.id})"   title="Editar"><i class="bi bi-pencil"></i></button>
                  ${x.isActive
                    ? `<button class="btn btn-sm btn-outline-warning py-0 px-2" onclick="LOC.toggleStatus(${x.id},false)" title="Desactivar"><i class="bi bi-ban"></i></button>`
                    : `<button class="btn btn-sm btn-outline-success py-0 px-2" onclick="LOC.toggleStatus(${x.id},true)"  title="Activar"><i class="bi bi-check-lg"></i></button>`}
                  <button class="btn btn-sm btn-outline-danger py-0 px-2" onclick="LOC.confirmDelete(${x.id})" title="Eliminar"><i class="bi bi-trash"></i></button>
                </div>
              </td>
            </tr>`;
        }).join('');

        container.innerHTML = `
        <div class="table-responsive">
          <table class="table table-dark table-hover align-middle" style="font-size:.875rem;">
            <thead><tr style="border-color:rgba(255,255,255,.1);">
              <th class="text-muted fw-normal" style="font-size:.72rem;text-transform:uppercase;">Tipo</th>
              <th class="text-muted fw-normal" style="font-size:.72rem;text-transform:uppercase;">DirecciÃ³n</th>
              <th class="text-muted fw-normal" style="font-size:.72rem;text-transform:uppercase;">C.P.</th>
              <th class="text-muted fw-normal" style="font-size:.72rem;text-transform:uppercase;">Estado</th>
              <th class="text-muted fw-normal" style="font-size:.72rem;text-transform:uppercase;">Acciones</th>
            </tr></thead>
            <tbody style="border-color:rgba(255,255,255,.07);">${rows}</tbody>
          </table>
        </div>`;
    }

    function renderPager(totalPages, current) {
        const pager = document.getElementById('locPager');
        if (!totalPages || totalPages <= 1) { pager.innerHTML = ''; return; }
        let html = '';
        html += `<li class="page-item${current===1?' disabled':''}"><a class="page-link" href="#" onclick="LOC.goPage(${current-1});return false;">â€¹</a></li>`;
        for (let i = 1; i <= totalPages; i++)
            html += `<li class="page-item${i===current?' active':''}"><a class="page-link" href="#" onclick="LOC.goPage(${i});return false;">${i}</a></li>`;
        html += `<li class="page-item${current===totalPages?' disabled':''}"><a class="page-link" href="#" onclick="LOC.goPage(${current+1});return false;">â€º</a></li>`;
        pager.innerHTML = html;
    }

    function goPage(p) { _page = p; load(); }
    function applyFilters() { _page = 1; load(); }
    function clearFilters() {
        document.getElementById('locSearch').value = '';
        document.getElementById('locTypeFilter').value = '';
        document.getElementById('locStatus').value = 'true';
        _page = 1; load();
    }

    // â”€â”€ GeografÃ­a â”€â”€
    async function loadCountries() {
        try {
            const qs = LOC_CFG.companyCountryId ? `?filterCountryId=${LOC_CFG.companyCountryId}` : '';
            const res = await apiFetch(`/api/location/geo/countries${qs}`);
            const list = await res.json();
            const sel = document.getElementById('fLocCountry');
            if (!sel) return;
            sel.innerHTML = '<option value="">-- Seleccione paÃ­s --</option>' +
                (list || []).map(c => `<option value="${c.id}">${esc(c.name)}</option>`).join('');
            if (list && list.length === 1) { sel.value = list[0].id; await loadProvinces(list[0].id); }
        } catch { /* ignore */ }
    }

    async function loadProvinces(idCountry) {
        resetSel('fLocProvince', '-- Provincia --');
        resetSel('fLocCanton',   '-- CantÃ³n --');
        resetSel('fLocDistrict', '-- Distrito --');
        resetSel('fLocNeighborhood', '-- Barrio --');
        if (!idCountry) return;
        try {
            const res = await apiFetch(`/api/location/geo/provinces?idCountry=${idCountry}`);
            const list = await res.json();
            const sel = document.getElementById('fLocProvince');
            if (sel) sel.innerHTML = '<option value="">-- Provincia --</option>' +
                (list || []).map(p => `<option value="${p.id}">${esc(p.name)}</option>`).join('');
        } catch { /* ignore */ }
    }

    async function loadCantons(idProvince) {
        resetSel('fLocCanton',   '-- CantÃ³n --');
        resetSel('fLocDistrict', '-- Distrito --');
        resetSel('fLocNeighborhood', '-- Barrio --');
        if (!idProvince) return;
        try {
            const res = await apiFetch(`/api/location/geo/cantons?idProvince=${idProvince}`);
            const list = await res.json();
            const sel = document.getElementById('fLocCanton');
            if (sel) sel.innerHTML = '<option value="">-- CantÃ³n --</option>' +
                (list || []).map(c => `<option value="${c.id}">${esc(c.name)}</option>`).join('');
        } catch { /* ignore */ }
    }

    async function loadDistricts(idCanton) {
        resetSel('fLocDistrict', '-- Distrito --');
        resetSel('fLocNeighborhood', '-- Barrio --');
        if (!idCanton) return;
        try {
            const res = await apiFetch(`/api/location/geo/districts?idCanton=${idCanton}`);
            const list = await res.json();
            const sel = document.getElementById('fLocDistrict');
            if (sel) sel.innerHTML = '<option value="">-- Distrito --</option>' +
                (list || []).map(d => `<option value="${d.id}" data-zip="${d.zip||''}">${esc(d.name)}${d.zip ? ' (' + d.zip + ')' : ''}</option>`).join('');
        } catch { /* ignore */ }
    }

    async function loadNeighborhoods(idDistrict) {
        resetSel('fLocNeighborhood', '-- Barrio --');
        if (!idDistrict) return;
        try {
            const res = await apiFetch(`/api/location/geo/neighborhoods?idDistrict=${idDistrict}`);
            const list = await res.json();
            const sel = document.getElementById('fLocNeighborhood');
            if (sel) sel.innerHTML = '<option value="">-- Barrio --</option>' +
                (list || []).map(n => `<option value="${n.id}">${esc(n.name)}</option>`).join('');
        } catch { /* ignore */ }
    }

    function resetSel(id, placeholder) {
        const el = document.getElementById(id);
        if (el) el.innerHTML = `<option value="">${placeholder}</option>`;
    }

    async function openDetail(id) {
        try {
            const res = await apiFetch(`/api/location/${id}`);
            const x = await res.json();
            const color = x.locationTypeColor || '#6366f1';
            const icon  = x.locationTypeIcon  || 'bi-geo-alt';
            document.getElementById('dType').innerHTML =
                `<span class="badge" style="background:${color}33;color:${color};">
                   <i class="bi ${esc(icon)} me-1"></i>${esc(x.locationTypeName || 'â€”')}</span>`;
            document.getElementById('dStatus').innerHTML = x.isActive
                ? '<span class="badge bg-success">Activa</span>'
                : '<span class="badge bg-secondary">Inactiva</span>';
            document.getElementById('dAddress').textContent = [x.address, x.address2].filter(Boolean).join(' | ') || 'â€”';
            document.getElementById('dPostal').textContent  = x.postalCode || 'â€”';
            document.getElementById('dGps').textContent     = (x.gpsLatitude && x.gpsLongitude)
                ? `${x.gpsLatitude}, ${x.gpsLongitude}` : 'â€”';
            document.getElementById('locDetailEditBtn').onclick = () => {
                bootstrap.Modal.getInstance(document.getElementById('locDetailModal'))?.hide();
                openEdit(id);
            };
            new bootstrap.Modal(document.getElementById('locDetailModal')).show();
        } catch (e) {
            showAlert('Error al cargar detalle: ' + e.message);
        }
    }

    function openCreate() {
        _editingId = null;
        resetForm();
        document.getElementById('locModalTitle').textContent    = 'Nueva LocalizaciÃ³n';
        document.getElementById('locModalSubtitle').textContent = 'Complete los datos de la localizaciÃ³n.';
        new bootstrap.Modal(document.getElementById('locModal')).show();
    }

    async function openEdit(id) {
        _editingId = id;
        resetForm();
        try {
            const res = await apiFetch(`/api/location/${id}`);
            const x = await res.json();
            const selType = document.getElementById('fLocType');
            if (selType && x.idLocationType) selType.value = x.idLocationType;
            document.getElementById('fLocAddress').value    = x.address    || '';
            document.getElementById('fLocAddress2').value   = x.address2   || '';
            document.getElementById('fLocPostalCode').value = x.postalCode  || '';
            document.getElementById('fLocLat').value        = x.gpsLatitude  != null ? x.gpsLatitude  : '';
            document.getElementById('fLocLon').value        = x.gpsLongitude != null ? x.gpsLongitude : '';
            document.getElementById('fLocIsActive').checked = x.isActive;

            if (x.idCountry) {
                const selC = document.getElementById('fLocCountry');
                if (selC) { selC.value = x.idCountry; await loadProvinces(x.idCountry); }
            }
            if (x.idProvince) {
                const selP = document.getElementById('fLocProvince');
                if (selP) { selP.value = x.idProvince; await loadCantons(x.idProvince); }
            }
            if (x.idCanton) {
                const selCa = document.getElementById('fLocCanton');
                if (selCa) { selCa.value = x.idCanton; await loadDistricts(x.idCanton); }
            }
            if (x.idDistrict) {
                const selD = document.getElementById('fLocDistrict');
                if (selD) { selD.value = x.idDistrict; await loadNeighborhoods(x.idDistrict); }
            }
            if (x.idNeighborhood) {
                const selN = document.getElementById('fLocNeighborhood');
                if (selN) selN.value = x.idNeighborhood;
            }

            document.getElementById('locModalTitle').textContent    = 'Editar LocalizaciÃ³n';
            document.getElementById('locModalSubtitle').textContent = `ID: ${x.id}`;
            new bootstrap.Modal(document.getElementById('locModal')).show();
        } catch (e) {
            showAlert('Error al cargar localizaciÃ³n: ' + e.message);
        }
    }

    async function save() {
        const typeId = document.getElementById('fLocType').value;
        const selType = document.getElementById('fLocType');
        selType.classList.remove('is-invalid');

        if (!typeId) {
            selType.classList.add('is-invalid');
            document.getElementById('fLocTypeFb').textContent = 'Seleccione un tipo.';
            return;
        }

        const payload = {
            idLocationType: parseInt(typeId),
            idCountry:      parseInt(document.getElementById('fLocCountry')?.value)     || null,
            idProvince:     parseInt(document.getElementById('fLocProvince')?.value)    || null,
            idCanton:       parseInt(document.getElementById('fLocCanton')?.value)      || null,
            idDistrict:     parseInt(document.getElementById('fLocDistrict')?.value)    || null,
            idNeighborhood: parseInt(document.getElementById('fLocNeighborhood')?.value) || null,
            address:        document.getElementById('fLocAddress').value.trim()   || null,
            address2:       document.getElementById('fLocAddress2').value.trim()  || null,
            postalCode:     document.getElementById('fLocPostalCode').value.trim() || null,
            gpsLatitude:    parseFloat(document.getElementById('fLocLat').value)  || null,
            gpsLongitude:   parseFloat(document.getElementById('fLocLon').value)  || null,
            isActive:       document.getElementById('fLocIsActive').checked
        };

        const url    = _editingId ? `/api/location/${_editingId}` : '/api/location';
        const method = _editingId ? 'PUT' : 'POST';
        try {
            const res = await apiFetch(url, { method, body: JSON.stringify(payload) });
            if (!res.ok) {
                const err = await res.json().catch(() => ({ message: res.statusText }));
                throw new Error(err.message || res.statusText);
            }
            bootstrap.Modal.getInstance(document.getElementById('locModal'))?.hide();
            showAlert(_editingId ? 'LocalizaciÃ³n actualizada.' : 'LocalizaciÃ³n creada.', 'success');
            load();
        } catch (e) {
            showAlert('Error al guardar: ' + e.message);
        }
    }

    async function toggleStatus(id, activate) {
        const action = activate ? 'activate' : 'deactivate';
        try {
            const res = await apiFetch(`/api/location/${id}/${action}`, { method: 'PATCH' });
            if (!res.ok) throw new Error(await res.text());
            showAlert(activate ? 'LocalizaciÃ³n activada.' : 'LocalizaciÃ³n desactivada.', 'success');
            load();
        } catch (e) {
            showAlert('Error: ' + e.message);
        }
    }

    async function confirmDelete(id) {
        if (!confirm('Â¿Eliminar esta localizaciÃ³n?')) return;
        try {
            const res = await apiFetch(`/api/location/${id}`, { method: 'DELETE' });
            if (!res.ok) throw new Error(await res.text());
            showAlert('LocalizaciÃ³n eliminada.', 'success');
            load();
        } catch (e) {
            showAlert('No se pudo eliminar: ' + e.message);
        }
    }

    function resetForm() {
        ['fLocAddress','fLocAddress2','fLocPostalCode','fLocLat','fLocLon'].forEach(id => {
            const el = document.getElementById(id);
            if (el) { el.value = ''; el.classList.remove('is-invalid','is-valid'); }
        });
        document.getElementById('fLocIsActive').checked = true;
        document.getElementById('fLocType').classList.remove('is-invalid');
        loadCountries();
    }

    document.addEventListener('DOMContentLoaded', async () => {
        await loadTypes();
        await loadCountries();
        load();
        document.getElementById('locSearch').addEventListener('keydown', e => {
            if (e.key === 'Enter') applyFilters();
        });
    });

    return { load, applyFilters, clearFilters, goPage, openCreate, openEdit, openDetail, save,
             toggleStatus, confirmDelete, loadProvinces, loadCantons, loadDistricts, loadNeighborhoods };
})();
