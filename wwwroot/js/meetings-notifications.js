(function () {
    const POLL_INTERVAL_MS = 60_000;
    const STORAGE_KEY = "notifiedMeetingIds";

    let lastNotifiedIds = new Set(
        JSON.parse(sessionStorage.getItem(STORAGE_KEY) || "[]")
    );

    function saveNotifiedIds() {
        sessionStorage.setItem(
            STORAGE_KEY,
            JSON.stringify([...lastNotifiedIds])
        );
    }

    async function pollUpcoming() {
        try {
            const res = await fetch(`/api/meetings/upcoming?minutes=5`);
            if (!res.ok) return;
            const meetings = await res.json();

            meetings.forEach(m => {
                if (!lastNotifiedIds.has(m.id)) {
                    showToast(m);
                    lastNotifiedIds.add(m.id);
                }
            });
            saveNotifiedIds();

            const currentIds = new Set(meetings.map(x => x.id));
            lastNotifiedIds.forEach(id => {
                if (!currentIds.has(id)) {
                    lastNotifiedIds.delete(id);
                }
            });
            saveNotifiedIds();
        }
        catch (e) {
            console.error("Error polling upcoming meetings:", e);
        }
    }

    function showToast(meeting) {
        const container = document.getElementById("toast-container");
        const tpl = document.getElementById("toast-template");
        const toastEl = tpl.cloneNode(true);
        toastEl.id = "";
        toastEl.hidden = false;

        toastEl.querySelector(".toast-project").textContent = meeting.projectName;
        toastEl.querySelector(".toast-title").textContent = meeting.name;
        toastEl.querySelector(".toast-meta").textContent = meeting.typeOfMeeting;
        toastEl.querySelector(".toast-time").textContent = meeting.time;

        toastEl.addEventListener("click", () => {
            window.location.href = `/Meetings`;
        });

        container.appendChild(toastEl);
        const toast = new bootstrap.Toast(toastEl);
        toast.show();

        toastEl.addEventListener("hidden.bs.toast", () => toastEl.remove());
    }

    document.addEventListener("DOMContentLoaded", () => {
        pollUpcoming();
        setInterval(pollUpcoming, POLL_INTERVAL_MS);
    });
})();
