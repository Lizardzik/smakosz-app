// Dodawanie nowego pola na kolejny składnik
document.getElementById('addIngredientBtn').addEventListener('click', function () {
    const list = document.getElementById('ingredientsList');
    const input = document.createElement('input');
    input.type = 'text';
    input.name = 'IngredientsList';
    input.className = 'ingredient-input';
    input.placeholder = 'kolejny składnik';
    input.style.marginTop = '10px';
    list.appendChild(input);
});

const fileInput = document.querySelector('input[name="ImageFile"]');
const uploadBox = document.querySelector('.image-upload-box');

// Wyświetlanie podglądu wgranego zdjęcia
if (fileInput) {
    fileInput.addEventListener('change', function () {
        if (this.files && this.files[0]) {
            const reader = new FileReader();
            reader.onload = function (e) {
                uploadBox.style.backgroundImage = `url('${e.target.result}')`;
                uploadBox.style.backgroundSize = 'cover';
                uploadBox.style.backgroundPosition = 'center';
                uploadBox.querySelector('.plus-icon').style.display = 'none';
                uploadBox.querySelector('span:last-child').style.display = 'none';
            }
            reader.readAsDataURL(this.files[0]);
        }
    });
}

const mainSelect = document.getElementById('mainCategorySelect');
const subSelect = document.getElementById('subCategorySelect');

const subCategoriesData = {
    'śniadania': ['na słodko', 'na słono'],
    'obiady': ['zupy', 'makarony', 'steki', 'sałatki', 'burgery'],
    'desery': ['ciasta', 'na zimno', 'puddingi', 'mrożone']
};

// Dynamiczne wczytywanie podkategorii po zmianie kategorii głównej
if (mainSelect) {
    mainSelect.addEventListener('change', function () {
        const selectedMain = this.value;
        const options = subCategoriesData[selectedMain] || [];

        subSelect.innerHTML = '<option value="" disabled selected>Podkategoria</option>';

        if (options.length > 0) {
            subSelect.disabled = false;
            options.forEach(opt => {
                const optionElement = document.createElement('option');
                optionElement.value = opt;
                optionElement.textContent = opt;
                subSelect.appendChild(optionElement);
            });
        } else {
            subSelect.disabled = true;
        }
    });
}