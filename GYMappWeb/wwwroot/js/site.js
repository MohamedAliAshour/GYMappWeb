document.addEventListener('DOMContentLoaded', function () {
    // Force white text on all inputs
    const inputs = document.querySelectorAll('input, select, textarea');
    inputs.forEach(input => {
        input.style.color = 'white';

        // For selects, ensure the selected option shows white text
        if (input.tagName === 'SELECT') {
            input.addEventListener('change', function () {
                this.style.color = 'white';
            });
        }
    });

    // Initialize Flatpickr with consistent styling
    if (typeof flatpickr !== 'undefined') {
        flatpickr('.datepicker', {
            theme: 'dark',
            onReady: function () {
                this.input.style.color = 'white';
            }
        });
    }
});