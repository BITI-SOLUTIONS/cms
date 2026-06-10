// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/location-type.js
// PROPÓSITO: Lógica cliente para Settings/LocalizationType
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-03
// ================================================================================

'use strict';

const LT_CFG = { apiBase: '', token: '' };

const LT = (() => {
    let _editingId = null;
    let _modal     = null;

    const esc = s => !s ? '' : String(s)
        .replace(/&/g,'&amp;')
        .replace(/</g,'&lt;')
        .replace(/>/g,'&gt;')
        .replace(/"/g,'&quot;');

    function apiFetch(path, opts = {}) {
        return fetch(LT_CFG.apiBase + path, {
            ...opts,
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + LT_CFG.token,
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

    async function load() {
        const search   = document.getElementById('ltSearch').value.trim();
        const isActive = document.getElementById('ltStatus').value;
        const spinner  = document.getElementById('ltSpinner');
        spinner.classList.remove('d-none');

        try {
            let url = '/api/locationtype?';
            if (isActive !== '') url += `isActive=${isActive}&`;

            const res = await apiFetch(url);
            if (!res.ok) throw new Error(await res.text());

            let items = await res.json();

            // Filtrar por búsqueda local
            if (search) {
                const s = search.toLowerCase();
                items = items.filter(x =>
                    x.code.toLowerCase().includes(s) ||
                    x.name.toLowerCase().includes(s) ||
                    (x.description && x.description.toLowerCase().includes(s)));
            }

            document.getElementById('ltTotal').textContent = items.length;
            renderCards(items);
        } catch (e) {
            showAlert('Error al cargar tipos: ' + e.message);
        } finally {
            spinner.classList.add('d-none');
        }
    }

    function renderCards(items) {
        const container = document.getElementById('ltContainer');
        const empty     = document.getElementById('ltEmpty');

        if (!items.length) {
            container.innerHTML = '';
            empty.classList.remove('d-none');
            return;
        }
        empty.classList.add('d-none');

        container.innerHTML = items.map(x => {
            const color   = x.color || '#6366f1';
            const icon    = x.icon  || 'bi-geo-alt';
            const badgeCls = x.isActive ? 'bg-success' : 'bg-secondary';
            const badgeTxt = x.isActive ? 'Activo' : 'Inactivo';
            return `
            <div class="col-sm-6 col-md-4 col-xl-3">
              <div class="card bg-dark-card border-0 h-100 lt-card" onclick="LT.openEdit(${x.id})">
                <div class="card-body d-flex gap-3 align-items-start">
                  <div class="rounded-3 d-flex align-items-center justify-content-center flex-shrink-0"
                       style="width:44px;height:44px;background:${color}22;">
                    <i class="bi ${esc(icon)} fs-4" style="color:${esc(color)};"></i>
                  </div>
                  <div class="flex-grow-1 overflow-hidden">
                    <div class="d-flex align-items-center gap-2 mb-1">
                      <code class="text-info" style="font-size:.8rem;">${esc(x.code)}</code>
                      <span class="badge ${badgeCls}" style="font-size:.65rem;">${badgeTxt}</span>
                    </div>
                    <div class="text-white fw-semibold text-truncate">${esc(x.name)}</div>
                    ${x.description ? `<div class="small text-truncate mt-1" style="color:#cbd5e1;">${esc(x.description)}</div>` : ''}
                    <div class="mt-2 d-flex gap-2 flex-wrap">
                      ${x.isActive
                        ? `<button class="btn btn-sm btn-outline-danger py-0 px-2" style="font-size:.72rem;"
                               onclick="event.stopPropagation();LT.toggleStatus(${x.id},false)">
                               <i class="bi bi-ban me-1"></i>Desactivar</button>`
                        : `<button class="btn btn-sm btn-outline-success py-0 px-2" style="font-size:.72rem;"
                               onclick="event.stopPropagation();LT.toggleStatus(${x.id},true)">
                               <i class="bi bi-check me-1"></i>Activar</button>`}
                      <button class="btn btn-sm btn-outline-secondary py-0 px-2" style="font-size:.72rem;"
                              onclick="event.stopPropagation();LT.confirmDelete(${x.id},'${esc(x.name)}')">
                              <i class="bi bi-trash me-1"></i>Eliminar</button>
                    </div>
                  </div>
                </div>
              </div>
            </div>`;
        }).join('');
    }

    function openCreate() {
        _editingId = null;
        resetForm();
        document.getElementById('ltModalTitle').textContent = 'Nuevo Tipo de Localización';
        _modal = _modal || new bootstrap.Modal(document.getElementById('ltModal'));
        _modal.show();
    }

    function openEdit(id) {
        _editingId = id;
        apiFetch(`/api/locationtype/${id}`)
            .then(r => r.json())
            .then(x => {
                document.getElementById('fLtCode').value      = x.code;
                document.getElementById('fLtName').value      = x.name;
                document.getElementById('fLtDescription').value = x.description || '';
                document.getElementById('fLtIcon').value      = x.icon || '';
                document.getElementById('fLtColor').value     = x.color || '#6366f1';
                try { document.getElementById('fLtColorPicker').value = x.color || '#6366f1'; } catch(e){}
                document.getElementById('ltIconPreview').className = 'bi ' + (x.icon || 'bi-geo-alt');
                document.getElementById('fLtSortOrder').value = x.sortOrder ?? 0;
                document.getElementById('fLtIsActive').checked = x.isActive;
                document.getElementById('ltModalTitle').textContent = 'Editar Tipo de Localización';
                _modal = _modal || new bootstrap.Modal(document.getElementById('ltModal'));
                _modal.show();
            })
            .catch(e => showAlert('Error al cargar: ' + e.message));
    }

    async function save() {
        const code = document.getElementById('fLtCode').value.trim();
        const name = document.getElementById('fLtName').value.trim();
        let valid  = true;

        // Limpiar
        ['fLtCode','fLtName'].forEach(id => {
            document.getElementById(id).classList.remove('is-invalid');
        });

        if (!code) {
            document.getElementById('fLtCode').classList.add('is-invalid');
            document.getElementById('fLtCodeFb').textContent = 'El código es obligatorio.';
            valid = false;
        }
        if (!name) {
            document.getElementById('fLtName').classList.add('is-invalid');
            document.getElementById('fLtNameFb').textContent = 'El nombre es obligatorio.';
            valid = false;
        }
        if (!valid) return;

        // Verificar duplicado
        const checkRes = await apiFetch(`/api/locationtype/check-code?code=${encodeURIComponent(code)}${_editingId ? '&excludeId=' + _editingId : ''}`);
        const checkData = await checkRes.json();
        if (checkData.exists) {
            document.getElementById('fLtCode').classList.add('is-invalid');
            document.getElementById('fLtCodeFb').textContent = `El código '${code}' ya existe.`;
            return;
        }

        const payload = {
            code,
            name,
            description: document.getElementById('fLtDescription').value.trim() || null,
            icon:        document.getElementById('fLtIcon').value.trim() || null,
            color:       document.getElementById('fLtColor').value.trim() || null,
            sortOrder:   parseInt(document.getElementById('fLtSortOrder').value) || 0,
            isActive:    document.getElementById('fLtIsActive').checked
        };

        const url    = _editingId ? `/api/locationtype/${_editingId}` : '/api/locationtype';
        const method = _editingId ? 'PUT' : 'POST';

        try {
            const res = await apiFetch(url, { method, body: JSON.stringify(payload) });
            if (!res.ok) {
                const err = await res.json().catch(() => ({ message: res.statusText }));
                throw new Error(err.message || res.statusText);
            }
            bootstrap.Modal.getInstance(document.getElementById('ltModal'))?.hide();
            showAlert(_editingId ? 'Tipo actualizado correctamente.' : 'Tipo creado correctamente.', 'success');
            load();
        } catch (e) {
            showAlert('Error al guardar: ' + e.message);
        }
    }

    async function toggleStatus(id, activate) {
        const action = activate ? 'activate' : 'deactivate';
        try {
            const res = await apiFetch(`/api/locationtype/${id}/${action}`, { method: 'PATCH' });
            if (!res.ok) throw new Error(await res.text());
            showAlert(activate ? 'Tipo activado.' : 'Tipo desactivado.', 'success');
            load();
        } catch (e) {
            showAlert('Error: ' + e.message);
        }
    }

    async function confirmDelete(id, name) {
        if (!confirm(`¿Eliminar el tipo "${name}"?\nSolo es posible si no tiene localizaciones asociadas.`)) return;
        try {
            const res = await apiFetch(`/api/locationtype/${id}`, { method: 'DELETE' });
            if (!res.ok) {
                const err = await res.json().catch(() => ({ message: res.statusText }));
                throw new Error(err.message || res.statusText);
            }
            showAlert('Tipo eliminado.', 'success');
            load();
        } catch (e) {
            showAlert('No se pudo eliminar: ' + e.message);
        }
    }

    function resetForm() {
        document.getElementById('ltForm').reset();
        document.getElementById('fLtIsActive').checked = true;
        document.getElementById('fLtSortOrder').value  = '0';
        document.getElementById('ltIconPreview').className = 'bi bi-geo-alt text-white';
        ['fLtCode','fLtName'].forEach(id => document.getElementById(id).classList.remove('is-invalid'));
    }

    function clearFilters() {
        document.getElementById('ltSearch').value = '';
        document.getElementById('ltStatus').value = 'true';
        load();
    }

    // Arrancar
    document.addEventListener('DOMContentLoaded', () => {
        load();
        // Enter en búsqueda
        document.getElementById('ltSearch').addEventListener('keydown', e => {
            if (e.key === 'Enter') load();
        });
    });

    return { load, openCreate, openEdit, save, toggleStatus, confirmDelete, clearFilters };
})();
