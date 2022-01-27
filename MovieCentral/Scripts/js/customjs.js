function clearForm(element) {
    if ($(element).is('input') || $(element).is('textarea')) {
        $(element).val('');
    } else if ($(element).is('select')) {
        $(element).val('');
    }
}

