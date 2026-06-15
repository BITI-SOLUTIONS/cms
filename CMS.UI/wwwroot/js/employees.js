// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/employees.js
// PROPOSITO: Logica cliente para la pantalla de Empleados (HR)
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-07-04
// ================================================================================

const EMP = (() => {
    const API   = () => window.EMP_API   || '';
    const TOKEN = () => window.EMP_TOKEN || '';
    const hdrs  = () => ({ 'Content-Type': 'application/json', 'Authorization': `Bearer ${TOKEN()}` });

    let _editId       = null;
    let _deactivateId = null;
    let _deleteId     = null;
    let _page         = 1;
    let _total        = 0;
    const PAGE_SIZE   = 20;
    let _searchTimer;
    let _systemUsers  = [];
    let _departments  = [];
    let _jobPositions = [];
    let _locations    = [];
    let _genders      = [];
    let _typeIds      = [];
    let _currencies   = [];
    let _defaultCurrencyId = null;
    const _rowMap = {};   // id → employee row data

    /* ── fetch helper ─────────────────────────────────────────── */
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

    /* ── toast ────────────────────────────────────────────────── */
    function toast(msg, isError = false) {
        const el = document.createElement('div');
        el.className = `alert alert-${isError ? 'danger' : 'success'} position-fixed bottom-0 end-0 m-3 py-2 px-3 shadow`;
        el.style.zIndex = 9999;
        el.innerHTML = `<i class="bi bi-${isError ? 'exclamation-triangle' : 'check-circle'} me-2"></i>${msg}`;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 3500);
    }

    /* ── helpers ──────────────────────────────────────────────── */
    const activeBadge = v => v
        ? '<span class="badge bg-success">Activo</span>'
        : '<span class="badge bg-secondary">Inactivo</span>';

    function typeLabel(t) {
        const MAP = {
            FULL_TIME: { label: 'T. Completo', cls: 'bg-primary' },
            PART_TIME: { label: 'Medio T.',    cls: 'bg-info text-dark' },
            CONTRACT:  { label: 'Contrato',    cls: 'bg-warning text-dark' },
            INTERN:    { label: 'Pasante',     cls: 'bg-secondary' },
            OTHER:     { label: 'Otro',        cls: 'bg-dark border border-secondary' }
        };
        const m = MAP[t] || MAP.OTHER;
        return `<span class="badge ${m.cls} emp-type-badge">${m.label}</span>`;
    }

    function deptBadge(name, icon, color) {
        if (!name) return '<span class="text-muted small">-</span>';
        const bg = color ? color + '33' : '#6366f133';
        const border = color || '#6366f1';
        const ico = icon ? `<i class="${icon}"></i> ` : '';
        return `<span class="dept-badge" style="background:${bg};border-color:${border};color:#e2e8f0">${ico}${name}</span>`;
    }

    function initials(name) {
        return (name || '?').split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase();
    }

    function formatDate(d) {
        if (!d) return '-';
        return new Date(d + 'T00:00:00').toLocaleDateString('es-CR', { day: '2-digit', month: 'short', year: 'numeric' });
    }

    function calcAge(dateStr) {
        if (!dateStr) return null;
        const d = new Date(dateStr + 'T00:00:00');
        const now = new Date();
        let age = now.getFullYear() - d.getFullYear();
        if (now < new Date(now.getFullYear(), d.getMonth(), d.getDate())) age--;
        return age;
    }

    function calcTenure(hireDateStr) {
        if (!hireDateStr) return null;
        const start = new Date(hireDateStr + 'T00:00:00');
        const now   = new Date();
        const diff  = now - start;
        const years  = Math.floor(diff / (365.25 * 24 * 3600 * 1000));
        const months = Math.floor((diff % (365.25 * 24 * 3600 * 1000)) / (30.44 * 24 * 3600 * 1000));
        if (years === 0) return `${months} mes(es)`;
        return `${years} ano(s) y ${months} mes(es)`;
    }

    function formatSalary(amount, currencyId) {
        if (!amount) return '-';
        const cur = _currencies.find(c => c.id === currencyId);
        const sym = cur?.symbol || '';
        return sym + ' ' + Number(amount).toLocaleString('es-CR', { minimumFractionDigits: 2 });
    }

    /* ── LOAD ─────────────────────────────────────────────────── */
    async function load(page = 1) {
        _page = page;
        const search  = document.getElementById('empSearch')?.value?.trim() || '';
        const active  = document.getElementById('empFilterActive')?.value || '';
        const dept    = document.getElementById('empFilterDept')?.value || '';
        const empType = document.getElementById('empFilterType')?.value || '';

        let url = `employees?page=${_page}&pageSize=${PAGE_SIZE}`;
        if (search)  url += `&search=${encodeURIComponent(search)}`;
        if (active)  url += `&isActive=${active}`;
        if (dept)    url += `&idDepartment=${dept}`;
        if (empType) url += `&employmentType=${empType}`;

        const body = document.getElementById('empBody');
        body.innerHTML = '<tr><td colspan="10" class="text-center text-muted py-3"><i class="bi bi-hourglass-split me-2"></i>Cargando...</td></tr>';

        try {
            const data = await req('GET', url);
            _total = data.total;
            renderTable(data.items);
            renderPagination(data.total, data.page, data.pageSize);
            loadStats();
        } catch (e) {
            body.innerHTML = `<tr><td colspan="10" class="text-center text-danger py-3"><i class="bi bi-exclamation-triangle me-2"></i>${e.message}</td></tr>`;
        }
    }

    async function loadStats() {
        try {
            const s = await req('GET', 'employees/stats');
            document.getElementById('kpiTotal').textContent    = s.total ?? 0;
            document.getElementById('kpiActive').textContent   = s.active ?? 0;
            document.getElementById('kpiInactive').textContent = s.inactive ?? 0;
            document.getElementById('kpiNewMonth').textContent = s.newThisMonth ?? 0;
        } catch { /* ignorar */ }
    }

    function renderTable(items) {
        const body = document.getElementById('empBody');
        if (!items || !items.length) {
            body.innerHTML = `<tr><td colspan="10" class="text-center py-5">
                <i class="bi bi-people empty-state-icon d-block mb-2"></i>
                <span class="empty-state-text">Sin empleados registrados</span>
            </td></tr>`;
            return;
        }

        const jobPosMap = {};
        _jobPositions.forEach(p => { jobPosMap[p.id] = p.name; });

        body.innerHTML = items.map(e => {
            _rowMap[e.id] = e;   // guardar datos para edición
            return `
        <tr class="${e.isActive ? '' : 'opacity-50'}">
            <td><span class="emp-avatar ${e.isActive ? '' : 'inactive'}" title="${e.fullName}">${initials(e.fullName)}</span></td>
            <td><code class="text-info">${e.code}</code></td>
            <td>
                <div class="fw-semibold text-light">${e.fullName}</div>
                ${e.systemUserName ? `<small class="text-info"><i class="bi bi-person-check me-1"></i>${e.systemUserName}</small>` : ''}
            </td>
            <td>${deptBadge(e.departmentName, e.departmentIcon, e.departmentColor)}</td>
            <td><small class="text-light">${jobPosMap[e.idJobPosition] || '<span class="text-muted">-</span>'}</small></td>
            <td>${typeLabel(e.employmentType)}</td>
            <td>
                <small class="text-muted">${formatDate(e.hireDate)}</small>
                ${e.hireDate ? `<br><small class="text-muted" style="font-size:.65rem">${calcTenure(e.hireDate) || ''}</small>` : ''}
            </td>
            <td>
                <small>${e.mobile || e.phone || '<span class="text-muted">-</span>'}</small>
                ${e.email ? `<br><small class="text-muted">${e.email}</small>` : ''}
            </td>
            <td class="text-center">${activeBadge(e.isActive)}</td>
            <td class="text-end">
                <div class="d-flex gap-1 justify-content-end">
                    <button class="btn btn-xs btn-outline-info btn-sm py-0 px-1"
                            onclick="EMP.openModal(${e.id})" title="Editar">
                        <i class="bi bi-pencil"></i>
                    </button>
                    ${e.isActive
                        ? `<button class="btn btn-xs btn-outline-warning btn-sm py-0 px-1"
                                   onclick='EMP.openDeactivate(${e.id},"${(e.fullName || '').replace(/"/g, '&quot;')}")' title="Dar de baja">
                               <i class="bi bi-person-dash"></i></button>`
                        : `<button class="btn btn-xs btn-outline-success btn-sm py-0 px-1"
                                   onclick="EMP.activate(${e.id})" title="Reactivar">
                               <i class="bi bi-person-check"></i></button>`
                    }
                    <button class="btn btn-xs btn-outline-danger btn-sm py-0 px-1"
                            onclick='EMP.openDelete(${e.id},"${(e.fullName || '').replace(/"/g, '&quot;')}")' title="Eliminar">
                        <i class="bi bi-trash"></i>
                    </button>
                </div>
            </td>
        </tr>`;
        }).join('');
    }

    function renderPagination(total, page, pageSize) {
        const pages = Math.ceil(total / pageSize);
        const from = Math.min((page - 1) * pageSize + 1, total);
        const to   = Math.min(page * pageSize, total);
        document.getElementById('empPagInfo').textContent = total
            ? `${from}-${to} de ${total} empleados`
            : 'Sin resultados';

        const nav = document.getElementById('empPagNav');
        if (pages <= 1) { nav.innerHTML = ''; return; }
        const btns = [];
        let start = Math.max(1, page - 3), end = Math.min(pages, start + 6);
        if (end - start < 6) start = Math.max(1, end - 6);
        if (start > 1) btns.push(`<button class="btn btn-xs btn-sm btn-outline-secondary py-0 px-2" onclick="EMP.load(1)">1</button>`);
        if (start > 2) btns.push(`<span class="text-muted px-1">...</span>`);
        for (let i = start; i <= end; i++)
            btns.push(`<button class="btn btn-xs btn-sm ${i === page ? 'btn-primary' : 'btn-outline-secondary'} py-0 px-2" onclick="EMP.load(${i})">${i}</button>`);
        if (end < pages - 1) btns.push(`<span class="text-muted px-1">...</span>`);
        if (end < pages) btns.push(`<button class="btn btn-xs btn-sm btn-outline-secondary py-0 px-2" onclick="EMP.load(${pages})">${pages}</button>`);
        nav.innerHTML = btns.join('');
    }

    /* ── CATALOGS ─────────────────────────────────────────────── */
    async function loadCatalogs() {
        // Departamentos
        try { _departments = await req('GET', 'employees/departments?isActive=true') || []; } catch { _departments = []; }
        // Puestos
        try { _jobPositions = await req('GET', 'employees/job-positions?isActive=true') || []; } catch { _jobPositions = []; }
        // Generos
        try { _genders = await req('GET', 'employees/genders') || []; } catch { _genders = []; }
        // Tipos de ID
        try { _typeIds = await req('GET', 'employees/type-ids') || []; } catch { _typeIds = []; }
        // Monedas
        try { _currencies = await req('GET', 'employees/currencies') || []; } catch { _currencies = []; }
        // Moneda por defecto de la compania
        try {
            const dc = await req('GET', 'employees/company-currency');
            if (dc) _defaultCurrencyId = dc.id;
        } catch { _defaultCurrencyId = null; }
        // Ubicaciones (solo disponibles + la actual del empleado si es edición)
        try {
            const locUrl = _editId ? `employees/locations?currentEmployeeId=${_editId}` : 'employees/locations';
            _locations = await req('GET', locUrl) || [];
        } catch { _locations = []; }

        // Poblar filtro de departamento
        const filterDept = document.getElementById('empFilterDept');
        if (filterDept) {
            const cur = filterDept.value;
            filterDept.innerHTML = '<option value="">Todos los dptos.</option>' +
                _departments.map(d => `<option value="${d.id}">${d.name}</option>`).join('');
            filterDept.value = cur;
        }

        // Poblar selector de departamento del modal
        const modalDept = document.getElementById('empDepartment');
        if (modalDept) {
            const cur = modalDept.value;
            modalDept.innerHTML = '<option value="">- Seleccione departamento -</option>' +
                _departments.map(d => `<option value="${d.id}">${d.name}</option>`).join('');
            if (cur) modalDept.value = cur;
            // Registrar listener una sola vez
            if (!modalDept._deptHandlerAttached) {
                modalDept.addEventListener('change', onDepartmentChange);
                modalDept._deptHandlerAttached = true;
            }
        }

        // Poblar selector de puestos (filtrado por departamento si hay uno seleccionado)
        const modalPos = document.getElementById('empJobPosition');
        if (modalPos) {
            const cur = modalPos.value;
            filterJobPositionsByDept(document.getElementById('empDepartment')?.value || '');
            if (cur) modalPos.value = cur;
        }

        // Poblar generos
        const selGender = document.getElementById('empGender');
        if (selGender) {
            const cur = selGender.value;
            selGender.innerHTML = '<option value="">- Seleccione -</option>' +
                _genders.map(g => `<option value="${g.code}">${g.description}</option>`).join('');
            if (cur) selGender.value = cur;
        }

        // Poblar tipos de ID
        const selIdType = document.getElementById('empIdType');
        if (selIdType) {
            const cur = selIdType.value;
            selIdType.innerHTML = '<option value="">- Seleccione -</option>' +
                _typeIds.map(t => `<option value="${t.id}" data-regex="${t.formatValidation || ''}" data-nchars="${t.numberChars || 0}" data-letters="${t.allowLetters}">${t.description}</option>`).join('');
            if (cur) selIdType.value = cur;
        }

        // Poblar monedas
        const selCur = document.getElementById('empSalaryCurrency');
        if (selCur) {
            const cur = selCur.value;
            selCur.innerHTML = '<option value="">- Seleccione -</option>' +
                _currencies.map(c =>
                    `<option value="${c.id}" data-symbol="${c.symbol}" data-code="${c.code}">${c.symbol} ${c.code} - ${c.name}</option>`
                ).join('');
            if (cur) selCur.value = cur;
            else if (_defaultCurrencyId) {
                selCur.value = _defaultCurrencyId;
                updateCurrencySymbol();
            }
        }

        // Poblar ubicaciones
        const selLoc = document.getElementById('empLocation');
        if (selLoc) {
            const cur = selLoc.value;
            selLoc.innerHTML = '<option value="">- Sin direccion asignada -</option>' +
                _locations.map(l => `<option value="${l.id}">${l.display}</option>`).join('');
            if (cur) selLoc.value = cur;
        }
    }

    async function refreshLocations() {
        try {
            const locUrl = _editId ? `employees/locations?currentEmployeeId=${_editId}` : 'employees/locations';
            _locations = await req('GET', locUrl) || [];
        } catch { _locations = []; }
        const selLoc = document.getElementById('empLocation');
        if (!selLoc) return;
        const cur = selLoc.value;
        selLoc.innerHTML = '<option value="">- Sin direccion asignada -</option>' +
            _locations.map(l => `<option value="${l.id}">${l.display}</option>`).join('');
        if (cur) selLoc.value = cur;
        toast('Lista de ubicaciones actualizada.');
    }

    async function refreshJobPositions() {
        try { _jobPositions = await req('GET', 'employees/job-positions?isActive=true') || []; } catch { _jobPositions = []; }
        filterJobPositionsByDept(document.getElementById('empDepartment')?.value || '');
        toast('Lista de puestos actualizada.');
    }

    async function loadSystemUsers(selectedId) {
        try { _systemUsers = await req('GET', 'employees/system-users') || []; } catch { _systemUsers = []; }
        const sel = document.getElementById('empSystemUser');
        if (!sel) return;
        sel.innerHTML = '<option value="">- Sin usuario vinculado -</option>' +
            _systemUsers.map(u => `<option value="${u.id}" ${u.id === selectedId ? 'selected' : ''}>${u.name} (${u.email || ''})</option>`).join('');
        if (!sel._empHandlerAttached) {
            sel.addEventListener('change', onSystemUserChange);
            sel._empHandlerAttached = true;
        }
    }

    function onSystemUserChange() {
        if (_editId) return;
        const selVal = document.getElementById('empSystemUser')?.value;
        if (!selVal) return;
        const user = _systemUsers.find(u => String(u.id) === String(selVal));
        if (!user) return;
        const setIfEmpty = (id, val) => {
            const el = document.getElementById(id);
            if (el && !el.value && val) el.value = val;
        };
        setIfEmpty('empFirstName', user.firstName);
        setIfEmpty('empLastName',  user.lastName);
        setIfEmpty('empEmail',     user.email);
        setIfEmpty('empPhone',     user.phone);
        setIfEmpty('empMobile',    user.phone);
    }

    function clearSystemUser() { setValue('empSystemUser', ''); }

    /* ── Filtrar puestos por departamento ─────────────────────── */
    function filterJobPositionsByDept(deptId) {
        const sel = document.getElementById('empJobPosition');
        if (!sel) return;
        const cur = sel.value;
        const filtered = deptId
            ? _jobPositions.filter(p => String(p.idDepartment) === String(deptId))
            : _jobPositions;
        sel.innerHTML = '<option value="">- Seleccione puesto -</option>' +
            filtered.map(p => `<option value="${p.id}">${p.name}${p.level ? ' (' + p.level + ')' : ''}</option>`).join('');
        // Restaurar selección si sigue disponible
        if (cur && filtered.some(p => String(p.id) === String(cur))) sel.value = cur;
    }

    function onDepartmentChange() {
        filterJobPositionsByDept(document.getElementById('empDepartment')?.value || '');
    }

    /* ── Validacion ID segun Tipo ID ──────────────────────────── */
    function onIdTypeChange() {
        const sel  = document.getElementById('empIdType');
        const hint = document.getElementById('empIdNumberHint');
        const inp  = document.getElementById('empIdNumber');
        if (!sel || !hint || !inp) return;

        const opt     = sel.options[sel.selectedIndex];
        const nchars  = parseInt(opt.getAttribute('data-nchars') || '0');
        const regex   = opt.getAttribute('data-regex') || '';
        const letters = opt.getAttribute('data-letters') === 'true';

        // Aplicar maxlength al input
        if (nchars > 0) {
            inp.setAttribute('maxlength', nchars);
            inp.setAttribute('data-maxchars', nchars);
        } else {
            inp.removeAttribute('maxlength');
            inp.removeAttribute('data-maxchars');
        }

        // Restringir tipo de caracteres via inputmode
        if (!letters) {
            inp.setAttribute('inputmode', 'numeric');
            inp.setAttribute('pattern', '[0-9]*');
        } else {
            inp.removeAttribute('inputmode');
            inp.removeAttribute('pattern');
        }

        // Guardar regex para validación al guardar
        if (regex) inp.setAttribute('data-regex', regex);
        else        inp.removeAttribute('data-regex');

        // Texto de ayuda
        let hintText = '';
        if (nchars > 0) hintText = `${nchars} caracteres`;
        if (!letters)   hintText += (hintText ? ', ' : '') + 'solo números';
        hint.textContent = hintText;

        // Limpiar valor si excede el nuevo maxlength
        if (nchars > 0 && inp.value.length > nchars) inp.value = inp.value.substring(0, nchars);

        // Registrar listener oninput para bloquear caracteres de más (una sola vez)
        if (!inp._idValidationAttached) {
            inp.addEventListener('input', function () {
                const maxc = parseInt(this.getAttribute('data-maxchars') || '0');
                if (maxc > 0 && this.value.length > maxc) this.value = this.value.substring(0, maxc);
            });
            inp.addEventListener('keypress', function (e) {
                const selEl = document.getElementById('empIdType');
                if (!selEl) return;
                const allowLetters = selEl.options[selEl.selectedIndex]?.getAttribute('data-letters') === 'true';
                if (!allowLetters && !/[0-9]/.test(e.key)) e.preventDefault();
            });
            inp._idValidationAttached = true;
        }
    }

    function validateIdNumber() {
        const inp = document.getElementById('empIdNumber');
        const sel = document.getElementById('empIdType');
        if (!inp || !sel) return true;
        const val = inp.value.trim();
        if (!val) return true;

        const opt          = sel.options[sel.selectedIndex];
        const nchars       = parseInt(opt?.getAttribute('data-nchars') || '0');
        const allowLetters = opt?.getAttribute('data-letters') === 'true';

        // Validar longitud exacta si se definió un número de caracteres
        if (nchars > 0 && val.length !== nchars) return false;

        // Validar que solo contenga números si no se permiten letras
        if (!allowLetters && !/^\d+$/.test(val)) return false;

        return true;
    }

    /* ── MODAL ABRIR ──────────────────────────────────────────── */
    async function openModal(rowOrId = null) {
        let d = null;
        if (rowOrId !== null && rowOrId !== undefined) {
            if (typeof rowOrId === 'number' || (typeof rowOrId === 'string' && !rowOrId.startsWith('{'))) {
                // Recibimos solo el ID — buscar en el mapa
                d = _rowMap[parseInt(rowOrId)] || null;
            } else {
                d = typeof rowOrId === 'string' ? JSON.parse(rowOrId) : rowOrId;
            }
        }
        _editId = d?.id || null;

        document.getElementById('empModalTitle').innerHTML =
            `<i class="bi bi-person-badge me-2 text-primary"></i>${d ? 'Editar Empleado' : 'Nuevo Empleado'}`;

        setValue('empCode',              d?.code || '');
        setValue('empIdNumber',          d?.idNumber || '');
        setValue('empFirstName',         d?.firstName || '');
        setValue('empSecondName',        d?.secondName || '');
        setValue('empLastName',          d?.lastName || '');
        setValue('empSecondLastName',    d?.secondLastName || '');

        // Fecha de nacimiento: si es nuevo, pre-cargar con hace 18 años
        const defaultBirthDate = (() => {
            const d18 = new Date();
            d18.setFullYear(d18.getFullYear() - 18);
            return d18.toISOString().substring(0, 10);
        })();
        setValue('empBirthDate', d?.birthDate?.substring(0, 10) || defaultBirthDate);

        setValue('empPhone',             d?.phone || '');
        setValue('empMobile',            d?.mobile || '');
        setValue('empEmail',             d?.email || '');
        setValue('empEmploymentType',    d?.employmentType || 'FULL_TIME');
        setValue('empHireDate',          d?.hireDate?.substring(0, 10) || '');
        setValue('empTerminationDate',   d?.terminationDate?.substring(0, 10) || '');
        setValue('empTerminationReason', d?.terminationReason || '');
        setValue('empPaymentFrequency',  d?.paymentFrequency || 'MONTHLY');
        setValue('empBaseSalary',        d?.baseSalary || '');
        setValue('empEmergencyName',     d?.emergencyContactName || '');
        setValue('empEmergencyPhone',    d?.emergencyContactPhone || '');
        setValue('empEmergencyRelation', d?.emergencyContactRelation || '');
        setValue('empNotes',             d?.notes || '');
        document.getElementById('empIsActive').checked = d?.isActive ?? true;

        // Cargar catalogos primero
        await Promise.all([
            loadCatalogs(),
            loadSystemUsers(d?.idSystemUser)
        ]);

        // Restaurar valores de selects (deben poblarse antes)
        if (d?.idTypeId) setValue('empIdType', d.idTypeId);
        if (d?.gender)   setValue('empGender', d.gender);
        if (d?.idDepartment) setValue('empDepartment', d.idDepartment);
        if (d?.idJobPosition) setValue('empJobPosition', d.idJobPosition);
        if (d?.idLocation) setValue('empLocation', d.idLocation);
        if (d?.idCurrency) {
            setValue('empSalaryCurrency', d.idCurrency);
        } else if (_defaultCurrencyId) {
            setValue('empSalaryCurrency', _defaultCurrencyId);
        }

        onIdTypeChange();
        updateCurrencySymbol();
        updateAgeDisplay();
        updateTenureDisplay();

        document.querySelector('#empFormTabs .nav-link')?.click();
        bootstrap.Modal.getOrCreateInstance(document.getElementById('empModal')).show();
    }

    /* ── DEACTIVATE ───────────────────────────────────────────── */
    function openDeactivate(id, name) {
        _deactivateId = id;
        document.getElementById('empDeactivateName').textContent = name;
        setValue('empDeactivateDate', new Date().toISOString().substring(0, 10));
        setValue('empDeactivateReason', '');
        bootstrap.Modal.getOrCreateInstance(document.getElementById('empDeactivateModal')).show();
    }

    async function confirmDeactivate() {
        try {
            await req('PATCH', `employees/${_deactivateId}/deactivate`, {
                terminationDate: document.getElementById('empDeactivateDate')?.value || null,
                reason: document.getElementById('empDeactivateReason')?.value?.trim() || null
            });
            bootstrap.Modal.getOrCreateInstance(document.getElementById('empDeactivateModal')).hide();
            toast('Empleado dado de baja correctamente.');
            await load(_page);
        } catch (e) { toast(e.message, true); }
    }

    async function activate(id) {
        try {
            await req('PATCH', `employees/${id}/activate`);
            toast('Empleado reactivado.');
            await load(_page);
        } catch (e) { toast(e.message, true); }
    }

    /* ── DELETE ───────────────────────────────────────────────── */
    function openDelete(id, name) {
        _deleteId = id;
        document.getElementById('empDeleteName').textContent = name;
        bootstrap.Modal.getOrCreateInstance(document.getElementById('empDeleteModal')).show();
    }

    async function confirmDelete() {
        try {
            await req('DELETE', `employees/${_deleteId}`);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('empDeleteModal')).hide();
            toast('Empleado eliminado.');
            await load(_page);
        } catch (e) { toast(e.message, true); }
    }

    /* ── SAVE ─────────────────────────────────────────────────── */
    async function save() {
        const code           = document.getElementById('empCode')?.value?.trim();
        const firstName      = document.getElementById('empFirstName')?.value?.trim();
        const lastName       = document.getElementById('empLastName')?.value?.trim();
        const secondLastName = document.getElementById('empSecondLastName')?.value?.trim();
        const idNumber       = document.getElementById('empIdNumber')?.value?.trim();
        const idTypeId       = document.getElementById('empIdType')?.value;
        const gender         = document.getElementById('empGender')?.value;
        const birthDate      = document.getElementById('empBirthDate')?.value;
        const mobile         = document.getElementById('empMobile')?.value?.trim();
        const email          = document.getElementById('empEmail')?.value?.trim();
        const hireDate       = document.getElementById('empHireDate')?.value;
        const dept           = document.getElementById('empDepartment')?.value;
        const jobPos         = document.getElementById('empJobPosition')?.value;
        const baseSalary     = document.getElementById('empBaseSalary')?.value;
        const currencyId     = document.getElementById('empSalaryCurrency')?.value;

        // Validaciones obligatorias
        const errors = [];
        if (!code)           errors.push('Codigo es obligatorio');
        if (!idTypeId)       errors.push('Tipo ID es obligatorio');
        if (!idNumber)       errors.push('Numero de ID es obligatorio');
        if (!validateIdNumber()) errors.push('El formato del Numero de ID no es valido para el Tipo ID seleccionado');
        if (!gender)         errors.push('Genero es obligatorio');
        if (!firstName)      errors.push('Primer Nombre es obligatorio');
        if (!lastName)       errors.push('Primer Apellido es obligatorio');
        if (!secondLastName) errors.push('Segundo Apellido es obligatorio');
        if (!birthDate)      errors.push('Fecha de Nacimiento es obligatoria');
        if (!mobile)         errors.push('Movil es obligatorio');
        if (!email)          errors.push('Correo Electronico es obligatorio');
        if (!dept)           errors.push('Departamento es obligatorio');
        if (!jobPos)         errors.push('Puesto es obligatorio');
        if (!hireDate)       errors.push('Fecha de Ingreso es obligatoria');
        if (!baseSalary || parseFloat(baseSalary) <= 0) errors.push('Salario Base debe ser mayor a 0');
        if (!currencyId)     errors.push('Moneda es obligatoria');

        if (errors.length) {
            toast(errors[0], true);
            // Ir al tab con el primer error
            const personalFields = ['empCode','empIdType','empIdNumber','empGender','empFirstName','empLastName','empSecondLastName','empBirthDate','empMobile','empEmail'];
            const laboralFields  = ['empDepartment','empJobPosition','empHireDate'];
            const salaryFields   = ['empBaseSalary','empSalaryCurrency'];
            if (['Codigo es obligatorio','Tipo ID es obligatorio','Numero de ID es obligatorio','Formato','Genero','Primer Nombre','Primer Apellido','Segundo Apellido','Fecha de Nacimiento','Movil','Correo'].some(k => errors[0].includes(k)))
                document.querySelector('#empFormTabs .nav-link:first-child').click();
            else if (errors[0].includes('Departamento') || errors[0].includes('Puesto') || errors[0].includes('Ingreso'))
                document.querySelectorAll('#empFormTabs .nav-link')[1].click();
            else if (errors[0].includes('Salario') || errors[0].includes('Moneda'))
                document.querySelectorAll('#empFormTabs .nav-link')[2].click();
            return;
        }

        const secondName = document.getElementById('empSecondName')?.value?.trim() || null;
        const payload = {
            code,
            firstName,
            secondName,
            lastName,
            secondLastName,
            fullName: `${firstName}${secondName ? ' '+secondName : ''} ${lastName}${secondLastName ? ' '+secondLastName : ''}`.trim(),
            idTypeId: parseInt(idTypeId),
            idNumber,
            gender,
            birthDate,
            phone:    document.getElementById('empPhone')?.value?.trim() || null,
            mobile,
            email,
            idLocation:    document.getElementById('empLocation')?.value ? parseInt(document.getElementById('empLocation').value) : null,
            idDepartment:  parseInt(dept),
            idJobPosition: parseInt(jobPos),
            employmentType: document.getElementById('empEmploymentType')?.value || 'FULL_TIME',
            hireDate,
            terminationDate:   document.getElementById('empTerminationDate')?.value || null,
            terminationReason: document.getElementById('empTerminationReason')?.value?.trim() || null,
            baseSalary:    parseFloat(baseSalary),
            idCurrency:    parseInt(currencyId),
            paymentFrequency: document.getElementById('empPaymentFrequency')?.value || 'MONTHLY',
            emergencyContactName:     document.getElementById('empEmergencyName')?.value?.trim() || null,
            emergencyContactPhone:    document.getElementById('empEmergencyPhone')?.value?.trim() || null,
            emergencyContactRelation: document.getElementById('empEmergencyRelation')?.value?.trim() || null,
            notes:      document.getElementById('empNotes')?.value?.trim() || null,
            isActive:   document.getElementById('empIsActive').checked,
            idSystemUser: document.getElementById('empSystemUser')?.value ? parseInt(document.getElementById('empSystemUser').value) : null,
        };

        const method = _editId ? 'PUT' : 'POST';
        const url    = _editId ? `employees/${_editId}` : 'employees';

        try {
            document.getElementById('empBtnSave').disabled = true;
            await req(method, url, payload);
            bootstrap.Modal.getOrCreateInstance(document.getElementById('empModal')).hide();
            toast('Empleado guardado correctamente.');
            await load(_page);
        } catch (e) {
            toast(e.message, true);
        } finally {
            document.getElementById('empBtnSave').disabled = false;
        }
    }

    /* ── UI helpers ───────────────────────────────────────────── */
    function setValue(id, val) {
        const el = document.getElementById(id);
        if (el) el.value = val ?? '';
    }

    function updateCurrencySymbol() {
        const sel    = document.getElementById('empSalaryCurrency');
        const prefix = document.getElementById('empCurrencyPrefix');
        if (!prefix || !sel) return;
        const opt = sel.options[sel.selectedIndex];
        prefix.textContent = opt ? (opt.getAttribute('data-symbol') || opt.getAttribute('data-code') || '?') : '?';
    }

    function updateAgeDisplay() {
        const val = document.getElementById('empBirthDate')?.value;
        const el  = document.getElementById('empAge');
        if (!el) return;
        const age = calcAge(val);
        el.textContent = age !== null ? `${age} años` : '';
    }

    function updateTenureDisplay() {
        const val = document.getElementById('empHireDate')?.value;
        const el  = document.getElementById('empTenure');
        if (!el) return;
        const t = calcTenure(val);
        el.textContent = t ? `Antiguedad: ${t}` : '';
    }

    document.addEventListener('change', e => {
        if (e.target.id === 'empBirthDate') updateAgeDisplay();
        if (e.target.id === 'empHireDate')  updateTenureDisplay();
    });

    function debounceSearch() {
        clearTimeout(_searchTimer);
        _searchTimer = setTimeout(() => load(1), 350);
    }

    /* ── Init ─────────────────────────────────────────────────── */
    document.addEventListener('DOMContentLoaded', async () => {
        await loadCatalogs();
        await load(1);
    });

    return {
        load, openModal, save,
        openDeactivate, confirmDeactivate, activate,
        openDelete, confirmDelete,
        debounceSearch, clearSystemUser,
        updateCurrencySymbol, onIdTypeChange,
        refreshLocations, refreshJobPositions
    };
})();
