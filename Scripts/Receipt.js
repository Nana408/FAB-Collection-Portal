$('#printInvoice').click(function () {
    Popup($('#invoice-POS')[0].outerHTML);
    function Popup(data) {
        window.print();
        return true;
    }
});