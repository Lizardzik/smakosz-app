function showForm(formType) {
    const loginForm = document.getElementById('loginForm');
    const registerForm = document.getElementById('registerForm');
    const tabLogin = document.getElementById('tabLogin');
    const tabRegister = document.getElementById('tabRegister');

    if (formType === 'register') {
        if (loginForm) loginForm.classList.add('d-none');
        if (registerForm) registerForm.classList.remove('d-none');
        if (tabLogin) tabLogin.classList.remove('active');
        if (tabRegister) tabRegister.classList.add('active');
    } else {
        if (registerForm) registerForm.classList.add('d-none');
        if (loginForm) loginForm.classList.remove('d-none');
        if (tabRegister) tabRegister.classList.remove('active');
        if (tabLogin) tabLogin.classList.add('active');
    }
}

// Zastępuje starą funkcję toggleForms dla wstecznej kompatybilności
function toggleForms() {
    const registerForm = document.getElementById('registerForm');
    if (registerForm && registerForm.classList.contains('d-none')) {
        showForm('register');
    } else {
        showForm('login');
    }
}

document.addEventListener('DOMContentLoaded', function () {
    // Automatyczne otwarcie rejestracji po wejściu z linku "/Account/Index?show=register"
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('show') === 'register') {
        showForm('register');
    }
});
document.body.classList.remove('dark-theme');