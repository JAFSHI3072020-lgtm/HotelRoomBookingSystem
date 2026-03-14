// wwwroot/js/datepicker.js
import HotelDatepicker from 'hotel-datepicker';
import 'hotel-datepicker/dist/css/hotel-datepicker.css';

document.addEventListener('DOMContentLoaded', function () {
    const checkInInput = document.getElementById('checkIn');
    const checkOutInput = document.getElementById('checkOut');

    // Configure hotel datepicker [citation:2][citation:6]
    const datepicker = new HotelDatepicker(checkInInput, {
        format: 'YYYY-MM-DD',
        startOfWeek: 'sunday',
        minNights: 1,
        maxNights: 30,
        selectForward: true,
        disabledDates: [], // Load from backend API
        disabledDaysOfWeek: [], // e.g., ['Saturday', 'Sunday']
        noCheckInDates: [], // Specific dates where check-in not allowed
        noCheckOutDates: [], // Specific dates where check-out not allowed
        enableCheckout: false,
        autoClose: true,
        showTopbar: true,
        clearButton: true,
        submitButton: false,
        i18n: {
            selected: 'Your stay:',
            night: 'Night',
            nights: 'Nights',
            button: 'Close',
            clearButton: 'Clear',
            'checkin-disabled': 'Check-in disabled',
            'checkout-disabled': 'Check-out disabled',
            'day-names-short': ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
            'day-names': ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
            'month-names-short': ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
            'month-names': ['January', 'February', 'March', 'April', 'May', 'June', 'July', 'August', 'September', 'October', 'November', 'December']
        },
        onSelectRange: function () {
            // Calculate number of nights and update price
            const nights = this.getNights();
            const pricePerNight = parseFloat(document.getElementById('pricePerNight').value);
            const totalAmount = nights * pricePerNight;

            document.getElementById('nights').value = nights;
            document.getElementById('totalAmount').value = totalAmount.toFixed(2);
            document.getElementById('advanceAmount').value = (totalAmount * 0.5).toFixed(2);

            // Update display
            document.getElementById('nightsDisplay').textContent = nights;
            document.getElementById('totalDisplay').textContent = '$' + totalAmount.toFixed(2);
            document.getElementById('advanceDisplay').textContent = '$' + (totalAmount * 0.5).toFixed(2);
        }
    });

    // Load disabled dates from API
    fetch('/api/booking/disabled-dates')
        .then(response => response.json())
        .then(data => {
            datepicker.update({
                disabledDates: data.disabledDates,
                noCheckInDates: data.noCheckInDates,
                noCheckOutDates: data.noCheckOutDates
            });
        });
});