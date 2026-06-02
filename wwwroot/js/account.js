// Przełączanie widoczności formularzy logowania i rejestracji
function toggleForms() {
    var login = document.getElementById('loginForm');
    var register = document.getElementById('registerForm');

    if (login.classList.contains('d-none')) {
        login.classList.remove('d-none');
        register.classList.add('d-none');
    } else {
        login.classList.add('d-none');
        register.classList.remove('d-none');
    }
}

// Sprawdzanie parametru w URL i wymuszanie widoku rejestracji
document.addEventListener("DOMContentLoaded", function () {
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('show') === 'register') {
        document.getElementById('loginForm').classList.add('d-none');
        document.getElementById('registerForm').classList.remove('d-none');
    }
});