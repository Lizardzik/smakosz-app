// Obsługa rozwijanego menu filtrów
document.querySelectorAll('.custom-filter-btn').forEach(btn => {
    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        document.querySelectorAll('.custom-dropdown-content').forEach(menu => {
            if (menu !== this.nextElementSibling) {
                menu.classList.remove('show');
            }
        });
        this.nextElementSibling.classList.toggle('show');
    });
});

// Zamykanie menu po kliknięciu poza nim
document.addEventListener('click', function () {
    document.querySelectorAll('.custom-dropdown-content').forEach(menu => {
        menu.classList.remove('show');
    });
});

// Zapobieganie zamykaniu menu przy kliknięciu w jego zawartość
document.querySelectorAll('.custom-dropdown-content').forEach(menu => {
    menu.addEventListener('click', function (e) {
        e.stopPropagation();
    });
});

const activeFiltersContainer = document.getElementById('active-filters-list');
const emptyTextHTML = '<p class="empty-filters-text">Brak aktywnych filtrów</p>';
const resetBtn = document.getElementById('reset-filters-btn');
const searchInput = document.getElementById('searchInput');

// Inicjalizacja nasłuchiwania dla filtrów
document.querySelectorAll('.radio-label input').forEach(input => {
    input.addEventListener('change', updateFilters);
});

// Inicjalizacja nasłuchiwania dla wyszukiwarki
let searchTimeout;

searchInput.addEventListener('input', function () {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(function () {
        updateFilters();
    }, 400);
});

// Czyszczenie wszystkich aktywnych filtrów i pola wyszukiwania
resetBtn.addEventListener('click', function () {
    document.querySelectorAll('.radio-label input:checked').forEach(input => {
        input.checked = false;
    });
    searchInput.value = '';
    updateFilters();
});

// Aktualizacja aktywnych tagów i widoczności kart potraw
function updateFilters() {
    const checkedInputs = document.querySelectorAll('.radio-label input:checked');
    const selectedFilters = {};

    checkedInputs.forEach(input => {
        selectedFilters[input.name] = input.value;
    });

    if (checkedInputs.length === 0) {
        activeFiltersContainer.innerHTML = emptyTextHTML;
        resetBtn.style.display = 'none';
    } else {
        activeFiltersContainer.innerHTML = '';
        resetBtn.style.display = 'block';
        checkedInputs.forEach(checkedInput => {
            const span = document.createElement('span');
            span.className = 'filter-tag';
            span.textContent = checkedInput.value;
            activeFiltersContainer.appendChild(span);
        });
    }

    const searchQuery = searchInput.value.toLowerCase();
    const cards = document.querySelectorAll('.dish-item-card');

    cards.forEach(card => {
        let show = true;

        if (selectedFilters['MainCategory'] && card.dataset.main !== selectedFilters['MainCategory']) show = false;
        if (selectedFilters['SubCategory'] && card.dataset.sub !== selectedFilters['SubCategory']) show = false;
        if (selectedFilters['DietType'] && card.dataset.diet !== selectedFilters['DietType']) show = false;
        if (selectedFilters['CuisineType'] && card.dataset.cuisine !== selectedFilters['CuisineType']) show = false;

        if (searchQuery && !card.dataset.title.includes(searchQuery)) show = false;

        card.style.display = show ? 'block' : 'none';
    });
}

// Obsługa dodawania i usuwania potraw z ulubionych przez AJAX
document.querySelectorAll('.favorite-btn').forEach(btn => {
    btn.addEventListener('click', async function (e) {
        e.preventDefault();
        const recipeId = this.dataset.id;

        const response = await fetch(`/Recipe/ToggleFavorite?recipeId=${recipeId}`, {
            method: 'POST'
        });

        if (response.ok) {
            const data = await response.json();
            if (data.isFavorite) {
                this.classList.add('active');
                this.textContent = '♥';
            } else {
                this.classList.remove('active');
                this.textContent = '♡';
            }
        }
    });
});