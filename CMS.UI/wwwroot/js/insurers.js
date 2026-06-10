// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/insurers.js
// PROPÓSITO: Lógica cliente para la pantalla de aseguradoras
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-14
// ================================================================================

const INS = (() => {
    const API   = () => window.INS_API   || '';
    const TOKEN = () => window.INS_TOKEN || '';
    const hdrs  = () => ({ 'Content-Type': 'application/json', 'Authorization': `Bearer ${TOKEN()}` });

    let _editId    = null;
    let _deleteId  = null;
    let _page      = 1;
    const PAGE_SIZE = 20;
    let _searchTimer;

    async function req(method, path, body) {
        const opts = { method, headers: hdrs() };
        if (body) opts.body = JSON.stringify(body);
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

    const activeBadge = v => v
        ? '<span class="badge bg-success">Sí</span>'
        : '<span class="badge bg-secondary">No</span>';

    /* ── LOAD ───────────────────────────────────────────────── */
    async function load(page = 1) {
        _page = page;
        const search   = document.getElementById('insSearch')?.value?.trim() || '';
        const isActive = document.getElementById('insFilterActive')?.value || '';

        let url = `insurers?page=${_page}&pageSize=${PAGE_SIZE}`;
        if (search)   url += `&search=${encodeURIComponent(search)}`;
        if (isActive) url += `&isActive=${isActive}`;

        const body = document.getElementById('insBody');
        body.innerHTML = '<tr><td colspan="9" class="text-center text-muted py-3"><i class="bi bi-hourglass-split"></i> Cargando…</td></tr>';

        try {
            const data = await req('GET', url);
            renderTable(data.items);
            renderPagination(data.total, data.page, data.pageSize);
        } catch (e) {
            body.innerHTML = `<tr><td colspan="9" class="text-center text-danger py-3">${e.message}</td></tr>`;
        }
    }

    function renderTable(items) {
        const body = document.getElementById('insBody');
        if (!items.length) {
            body.innerHTML = '<tr><td colspan="9" class="text-center text-muted py-4">Sin aseguradoras registradas</td></tr>';
            return;
        }
        body.innerHTML = items.map(r => `<tr>
            <td><code>${r.code}</code></td>
            <td><div class="fw-semibold text-light">${r.name}</div></td>
            <td><small class="text-muted">${r.tradeName || '—'}</small></td>
            <td><small>${r.taxId || '—'}</small></td>
            <td><small>${r.phone || '—'}</small></td>
            <td><small class="text-warning">${r.phoneClaims || '—'}</small></td>
            <td>
                ${r.agentName ? `<div class="text-light">${r.agentName}</div>
                <small class="text-muted">${r.agentPhone || ''} ${r.agentEmail ? '· ' + r.agentEmail : ''}</small>` : '<span class="text-muted">—</span>'}
            </td>
            <td class="text-center">${activeBadge(r.isActive)}</td>
            <td class="text-end">
                <button class="btn btn-xs btn-outline-info btn-sm py-0 px-1 me-1"
                        onclick='INS.openModal(${JSON.stringify(JSON.stringify(r))})'>
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-xs btn-outline-danger btn-sm py-0 px-1"
                        onclick='INS.openDelete(${r.id}, "${r.name.replace(/"/g, '&quot;')}")'>
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        </tr>`).join('');
    }

    function renderPagination(total, page, pageSize) {
        const pages = Math.ceil(total / pageSize);
        document.getElementById('insPagInfo').textContent =
            `${Math.min((page - 1) * pageSize + 1, total)}–${Math.min(page * pageSize, total)} de ${total}`;
        const nav = document.getElementById('insPagNav');
        if (pages <= 1) { nav.innerHTML = ''; return; }
        const btns = [];
        for (let i = 1; i <= pages; i++)
            btns.push(`<button class="btn btn-xs btn-sm ${i === page ? 'btn-primary' : 'btn-outline-secondary'} py-0 px-2"
                onclick="INS.load(${i})">${i}</button>`);
        nav.innerHTML = btns.join(' ');
    }

    /* ── MODAL ──────────────────────────────────────────────── */
    function openModal(rowJson = null) {
        const d = rowJson ? (typeof rowJson === 'string' ? JSON.parse(rowJson) : rowJson) : null;
        _editId = d?.id || null;
        document.getElementById('insModalTitle').textContent = d ? 'Editar Aseguradora' : 'Nueva Aseguradora';

        setVal('insCode',        d?.code || '');
        setVal('insName',        d?.name || '');
        setVal('insTradeName',   d?.tradeName || '');
        setVal('insTaxId',       d?.taxId || '');
        setVal('insWebsite',     d?.website || '');
        setVal('insAddress',     d?.address || '');
        setVal('insNotes',       d?.notes || '');
        setVal('insPhone',       d?.phone || '');
        setVal('insPhoneClaims', d?.phoneClaims || '');
        setVal('insEmail',       d?.email || '');
        setVal('insAgentName',   d?.agentName || '');
        setVal('insAgentPhone',  d?.agentPhone || '');
        setVal('insAgentEmail',  d?.agentEmail || '');
        document.getElementById('insIsActive').checked = d?.isActive ?? true;

        // Ir al primer tab
        document.querySelector('#insFormTabs .nav-link')?.click();
        bootstrap.Modal.getOrCreateInstance(document.getElementById('insModal')).show();
    }

    function setVal(id, val) {
        const el = document.getElementById(id);
        if (el) el.value = val;
    }

    /* ── SAVE ───────────────────────────────────────────────── */
    async function save() {
        const code = document.getElementById('insCode')?.value?.trim();
        const name = document.getElementById('insName')?.value?.trim();
        if (!code || !name) {
            toast('Código y Nombre son obligatorios.', true);
            document.querySelector('#insFormTabs .nav-link')?.click();
            return;
        }

        const payload = {
            code,
            name,
            tradeName   : document.getElementById('insTradeName')?.value?.trim() || null,
            taxId       : document.getElementById('insTaxId')?.value?.trim() || null,
            website     : document.getElementById('insWebsite')?.value?.trim() || null,
            address     : document.getElementById('insAddress')?.value?.trim() || null,
            notes       : document.getElementById('insNotes')?.value?.trim() || null,
            phone       : document.getElementById('insPhone')?.value?.trim() || null,
            phoneClaims : document.getElementById('insPhoneClaims')?.value?.trim() || null,
            email       : document.getElementById('insEmail')?.value?.trim() || null,
            agentName   : document.getElementById('insAgentName')?.value?.trim() || null,
            agentPhone  : document.getElementById('insAgentPhone')?.value?.trim() || null,
            agentEmail  : document.getElementById('insAgentEmail')?.value?.trim() || null,
            isActive    : document.getElementById('insIsActive').checked,
        };

        const method = _editId ? 'PUT' : 'POST';
        const url    = _editId ? `insurers/${_editId}` : 'insurers';

        try {
            document.getElementById('insBtnSave').disabled = true;
            await req(method, url, payload);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('insModal')).hide();
            toast('Aseguradora guardada correctamente.');
            await load(_page);
        } catch (e) {
            toast(e.message, true);
        } finally {
            document.getElementById('insBtnSave').disabled = false;
        }
    }

    /* ── DELETE ─────────────────────────────────────────────── */
    function openDelete(id, name) {
        _deleteId = id;
        document.getElementById('insDeleteName').textContent = name;
        bootstrap.Modal.getOrCreateInstance(document.getElementById('insDeleteModal')).show();
    }

    async function confirmDelete() {
        try {
            await req('DELETE', `insurers/${_deleteId}`);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('insDeleteModal')).hide();
            toast('Aseguradora desactivada.');
            await load(_page);
        } catch (e) {
            toast(e.message, true);
        }
    }

    function debounceSearch() {
        clearTimeout(_searchTimer);
        _searchTimer = setTimeout(() => load(1), 350);
    }

    document.addEventListener('DOMContentLoaded', () => load(1));

    return { load, openModal, save, openDelete, confirmDelete, debounceSearch };
})();
