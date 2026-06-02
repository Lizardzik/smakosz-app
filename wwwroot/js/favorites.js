// Usuwanie przepisu z ulubionych i odświeżenie widoku
document.querySelectorAll('.favorite-btn').forEach(btn => {
    btn.addEventListener('click', async function (e) {
        const recipeId = this.dataset.id;
        const response = await fetch(`/Recipe/ToggleFavorite?recipeId=${recipeId}`, {
            method: 'POST'
        });

        if (response.ok) {
            location.reload();
        }
    });
});