// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/positions.js
// PROPOSITO: Logica cliente para la pantalla de Puestos (HR)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-07-05
// ================================================================================

const POS = (() => {
    const API   = () => window.POS_API   || '';
    const TOKEN = () => window.POS_TOKEN || '';
    const hdrs  = () => ({ 'Content-Type': 'application/json', 'Authorization': `Bearer ${TOKEN()}` });

    let _editId      = null;
    let _deleteId    = null;
    let _searchTimer;
    let _departments = [];
    const _rowMap    = {};

    async function req(method, path, body) {
        const opts = { method, headers: hdrs() };
        if (body !== undefined) opts.body = JSON.stringify(body);
        const r = await fetch(`${API()}/api/${path}`, opts);
        if (!r.ok) {
            const e = await r.json().catch(() => ({ message: r.statusText }));
            throw new Error(e.message || r.statusText);
        }
        if (r.status === 204) return null;
        return r.json();
    }

    function toast(msg, isError = false) {
        const el = document.createElement('div');
        el.className = `alert alert-${isError ? 'danger' : 'success'} position-fixed bottom-0 end-0 m-3 py-2 px-3 shadow`;
        el.style.zIndex = 9999;
        el.innerHTML = `<i class="bi bi-${isError ? 'exclamation-triangle' : 'check-circle'} me-2"></i>${msg}`;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 3500);
    }

    function levelBadge(level) {
        if (!level) return '<span class="text-muted small">-</span>';
        const map = {
            'GERENCIA':     'bg-danger',
            'JEFATURA':     'bg-warning text-dark',
            'COORDINACION': 'bg-info text-dark',
            'ANALISTA':     'bg-primary',
            'TECNICO':      'bg-secondary',
            'OPERATIVO':    'bg-dark border border-secondary',
            'ASISTENTE':    'bg-secondary'
        };
        const cls = map[(level || '').toUpperCase()] || 'bg-secondary';
        return `<span class="badge ${cls} level-badge">${level}</span>`;
    }

    function deptName(idDepartment) {
        if (!idDepartment) return '<span class="text-muted small">-</span>';
        const d = _departments.find(x => x.id === idDepartment);
        if (!d) return `<span class="text-muted small">#${idDepartment}</span>`;
        const bg     = d.color ? d.color + '33' : '#6366f133';
        const border = d.color || '#6366f1';
        const ico    = d.icon  ? `<i class="${d.icon} me-1"></i>` : '';
        return `<span class="badge" style="background:${bg};color:${border};border:1px solid ${border};font-size:.72rem;">${ico}${d.name}</span>`;
    }

    async function loadDepartments() {
        try {
            _departments = await req('GET', 'employees/departments?isActive=true') || [];
        } catch { _departments = []; }

        // Poblar filtro de departamento
        const filterSel = document.getElementById('posFilterDept');
        if (filterSel) {
            const cur = filterSel.value;
            filterSel.innerHTML = '<option value="">Todos los departamentos</option>' +
                _departments.map(d => `<option value="${d.id}">${d.name}</option>`).join('');
            if (cur) filterSel.value = cur;
        }

        // Poblar selector de departamento del modal
        const formSel = document.getElementById('posDepartment');
        if (formSel) {
            const cur = formSel.value;
            formSel.innerHTML = '<option value="">- Sin departamento -</option>' +
                _departments.map(d => `<option value="${d.id}">${d.name}</option>`).join('');
            if (cur) formSel.value = cur;
        }
    }

    async function load() {
        const search = document.getElementById('posSearch')?.value?.trim().toLowerCase() || '';
        const active = document.getElementById('posFilterActive')?.value || '';
        const deptId = document.getElementById('posFilterDept')?.value  || '';
        const body   = document.getElementById('posBody');

        body.innerHTML = '<tr><td colspan="8" class="text-center text-muted py-3"><i class="bi bi-hourglass-split me-2"></i>Cargando...</td></tr>';

        try {
            let url = 'jobpositions';
            const params = [];
            if (active !== '') params.push(`isActive=${active}`);
            if (deptId)         params.push(`idDepartment=${deptId}`);
            if (params.length)  url += '?' + params.join('&');

            let items = await req('GET', url) || [];

            if (search) items = items.filter(p =>
                p.name.toLowerCase().includes(search) ||
                p.code.toLowerCase().includes(search));

            document.getElementById('posPagInfo').textContent = `${items.length} puesto(s)`;

            if (!items.length) {
                body.innerHTML = `<tr><td colspan="8" class="text-center py-5">
                    <i class="bi bi-briefcase fs-2 d-block mb-2 text-muted"></i>
                    <span style="color:#94a3b8">Sin puestos registrados</span>
                </td></tr>`;
                return;
            }

            body.innerHTML = items.map(p => `
            <tr>
                <td><code class="text-info">${p.code}</code></td>
                <td class="fw-semibold text-light">${p.name}</td>
                <td>${deptName(p.idDepartment)}</td>
                <td>${levelBadge(p.level)}</td>
                <td><small class="text-muted">${p.description || '-'}</small></td>
                <td><small class="text-muted">${p.sortOrder}</small></td>
                <td class="text-center">${p.isActive
                    ? '<span class="badge bg-success">Activo</span>'
                    : '<span class="badge bg-secondary">Inactivo</span>'}</td>
                <td class="text-end">
                    <div class="d-flex gap-1 justify-content-end">
                        <button class="btn btn-xs btn-outline-info btn-sm py-0 px-1"
                                onclick="POS.openModal(${JSON.stringify(JSON.stringify(p))})" title="Editar">
                            <i class="bi bi-pencil"></i>
                        </button>
                        <button class="btn btn-xs btn-outline-danger btn-sm py-0 px-1"
                                onclick='POS.openDelete(${p.id},"${(p.name||'').replace(/"/g,"&quot;")}")' title="Eliminar">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </td>
            </tr>`).join('');
        } catch (e) {
            body.innerHTML = `<tr><td colspan="8" class="text-center text-danger py-3">
                <i class="bi bi-exclamation-triangle me-2"></i>${e.message}
            </td></tr>`;
        }
    }

    function openModal(rowOrId = null) {
        let d = null;
        if (rowOrId !== null && rowOrId !== undefined) {
            if (typeof rowOrId === 'number' || (typeof rowOrId === 'string' && !String(rowOrId).startsWith('{'))) {
                d = _rowMap[parseInt(rowOrId)] || null;
            } else {
                d = typeof rowOrId === 'string' ? JSON.parse(rowOrId) : rowOrId;
            }
        }
        _editId = d?.id || null;

        document.getElementById('posModalTitle').innerHTML =
            `<i class="bi bi-briefcase me-2 text-primary"></i>${d ? 'Editar Puesto' : 'Nuevo Puesto'}`;

        const sv = (id, val) => { const el = document.getElementById(id); if (el) el.value = val ?? ''; };
        sv('posCode',        d?.code || '');
        sv('posName',        d?.name || '');
        sv('posLevel',       d?.level || '');
        sv('posDescription', d?.description || '');
        sv('posSortOrder',   d?.sortOrder ?? 0);
        document.getElementById('posIsActive').checked = d?.isActive ?? true;
        document.getElementById('posIsDriver').checked = d?.isDriver ?? false;

        // Restaurar departamento
        const deptSel = document.getElementById('posDepartment');
        if (deptSel) deptSel.value = d?.idDepartment ?? '';

        bootstrap.Modal.getOrCreateInstance(document.getElementById('posModal')).show();
    }

    function openDelete(id, name) {
        _deleteId = id;
        document.getElementById('posDeleteName').textContent = name;
        bootstrap.Modal.getOrCreateInstance(document.getElementById('posDeleteModal')).show();
    }

    async function confirmDelete() {
        try {
            await req('DELETE', `jobpositions/${_deleteId}`);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('posDeleteModal')).hide();
            toast('Puesto eliminado.');
            await load();
        } catch (e) { toast(e.message, true); }
    }

    async function save() {
        const code = document.getElementById('posCode')?.value?.trim();
        const name = document.getElementById('posName')?.value?.trim();
        const dept = document.getElementById('posDepartment')?.value;
        if (!code || !name) { toast('Codigo y Nombre son obligatorios.', true); return; }
        if (!dept)          { toast('El Departamento es obligatorio.', true); return; }

        const payload = {
            code,
            name,
            idDepartment: parseInt(dept),
            isDriver:     document.getElementById('posIsDriver').checked,
            level:        document.getElementById('posLevel')?.value?.trim() || null,
            description:  document.getElementById('posDescription')?.value?.trim() || null,
            sortOrder:    parseInt(document.getElementById('posSortOrder')?.value || '0'),
            isActive:     document.getElementById('posIsActive').checked
        };

        const method = _editId ? 'PUT' : 'POST';
        const url    = _editId ? `jobpositions/${_editId}` : 'jobpositions';

        try {
            document.getElementById('posBtnSave').disabled = true;
            await req(method, url, payload);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('posModal')).hide();
            toast('Puesto guardado correctamente.');
            await load();
        } catch (e) {
            toast(e.message, true);
        } finally {
            document.getElementById('posBtnSave').disabled = false;
        }
    }

    function debounceSearch() {
        clearTimeout(_searchTimer);
        _searchTimer = setTimeout(() => load(), 300);
    }

    document.addEventListener('DOMContentLoaded', async () => {
        await loadDepartments();
        await load();
    });

    return { load, openModal, save, openDelete, confirmDelete, debounceSearch };
})();
