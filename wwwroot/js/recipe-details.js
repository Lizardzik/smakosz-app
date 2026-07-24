document.addEventListener('DOMContentLoaded', function () {

    const favBtn = document.querySelector('.details-favorite-btn');
    if (favBtn) {
        favBtn.addEventListener('click', async function () {
            const recipeId = this.dataset.id;
            const title = this.dataset.title;
            const thumb = this.dataset.thumb;
            const category = this.dataset.category;
            const area = this.dataset.area;

            console.log('[FAVORITE] Toggling favorite for ID:', recipeId);

            try {
                const response = await fetch(`/Recipe/ToggleFavorite?recipeId=${recipeId}&title=${encodeURIComponent(title)}&thumb=${encodeURIComponent(thumb)}&category=${encodeURIComponent(category)}&area=${encodeURIComponent(area)}`, {
                    method: 'POST'
                });

                if (response.ok) {
                    const data = await response.json();
                    console.log('[FAVORITE RESPONSE]:', data);
                    if (data.isFavorite) {
                        this.classList.add('active');
                    } else {
                        this.classList.remove('active');
                    }
                } else {
                    console.error('[FAVORITE ERROR] Status:', response.status);
                }
            } catch (err) {
                console.error('[FAVORITE EXCEPTION]:', err);
            }
        });
    }

    const stars = document.querySelectorAll('.interactive-star');
    const interactiveStarsContainer = document.querySelector('.interactive-stars');
    const recipeId = interactiveStarsContainer ? interactiveStarsContainer.dataset.id : null;

    if (stars.length > 0 && recipeId) {
        stars.forEach(star => {
            star.addEventListener('mouseenter', function () {
                const val = parseInt(this.dataset.value);
                stars.forEach(s => {
                    if (parseInt(s.dataset.value) <= val) s.classList.add('hovered');
                    else s.classList.remove('hovered');
                });
            });

            star.addEventListener('click', async function () {
                const score = parseInt(this.dataset.value);
                console.log(`[RATING] Submitting score ${score} for recipe: ${recipeId}`);

                try {
                    const response = await fetch(`/Recipe/RateRecipe?recipeId=${recipeId}&score=${score}`, { method: 'POST' });
                    if (response.ok) {
                        const data = await response.json();
                        console.log('[RATING RESPONSE]:', data);
                        location.reload();
                    } else {
                        console.error('[RATING ERROR] Status:', response.status);
                    }
                } catch (err) {
                    console.error('[RATING EXCEPTION]:', err);
                }
            });
        });

        interactiveStarsContainer?.addEventListener('mouseleave', function () {
            stars.forEach(s => s.classList.remove('hovered'));
        });
    }

    const submitCommentBtn = document.getElementById('submitCommentBtn');
    if (submitCommentBtn) {
        submitCommentBtn.addEventListener('click', async function (e) {
            e.preventDefault();
            e.stopPropagation();

            const input = document.getElementById('commentInput');
            const errorBox = document.getElementById('commentErrorBox');
            const text = input ? input.value.trim() : '';
            const recipeId = this.dataset.id;

            console.log('[COMMENT] Attempting to submit comment:', { recipeId, text });

            if (errorBox) {
                errorBox.classList.add('d-none');
                errorBox.textContent = '';
            }

            if (!text) {
                console.warn('[COMMENT] Empty comment text aborted.');
                return false;
            }

            this.disabled = true;

            const formData = new FormData();
            formData.append('recipeId', recipeId);
            formData.append('text', text);

            try {
                const response = await fetch('/Recipe/AddComment', {
                    method: 'POST',
                    body: formData
                });

                console.log('[COMMENT] HTTP Response status:', response.status);
                const data = await response.json();
                console.log('[COMMENT] Server payload received:', data);

                if (data.success) {
                    console.log('[COMMENT] Moderation passed. Reloading view.');
                    location.reload();
                } else {
                    console.warn('[COMMENT REJECTED BY MODERATION]:', data.message);
                    if (errorBox) {
                        errorBox.textContent = data.message || "Your comment violates community rules.";
                        errorBox.classList.remove('d-none');
                    } else {
                        alert(data.message || "Your comment violates community rules.");
                    }
                }
            } catch (err) {
                console.error('[COMMENT EXCEPTION]:', err);
            } finally {
                this.disabled = false;
            }

            return false;
        });
    }

    document.querySelectorAll('.like-comment-btn').forEach(btn => {
        btn.addEventListener('click', async function () {
            const commentId = this.dataset.id;
            console.log('[LIKE COMMENT] Liking comment ID:', commentId);

            try {
                const response = await fetch(`/Recipe/LikeComment?commentId=${commentId}`, { method: 'POST' });
                if (response.ok) {
                    const data = await response.json();
                    console.log('[LIKE COMMENT RESPONSE]:', data);
                    this.querySelector('.like-count').textContent = data.likes;
                } else {
                    console.error('[LIKE COMMENT ERROR] Status:', response.status);
                }
            } catch (err) {
                console.error('[LIKE COMMENT EXCEPTION]:', err);
            }
        });
    });

    const saveCustomBtn = document.getElementById('saveCustomBtn');
    if (saveCustomBtn) {
        async function sendCustomRecipe(overwriteFlag) {
            const errorBox = document.getElementById('modalErrorBox');
            const overwriteNotice = document.getElementById('modalOverwriteNotice');
            const spinner = document.getElementById('saveSpinner');
            const btnText = document.getElementById('saveBtnText');
            const title = document.getElementById('customTitle').value;
            const ingredients = document.getElementById('customIngredients').value;
            const instructions = document.getElementById('customInstructions').value;

            console.log('[CUSTOM RECIPE] Submitting recipe draft:', { title, overwriteFlag });

            if (errorBox) errorBox.classList.add('d-none');
            if (overwriteNotice) overwriteNotice.classList.add('d-none');

            saveCustomBtn.disabled = true;
            if (spinner) spinner.classList.remove('d-none');
            if (btnText) btnText.textContent = 'Saving...';

            const formData = new FormData();
            formData.append('externalId', saveCustomBtn.dataset.externalid);
            formData.append('title', title);
            formData.append('ingredients', ingredients);
            formData.append('instructions', instructions);
            formData.append('category', saveCustomBtn.dataset.category);
            formData.append('area', saveCustomBtn.dataset.area);
            formData.append('thumb', saveCustomBtn.dataset.thumb);
            formData.append('overwrite', overwriteFlag);

            try {
                const response = await fetch('/Recipe/SaveCustomRecipe', {
                    method: 'POST',
                    body: formData
                });

                const data = await response.json();
                console.log('[CUSTOM RECIPE RESPONSE]:', data);

                if (data.success) {
                    alert(data.message);
                    const modalEl = document.getElementById('customizeModal');
                    const modalInstance = bootstrap.Modal.getInstance(modalEl);
                    if (modalInstance) modalInstance.hide();
                } else if (data.exists) {
                    console.warn('[CUSTOM RECIPE] Duplicate detected on server.');
                    if (overwriteNotice) {
                        document.getElementById('overwriteMsgText').textContent = data.message;
                        overwriteNotice.classList.remove('d-none');
                    }
                } else {
                    console.warn('[CUSTOM RECIPE MODERATION REJECTED]:', data.message);
                    if (errorBox) {
                        errorBox.textContent = data.message;
                        errorBox.classList.remove('d-none');
                    }
                }
            } catch (err) {
                console.error('[CUSTOM RECIPE EXCEPTION]:', err);
            } finally {
                saveCustomBtn.disabled = false;
                if (spinner) spinner.classList.add('d-none');
                if (btnText) btnText.textContent = 'Save My Version';
            }
        }

        saveCustomBtn.addEventListener('click', () => sendCustomRecipe(false));
        document.getElementById('btnConfirmOverwrite')?.addEventListener('click', () => sendCustomRecipe(true));
        document.getElementById('btnSaveAsNew')?.addEventListener('click', () => sendCustomRecipe(false));
    }
});