/*Перемикач для типу доставки*/
$(document).ready(function () {
    $('.delivery-method').change(function () {
        if ($(this).val() === 'Доставка Новою Поштою') {
            $('.delivery-details').removeClass('d-none');
        }
        else {
            $('.delivery-details').addClass('d-none');
            $('#regionSelect').val("");
            $('#citySelect').val("");
            $('#officeSelect').val("");
            $('#regionSelect').innerHTML("");
            $('#citySelect').innerHTML("");
            $('#officeSelect').innerHTML("");
        }
    }).trigger('change');

});

$(document).ready(function () {
    var regionSelect = $("#regionSelect");
    var citySelect = $("#citySelect");
    var officeSelect = $("#officeSelect");
    var apiKey = "e2cf44cd7f5c2cbc67892f3427d71234";
    // Завантаження областей та міст
    $.ajax({
        url: "https://api.novaposhta.ua/v2.0/json/Address/getAreas",
        type: "POST",
        dataType: "json",
        data: JSON.stringify({
            "apiKey": apiKey,
            "modelName": "Address",
            "calledMethod": "getAreas"
        }),
        success: function (data) {
            var areas = data.data;

            regionSelect.empty();
            regionSelect.append($('<option>', {
                value: "--Оберіть область--",
                text: "--Оберіть область--"
            }));

            for (var i = 0; i < areas.length; i++) {
                var option = $('<option>', {
                    value: areas[i].Description,
                    text: areas[i].Description
                });
                regionSelect.append(option);
            }
        }
    });

    // Завантаження міст для вибраної області
    regionSelect.on("change", function () {
        var selectedRegion = regionSelect.val();

        $.ajax({
            url: "https://api.novaposhta.ua/v2.0/json/Address/getCities",
            type: "POST",
            dataType: "json",
            data: JSON.stringify({
                "apiKey": apiKey,
                "modelName": "Address",
                "calledMethod": "getCities",
                "methodProperties": {
                    "Area": selectedRegion
                }
            }),
            success: function (data) {
                var cities = data.data;

                citySelect.empty();
                citySelect.append($('<option>', {
                    value: "--Оберіть місто--",
                    text: "--Оберіть місто--"
                }));

                for (var i = 0; i < cities.length; i++) {
                    var option = $('<option>', {
                        value: cities[i].Description,
                        text: cities[i].Description
                    });
                    citySelect.append(option);
                }
            }
        });
    });

    // Завантаження відділень для вибраного міста
    citySelect.on("change", function () {
        var selectedCity = citySelect.val();

        $.ajax({
            url: "https://api.novaposhta.ua/v2.0/json/Address/getWarehouses",
            type: "POST",
            dataType: "json",
            data: JSON.stringify({
                "apiKey": apiKey,
                "modelName": "Address",
                "calledMethod": "getWarehouses",
                "methodProperties": {
                    "CityName": selectedCity
                }
            }),
            success: function (data) {
                var offices = data.data;

                officeSelect.empty();
                officeSelect.append($('<option>', {
                    value: "--Оберіть відділення--",
                    text: "--Оберіть відділення--"
                }));

                for (var i = 0; i < offices.length; i++) {
                    var option = $('<option>', {
                        value: offices[i].Description,
                        text: offices[i].Description
                    });
                    officeSelect.append(option);
                }
            }
        });
    });
});