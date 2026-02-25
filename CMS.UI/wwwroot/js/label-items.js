// ================================================================================
// ARCHIVO: CMS.UI/wwwroot/js/label-items.js
// PROPÓSITO: JavaScript para la vista de impresión de etiquetas de artículos
// AUTOR: EAMR, BITI SOLUTIONS S.A
// CREADO: 2026-02-23
// ================================================================================

var originalData = {};
var selectedElement = null;

function selectItem(element) {
    console.log('selectItem called');

    var itemId = element.getAttribute('data-item-id');
    var itemCode = element.getAttribute('data-item-code');
    var itemName = element.getAttribute('data-item-name');
    var labelItem = element.getAttribute('data-item-label') || itemName;
    var labelPrice = parseFloat(element.getAttribute('data-item-price')) || 0;
    var labelBarcode = element.getAttribute('data-item-barcode') || '';
    var printName = element.getAttribute('data-print-name') !== 'false';
    var printPrice = element.getAttribute('data-print-price') !== 'false';
    var printBarcode = element.getAttribute('data-print-barcode') !== 'false';

    // Campos de tamaño y colores
    var labelWidth = parseFloat(element.getAttribute('data-label-width')) || 4;
    var labelHeight = parseFloat(element.getAttribute('data-label-height')) || 2;
    var labelOrientation = element.getAttribute('data-label-orientation') || 'horizontal';
    var printBorder = element.getAttribute('data-print-border') !== 'false';
    var borderColor = element.getAttribute('data-border-color') || '#000000';
    var nameColor = element.getAttribute('data-name-color') || '#000000';
    var priceColor = element.getAttribute('data-price-color') || '#16a34a';
    var barcodeColor = element.getAttribute('data-barcode-color') || '#000000';

    // Campos de fuente y formato de precio
    var fontSize = parseFloat(element.getAttribute('data-font-size')) || 14;
    var fontFamily = element.getAttribute('data-font-family') || 'Arial';
    var priceDecimalsAttr = element.getAttribute('data-price-decimals');
    var priceDecimals = (priceDecimalsAttr !== null && priceDecimalsAttr !== '') ? parseInt(priceDecimalsAttr) : 2;
    var thousandSeparator = element.getAttribute('data-thousand-separator') || ',';
    var currencySymbol = element.getAttribute('data-currency-symbol') || '₡';
    var printCurrency = element.getAttribute('data-print-currency') !== 'false';

    console.log('Item selected:', itemId, itemCode, itemName);

    // Guardar referencia al elemento seleccionado
    selectedElement = element;

    // Resaltar el item seleccionado
    var rows = document.querySelectorAll('.item-row');
    for (var i = 0; i < rows.length; i++) {
        rows[i].style.background = 'rgba(0,0,0,0.2)';
        rows[i].style.borderLeft = 'none';
    }
    element.style.background = 'rgba(6, 182, 212, 0.2)';
    element.style.borderLeft = '4px solid #06b6d4';

    // Guardar datos originales
    originalData = {
        labelItem: labelItem,
        labelPrice: labelPrice,
        labelBarcode: labelBarcode,
        printLabelName: printName,
        printLabelPrice: printPrice,
        printLabelBarcode: printBarcode,
        labelWidthCm: labelWidth,
        labelHeightCm: labelHeight,
        labelOrientation: labelOrientation,
        printLabelBorder: printBorder,
        labelBorderColor: borderColor,
        labelNameColor: nameColor,
        labelPriceColor: priceColor,
        labelBarcodeColor: barcodeColor,
        labelFontSize: fontSize,
        labelFontFamily: fontFamily,
        labelPriceDecimals: priceDecimals,
        labelThousandSeparator: thousandSeparator,
        labelCurrencySymbol: currencySymbol,
        printCurrencySymbol: printCurrency
    };

    // Mostrar formulario
    var noItemSelected = document.getElementById('noItemSelected');
    var labelForm = document.getElementById('labelForm');
    var btnPrint = document.getElementById('btnPrint');

    if (noItemSelected) noItemSelected.style.display = 'none';
    if (labelForm) labelForm.style.display = 'block';
    if (btnPrint) btnPrint.disabled = false;

    // Llenar datos básicos
    setElementValue('selectedItemId', itemId);
    setElementText('itemCode', itemCode);
    setElementText('itemName', itemName);
    setElementValue('labelItem', labelItem);
    setElementValue('labelPrice', labelPrice);
    setElementValue('labelBarcode', labelBarcode);
    setElementChecked('printLabelName', printName);
    setElementChecked('printLabelPrice', printPrice);
    setElementChecked('printLabelBarcode', printBarcode);

    // Llenar campos de tamaño y colores
    setElementValue('labelWidthCm', labelWidth);
    setElementValue('labelHeightCm', labelHeight);
    setElementValue('labelOrientation', labelOrientation);
    setElementChecked('printLabelBorder', printBorder);
    setElementValue('labelBorderColor', borderColor);
    setElementValue('labelNameColor', nameColor);
    setElementValue('labelPriceColor', priceColor);
    setElementValue('labelBarcodeColor', barcodeColor);

    // Llenar campos de fuente y formato de precio
    setElementValue('labelFontSize', fontSize);
    setElementValue('labelFontFamily', fontFamily);
    setElementValue('labelPriceDecimals', priceDecimals);
    setElementValue('labelThousandSeparator', thousandSeparator);
    setElementValue('labelCurrencySymbol', currencySymbol);
    setElementChecked('printCurrencySymbol', printCurrency);

    // Actualizar vista previa
    updatePreview();

    console.log('selectItem completed successfully');
}

function setElementValue(id, value) {
    var el = document.getElementById(id);
    if (el) el.value = value;
}

function setElementText(id, text) {
    var el = document.getElementById(id);
    if (el) el.textContent = text;
}

function setElementChecked(id, checked) {
    var el = document.getElementById(id);
    if (el) el.checked = checked;
}

function getElementValue(id, defaultValue) {
    var el = document.getElementById(id);
    return el ? el.value : defaultValue;
}

function getElementChecked(id, defaultValue) {
    var el = document.getElementById(id);
    return el ? el.checked : defaultValue;
}

function formatPrice(price, decimals, thousandSep, currencySymbol, showCurrency) {
    // Determinar separador de decimales (contrario al de miles)
    var decimalSep = thousandSep === ',' ? '.' : ',';

    // Formatear el número con los decimales especificados
    var parts = price.toFixed(decimals).split('.');
    var intPart = parts[0];
    var decPart = parts[1] || '';

    // Agregar separador de miles
    var formattedInt = intPart.replace(/\B(?=(\d{3})+(?!\d))/g, thousandSep);

    // Construir precio formateado
    var formattedPrice = decimals > 0 ? formattedInt + decimalSep + decPart : formattedInt;

    // Agregar símbolo de moneda si está habilitado
    return showCurrency ? currencySymbol + formattedPrice : formattedPrice;
}

function updateCurrencyDisplay() {
    var currencySymbol = getElementValue('labelCurrencySymbol', '₡');

    // Actualizar el símbolo en el input-group del precio
    var priceInputSymbol = document.getElementById('priceInputSymbol');
    if (priceInputSymbol) {
        priceInputSymbol.textContent = currencySymbol;
    }

    // Actualizar el icono del label según la moneda
    var priceLabelIcon = document.getElementById('priceLabelIcon');
    if (priceLabelIcon) {
        if (currencySymbol === '$') {
            priceLabelIcon.className = 'bi bi-currency-dollar me-1 text-success';
        } else if (currencySymbol === '€') {
            priceLabelIcon.className = 'bi bi-currency-euro me-1 text-success';
        } else {
            // Para colones y otros, usar un icono genérico
            priceLabelIcon.className = 'bi bi-cash me-1 text-success';
        }
    }
}

function updatePreview() {
    var name = getElementValue('labelItem', 'Nombre del Producto');
    var price = parseFloat(getElementValue('labelPrice', 0)) || 0;
    var barcode = getElementValue('labelBarcode', '');
    var showName = getElementChecked('printLabelName', true);
    var showPrice = getElementChecked('printLabelPrice', true);
    var showBarcode = getElementChecked('printLabelBarcode', true);
    var showBorder = getElementChecked('printLabelBorder', true);

    var widthCm = parseFloat(getElementValue('labelWidthCm', 4)) || 4;
    var heightCm = parseFloat(getElementValue('labelHeightCm', 2)) || 2;
    var orientation = getElementValue('labelOrientation', 'horizontal');
    var borderColor = getElementValue('labelBorderColor', '#000000');
    var nameColor = getElementValue('labelNameColor', '#000000');
    var priceColor = getElementValue('labelPriceColor', '#16a34a');
    var barcodeColor = getElementValue('labelBarcodeColor', '#000000');

    // Campos de fuente y formato
    var fontSize = parseFloat(getElementValue('labelFontSize', 14)) || 14;
    var fontFamily = getElementValue('labelFontFamily', 'Arial');

    // Obtener decimales correctamente (puede ser 0)
    var decimalsEl = document.getElementById('labelPriceDecimals');
    var priceDecimals = decimalsEl ? parseInt(decimalsEl.value) : 2;
    if (isNaN(priceDecimals)) priceDecimals = 2;

    var thousandSeparator = getElementValue('labelThousandSeparator', ',');
    var currencySymbol = getElementValue('labelCurrencySymbol', '₡');
    var printCurrency = getElementChecked('printCurrencySymbol', true);

    // Actualizar símbolo de moneda en el formulario
    updateCurrencyDisplay();

    var labelPreview = document.getElementById('labelPreview');
    var previewName = document.getElementById('previewName');
    var previewPrice = document.getElementById('previewPrice');
    var previewBarcode = document.getElementById('previewBarcode');

    // Calcular dimensiones (1cm ≈ 37.8px)
    var pxPerCm = 37.8;
    var width = orientation === 'horizontal' ? widthCm * pxPerCm : heightCm * pxPerCm;
    var height = orientation === 'horizontal' ? heightCm * pxPerCm : widthCm * pxPerCm;

    if (labelPreview) {
        labelPreview.style.width = width + 'px';
        labelPreview.style.minHeight = height + 'px';
        labelPreview.style.border = showBorder ? ('2px solid ' + borderColor) : 'none';
        labelPreview.style.fontFamily = fontFamily;
    }

    if (previewName) {
        previewName.textContent = showName ? (name || 'Nombre del Producto') : '';
        previewName.style.display = showName ? 'block' : 'none';
        previewName.style.color = nameColor;
        previewName.style.fontSize = fontSize + 'px';
        previewName.style.fontFamily = fontFamily;
    }
    if (previewPrice) {
        var formattedPrice = formatPrice(price, priceDecimals, thousandSeparator, currencySymbol, printCurrency);
        previewPrice.textContent = showPrice ? formattedPrice : '';
        previewPrice.style.display = showPrice ? 'block' : 'none';
        previewPrice.style.color = priceColor;
        previewPrice.style.fontSize = (fontSize * 1.4) + 'px';
        previewPrice.style.fontFamily = fontFamily;
    }
    if (previewBarcode) {
        previewBarcode.textContent = showBarcode ? (barcode || '||||||||||||||||') : '';
        previewBarcode.style.display = showBarcode ? 'block' : 'none';
        previewBarcode.style.color = barcodeColor;
        previewBarcode.style.fontSize = (fontSize * 0.7) + 'px';
    }
}

function resetColor(elementId, color) {
    var el = document.getElementById(elementId);
    if (el) {
        el.value = color;
        updatePreview();
    }
    return false;
}

function saveLabel() {
    var itemId = getElementValue('selectedItemId', '');

    // Obtener el valor de decimales correctamente (puede ser 0)
    var decimalsEl = document.getElementById('labelPriceDecimals');
    var decimalsValue = decimalsEl ? parseInt(decimalsEl.value) : 2;
    if (isNaN(decimalsValue)) decimalsValue = 2;

    var data = {
        itemId: parseInt(itemId),
        labelItem: getElementValue('labelItem', ''),
        labelPrice: parseFloat(getElementValue('labelPrice', 0)) || 0,
        labelItemBarcode: getElementValue('labelBarcode', ''),
        printLabelName: getElementChecked('printLabelName', true),
        printLabelPrice: getElementChecked('printLabelPrice', true),
        printLabelBarcode: getElementChecked('printLabelBarcode', true),
        labelWidthCm: parseFloat(getElementValue('labelWidthCm', 4)) || 4,
        labelHeightCm: parseFloat(getElementValue('labelHeightCm', 2)) || 2,
        labelOrientation: getElementValue('labelOrientation', 'horizontal'),
        printLabelBorder: getElementChecked('printLabelBorder', true),
        labelBorderColor: getElementValue('labelBorderColor', '#000000'),
        labelNameColor: getElementValue('labelNameColor', '#000000'),
        labelPriceColor: getElementValue('labelPriceColor', '#16a34a'),
        labelBarcodeColor: getElementValue('labelBarcodeColor', '#000000'),
        labelFontSize: parseFloat(getElementValue('labelFontSize', 14)) || 14,
        labelFontFamily: getElementValue('labelFontFamily', 'Arial'),
        labelPriceDecimals: decimalsValue,
        labelThousandSeparator: getElementValue('labelThousandSeparator', ','),
        labelCurrencySymbol: getElementValue('labelCurrencySymbol', '₡'),
        printCurrencySymbol: getElementChecked('printCurrencySymbol', true)
    };

    fetch('/Inventory/LabelItems/SaveLabel', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
    .then(function(response) { return response.json(); })
    .then(function(result) {
        if (result.success) {
            // Actualizar datos originales
            originalData = {
                labelItem: data.labelItem,
                labelPrice: data.labelPrice,
                labelBarcode: data.labelItemBarcode,
                printLabelName: data.printLabelName,
                printLabelPrice: data.printLabelPrice,
                printLabelBarcode: data.printLabelBarcode,
                labelWidthCm: data.labelWidthCm,
                labelHeightCm: data.labelHeightCm,
                labelOrientation: data.labelOrientation,
                printLabelBorder: data.printLabelBorder,
                labelBorderColor: data.labelBorderColor,
                labelNameColor: data.labelNameColor,
                labelPriceColor: data.labelPriceColor,
                labelBarcodeColor: data.labelBarcodeColor,
                labelFontSize: data.labelFontSize,
                labelFontFamily: data.labelFontFamily,
                labelPriceDecimals: data.labelPriceDecimals,
                labelThousandSeparator: data.labelThousandSeparator,
                labelCurrencySymbol: data.labelCurrencySymbol,
                printCurrencySymbol: data.printCurrencySymbol
            };

            // Actualizar el elemento en la lista sin recargar la página
            if (selectedElement && result.item) {
                selectedElement.setAttribute('data-item-label', result.item.labelItem || result.item.name);
                selectedElement.setAttribute('data-item-price', result.item.labelPrice);
                selectedElement.setAttribute('data-item-barcode', result.item.labelItemBarcode || '');
                selectedElement.setAttribute('data-print-name', result.item.printLabelName.toString().toLowerCase());
                selectedElement.setAttribute('data-print-price', result.item.printLabelPrice.toString().toLowerCase());
                selectedElement.setAttribute('data-print-barcode', result.item.printLabelBarcode.toString().toLowerCase());
                selectedElement.setAttribute('data-label-width', result.item.labelWidthCm);
                selectedElement.setAttribute('data-label-height', result.item.labelHeightCm);
                selectedElement.setAttribute('data-label-orientation', result.item.labelOrientation);
                selectedElement.setAttribute('data-print-border', result.item.printLabelBorder.toString().toLowerCase());
                selectedElement.setAttribute('data-border-color', result.item.labelBorderColor);
                selectedElement.setAttribute('data-name-color', result.item.labelNameColor);
                selectedElement.setAttribute('data-price-color', result.item.labelPriceColor);
                selectedElement.setAttribute('data-barcode-color', result.item.labelBarcodeColor);
                selectedElement.setAttribute('data-font-size', result.item.labelFontSize);
                selectedElement.setAttribute('data-font-family', result.item.labelFontFamily);
                selectedElement.setAttribute('data-price-decimals', result.item.labelPriceDecimals);
                selectedElement.setAttribute('data-thousand-separator', result.item.labelThousandSeparator);
                selectedElement.setAttribute('data-currency-symbol', result.item.labelCurrencySymbol);
                selectedElement.setAttribute('data-print-currency', result.item.printCurrencySymbol.toString().toLowerCase());

                // Actualizar el texto visible en la lista
                var priceSpan = selectedElement.querySelector('.badge');
                if (priceSpan) {
                    priceSpan.textContent = '₡' + result.item.labelPrice.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
                }

                var labelSmall = selectedElement.querySelector('small.text-success');
                if (labelSmall && result.item.labelItem) {
                    labelSmall.innerHTML = '<i class="bi bi-tag me-1"></i>' + result.item.labelItem;
                }
            }

            alert('Etiqueta guardada exitosamente');
        } else {
            alert('Error: ' + result.message);
        }
    })
    .catch(function(error) {
        console.error('Error:', error);
        alert('Error al guardar la etiqueta');
    });
}

function resetLabel() {
    setElementValue('labelItem', originalData.labelItem || '');
    setElementValue('labelPrice', originalData.labelPrice || 0);
    setElementValue('labelBarcode', originalData.labelBarcode || '');
    setElementChecked('printLabelName', originalData.printLabelName !== false);
    setElementChecked('printLabelPrice', originalData.printLabelPrice !== false);
    setElementChecked('printLabelBarcode', originalData.printLabelBarcode !== false);
    setElementValue('labelWidthCm', originalData.labelWidthCm || 4);
    setElementValue('labelHeightCm', originalData.labelHeightCm || 2);
    setElementValue('labelOrientation', originalData.labelOrientation || 'horizontal');
    setElementChecked('printLabelBorder', originalData.printLabelBorder !== false);
    setElementValue('labelBorderColor', originalData.labelBorderColor || '#000000');
    setElementValue('labelNameColor', originalData.labelNameColor || '#000000');
    setElementValue('labelPriceColor', originalData.labelPriceColor || '#16a34a');
    setElementValue('labelBarcodeColor', originalData.labelBarcodeColor || '#000000');
    setElementValue('labelFontSize', originalData.labelFontSize || 14);
    setElementValue('labelFontFamily', originalData.labelFontFamily || 'Arial');
    setElementValue('labelPriceDecimals', originalData.labelPriceDecimals !== undefined ? originalData.labelPriceDecimals : 2);
    setElementValue('labelThousandSeparator', originalData.labelThousandSeparator || ',');
    setElementValue('labelCurrencySymbol', originalData.labelCurrencySymbol || '₡');
    setElementChecked('printCurrencySymbol', originalData.printCurrencySymbol !== false);
    updatePreview();
}

function printLabel() {
    var itemId = getElementValue('selectedItemId', '');
    if (!itemId) {
        alert('Por favor seleccione un artículo primero');
        return;
    }

    var itemCode = getElementText('itemCode', '');
    var itemName = getElementText('itemName', '');
    var name = getElementValue('labelItem', '');
    var price = parseFloat(getElementValue('labelPrice', 0)) || 0;
    var barcode = getElementValue('labelBarcode', '');
    var showName = getElementChecked('printLabelName', true);
    var showPrice = getElementChecked('printLabelPrice', true);
    var showBarcode = getElementChecked('printLabelBarcode', true);
    var showBorder = getElementChecked('printLabelBorder', true);
    var printCurrency = getElementChecked('printCurrencySymbol', true);

    var widthCm = parseFloat(getElementValue('labelWidthCm', 4)) || 4;
    var heightCm = parseFloat(getElementValue('labelHeightCm', 2)) || 2;
    var orientation = getElementValue('labelOrientation', 'horizontal');
    var borderColor = getElementValue('labelBorderColor', '#000000');
    var nameColor = getElementValue('labelNameColor', '#000000');
    var priceColor = getElementValue('labelPriceColor', '#16a34a');
    var barcodeColor = getElementValue('labelBarcodeColor', '#000000');

    // Campos de fuente y formato
    var fontSize = parseFloat(getElementValue('labelFontSize', 14)) || 14;
    var fontFamily = getElementValue('labelFontFamily', 'Arial');

    // Obtener decimales correctamente (puede ser 0)
    var decimalsEl = document.getElementById('labelPriceDecimals');
    var priceDecimals = decimalsEl ? parseInt(decimalsEl.value) : 2;
    if (isNaN(priceDecimals)) priceDecimals = 2;

    var thousandSeparator = getElementValue('labelThousandSeparator', ',');
    var currencySymbol = getElementValue('labelCurrencySymbol', '₡');

    // Ajustar dimensiones según orientación
    var width = orientation === 'horizontal' ? widthCm : heightCm;
    var height = orientation === 'horizontal' ? heightCm : widthCm;

    // Formatear precio
    var formattedPrice = formatPrice(price, priceDecimals, thousandSeparator, currencySymbol, printCurrency);

    // Registrar la impresión en el historial
    recordPrint({
        itemId: parseInt(itemId),
        itemCode: itemCode,
        itemName: itemName,
        labelItem: name,
        labelPrice: price,
        labelItemBarcode: barcode,
        printLabelName: showName,
        printLabelPrice: showPrice,
        printLabelBarcode: showBarcode,
        printLabelBorder: showBorder,
        printCurrencySymbol: printCurrency,
        labelWidthCm: widthCm,
        labelHeightCm: heightCm,
        labelOrientation: orientation,
        labelBorderColor: borderColor,
        labelNameColor: nameColor,
        labelPriceColor: priceColor,
        labelBarcodeColor: barcodeColor,
        labelFontSize: fontSize,
        labelFontFamily: fontFamily,
        labelPriceDecimals: priceDecimals,
        labelThousandSeparator: thousandSeparator,
        labelCurrencySymbol: currencySymbol,
        formattedPrice: formattedPrice,
        quantityPrinted: 1
    });

    // Abrir ventana de impresión
    var printWindow = window.open('', '_blank');
    var html = '<!DOCTYPE html><html><head><title>Imprimir Etiqueta</title>' +
        '<style>' +
        '@page { size: ' + width + 'cm ' + height + 'cm; margin: 0; }' +
        'body { font-family: ' + fontFamily + ', sans-serif; margin: 0; padding: 0; display: flex; justify-content: center; align-items: center; min-height: 100vh; }' +
        '.label { width: ' + width + 'cm; height: ' + height + 'cm; ' + (showBorder ? 'border: 2px solid ' + borderColor + ';' : '') + ' padding: 5px; text-align: center; box-sizing: border-box; display: flex; flex-direction: column; justify-content: center; }' +
        '.name { font-size: ' + fontSize + 'px; font-weight: bold; margin-bottom: 3px; color: ' + nameColor + '; font-family: ' + fontFamily + '; }' +
        '.price { font-size: ' + (fontSize * 1.4) + 'px; font-weight: bold; color: ' + priceColor + '; margin-bottom: 3px; font-family: ' + fontFamily + '; }' +
        '.barcode { font-family: monospace; font-size: ' + (fontSize * 0.7) + 'px; color: ' + barcodeColor + '; }' +
        '</style>' +
        '</head><body>' +
        '<div class="label">' +
        (showName ? '<div class="name">' + name + '</div>' : '') +
        (showPrice ? '<div class="price">' + formattedPrice + '</div>' : '') +
        (showBarcode ? '<div class="barcode">' + barcode + '</div>' : '') +
        '</div>' +
        '<scr' + 'ipt>window.print(); window.close();</scr' + 'ipt>' +
        '</body></html>';
    printWindow.document.write(html);
    printWindow.document.close();
}

// Función para registrar la impresión en el historial
function recordPrint(data) {
    fetch('/Inventory/LabelItems/RecordPrint', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    })
    .then(function(response) { return response.json(); })
    .then(function(result) {
        if (result.success) {
            console.log('Impresión registrada en historial');
        } else {
            console.error('Error registrando impresión:', result.message);
        }
    })
    .catch(function(error) {
        console.error('Error en recordPrint:', error);
    });
}

// Función auxiliar para obtener texto de un elemento
function getElementText(id, defaultValue) {
    var el = document.getElementById(id);
    return el ? (el.textContent || el.innerText || defaultValue) : defaultValue;
}

// Inicializar event listeners cuando el DOM esté listo
document.addEventListener('DOMContentLoaded', function() {
    // Campos de texto
    var textFields = ['labelItem', 'labelPrice', 'labelBarcode', 'labelWidthCm', 'labelHeightCm', 'labelFontSize'];
    textFields.forEach(function(id) {
        var el = document.getElementById(id);
        if (el) el.addEventListener('input', updatePreview);
    });

    // Checkboxes
    var checkboxes = ['printLabelName', 'printLabelPrice', 'printLabelBarcode', 'printLabelBorder', 'printCurrencySymbol'];
    checkboxes.forEach(function(id) {
        var el = document.getElementById(id);
        if (el) el.addEventListener('change', updatePreview);
    });

    // Selects
    var selects = ['labelOrientation', 'labelFontFamily', 'labelPriceDecimals', 'labelThousandSeparator', 'labelCurrencySymbol'];
    selects.forEach(function(id) {
        var el = document.getElementById(id);
        if (el) el.addEventListener('change', updatePreview);
    });

    // Color pickers
    var colorPickers = ['labelBorderColor', 'labelNameColor', 'labelPriceColor', 'labelBarcodeColor'];
    colorPickers.forEach(function(id) {
        var el = document.getElementById(id);
        if (el) el.addEventListener('input', updatePreview);
    });

    console.log('Label Items JS initialized');
});
