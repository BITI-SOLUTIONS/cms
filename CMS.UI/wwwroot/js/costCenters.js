// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/costCenters.js
// PROPÓSITO: Lógica cliente para mantenimiento de centros de costo
// DESCRIPCIÓN: CRUD completo, vista de tarjetas, vista de árbol jerárquico
// AUTOR: BITI SOLUTIONS S.A
// CREADO: 2025-01-XX
// ================================================================================

const costCenters = (function () {
    let allCostCenters = [];
    let currentPage = 1;
    const itemsPerPage = 12;

    // ===== INICIALIZACIÓN =====
    function init() {
        load();
        attachEventListeners();
    }

    function attachEventListeners() {
        // Filtros
        ['#filterSearch', '#filterType', '#filterCategory', '#filterActive', '#filterPosting'].forEach(id => {
            document.querySelector(id)?.addEventListener('change', () => applyFilters());
            document.querySelector(id)?.addEventListener('keyup', () => applyFilters());
        });

        // Tabs
        document.getElementById('tree-tab')?.addEventListener('shown.bs.tab', () => loadHierarchy());

        // Centro de costo padre - actualizar nivel jerárquico automáticamente
        document.getElementById('idParentCostCenter')?.addEventListener('change', function() {
            const parentId = parseInt(this.value);
            if (parentId) {
                const parent = allCostCenters.find(cc => cc.idCostCenter === parentId);
                if (parent) {
                    document.getElementById('hierarchyLevel').value = parent.hierarchyLevel + 1;
                }
            } else {
                document.getElementById('hierarchyLevel').value = 1;
            }
        });
    }

    // ===== CARGA DE DATOS =====
    async function load() {
        try {
            showLoading();
            const response = await fetch(CC_API, {
                headers: { 'Authorization': `Bearer ${CC_TOKEN}` }
            });

            if (!response.ok) throw new Error('Error al cargar centros de costo');

            allCostCenters = await response.json();
            applyFilters();
        } catch (error) {
            console.error('Error loading cost centers:', error);
            showError('Error al cargar centros de costo');
        }
    }

    async function loadHierarchy() {
        try {
            const response = await fetch(`${CC_API}/hierarchy`, {
                headers: { 'Authorization': `Bearer ${CC_TOKEN}` }
            });

            if (!response.ok) throw new Error('Error al cargar jerarquía');

            const hierarchy = await response.json();
            renderTree(hierarchy);
        } catch (error) {
            console.error('Error loading hierarchy:', error);
            showError('Error al cargar jerarquía');
        }
    }

    // ===== FILTROS =====
    function applyFilters() {
        const search = document.getElementById('filterSearch').value.toLowerCase();
        const type = document.getElementById('filterType').value;
        const category = document.getElementById('filterCategory').value.toLowerCase();
        const active = document.getElementById('filterActive').value;
        const posting = document.getElementById('filterPosting').value;

        let filtered = allCostCenters.filter(cc => {
            const matchSearch = !search || 
                cc.code.toLowerCase().includes(search) || 
                cc.name.toLowerCase().includes(search);
            const matchType = !type || cc.costCenterType === type;
            const matchCategory = !category || (cc.category && cc.category.toLowerCase().includes(category));
            const matchActive = !active || cc.isActive.toString() === active;
            const matchPosting = !posting || cc.isPostingAllowed.toString() === posting;

            return matchSearch && matchType && matchCategory && matchActive && matchPosting;
        });

        renderTable(filtered);
    }

    function clearFilters() {
        document.getElementById('filterSearch').value = '';
        document.getElementById('filterType').value = '';
        document.getElementById('filterCategory').value = '';
        document.getElementById('filterActive').value = 'true';
        document.getElementById('filterPosting').value = '';
        applyFilters();
    }

    // ===== RENDERIZADO DE TARJETAS =====
    function renderTable(items) {
        const container = document.getElementById('costCentersContainer');
        if (!items || items.length === 0) {
            container.innerHTML = `
                <div class="col-12 text-center py-5">
                    <i class="bi bi-inbox display-1 text-muted"></i>
                    <p class="text-muted mt-3">No se encontraron centros de costo</p>
                </div>`;
            return;
        }

        // Paginación
        const totalPages = Math.ceil(items.length / itemsPerPage);
        const startIndex = (currentPage - 1) * itemsPerPage;
        const endIndex = startIndex + itemsPerPage;
        const paginatedItems = items.slice(startIndex, endIndex);

        // Renderizar tarjetas
        container.innerHTML = paginatedItems.map(cc => `
            <div class="col-md-6 col-lg-4 col-xl-3">
                <div class="card cost-center-card type-${cc.costCenterType.toLowerCase()}">
                    <div class="card-body">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <div>
                                <span class="hierarchy-level-indicator hierarchy-level-${cc.hierarchyLevel}"></span>
                                <small class="text-light">Nivel ${cc.hierarchyLevel}</small>
                            </div>
                            <span class="badge badge-${cc.costCenterType.toLowerCase()}">${getTypeLabel(cc.costCenterType)}</span>
                        </div>
                        <h6 class="card-title text-light mb-1">${escapeHtml(cc.code)}</h6>
                        <p class="card-text text-light small mb-2">${escapeHtml(cc.name)}</p>
                        ${cc.category ? `<p class="card-text text-muted small mb-2"><i class="bi bi-tag me-1"></i>${escapeHtml(cc.category)}</p>` : ''}
                        ${cc.parentCostCenterCode ? `<p class="card-text text-muted small mb-2"><i class="bi bi-arrow-up-circle me-1"></i>${escapeHtml(cc.parentCostCenterCode)}</p>` : ''}
                        <div class="d-flex justify-content-between align-items-center mt-3">
                            <div>
                                <span class="status-indicator ${cc.isActive ? 'status-active' : (cc.isBlocked ? 'status-blocked' : 'status-inactive')}"></span>
                                ${cc.isPostingAllowed ? '<i class="bi bi-check-circle text-success" title="Permite imputación"></i>' : ''}
                            </div>
                            <div class="btn-group btn-group-sm">
                                <button class="btn btn-outline-primary" onclick="costCenters.openEdit(${cc.idCostCenter})" title="Editar">
                                    <i class="bi bi-pencil"></i>
                                </button>
                                <button class="btn btn-outline-danger" onclick="costCenters.confirmDelete(${cc.idCostCenter}, '${escapeHtml(cc.code)}')" title="Eliminar">
                                    <i class="bi bi-trash"></i>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `).join('');

        renderPagination(totalPages);
    }

    function renderPagination(totalPages) {
        const pagination = document.getElementById('pagination');
        if (totalPages <= 1) {
            pagination.innerHTML = '';
            return;
        }

        let html = '';
        html += `<li class="page-item ${currentPage === 1 ? 'disabled' : ''}">
                    <a class="page-link" href="#" onclick="costCenters.goToPage(${currentPage - 1}); return false;">Anterior</a>
                 </li>`;

        for (let i = 1; i <= totalPages; i++) {
            if (i === 1 || i === totalPages || (i >= currentPage - 2 && i <= currentPage + 2)) {
                html += `<li class="page-item ${i === currentPage ? 'active' : ''}">
                            <a class="page-link" href="#" onclick="costCenters.goToPage(${i}); return false;">${i}</a>
                         </li>`;
            } else if (i === currentPage - 3 || i === currentPage + 3) {
                html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
            }
        }

        html += `<li class="page-item ${currentPage === totalPages ? 'disabled' : ''}">
                    <a class="page-link" href="#" onclick="costCenters.goToPage(${currentPage + 1}); return false;">Siguiente</a>
                 </li>`;

        pagination.innerHTML = html;
    }

    function goToPage(page) {
        currentPage = page;
        applyFilters();
    }

    // ===== RENDERIZADO DE ÁRBOL =====
    function renderTree(items) {
        const container = document.getElementById('treeContainer');

        // Construir árbol jerárquico
        const rootNodes = items.filter(cc => !cc.idParentCostCenter);
        const childMap = {};

        items.forEach(cc => {
            if (cc.idParentCostCenter) {
                if (!childMap[cc.idParentCostCenter]) childMap[cc.idParentCostCenter] = [];
                childMap[cc.idParentCostCenter].push(cc);
            }
        });

        function renderNode(node, indent = 0) {
            const prefix = '  '.repeat(indent);
            const icon = childMap[node.idCostCenter] ? '📁' : '📄';
            const typeColor = getTypeColor(node.costCenterType);

            let html = `<div class="tree-node" data-id="${node.idCostCenter}" onclick="costCenters.selectTreeNode(${node.idCostCenter})">
                           ${prefix}${icon} <span style="color: ${typeColor}">${escapeHtml(node.code)}</span> - ${escapeHtml(node.name)}
                       </div>`;

            if (childMap[node.idCostCenter]) {
                childMap[node.idCostCenter]
                    .sort((a, b) => a.code.localeCompare(b.code))
                    .forEach(child => {
                        html += renderNode(child, indent + 1);
                    });
            }

            return html;
        }

        container.innerHTML = rootNodes
            .sort((a, b) => a.code.localeCompare(b.code))
            .map(root => renderNode(root))
            .join('');
    }

    function selectTreeNode(id) {
        document.querySelectorAll('.tree-node').forEach(n => n.classList.remove('selected'));
        const node = document.querySelector(`.tree-node[data-id="${id}"]`);
        if (node) node.classList.add('selected');
        openEdit(id);
    }

    // ===== MODAL CRUD =====
    function openNew() {
        clearForm();
        document.getElementById('modalTitle').innerHTML = '<i class="bi bi-plus-circle me-2"></i>Nuevo Centro de Costo';
        document.getElementById('validFrom').value = new Date().toISOString().split('T')[0];
        loadParentOptions();
        new bootstrap.Modal(document.getElementById('costCenterModal')).show();
    }

    async function openEdit(id) {
        try {
            const response = await fetch(`${CC_API}/${id}`, {
                headers: { 'Authorization': `Bearer ${CC_TOKEN}` }
            });

            if (!response.ok) throw new Error('Error al cargar centro de costo');

            const cc = await response.json();
            populateForm(cc);
            document.getElementById('modalTitle').innerHTML = `<i class="bi bi-pencil me-2"></i>Editar Centro de Costo: ${escapeHtml(cc.code)}`;
            await loadParentOptions(id);
            new bootstrap.Modal(document.getElementById('costCenterModal')).show();
        } catch (error) {
            console.error('Error loading cost center:', error);
            showError('Error al cargar centro de costo');
        }
    }

    async function save() {
        const data = {
            idCostCenter: parseInt(document.getElementById('idCostCenter').value) || 0,
            code: document.getElementById('code').value.trim(),
            name: document.getElementById('name').value.trim(),
            description: document.getElementById('description').value.trim() || null,
            idParentCostCenter: document.getElementById('idParentCostCenter').value ? parseInt(document.getElementById('idParentCostCenter').value) : null,
            hierarchyLevel: parseInt(document.getElementById('hierarchyLevel').value) || 1,
            costCenterType: document.getElementById('costCenterType').value,
            category: document.getElementById('category').value.trim() || null,
            location: document.getElementById('location').value.trim() || null,
            department: document.getElementById('department').value.trim() || null,
            division: document.getElementById('division').value.trim() || null,
            validFrom: document.getElementById('validFrom').value,
            validTo: document.getElementById('validTo').value || null,
            annualBudget: document.getElementById('annualBudget').value ? parseFloat(document.getElementById('annualBudget').value) : null,
            budgetCurrency: document.getElementById('budgetCurrency').value,
            allowOverBudget: document.getElementById('allowOverBudget').checked,
            isPostingAllowed: document.getElementById('isPostingAllowed').checked,
            isBlocked: document.getElementById('isBlocked').checked,
            isActive: document.getElementById('isActive').checked,
            profitCenterCode: document.getElementById('profitCenterCode').value.trim() || null,
            businessAreaCode: document.getElementById('businessAreaCode').value.trim() || null,
            companyCode: document.getElementById('companyCode').value.trim() || null,
            notes: document.getElementById('notes').value.trim() || null
        };

        // Validaciones
        if (!data.code || !data.name || !data.validFrom) {
            showError('Complete los campos obligatorios');
            return;
        }

        try {
            const url = data.idCostCenter ? `${CC_API}/${data.idCostCenter}` : CC_API;
            const method = data.idCostCenter ? 'PUT' : 'POST';

            const response = await fetch(url, {
                method: method,
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${CC_TOKEN}`
                },
                body: JSON.stringify(data)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Error al guardar');
            }

            showSuccess(data.idCostCenter ? 'Centro de costo actualizado correctamente' : 'Centro de costo creado correctamente');
            bootstrap.Modal.getInstance(document.getElementById('costCenterModal')).hide();
            load();
        } catch (error) {
            console.error('Error saving cost center:', error);
            showError(error.message);
        }
    }

    async function confirmDelete(id, code) {
        if (!confirm(`¿Está seguro de eliminar el centro de costo ${code}?\n\nEsta acción no se puede deshacer.`)) return;

        try {
            const response = await fetch(`${CC_API}/${id}`, {
                method: 'DELETE',
                headers: { 'Authorization': `Bearer ${CC_TOKEN}` }
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Error al eliminar');
            }

            showSuccess('Centro de costo eliminado correctamente');
            load();
        } catch (error) {
            console.error('Error deleting cost center:', error);
            showError(error.message);
        }
    }

    // ===== FORMULARIO =====
    function clearForm() {
        document.getElementById('idCostCenter').value = '';
        document.getElementById('code').value = '';
        document.getElementById('name').value = '';
        document.getElementById('description').value = '';
        document.getElementById('idParentCostCenter').value = '';
        document.getElementById('hierarchyLevel').value = '1';
        document.getElementById('costCenterType').value = 'Operational';
        document.getElementById('category').value = '';
        document.getElementById('location').value = '';
        document.getElementById('department').value = '';
        document.getElementById('division').value = '';
        document.getElementById('validFrom').value = '';
        document.getElementById('validTo').value = '';
        document.getElementById('annualBudget').value = '';
        document.getElementById('budgetCurrency').value = 'CRC';
        document.getElementById('allowOverBudget').checked = false;
        document.getElementById('isPostingAllowed').checked = true;
        document.getElementById('isBlocked').checked = false;
        document.getElementById('isActive').checked = true;
        document.getElementById('profitCenterCode').value = '';
        document.getElementById('businessAreaCode').value = '';
        document.getElementById('companyCode').value = '';
        document.getElementById('notes').value = '';
    }

    function populateForm(cc) {
        document.getElementById('idCostCenter').value = cc.idCostCenter;
        document.getElementById('code').value = cc.code;
        document.getElementById('name').value = cc.name;
        document.getElementById('description').value = cc.description || '';
        document.getElementById('idParentCostCenter').value = cc.idParentCostCenter || '';
        document.getElementById('hierarchyLevel').value = cc.hierarchyLevel;
        document.getElementById('costCenterType').value = cc.costCenterType;
        document.getElementById('category').value = cc.category || '';
        document.getElementById('location').value = cc.location || '';
        document.getElementById('department').value = cc.department || '';
        document.getElementById('division').value = cc.division || '';
        document.getElementById('validFrom').value = cc.validFrom.split('T')[0];
        document.getElementById('validTo').value = cc.validTo ? cc.validTo.split('T')[0] : '';
        document.getElementById('annualBudget').value = cc.annualBudget || '';
        document.getElementById('budgetCurrency').value = cc.budgetCurrency;
        document.getElementById('allowOverBudget').checked = cc.allowOverBudget;
        document.getElementById('isPostingAllowed').checked = cc.isPostingAllowed;
        document.getElementById('isBlocked').checked = cc.isBlocked;
        document.getElementById('isActive').checked = cc.isActive;
        document.getElementById('profitCenterCode').value = cc.profitCenterCode || '';
        document.getElementById('businessAreaCode').value = cc.businessAreaCode || '';
        document.getElementById('companyCode').value = cc.companyCode || '';
        document.getElementById('notes').value = cc.notes || '';
    }

    async function loadParentOptions(excludeId = null) {
        const select = document.getElementById('idParentCostCenter');
        select.innerHTML = '<option value="">Sin padre (nivel raíz)</option>';

        const parents = allCostCenters
            .filter(cc => !excludeId || cc.idCostCenter !== excludeId)
            .sort((a, b) => a.code.localeCompare(b.code));

        parents.forEach(cc => {
            const option = document.createElement('option');
            option.value = cc.idCostCenter;
            option.textContent = `${cc.code} - ${cc.name}`;
            select.appendChild(option);
        });
    }

    // ===== UTILIDADES =====
    function getTypeLabel(type) {
        const labels = {
            'Operational': 'Operacional',
            'Administrative': 'Administrativo',
            'ServiceCenter': 'Servicio',
            'Auxiliary': 'Auxiliar'
        };
        return labels[type] || type;
    }

    function getTypeColor(type) {
        const colors = {
            'Operational': '#3b82f6',
            'Administrative': '#8b5cf6',
            'ServiceCenter': '#10b981',
            'Auxiliary': '#f59e0b'
        };
        return colors[type] || '#6b7280';
    }

    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

    function showLoading() {
        document.getElementById('costCentersContainer').innerHTML = `
            <div class="col-12 text-center py-5">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Cargando...</span>
                </div>
            </div>`;
    }

    function showError(message) {
        alert('❌ ' + message);
    }

    function showSuccess(message) {
        alert('✅ ' + message);
    }

    // ===== API PÚBLICA =====
    return {
        init,
        load,
        loadHierarchy,
        applyFilters,
        clearFilters,
        openNew,
        openEdit,
        save,
        confirmDelete,
        goToPage,
        selectTreeNode
    };
})();

// Inicializar al cargar la página
document.addEventListener('DOMContentLoaded', () => costCenters.init());
