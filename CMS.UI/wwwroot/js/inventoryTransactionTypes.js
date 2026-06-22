// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/inventoryTransactionTypes.js
// PROPÓSITO: Lógica cliente para mantenimiento de admin.inventory_transaction_type
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-07-02
// ================================================================================

'use strict';

const ITT = (() => {
    const API   = () => window.ITT_API   || '';
    const TOKEN = () => window.ITT_TOKEN || '';

    let _deleteId   = null;
    let _deleteName = null;

    // ============================================================
    // FETCH HELPER
    // ============================================================

    async function ittFetch(path, options = {}) {
        const url = `${API()}${path}`;
        const opts = {
            headers: {
                'Content-Type':  'application/json',
                'Authorization': `Bearer ${TOKEN()}`,
            },
            ...options,
        };
        if (opts.body && typeof opts.body !== 'string') opts.body = JSON.stringify(opts.body);
        const res = await fetch(url, opts);
        if (!res.ok) {
            let msg = `HTTP ${res.status}`;
            try { const d = await res.json(); msg = d.message || d.error || d.title || msg; } catch {}
            throw new Error(msg);
        }
        if (res.status === 204) return null;
        return res.json();
    }

    // ============================================================
    // ALERT
    // ============================================================

    function showAlert(msg, type = 'success') {
        const el = document.getElementById('ittAlert');
        if (!el) return;
        el.className = `alert alert-${type}`;
        el.innerHTML = `<i class="bi bi-${type === 'success' ? 'check-circle' : 'exclamation-triangle'} me-2"></i>${msg}`;
        el.classList.remove('d-none');
        setTimeout(() => el.classList.add('d-none'), 5000);
    }

    // ============================================================
    // CARGAR Y RENDERIZAR
    // ============================================================

    async function load() {
        const tbody = document.getElementById('bodyTypes');
        tbody.innerHTML = '<tr><td colspan="10" class="text-center text-muted py-3"><i class="bi bi-hourglass-split me-1"></i>Cargando…</td></tr>';
        try {
            const items = await ittFetch('/api/inventory-transaction-type');
            renderTable(items);
        } catch (e) {
            tbody.innerHTML = `<tr><td colspan="10" class="text-center text-danger py-3">${e.message}</td></tr>`;
        }
    }

    function renderTable(items) {
        const tbody = document.getElementById('bodyTypes');
        if (!items || !items.length) {
            tbody.innerHTML = '<tr><td colspan="10" class="text-center text-light py-3">No hay tipos registrados.</td></tr>';
            return;
        }
        tbody.innerHTML = items.map((t, i) => `
            <tr>
                <td class="text-light" style="font-size:.8rem;">${t.sortOrder}</td>
                <td>
                    <code style="color:#7dd3fc;font-size:.82rem;">${t.code}</code>
                </td>
                <td class="text-light">
                    ${t.emoji ? `<span style="font-size:1.1rem;margin-right:4px;">${t.emoji}</span>` : ''}
                    ${t.icon  ? `<i class="bi ${t.icon} me-1 text-info" title="${t.icon}"></i>` : ''}
                    ${t.name}
                </td>
                <td class="text-light">
                    ${t.icon  ? `<span style="font-size:.75rem;color:#cbd5e1;">${t.icon}</span>` : ''}
                    ${t.emoji ? `<span class="ms-2" style="font-size:1rem;">${t.emoji}</span>` : ''}
                </td>
                <td>
                    ${t.cssClass
                        ? `<span class="badge-type ${t.cssClass}" style="font-size:.7rem;padding:.15rem .4rem;">${t.cssClass}</span>`
                        : '<span class="text-light">—</span>'}
                </td>
                <td class="text-center">
                    ${t.isTransitTransfer
                        ? '<i class="bi bi-check-circle-fill text-success"></i>'
                        : '<i class="bi bi-dash text-light"></i>'}
                </td>
                <td class="text-center">
                    ${t.showInInventoryMovements
                        ? '<i class="bi bi-check-circle-fill text-success" title="Visible en Inventory Movements"></i>'
                        : '<i class="bi bi-x-circle text-danger" title="Oculto en Inventory Movements"></i>'}
                </td>
                <td class="text-center text-light" style="font-size:.82rem;">${t.sortOrder}</td>
                <td class="text-center">
                    ${t.isActive
                        ? '<span class="badge bg-success">Activo</span>'
                        : '<span class="badge bg-secondary">Inactivo</span>'}
                </td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-warning me-1"
                            onclick="ITT.openEdit(${t.id})" title="Editar">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-outline-danger"
                            onclick="ITT.openDelete(${t.id}, '${t.name.replace(/'/g, "\\'")}')" title="Desactivar">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>`
        ).join('');
    }

    // ============================================================
    // MODAL NUEVO
    // ============================================================

    function openNew() {
        document.getElementById('ittModalTitle').innerHTML =
            '<i class="bi bi-plus-circle me-2 text-success"></i>Nuevo Tipo de Movimiento';
        clearForm();
        document.getElementById('ittId').value = '';
        bootstrap.Modal.getOrCreateInstance(document.getElementById('ittModal')).show();
    }

    // ============================================================
    // MODAL EDITAR
    // ============================================================

    async function openEdit(id) {
        try {
            const item = await ittFetch(`/api/inventory-transaction-type/${id}`);
            document.getElementById('ittModalTitle').innerHTML =
                `<i class="bi bi-pencil me-2 text-warning"></i>Editar Tipo — ${item.code}`;
            document.getElementById('ittId').value      = item.id;
            document.getElementById('ittCode').value    = item.code;
            document.getElementById('ittName').value    = item.name;
            document.getElementById('ittDesc').value    = item.description || '';
            document.getElementById('ittIcon').value    = item.icon || '';
            document.getElementById('ittEmoji').value   = item.emoji || '';
            document.getElementById('ittCss').value     = item.cssClass || '';
            document.getElementById('ittOrder').value   = item.sortOrder;
            document.getElementById('ittTransit').checked = item.isTransitTransfer;
            document.getElementById('ittActive').checked  = item.isActive;
            document.getElementById('ittShowInMovements').checked = item.showInInventoryMovements ?? true;  // ✅ Cargar nuevo campo
            bootstrap.Modal.getOrCreateInstance(document.getElementById('ittModal')).show();
        } catch (e) {
            showAlert(e.message, 'danger');
        }
    }

    // ============================================================
    // GUARDAR (crear o actualizar)
    // ============================================================

    async function save() {
        const id   = document.getElementById('ittId').value;
        const code = document.getElementById('ittCode').value.trim();
        const name = document.getElementById('ittName').value.trim();

        if (!code) { showFieldError('ittCode', 'El código es obligatorio.'); return; }
        if (!name) { showFieldError('ittName', 'El nombre es obligatorio.');  return; }

        const payload = {
            id:               parseInt(id) || 0,
            code,
            name,
            description:      document.getElementById('ittDesc').value.trim()  || null,
            icon:             document.getElementById('ittIcon').value.trim()   || null,
            emoji:            document.getElementById('ittEmoji').value.trim()  || null,
            cssClass:         document.getElementById('ittCss').value.trim()    || null,
            sortOrder:        parseInt(document.getElementById('ittOrder').value) || 0,
            isTransitTransfer: document.getElementById('ittTransit').checked,
            isActive:         document.getElementById('ittActive').checked,
            showInInventoryMovements: document.getElementById('ittShowInMovements').checked,  // ✅ Incluir nuevo campo
        };

        const isNew  = !id;
        const method = isNew ? 'POST' : 'PUT';
        const path   = isNew ? '/api/inventory-transaction-type' : `/api/inventory-transaction-type/${id}`;

        try {
            await ittFetch(path, { method, body: payload });
            bootstrap.Modal.getInstance(document.getElementById('ittModal')).hide();
            showAlert(isNew ? 'Tipo creado correctamente.' : 'Tipo actualizado correctamente.', 'success');
            load();
        } catch (e) {
            showAlert(e.message, 'danger');
        }
    }

    // ============================================================
    // DESACTIVAR
    // ============================================================

    function openDelete(id, name) {
        _deleteId   = id;
        _deleteName = name;
        document.getElementById('ittDeleteName').textContent = name;
        bootstrap.Modal.getOrCreateInstance(document.getElementById('ittDeleteModal')).show();
    }

    async function confirmDelete() {
        if (!_deleteId) return;
        try {
            await ittFetch(`/api/inventory-transaction-type/${_deleteId}`, { method: 'DELETE' });
            bootstrap.Modal.getInstance(document.getElementById('ittDeleteModal')).hide();
            showAlert(`Tipo '${_deleteName}' desactivado.`, 'warning');
            load();
        } catch (e) {
            showAlert(e.message, 'danger');
        }
    }

    // ============================================================
    // HELPERS
    // ============================================================

    function clearForm() {
        ['ittCode', 'ittName', 'ittDesc', 'ittIcon', 'ittEmoji', 'ittCss'].forEach(id => {
            const el = document.getElementById(id);
            if (el) el.value = '';
        });
        document.getElementById('ittOrder').value     = '10';
        document.getElementById('ittTransit').checked = false;
        document.getElementById('ittActive').checked  = true;
        document.getElementById('ittShowInMovements').checked = true;  // ✅ Valor por defecto TRUE
        // clear validation
        document.querySelectorAll('#ittModal .is-invalid').forEach(el => el.classList.remove('is-invalid'));
    }

    function showFieldError(fieldId, msg) {
        const el = document.getElementById(fieldId);
        if (!el) return;
        el.classList.add('is-invalid');
        let fb = el.nextElementSibling;
        if (!fb || !fb.classList.contains('invalid-feedback')) {
            fb = document.createElement('div');
            fb.className = 'invalid-feedback';
            el.after(fb);
        }
        fb.textContent = msg;
        el.focus();
        el.addEventListener('input', () => { el.classList.remove('is-invalid'); }, { once: true });
    }

    // ============================================================
    // INIT
    // ============================================================

    document.addEventListener('DOMContentLoaded', load);

    return { load, openNew, openEdit, save, openDelete, confirmDelete };
})();
