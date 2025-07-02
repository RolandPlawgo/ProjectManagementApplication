﻿async function openModal(url, modalId) {
    const modalEl = document.getElementById(modalId);
    const contentEl = modalEl.querySelector('.modal-content');
    const response = await fetch(url);
    const html = await response.text();

    contentEl.innerHTML = html;
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
            const { success } = await res.json();
            if (success) {
                modal.hide();
                location.reload();
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
        const res = await fetch(url, { method: 'POST' });
        if (res.ok) {
            const json = await res.json();
            if (json.success) {
                modal.hide();
                location.reload();
            } else {
                alert(json.error || 'Delete failed.');
            }
        } else {
            alert('Server error deleting item.');
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


const commentForm = contentEl.querySelector('#addCommentForm');
if (commentForm) bindCommentForm(commentForm);


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
