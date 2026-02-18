document.body.addEventListener('click', function (e) {
    if (!(e.target instanceof Element)) return;

    const btn = e.target.closest('button[data-delete-url]');
    if (!btn) return;

    if (btn.closest('.modal-content')) return;

    e.preventDefault();
    if (!confirm('Are you sure you want to delete this user?')) return;

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

    fetch(btn.getAttribute('data-delete-url'), {
        method: 'POST',
        headers: { 'RequestVerificationToken': token }
    })
        .then(res => res.json())
        .then(json => {
            if (json.success) location.reload();
            else alert(json.error || 'Delete failed.');
        })
        .catch(err => {
            console.error(err);
            alert('Error deleting user.');
        });
});




async function openModal(url, modalId) {
    const modalEl = document.getElementById(modalId);
    const contentEl = modalEl.querySelector('.modal-content');
    const response = await fetch(url);
    const html = await response.text();

    contentEl.innerHTML = html;

    if (window.jQuery && window.jQuery.validator) {
        jQuery.validator.unobtrusive.parse(contentEl);
    }

    const modal = new bootstrap.Modal(modalEl);
    modal.show();

    const form = contentEl.querySelector('form');
    if (form) bindAjaxForm(form, modal);

    const deleteBtn = contentEl.querySelector('[data-delete-url]');
    if (deleteBtn) bindDeleteButton(deleteBtn, modal);
}

function bindAjaxForm(form, modal) {
    form.addEventListener('submit', async e => {
        e.preventDefault();
        const res = await fetch(form.action, {
            method: form.method || 'POST',
            body: new FormData(form),
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        const contentType = res.headers.get('Content-Type') || '';
        if (contentType.includes('application/json')) {
            const { success, error } = await res.json();
            if (success) {
                modal.hide();
                location.reload();
                return;
            } else {
                if (!document.getElementById("error-message")) {
                    const p = document.createElement("p");
                    p.id = "error-message";
                    p.innerHTML = error;
                    p.classList.add("text-danger");
                    p.style.textAlign = "center";
                    form.querySelector('.modal-body').appendChild(p);
                }
                const newForm = modal._element.querySelector('form');
                if (newForm) bindAjaxForm(newForm, modal);
                return;
            }
        }
        const html = await res.text();
        form.closest('.modal-content').innerHTML = html;
        const newForm = modal._element.querySelector('form');
        if (newForm) bindAjaxForm(newForm, modal);
    }, { once: true });
}


function bindDeleteButton(button, modal) {
    button.addEventListener('click', async () => {
        if (!confirm('Are you sure you want to delete this item?')) return;

        const url = button.getAttribute('data-delete-url');

        const tokenInput = modal._element.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : null;
        if (!token) {
            console.error("CSRF token not found in modal.");
            return alert("Security check failed.");
        }

        const res = await fetch(url, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': token,
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (!res.ok) {
            return alert('Server rejected the delete request.');
        }

        const json = await res.json();
        if (json.success) {
            modal.hide();
            location.reload();
        } else {
            alert(json.error || 'Delete failed.');
        }
    }, { once: true });
}





document.addEventListener('click', function (e) {
    const link = e.target.closest('a[data-modal-url]');
    if (!link) return;

    e.preventDefault();

    const url = link.getAttribute('data-modal-url');
    const modalId = link.getAttribute('data-modal-id') || 'storyModal';
    openModal(url, modalId);
});



function bindCommentForm(form) {
    form.addEventListener('submit', async e => {
        e.preventDefault();
        const res = await fetch(form.action, {
            method: form.method,
            headers: {
                'RequestVerificationToken':
                    document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: new FormData(form)
        });

        const html = await res.text();
        document.getElementById('commentsContainer').innerHTML = html;
        bindCommentForm(document.getElementById('addCommentForm'));
    });
}

try {
    const commentForm = contentEl.querySelector('#addCommentForm');
    if (commentForm) bindCommentForm(commentForm);
} catch { }

document.addEventListener('submit', function (e) {
    const form = e.target;
    if (form.id !== 'addCommentForm') return;

    e.preventDefault();
    const token = form.querySelector('input[name="__RequestVerificationToken"]').value;
    const data = new FormData(form);

    fetch(form.action, {
        method: form.method,
        headers: { 'X-Requested-With': 'XMLHttpRequest' },
        body: data
    })
        .then(res => {
            if (!res.ok) throw new Error('Network error');
            return res.text();
        })
        .then(html => {
            document.getElementById('commentsList').innerHTML = html;
            form.querySelector('textarea').value = '';
        })
        .catch(err => {
            console.error(err);
            alert('Failed to add comment.');
        });
}, false);


