//document.addEventListener("click", function (e) {
//    const toggle = e.target.closest(".nav-group-toggle");
//    if (!toggle) return;

//    const group = toggle.closest(".nav-group");
//    group.classList.toggle("open");
//});
// ============================================================
// FUNCIÓN PARA TOGGLE DEL SIDEBAR (responsive)
// ============================================================
function toggleSidebar() {
    document.querySelector('.layout-wrapper').classList.toggle('sidebar-collapsed');
}

// ============================================================
// COMPORTAMIENTO DE ACORDEÓN PARA EL MENÚ
// Solo permite un submenú abierto a la vez
// ============================================================
document.addEventListener("DOMContentLoaded", function () {

    console.log("✅ JavaScript cargado correctamente");

    // Obtener todos los toggles de grupos
    const toggles = document.querySelectorAll(".nav-group-toggle");

    console.log(`📋 Encontrados ${toggles.length} grupos de menú`);

    toggles.forEach((toggle, index) => {
        toggle.addEventListener("click", function (e) {
            e.preventDefault();
            e.stopPropagation();

            console.log(`🖱️ Clic en grupo ${index + 1}`);

            const parentGroup = this.closest(".nav-group");
            const wasOpen = parentGroup.classList.contains("open");

            console.log(`Estado anterior: ${wasOpen ? 'ABIERTO' : 'CERRADO'}`);

            // ⭐ CERRAR TODOS LOS GRUPOS PRIMERO ⭐
            document.querySelectorAll(".nav-group").forEach(group => {
                if (group !== parentGroup) {
                    group.classList.remove("open");
                    console.log("🔒 Cerrando otro grupo");
                }
            });

            // ⭐ SI NO ESTABA ABIERTO, ABRIRLO ⭐
            if (!wasOpen) {
                parentGroup.classList.add("open");
                console.log("🔓 Abriendo este grupo");
            } else {
                parentGroup.classList.remove("open");
                console.log("🔒 Cerrando este grupo");
            }
        });
    });
});