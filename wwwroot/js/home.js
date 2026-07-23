document.addEventListener('DOMContentLoaded', function () {

    const container = document.getElementById('dynamicContentContainer');
    const hiddenCategory = document.getElementById('hiddenCategory');
    const hiddenCuisine = document.getElementById('hiddenCuisine');
    const hiddenPage = document.getElementById('hiddenPage');
    const searchInput = document.getElementById('searchInput');

    async function fetchFilteredRecipes(pageVal = 1) {
        if (hiddenPage) hiddenPage.value = pageVal;

        if (container) container.classList.add('fade-out');

        const cat = hiddenCategory ? hiddenCategory.value : '';
        const cuis = hiddenCuisine ? hiddenCuisine.value : '';
        const search = searchInput ? searchInput.value : '';

        const url = `/Home/Index?category=${encodeURIComponent(cat)}&cuisine=${encodeURIComponent(cuis)}&search=${encodeURIComponent(search)}&page=${pageVal}`;

        try {
            const response = await fetch(url, {
                headers: {
                    'X-Requested-With': 'XMLHttpRequest'
                }
            });

            if (response.ok) {
                const html = await response.text();

                if (container) {
                    container.innerHTML = html;

                    container.classList.remove('fade-out');
                    container.classList.add('fade-in');
                    setTimeout(() => container.classList.remove('fade-in'), 300);

                    bindEvents();
                }
            }
        } catch (err) {
            console.error("Error loading recipes:", err);
            if (container) container.classList.remove('fade-out');
        }
    }

    function bindEvents() {
        // Paginacja
        document.querySelectorAll('.ajax-page-btn').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                const targetPage = this.dataset.page;
                fetchFilteredRecipes(targetPage);
            });
        });

        // Usuwanie pojedynczego filtra (przycisk X)
        document.querySelectorAll('.remove-filter-btn').forEach(btn => {
            btn.addEventListener('click', function (e) {
                e.preventDefault();
                const filterType = this.dataset.filter;

                if (filterType === 'category') {
                    if (hiddenCategory) hiddenCategory.value = '';
                    const catRadio = document.querySelector('input[name="MainCategory"]:checked');
                    if (catRadio) catRadio.checked = false;
                    document.getElementById('catBtn').textContent = 'Category';
                } else if (filterType === 'cuisine') {
                    if (hiddenCuisine) hiddenCuisine.value = '';
                    const cuisRadio = document.querySelector('input[name="CuisineType"]:checked');
                    if (cuisRadio) cuisRadio.checked = false;
                    document.getElementById('cuisBtn').textContent = 'Cuisine';
                } else if (filterType === 'search') {
                    if (searchInput) searchInput.value = '';
                }

                fetchFilteredRecipes(1);
            });
        });

        // Skok do strony
        const pageJumpInput = document.getElementById('pageJumpInput');
        if (pageJumpInput) {
            pageJumpInput.addEventListener('keydown', function (e) {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    let pageVal = parseInt(this.value);
                    let maxPages = parseInt(this.getAttribute('max'));
                    if (isNaN(pageVal) || pageVal < 1) pageVal = 1;
                    if (pageVal > maxPages) pageVal = maxPages;

                    fetchFilteredRecipes(pageVal);
                }
            });
        }
        // Obsługa dynamicznego oceniania gwiazdkami
        document.querySelectorAll('.star-rating-box').forEach(box => {
            const stars = box.querySelectorAll('.star');
            const recipeId = box.dataset.id;

            stars.forEach(star => {
                star.addEventListener('mouseenter', function () {
                    const val = parseInt(this.dataset.value);
                    stars.forEach(s => {
                        if (parseInt(s.dataset.value) <= val) {
                            s.classList.add('hovered');
                        } else {
                            s.classList.remove('hovered');
                        }
                    });
                });

                star.addEventListener('click', async function (e) {
                    e.preventDefault();
                    const score = parseInt(this.dataset.value);

                    try {
                        const response = await fetch(`/Recipe/RateRecipe?recipeId=${recipeId}&score=${score}`, {
                            method: 'POST'
                        });

                        if (response.ok) {
                            const data = await response.json();
                            if (data.success) {
                                // Aktualizacja gwiazdek
                                stars.forEach(s => {
                                    if (parseInt(s.dataset.value) <= Math.round(data.average)) {
                                        s.classList.add('filled');
                                    } else {
                                        s.classList.remove('filled');
                                    }
                                });

                                // Aktualizacja opisu tekstowego
                                const valSpan = box.querySelector('.rating-value');
                                if (valSpan) {
                                    const voteText = data.votes === 1 ? 'vote' : 'votes';
                                    valSpan.innerHTML = `<strong>${data.average.toFixed(1)}</strong> (${data.votes} ${voteText})`;
                                }
                            }
                        }
                    } catch (err) {
                        console.error("Error rating recipe:", err);
                    }
                });
            });

            box.addEventListener('mouseleave', function () {
                stars.forEach(s => s.classList.remove('hovered'));
            });
        });

        // Ulubione - przełączanie klasy bez niszczenia SVG
        document.querySelectorAll('.favorite-btn').forEach(btn => {
            btn.addEventListener('click', async function (e) {
                e.preventDefault();
                e.stopPropagation();

                const recipeId = this.dataset.id;
                const title = this.dataset.title || '';
                const thumb = this.dataset.thumb || '';
                const category = this.dataset.category || '';
                const area = this.dataset.area || '';

                try {
                    const response = await fetch(`/Recipe/ToggleFavorite?recipeId=${recipeId}&title=${encodeURIComponent(title)}&thumb=${encodeURIComponent(thumb)}&category=${encodeURIComponent(category)}&area=${encodeURIComponent(area)}`, {
                        method: 'POST'
                    });

                    if (response.ok) {
                        const data = await response.json();

                        // Tylko dodajemy lub usuwamy klasę active (CSS zajmuje się zmianą koloru SVG)
                        if (data.isFavorite) {
                            this.classList.add('active');
                        } else {
                            this.classList.remove('active');
                        }
                    }
                } catch (err) {
                    console.error("Error toggling favorite:", err);
                }
            });
        });
    }

    // Dropdowny
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

    document.addEventListener('click', function () {
        document.querySelectorAll('.custom-dropdown-content').forEach(menu => {
            menu.classList.remove('show');
        });
    });

    document.querySelectorAll('.custom-dropdown-content').forEach(menu => {
        menu.addEventListener('click', function (e) {
            e.stopPropagation();
        });
    });

    // Filtry Radio Button
    document.querySelectorAll('input[name="MainCategory"]').forEach(input => {
        input.addEventListener('change', function () {
            if (hiddenCategory) hiddenCategory.value = this.value;
            const formattedVal = this.value.charAt(0).toUpperCase() + this.value.slice(1);
            document.getElementById('catBtn').textContent = formattedVal;
            document.querySelectorAll('.custom-dropdown-content').forEach(m => m.classList.remove('show'));
            fetchFilteredRecipes(1);
        });
    });

    document.querySelectorAll('input[name="CuisineType"]').forEach(input => {
        input.addEventListener('change', function () {
            if (hiddenCuisine) hiddenCuisine.value = this.value;
            const formattedVal = this.value.charAt(0).toUpperCase() + this.value.slice(1);
            document.getElementById('cuisBtn').textContent = formattedVal;
            document.querySelectorAll('.custom-dropdown-content').forEach(m => m.classList.remove('show'));
            fetchFilteredRecipes(1);
        });
    });

    bindEvents();
});