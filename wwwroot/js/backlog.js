// backlog.js

function openStoryModal(epicId) {
    fetch(`/Backlog/CreateStory?epicId=${epicId}`)
        .then(res => res.text())
        .then(html => {
            const modalEl = document.getElementById('storyModal');
            document.getElementById('storyModalContent').innerHTML = html;
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
            bindCreateStoryForm(modal);
        });
}

function bindCreateStoryForm(modal) {
    const form = document.getElementById('createStoryForm');
    form.addEventListener('submit', async e => {
        e.preventDefault();
        const res = await fetch('/Backlog/CreateStory', {
            method: 'POST',
            body: new FormData(form)
        });
        if (res.headers.get('content-type')?.includes('application/json')) {
            const json = await res.json();
            if (json.success) {
                modal.hide();
                location.reload();
                return;
            }
        }
        const html = await res.text();
        document.getElementById('storyModalContent').innerHTML = html;
        bindCreateStoryForm(modal);
    }, { once: true });
}

function openEditStoryModal(storyId) {
    fetch(`/Backlog/EditStory?id=${storyId}`)
        .then(res => res.text())
        .then(html => {
            const modalEl = document.getElementById('storyModal');
            document.getElementById('storyModalContent').innerHTML = html;
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
            bindEditStoryForm(modal);
        });
}

function bindEditStoryForm(modal) {
    const form = document.getElementById('editStoryForm');

    form.addEventListener('submit', async e => {
        e.preventDefault();
        const res = await fetch('/Backlog/EditStory', {
            method: 'POST',
            body: new FormData(form)
        });
        if (res.headers.get('content-type')?.includes('application/json')) {
            modal.hide();
            location.reload();
        } else {
            const html = await res.text();
            document.getElementById('storyModalContent').innerHTML = html;
            bindEditStoryForm(modal);
        }
    }, { once: true });

    document.getElementById('deleteStoryBtn')
        .addEventListener('click', async () => {
            const id = form.querySelector('input[name="Id"]').value;
            const formData = new FormData();
            formData.append('id', id);

            const res = await fetch('/Backlog/DeleteStory', {
                method: 'POST',
                body: formData
            });
            const json = await res.json();
            if (json.success) {
                modal.hide();
                location.reload();
            } else {
                alert('Could not delete story.');
            }
        }, { once: true });
}

(function () {
    const storageKey = 'openEpics';

    document.addEventListener('DOMContentLoaded', () => {
        const openList = JSON.parse(localStorage.getItem(storageKey) || '[]');
        openList.forEach(id => {
            const el = document.getElementById(id);
            if (el) new bootstrap.Collapse(el, { toggle: false }).show();
        });

        document.querySelectorAll('.accordion-collapse').forEach(panel => {
            panel.addEventListener('shown.bs.collapse', () => {
                let list = JSON.parse(localStorage.getItem(storageKey) || '[]');
                if (!list.includes(panel.id)) {
                    list.push(panel.id);
                    localStorage.setItem(storageKey, JSON.stringify(list));
                }
            });
            panel.addEventListener('hidden.bs.collapse', () => {
                let list = JSON.parse(localStorage.getItem(storageKey) || '[]');
                list = list.filter(x => x !== panel.id);
                localStorage.setItem(storageKey, JSON.stringify(list));
            });
        });
    });
})();







function openEpicModal(projectId) {
    fetch(`/Backlog/CreateEpic?projectId=${projectId}`)
        .then(res => res.text())
        .then(html => {
            const modalEl = document.getElementById('epicModal');
            document.getElementById('epicModalContent').innerHTML = html;
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
            bindCreateEpicForm(modal);
        });
}

function bindCreateEpicForm(modal) {
    const form = document.getElementById('createEpicForm');
    form.addEventListener('submit', async e => {
        e.preventDefault();
        const res = await fetch('/Backlog/CreateEpic', {
            method: 'POST',
            body: new FormData(form)
        });
        if (res.headers.get('content-type')?.includes('application/json')) {
            const json = await res.json();
            if (json.success) {
                modal.hide();
                location.reload();
                return;
            }
        }
        const html = await res.text();
        document.getElementById('epicModalContent').innerHTML = html;
        bindCreateEpicForm(modal);
    }, { once: true });
}

window.openEpicModal = openEpicModal;






function openEditEpicModal(epicId) {
    fetch(`/Backlog/EditEpic?id=${epicId}`)
        .then(res => res.text())
        .then(html => {
            const modalEl = document.getElementById('epicModal');
            document.getElementById('epicModalContent').innerHTML = html;
            const modal = new bootstrap.Modal(modalEl);
            modal.show();
            bindEditEpicForm(modal);
        });
}

function bindEditEpicForm(modal) {
    const form = document.getElementById('editEpicForm');

    form.addEventListener('submit', async e => {
        e.preventDefault();
        const res = await fetch('/Backlog/EditEpic', {
            method: 'POST',
            body: new FormData(form)
        });
        if (res.headers.get('content-type')?.includes('application/json')) {
            modal.hide();
            location.reload();
        } else {
            const html = await res.text();
            document.getElementById('epicModalContent').innerHTML = html;
            bindEditEpicForm(modal);
        }
    }, { once: true });

    document.getElementById('deleteEpicBtn')
        .addEventListener('click', async () => {
            const id = form.querySelector('input[name="Id"]').value;
            const formData = new FormData();
            formData.append('id', id);

            const res = await fetch('/Backlog/DeleteEpic', {
                method: 'POST',
                body: formData
            });
            const json = await res.json();
            if (json.success) {
                modal.hide();
                location.reload();
            } else {
                alert('Could not delete epic.');
            }
        }, { once: true });
}

window.openEditEpicModal = openEditEpicModal;
