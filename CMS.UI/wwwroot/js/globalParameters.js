// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/globalParameters.js
// PROPÓSITO: Gestión de Parámetros Globales del Sistema por módulo
// DESCRIPCIÓN: CRUD de parámetros globales con UI dinámica según tipo de dato
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-01-22
// MODIFICADO: 2026-01-22 - Actualizado para usar MenuId y Code
// ================================================================================

const GP = {
    menuList: [],      // Lista de objetos { menuId, menuName }
    parameters: [],
    currentMenuId: null,
    editingParameter: null,
    currencies: [],    // Lista de monedas activas

    // ============================================================
    // INICIALIZACIÓN
    // ============================================================

    async init() {
        console.log('🚀 Global Parameters: Inicializando...');
        await this.loadCurrencies();
        await this.loadMenus();
    },

    // ============================================================
    // CARGA DE DATOS
    // ============================================================

    async loadMenus() {
        try {
            console.log('🔄 Cargando menús desde:', `${API_BASE_URL}/api/globalparameters/menus/with-names`);

            // Cargar menús con nombres desde el endpoint que combina ambas fuentes
            const response = await fetch(`${API_BASE_URL}/api/globalparameters/menus/with-names`, {
                headers: { 'Authorization': `Bearer ${API_TOKEN}` }
            });

            console.log('📡 Response status:', response.status);

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: 'Error desconocido' }));
                console.error('❌ Error en respuesta:', errorData);
                throw new Error(errorData.message || `HTTP ${response.status}`);
            }

            this.menuList = await response.json();
            console.log('📋 Menús recibidos:', this.menuList);

            if (!this.menuList || this.menuList.length === 0) {
                console.warn('⚠️ No se encontraron menús con parámetros');
                this.showAlert('No hay módulos con parámetros configurados. Ejecute el script SQL de creación.', 'warning');
                return;
            }

            this.renderMenuSelector();

            console.log(`✅ Menús cargados: ${this.menuList.length}`, this.menuList);
        } catch (error) {
            console.error('❌ Error cargando menús:', error);
            this.showAlert(`Error cargando la lista de módulos: ${error.message}`, 'danger');
        }
    },

    async loadCurrencies() {
        try {
            console.log('💱 Cargando monedas desde:', `${API_BASE_URL}/api/currency/active`);

            const response = await fetch(`${API_BASE_URL}/api/currency/active`, {
                headers: { 'Authorization': `Bearer ${API_TOKEN}` }
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}`);
            }

            this.currencies = await response.json();
            console.log('✅ Monedas cargadas:', this.currencies.length);
        } catch (error) {
            console.error('❌ Error cargando monedas:', error);
            this.showAlert('Error cargando la lista de monedas', 'warning');
            this.currencies = [];
        }
    },

    async loadMenuParameters(menuId = null) {
        // Si no se proporciona menuId, usar el del selector o el currentMenuId
        if (!menuId) {
            const selector = document.getElementById('moduleSelector');
            menuId = selector.value || this.currentMenuId;
        }

        console.log('🔄 loadMenuParameters llamado, menuId:', menuId);

        if (!menuId) {
            this.hideParameters();
            return;
        }

        this.currentMenuId = parseInt(menuId);

        try {
            const url = `${API_BASE_URL}/api/globalparameters/menu/${menuId}`;
            console.log('📡 Llamando a:', url);

            const response = await fetch(url, {
                headers: { 'Authorization': `Bearer ${API_TOKEN}` }
            });

            console.log('📡 Response status:', response.status);

            if (!response.ok) {
                const errorText = await response.text();
                console.error('❌ Error response:', errorText);
                throw new Error('Error cargando parámetros');
            }

            this.parameters = await response.json();
            console.log('📋 Parámetros recibidos:', this.parameters);

            this.renderParameters();

            const menuName = this.menuList.find(m => m.menuId === this.currentMenuId)?.menuName || `Menu ${menuId}`;
            console.log(`✅ Parámetros cargados para ${menuName}: ${this.parameters.length}`);
        } catch (error) {
            console.error('❌ Error cargando parámetros:', error);
            this.showAlert('Error cargando los parámetros del módulo', 'danger');
        }
    },

    async refreshParameters() {
        if (this.currentMenuId) {
            await this.loadMenuParameters();
            this.showAlert('Parámetros actualizados correctamente', 'success');
        }
    },

    // ============================================================
    // RENDERIZADO
    // ============================================================

    renderMenuSelector() {
        const selector = document.getElementById('moduleSelector');

        // Limpiar opciones existentes excepto la primera
        while (selector.options.length > 1) {
            selector.remove(1);
        }

        // Agregar menús
        this.menuList.forEach(menu => {
            const option = document.createElement('option');
            option.value = menu.menuId;
            option.textContent = menu.menuName;
            selector.appendChild(option);
        });
    },

    renderParameters() {
        const container = document.getElementById('parametersContainer');
        const list = document.getElementById('parametersList');
        const noMessage = document.getElementById('noParametersMessage');
        const initialMessage = document.getElementById('initialMessage');
        const countBadge = document.getElementById('parameterCount');

        // Ocultar mensaje inicial
        initialMessage.style.display = 'none';
        container.style.display = 'block';

        if (!this.parameters || this.parameters.length === 0) {
            list.innerHTML = '';
            noMessage.style.display = 'block';
            countBadge.textContent = '0 parámetros';
            return;
        }

        noMessage.style.display = 'none';
        countBadge.textContent = `${this.parameters.length} parámetro${this.parameters.length !== 1 ? 's' : ''}`;

        list.innerHTML = this.parameters.map(p => this.buildParameterCard(p)).join('');
    },

    buildParameterCard(param) {
        const typeColors = {
            'boolean': 'success',
            'string': 'primary',
            'integer': 'warning',
            'decimal': 'info',
            'json': 'secondary'
        };

        const typeBadge = `<span class="badge bg-${typeColors[param.dataType] || 'secondary'} param-type-badge">${param.dataType}</span>`;
        const systemBadge = param.isSystem ? '<span class="badge bg-danger param-type-badge ms-1">SYSTEM</span>' : '';
        const activeBadge = param.isActive 
            ? '<span class="badge bg-success param-type-badge ms-1">ACTIVE</span>' 
            : '<span class="badge bg-secondary param-type-badge ms-1">INACTIVE</span>';

        const valueDisplay = this.formatValue(param);

        return `
            <div class="param-card">
                <div class="row align-items-center">
                    <div class="col-lg-7">
                        <div class="d-flex align-items-center gap-2 mb-2">
                            <strong class="text-light">${param.parameterName}</strong>
                            ${typeBadge}${systemBadge}${activeBadge}
                        </div>
                        <div class="param-key mb-2">${param.code}</div>
                        ${param.description ? `<div class="param-description">${param.description}</div>` : ''}
                    </div>
                    <div class="col-lg-3">
                        <label class="param-label d-block mb-1">Valor Actual</label>
                        ${valueDisplay}
                    </div>
                    <div class="col-lg-2 text-end">
                        <button class="btn btn-sm btn-outline-primary" onclick="GP.openEditModal(${param.id})" title="Editar parámetro">
                            <i class="bi bi-pencil-square"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
    },

    formatValue(param) {
        // Usar valueBoolean directamente para booleanos en lugar de value
        let value;
        if (param.dataType === 'boolean') {
            value = param.valueBoolean;
        } else {
            value = param.value;
        }

        console.log('🎨 formatValue:', { 
            code: param.code, 
            dataType: param.dataType, 
            value: value,
            valueBoolean: param.valueBoolean,
            param: param 
        });

        if (param.dataType === 'boolean') {
            return value === true
                ? '<span class="badge bg-success" style="font-size: .9rem; padding: 0.5rem 1rem;">✓ TRUE</span>'
                : '<span class="badge bg-danger" style="font-size: .9rem; padding: 0.5rem 1rem;">✗ FALSE</span>';
        }

        // Si es parámetro de moneda, buscar por ID
        if (param.code === 'currency_local' || param.code === 'currency_exchange') {
            const currency = this.currencies.find(c => c.id == value);
            if (currency) {
                return `<span class="badge bg-primary" style="font-size: .9rem; padding: 0.5rem 1rem;">
                    ${currency.symbol} ${currency.code} - ${currency.name}
                </span>`;
            }
            return `<span class="text-light">${value || 'No configurado'}</span>`;
        }

        if (value === null || value === undefined) {
            return '<span class="text-muted fst-italic">NULL</span>';
        }

        if (param.dataType === 'json') {
            return `<code class="text-info" style="font-size: .8rem;">${JSON.stringify(value).substring(0, 60)}...</code>`;
        }

        return `<span class="text-light">${value}</span>`;
    },

    hideParameters() {
        document.getElementById('parametersContainer').style.display = 'none';
        document.getElementById('initialMessage').style.display = 'block';
        this.currentMenuId = null;
        this.parameters = [];
    },

    // ============================================================
    // EDICIÓN
    // ============================================================

    openEditModal(id) {
        const param = this.parameters.find(p => p.id === id);
        if (!param) return;

        console.log('✏️ Abriendo modal de edición para parámetro:', param);

        this.editingParameter = param;

        document.getElementById('editParamId').value = param.id;
        document.getElementById('editParamName').value = param.parameterName;
        document.getElementById('editParamKey').textContent = param.code;
        document.getElementById('editParamDescription').value = param.description || '';
        document.getElementById('editParamDataType').textContent = param.dataType;
        document.getElementById('editParamActive').checked = param.isActive;

        this.renderEditValueField(param);

        const modal = new bootstrap.Modal(document.getElementById('editParameterModal'));
        modal.show();
    },

    renderEditValueField(param) {
        const container = document.getElementById('editValueField');
        let html = '';

        console.log('🎨 Renderizando campo de edición:', { 
            dataType: param.dataType, 
            code: param.code,
            valueBoolean: param.valueBoolean,
            valueInteger: param.valueInteger,
            value: param.value 
        });

        // Detectar si es un parámetro de moneda por su código
        const isCurrencyParam = param.code === 'currency_local' || param.code === 'currency_exchange';

        if (isCurrencyParam) {
            // Renderizar selector de moneda
            const currentValue = param.valueInteger || param.value || '';

            html = `
                <select id="editParamValue" class="form-select param-value" data-currency-param="${param.code}">
                    <option value="">— Seleccione una moneda —</option>
                    ${this.currencies.map(c => `
                        <option value="${c.id}" ${c.id == currentValue ? 'selected' : ''}>
                            ${c.code} - ${c.name} (${c.symbol})
                        </option>
                    `).join('')}
                </select>
                <small class="form-text">
                    ${param.code === 'currency_local' 
                        ? 'Moneda oficial para todas las transacciones de la compañía' 
                        : 'Moneda secundaria para conversiones (debe ser diferente a la moneda local)'}
                </small>
            `;
        } else {
            // Renderizado normal según data_type
            switch (param.dataType) {
                case 'boolean':
                    const isChecked = param.valueBoolean === true || param.value === true;
                    html = `
                        <div class="form-check form-switch">
                            <input class="form-check-input" type="checkbox" id="editParamValue" 
                                   ${isChecked ? 'checked' : ''}>
                            <label class="form-check-label text-light" for="editParamValue">
                                ${isChecked ? 'Habilitado (TRUE)' : 'Deshabilitado (FALSE)'}
                            </label>
                        </div>
                    `;
                    break;

                case 'integer':
                    html = `
                        <input type="number" id="editParamValue" class="form-control param-value" 
                               value="${param.valueInteger || 0}" step="1">
                    `;
                    break;

                case 'decimal':
                    html = `
                        <input type="number" id="editParamValue" class="form-control param-value" 
                               value="${param.valueDecimal || 0}" step="0.0001">
                    `;
                    break;

                case 'json':
                    html = `
                        <textarea id="editParamValue" class="form-control param-value" rows="6">${param.valueJson || ''}</textarea>
                        <small class="form-text">Formato JSON válido</small>
                    `;
                    break;

                default: // string
                    html = `
                        <input type="text" id="editParamValue" class="form-control param-value" 
                               value="${param.valueString || ''}">
                    `;
                    break;
            }
        }

        container.innerHTML = html;

        // Agregar listener para actualizar el label del boolean
        if (param.dataType === 'boolean') {
            const checkbox = document.getElementById('editParamValue');
            const label = checkbox.nextElementSibling;
            checkbox.addEventListener('change', function() {
                label.textContent = this.checked ? 'Habilitado (TRUE)' : 'Deshabilitado (FALSE)';
            });
        }

        // Agregar listener para currency params
        if (isCurrencyParam) {
            const select = document.getElementById('editParamValue');
            select.addEventListener('change', () => this.onCurrencyChange(param.code, select.value));
        }
    },

    onCurrencyChange(paramCode, newValue) {
        console.log(`💱 Moneda cambiada en ${paramCode}:`, newValue);

        // Convertir a integer para comparación
        newValue = parseInt(newValue);

        // Buscar el ID de USD y EUR en la lista de monedas
        const usdCurrency = this.currencies.find(c => c.code === 'USD');
        const eurCurrency = this.currencies.find(c => c.code === 'EUR');

        if (!usdCurrency || !eurCurrency) {
            console.warn('No se encontraron monedas USD o EUR');
            return;
        }

        // Si es currency_local y se selecciona USD, sugerir EUR para currency_exchange
        // Si es currency_local y se selecciona otra, sugerir USD para currency_exchange
        if (paramCode === 'currency_local' && newValue) {
            const otherParam = this.parameters.find(p => p.code === 'currency_exchange');
            if (otherParam) {
                const suggestedId = newValue === usdCurrency.id ? eurCurrency.id : usdCurrency.id;
                const suggestedCode = newValue === usdCurrency.id ? 'EUR' : 'USD';

                console.log(`💡 Sugerencia para currency_exchange: ${suggestedCode} (ID: ${suggestedId})`);

                // Mostrar alerta informativa con el código de la moneda
                const currentCurrency = this.currencies.find(c => c.id === newValue);
                this.showAlert(
                    `💡 Recomendación: Para moneda local ${currentCurrency?.code || ''}, se sugiere usar ${suggestedCode} como moneda de cambio`,
                    'info'
                );
            }
        }
    },

    async saveParameter() {
        const id = parseInt(document.getElementById('editParamId').value);
        const param = this.editingParameter;
        if (!param) return;

        let value;
        const valueInput = document.getElementById('editParamValue');
        const isCurrencyParam = param.code === 'currency_local' || param.code === 'currency_exchange';

        if (isCurrencyParam) {
            // Lógica especial para parámetros de moneda
            value = valueInput.value;

            // Validar que no esté vacío
            if (!value) {
                this.showAlert('Debe seleccionar una moneda', 'warning');
                return;
            }

            // Convertir a integer
            value = parseInt(value);

            // Validar que currency_local y currency_exchange sean diferentes
            const otherCode = param.code === 'currency_local' ? 'currency_exchange' : 'currency_local';
            const otherParam = this.parameters.find(p => p.code === otherCode);
            const otherValue = otherParam?.valueInteger || otherParam?.value;

            if (value === otherValue) {
                this.showAlert(
                    'La moneda local y la moneda de cambio deben ser diferentes',
                    'danger'
                );
                return;
            }
        } else {
            // Lógica normal según data_type
            switch (param.dataType) {
                case 'boolean':
                    value = valueInput.checked;
                    break;
                case 'integer':
                    value = parseInt(valueInput.value) || 0;
                    break;
                case 'decimal':
                    value = parseFloat(valueInput.value) || 0;
                    break;
                case 'json':
                    try {
                        JSON.parse(valueInput.value); // Validar JSON
                        value = valueInput.value;
                    } catch (e) {
                        this.showAlert('El valor JSON no es válido', 'danger');
                        return;
                    }
                    break;
                default:
                    value = valueInput.value;
                    break;
            }
        }

        const payload = {
            menuId: param.menuId,
            code: param.code,
            parameterName: param.parameterName,
            description: param.description,
            dataType: param.dataType,
            value: value,
            category: param.category,
            sortOrder: param.sortOrder,
            isActive: document.getElementById('editParamActive').checked
        };

        try {
            const response = await fetch(`${API_BASE_URL}/api/globalparameters/${id}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${API_TOKEN}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Error guardando parámetro');
            }

            const data = await response.json();
            console.log('✅ Respuesta del servidor:', data);

            bootstrap.Modal.getInstance(document.getElementById('editParameterModal')).hide();
            this.showAlert('Parámetro actualizado correctamente', 'success');

            // Recargar usando el currentMenuId para asegurar que se actualiza la vista
            console.log('🔄 Recargando parámetros del menú:', this.currentMenuId);
            await this.loadMenuParameters(this.currentMenuId);
            console.log('✅ Parámetros recargados, nuevo estado:', this.parameters);

        } catch (error) {
            console.error('❌ Error guardando parámetro:', error);
            this.showAlert(error.message || 'Error guardando el parámetro', 'danger');
        }
    },

    // ============================================================
    // UTILIDADES
    // ============================================================

    showAlert(message, type = 'info') {
        const container = document.querySelector('.container-fluid');
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3`;
        alert.style.zIndex = '9999';
        alert.style.minWidth = '400px';
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        container.insertBefore(alert, container.firstChild);

        setTimeout(() => {
            alert.classList.remove('show');
            setTimeout(() => alert.remove(), 150);
        }, 4000);
    }
};
