// Upload form behaviour: file status, drag highlight, period presets and date validation.
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('analyzeForm');
    if (!form) return;

    const fileInput = document.getElementById('file');
    const fileStatus = document.getElementById('fileStatus');
    const dropzone = document.getElementById('dropzone');
    const analyzeBtn = document.getElementById('analyzeBtn');
    const dateFrom = document.getElementById('dateFrom');
    const dateTo = document.getElementById('dateTo');
    const customDates = document.getElementById('customDates');
    const dateHint = document.getElementById('dateHint');
    const presets = form.querySelectorAll('[data-period]');

    function toInputValue(date) {
        const offset = date.getTimezoneOffset() * 60000;
        return new Date(date.getTime() - offset).toISOString().slice(0, 10);
    }

    function showFile() {
        const file = fileInput.files[0];
        if (file) {
            const sizeMb = (file.size / 1024 / 1024).toFixed(2);
            fileStatus.textContent = `${file.name} · ${sizeMb} MB`;
            dropzone.classList.add('has-file');
        } else {
            fileStatus.textContent = 'No file selected';
            dropzone.classList.remove('has-file');
        }
        dropzone.classList.remove('is-invalid');
    }

    fileInput.addEventListener('change', showFile);
    ['dragenter', 'dragover'].forEach(e => fileInput.addEventListener(e, () => dropzone.classList.add('is-dragging')));
    ['dragleave', 'drop'].forEach(e => fileInput.addEventListener(e, () => dropzone.classList.remove('is-dragging')));

    function selectPreset(period) {
        presets.forEach(b => b.setAttribute('aria-pressed', String(b.dataset.period === period)));
        customDates.hidden = period !== 'custom';

        const today = new Date();
        if (period === '30d') {
            const from = new Date(today);
            from.setDate(today.getDate() - 30);
            dateFrom.value = toInputValue(from);
            dateTo.value = toInputValue(today);
        } else if (period === 'year') {
            dateFrom.value = `${today.getFullYear()}-01-01`;
            dateTo.value = toInputValue(today);
        } else if (period === 'all') {
            dateFrom.value = '';
            dateTo.value = '';
        }
        validateDates();
    }

    presets.forEach(b => b.addEventListener('click', () => selectPreset(b.dataset.period)));

    function validateDates() {
        const invalid = dateFrom.value && dateTo.value && dateFrom.value > dateTo.value;
        dateFrom.setCustomValidity(invalid ? 'From date must be before To date' : '');
        dateTo.setCustomValidity(invalid ? 'To date must be after From date' : '');
        dateHint.classList.toggle('is-error', Boolean(invalid));
        return !invalid;
    }

    dateFrom.addEventListener('change', validateDates);
    dateTo.addEventListener('change', validateDates);

    form.addEventListener('submit', function (event) {
        const hasFile = fileInput.files.length > 0;
        const datesOk = validateDates();

        if (!hasFile || !datesOk) {
            event.preventDefault();
            if (!hasFile) {
                dropzone.classList.add('is-invalid');
                fileStatus.textContent = 'Please choose a CSV file first';
                fileInput.focus();
            } else {
                customDates.hidden = false;
                dateFrom.focus();
            }
            return;
        }

        analyzeBtn.disabled = true;
        analyzeBtn.textContent = 'Analyzing…';
    });
});
