// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/drivers.js
// PROPÓSITO: Lógica cliente para la pantalla de conductores
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-06-14
// ================================================================================

const DRV = (() => {
    const API   = () => window.DRV_API   || '';
    const TOKEN = () => window.DRV_TOKEN || '';
    const hdrs  = () => ({ 'Content-Type': 'application/json', 'Authorization': `Bearer ${TOKEN()}` });

    let _editId    = null;
    let _deleteId  = null;
    let _page      = 1;
    let _total     = 0;
    const PAGE_SIZE = 20;
    let _searchTimer;
    let _systemUsers = [];

    /* ── fetch helper ──────────────────────────────────────── */
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

    /* ── toast ─────────────────────────────────────────────── */
    function toast(msg, isError = false) {
        const el = document.createElement('div');
        el.className = `alert alert-${isError ? 'danger' : 'success'} position-fixed bottom-0 end-0 m-3 py-2 px-3 shadow`;
        el.style.zIndex = 9999;
        el.innerHTML = `<i class="bi bi-${isError ? 'exclamation-triangle' : 'check-circle'} me-2"></i>${msg}`;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 3500);
    }

    /* ── helpers ────────────────────────────────────────────── */
    const activeBadge = v => v
        ? '<span class="badge bg-success">Sí</span>'
        : '<span class="badge bg-secondary">No</span>';

    function licenseExpiryBadge(dateStr) {
        if (!dateStr) return '<span class="text-muted">—</span>';
        const d = new Date(dateStr);
        const now = new Date();
        const diff = Math.ceil((d - now) / 86400000);
        if (diff < 0)  return `<span class="badge bg-danger">${d.toLocaleDateString()}</span>`;
        if (diff < 60) return `<span class="badge bg-warning text-dark">${d.toLocaleDateString()}</span>`;
        return `<span class="text-light">${d.toLocaleDateString()}</span>`;
    }

    function initials(name) {
        return (name || '?').split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase();
    }

    /* ── LOAD ────────────────────────────────────────────────── */
    async function load(page = 1) {
        _page = page;
        const search    = document.getElementById('drvSearch')?.value?.trim() || '';
        const isActive  = document.getElementById('drvFilterActive')?.value || '';

        let url = `drivers?page=${_page}&pageSize=${PAGE_SIZE}`;
        if (search)   url += `&search=${encodeURIComponent(search)}`;
        if (isActive) url += `&isActive=${isActive}`;

        const body = document.getElementById('drvBody');
        body.innerHTML = '<tr><td colspan="10" class="text-center text-muted py-3"><i class="bi bi-hourglass-split"></i> Cargando…</td></tr>';

        try {
            const data = await req('GET', url);
            _total = data.total;
            renderTable(data.items);
            renderPagination(data.total, data.page, data.pageSize);
            updateKpis(data.items);
        } catch (e) {
            body.innerHTML = `<tr><td colspan="10" class="text-center text-danger py-3">${e.message}</td></tr>`;
        }
    }

    function renderTable(items) {
        const body = document.getElementById('drvBody');
        if (!items.length) {
            body.innerHTML = '<tr><td colspan="10" class="text-center text-muted py-4">Sin conductores registrados</td></tr>';
            return;
        }
        body.innerHTML = items.map(d => `<tr>
            <td><span class="driver-avatar">${initials(d.fullName)}</span></td>
            <td><code>${d.code}</code></td>
            <td>
                <div class="fw-semibold text-light">${d.fullName}</div>
                ${d.systemUserName ? `<small class="text-info"><i class="bi bi-person-check me-1"></i>${d.systemUserName}</small>` : ''}
            </td>
            <td><small>${d.idType || ''} ${d.idNumber}</small></td>
            <td>
                ${d.licenseNumber ? `<span class="badge bg-secondary badge-license">${d.licenseCategory || ''}</span> ${d.licenseNumber}` : '<span class="text-muted">—</span>'}
            </td>
            <td>${licenseExpiryBadge(d.licenseExpiryDate)}</td>
            <td><small>${d.mobile || d.phone || '—'}</small></td>
            <td><small>${d.systemUserEmail || '—'}</small></td>
            <td class="text-center">${activeBadge(d.isActive)}</td>
            <td class="text-end">
                <button class="btn btn-xs btn-outline-info btn-sm py-0 px-1 me-1"
                        onclick="DRV.openModal(${JSON.stringify(JSON.stringify(d))})">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-xs btn-outline-danger btn-sm py-0 px-1"
                        onclick='DRV.openDelete(${d.id}, "${d.fullName.replace(/"/g, '&quot;')}")'>
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        </tr>`).join('');
    }

    function updateKpis(items) {
        const now = new Date();
        const in60 = new Date(); in60.setDate(in60.getDate() + 60);
        const active   = items.filter(d => d.isActive).length;
        const expiring = items.filter(d => d.licenseExpiryDate && new Date(d.licenseExpiryDate) > now && new Date(d.licenseExpiryDate) <= in60).length;
        const expired  = items.filter(d => d.licenseExpiryDate && new Date(d.licenseExpiryDate) < now).length;

        document.getElementById('kpiTotal').textContent   = items.length;
        document.getElementById('kpiActive').textContent  = active;
        document.getElementById('kpiExpiring').textContent = expiring;
        document.getElementById('kpiExpired').textContent = expired;
    }

    function renderPagination(total, page, pageSize) {
        const pages = Math.ceil(total / pageSize);
        document.getElementById('drvPagInfo').textContent =
            `${Math.min((page - 1) * pageSize + 1, total)}–${Math.min(page * pageSize, total)} de ${total}`;

        const nav = document.getElementById('drvPagNav');
        if (pages <= 1) { nav.innerHTML = ''; return; }

        const btns = [];
        for (let i = 1; i <= pages; i++) {
            btns.push(`<button class="btn btn-xs btn-sm ${i === page ? 'btn-primary' : 'btn-outline-secondary'} py-0 px-2"
                onclick="DRV.load(${i})">${i}</button>`);
        }
        nav.innerHTML = btns.join(' ');
    }

    /* ── MODAL ABRIR ─────────────────────────────────────────── */
    async function openModal(rowJson = null) {
        const d = rowJson ? (typeof rowJson === 'string' ? JSON.parse(rowJson) : rowJson) : null;
        _editId = d?.id || null;

        document.getElementById('drvModalTitle').textContent = d ? 'Editar Conductor' : 'Nuevo Conductor';

        // Limpiar / poblar campos
        setValue('drvCode',            d?.code || '');
        setValue('drvIdType',          d?.idType || 'CEDULA');
        setValue('drvIdNumber',        d?.idNumber || '');
        setValue('drvFirstName',       d?.firstName || '');
        setValue('drvLastName',        d?.lastName || '');
        setValue('drvSecondLastName',  d?.secondLastName || '');
        setValue('drvPhone',           d?.phone || '');
        setValue('drvMobile',          d?.mobile || '');
        setValue('drvEmail',           d?.email || '');
        setValue('drvAddress',         d?.address || '');
        setValue('drvPosition',        d?.position || '');
        setValue('drvNotes',           d?.notes || '');
        setValue('drvHireDate',        d?.hireDate?.substring(0, 10) || '');
        setValue('drvLicenseNumber',   d?.licenseNumber || '');
        setValue('drvLicenseCategory', d?.licenseCategory || '');
        setValue('drvLicenseExpiry',   d?.licenseExpiryDate?.substring(0, 10) || '');
        setValue('drvEmergencyName',   d?.emergencyContactName || '');
        setValue('drvEmergencyPhone',  d?.emergencyContactPhone || '');
        document.getElementById('drvIsActive').checked = d?.isActive ?? true;

        checkExpiryDate();

        // Cargar usuarios del sistema
        await loadSystemUsers(d?.idSystemUser);

        // Ir al primer tab
        const firstTab = document.querySelector('#drvFormTabs .nav-link.active');
        if (!firstTab) document.querySelector('#drvFormTabs .nav-link')?.click();

        bootstrap.Modal.getOrCreateInstance(document.getElementById('drvModal')).show();
    }

    async function loadSystemUsers(selectedId) {
        try {
            _systemUsers = await req('GET', 'drivers/system-users') || [];
        } catch { _systemUsers = []; }

        const sel = document.getElementById('drvSystemUser');
        sel.innerHTML = '<option value="">— Sin usuario vinculado —</option>' +
            _systemUsers.map(u => `<option value="${u.id}" ${u.id === selectedId ? 'selected' : ''}>${u.name} (${u.email || ''})</option>`).join('');
    }

    function setValue(id, val) {
        const el = document.getElementById(id);
        if (el) el.value = val;
    }

    function checkExpiryDate() {
        const val = document.getElementById('drvLicenseExpiry')?.value;
        const msg = document.getElementById('drvExpiryMsg');
        if (!msg || !val) return;
        const d   = new Date(val);
        const now = new Date();
        const diff = Math.ceil((d - now) / 86400000);
        if (diff < 0)    { msg.textContent = '⚠️ Licencia vencida'; msg.className = 'expiry-danger'; }
        else if (diff < 60) { msg.textContent = `⚠️ Vence en ${diff} días`; msg.className = 'expiry-warn'; }
        else              { msg.textContent = ''; }
    }

    document.addEventListener('change', e => { if (e.target.id === 'drvLicenseExpiry') checkExpiryDate(); });

    /* ── SAVE ────────────────────────────────────────────────── */
    async function save() {
        const code      = document.getElementById('drvCode')?.value?.trim();
        const firstName = document.getElementById('drvFirstName')?.value?.trim();
        const lastName  = document.getElementById('drvLastName')?.value?.trim();
        const idNumber  = document.getElementById('drvIdNumber')?.value?.trim();

        if (!code || !firstName || !lastName || !idNumber) {
            toast('Código, Nombre, Apellido y Número de ID son obligatorios.', true);
            document.querySelector('#drvFormTabs .nav-link:first-child').click();
            return;
        }

        const sysUser = document.getElementById('drvSystemUser')?.value;
        const payload = {
            code,
            firstName,
            lastName,
            secondLastName       : document.getElementById('drvSecondLastName')?.value?.trim() || null,
            idType               : document.getElementById('drvIdType')?.value || 'CEDULA',
            idNumber,
            phone                : document.getElementById('drvPhone')?.value?.trim() || null,
            mobile               : document.getElementById('drvMobile')?.value?.trim() || null,
            email                : document.getElementById('drvEmail')?.value?.trim() || null,
            address              : document.getElementById('drvAddress')?.value?.trim() || null,
            position             : document.getElementById('drvPosition')?.value?.trim() || null,
            notes                : document.getElementById('drvNotes')?.value?.trim() || null,
            hireDate             : document.getElementById('drvHireDate')?.value || null,
            licenseNumber        : document.getElementById('drvLicenseNumber')?.value?.trim() || null,
            licenseCategory      : document.getElementById('drvLicenseCategory')?.value || null,
            licenseExpiryDate    : document.getElementById('drvLicenseExpiry')?.value || null,
            emergencyContactName : document.getElementById('drvEmergencyName')?.value?.trim() || null,
            emergencyContactPhone: document.getElementById('drvEmergencyPhone')?.value?.trim() || null,
            isActive             : document.getElementById('drvIsActive').checked,
            idSystemUser         : sysUser ? parseInt(sysUser) : null,
        };

        const method = _editId ? 'PUT' : 'POST';
        const url    = _editId ? `drivers/${_editId}` : 'drivers';

        try {
            document.getElementById('drvBtnSave').disabled = true;
            await req(method, url, payload);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('drvModal')).hide();
            toast('Conductor guardado correctamente.');
            await load(_page);
        } catch (e) {
            toast(e.message, true);
        } finally {
            document.getElementById('drvBtnSave').disabled = false;
        }
    }

    /* ── DELETE ───────────────────────────────────────────────── */
    function openDelete(id, name) {
        _deleteId = id;
        document.getElementById('drvDeleteName').textContent = name;
        bootstrap.Modal.getOrCreateInstance(document.getElementById('drvDeleteModal')).show();
    }

    async function confirmDelete() {
        try {
            await req('DELETE', `drivers/${_deleteId}`);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('drvDeleteModal')).hide();
            toast('Conductor desactivado.');
            await load(_page);
        } catch (e) {
            toast(e.message, true);
        }
    }

    /* ── Debounce búsqueda ────────────────────────────────────── */
    function debounceSearch() {
        clearTimeout(_searchTimer);
        _searchTimer = setTimeout(() => load(1), 350);
    }

    /* ── Init ─────────────────────────────────────────────────── */
    document.addEventListener('DOMContentLoaded', () => load(1));

    return { load, openModal, save, openDelete, confirmDelete, debounceSearch };
})();
