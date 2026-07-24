document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.delete-custom-btn').forEach(btn => {
        btn.addEventListener('click', async function () {
            if (!confirm("Are you sure you want to delete this customized recipe?")) return;

            const id = this.dataset.id;
            try {
                const response = await fetch(`/Recipe/DeleteCustomRecipe?id=${id}`, { method: 'POST' });
                if (response.ok) {
                    location.reload();
                }
            } catch (err) {
                console.error(err);
            }
        });
    });
});