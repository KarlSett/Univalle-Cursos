const menu = document.querySelector('.menu-toggle');
const overlay = document.querySelector('.sidebar-overlay');

function cerrarMenu() {
    document.body.classList.remove('sidebar-open');
    menu?.setAttribute('aria-expanded', 'false');
}

menu?.addEventListener('click', () => {
    const abierto = document.body.classList.toggle('sidebar-open');
    menu.setAttribute('aria-expanded', String(abierto));
    menu.setAttribute('aria-label', abierto ? 'Cerrar menú' : 'Abrir menú');
});
overlay?.addEventListener('click', cerrarMenu);
document.addEventListener('keydown', event => {
    if (event.key === 'Escape' && document.body.classList.contains('sidebar-open')) {
        cerrarMenu();
        menu?.focus();
    }
});

document.querySelectorAll('form[data-confirmar]').forEach(form => {
    form.addEventListener('submit', event => {
        if (!window.confirm(form.dataset.confirmar)) event.preventDefault();
    });
});

document.querySelectorAll('.password-toggle').forEach(button => {
    button.addEventListener('click', () => {
        const input = document.getElementById(button.dataset.campo);
        const mostrar = input.type === 'password';
        input.type = mostrar ? 'text' : 'password';
        button.textContent = mostrar ? 'Ocultar' : 'Mostrar';
        button.setAttribute('aria-pressed', String(mostrar));
    });
});
