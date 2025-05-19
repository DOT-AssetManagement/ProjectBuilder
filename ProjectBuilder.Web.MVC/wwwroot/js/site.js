
var redirectToPage = false;
var redirectToPageWithImport = false;
function isnumber(value) {
    return !isNaN(value) && value !== '' && value !== null;
}
function validateminmaxyears(form) {
    debugger
    var minsuccess = true;
    var maxsuccess = true;
    form.classList.remove('was-validated');
    var minyear = form.minyear.value;
    var maxyear = form.maxyear.value;
    var isminyearnum = isnumber(minyear);
    var ismaxyearnum = isnumber(maxyear);
    var currentYear = new Date().getFullYear();

    if (isminyearnum) {
        if (!ismaxyearnum) {
            form.maxyear.classList.add('is-invalid');
            document.getElementById('maxyearfeedback').innerText = "Field is reqiured";
            minsuccess = false;
        } else if (minyear > maxyear) {
            form.maxyear.classList.add('is-invalid');
            document.getElementById('maxyearfeedback').innerText = "Maximum year must be greater than minimum year";
            minsuccess = false;
        } else {
            form.maxyear.classList.remove('is-invalid');
            minsuccess = true;
        }
    }
    if (ismaxyearnum) {
        if (!isminyearnum) {
            form.minyear.classList.add('is-invalid');
            document.getElementById('minyearfeedback').innerText = "Field is reqiured";
            maxsuccess = false;
        } else if (minyear < currentYear) {
            form.minyear.classList.add('is-invalid');
            document.getElementById('minyearfeedback').innerText = "Minimum year must be equal to or greater than the current year";
            maxsuccess = false;
        } else {
            form.minyear.classList.remove('is-invalid');
            maxsuccess = true;
        }
    }
    return maxsuccess && minsuccess;
}
function validatelibraryform(form) {
    if (!validateemptyfields(form))
        return;
    form.classList.remove('was-validated');
    validatelibraryname(form);
}
function validateemptyfields(form) {
    if (!form.checkValidity()) {
        if (form.libraryname.value === '' || form.libraryname.value === null) {
            document.getElementById('namefeedback').innerText = "Field is required";
            document.getElementById('editnamefeedback').innerText = "Field is required";
        }
        form.classList.add('was-validated');
        return false;
    }
    return true;
}
function validatelibraryname(form) {
    var libraryname = form.libraryname.value;
    var libraryid = '';
    if (form.libraryId !== undefined) {
        libraryid = form.libraryId.value;
    }
    $.ajax({
        type: 'POST',
        dataType: 'JSON',
        url: '/CandidatePools/CheckLibraryName',
        data: { libraryname: libraryname, libraryid: libraryid },
        success:
            function (response) {
                if (response.statusCode == 200) {
                    form.libraryname.classList.remove('is-invalid');
                    form.submit();
                    form.classList.remove('was-validated');
                    resetform($(`#${form.getAttribute("id")}`));
                }
            },
        error:
            function (response) {
                form.libraryname.classList.add('is-invalid');
                document.getElementById('namefeedback').innerText = response.responseText;
                document.getElementById('namefeedback').innerText = response.responseText;
            }
    });
}
function displaynotification(header, message, state = "P", autohide = false) {
    var note = bootstrap.Toast.getOrCreateInstance(document.getElementById("mainnotificationmessage"));
    var headercontainer = note._element.querySelector(".toast-header");
    var headerelement = note._element.querySelector(".me-auto");
    var messageelement = note._element.querySelector(".toast-body");
    note._config.autohide = autohide;
    $(messageelement).html(message);
    $(headerelement).text(header);
    switch (state) {
        case "S":
            headercontainer.classList.add("bg-success");
            headercontainer.classList.remove("bg-danger");
            headercontainer.classList.remove("bg-primary");
            break;
        case "E":
            headercontainer.classList.remove("bg-success");
            headercontainer.classList.add("bg-danger");
            headercontainer.classList.remove("bg-primary");
            break;
        default:
            headercontainer.classList.remove("bg-success");
            headercontainer.classList.remove("bg-danger");
            headercontainer.classList.add("bg-primary");
            break;
    }
    note.show();
}
function hidenotification() {
    var note = bootstrap.Toast.getOrCreateInstance(document.getElementById("mainnotificationmessage"));
    note.hide();
}
async function runscenario() {
    var runbutton = document.getElementById("runbutton");
    var selectedscenario = $(runbutton).attr('current-id');
    if (!isNaN(selectedscenario) && selectedscenario !== null && selectedscenario !== '') {
        try {
            runbutton.disabled = true;
            displaynotification("Running Scenario", `running scenario with Id: ${selectedscenario} please wait`, "P", false);
            await $.ajax({ type: 'POST', dataType: 'JSON', url: '/Scenarios/RunScenario', data: { scenarioId: selectedscenario } });
            displaynotification('Running Scenario', `the scenario: ${selectedscenario} has been run successfully.`, "S", true);
            setTimeout(() => { document.location.reload(true) }, 3000);
        } catch (e) {
            if (e.status === 400) {
                displaynotification('Error', e.responseText, "E", true);
            } else {
                displaynotification('Error', 'Unexpected error occured while trying to run the selected scenario', "E", true);
            }
        } finally {
            runbutton.disabled = false;
        }
    } else {
        displaynotification('Error', 'please select a scenario first then try again', "E", true);
    }
}
function getdefaultslacks(assettype) {

    $.get(`/Info/GetDefaultSlackValues?assettype=${assettype}`, function (data, status) {
        if (status === 'success') {
            $('#maxyear').val(parseInt($('#preferredyear').val()) + data.moveAfter);
            $('#minyear').val(parseInt($('#preferredyear').val()) - data.moveBefore);
        }
    });
}
function oneditingscenparam(button) {
    var paramname = $(button).parents('td').siblings('td')[0];
    var paravalue = $(button).parents('td').siblings('td')[1];
    var paravaluetext = $.trim($(paravalue).text());
    console.log($(button).val());
    $('#parameterid').val($(button).val());
    console.log($('#parameterid'));
    $('#parametername').val($.trim($(paramname).text()));
    if (paravaluetext === 'True' || paravaluetext === 'False') {
        $('#paramvalueparent').hide();
        $('#boolvalueparent').show();
        $("#boolvalue").prop('checked', paravaluetext === 'True');
        $('#parametervalue').val(paravaluetext === 'True' ? '1' : '0').removeAttr('required');
    }
    else {
        $('#paramvalueparent').show();
        $('#boolvalueparent').hide();
        $('#parametervalue').val(paravaluetext).attr('required', true);
    }
}
function oneditingbudgetconstraints(button) {
    var row = $(button).closest('tr');
    var bridgeInterstateBudget = row.find('#BridgeInterstateBudget').val();
    var bridgeNonInterstateBudget = row.find('#BridgeNonInterstateBudget').val();
    var pavementInterstateBudget = row.find('#PavementInterstateBudget').val();
    var pavementNonInterstateBudget = row.find('#PavementNonInterstateBudget').val();

    //console.log('bridgeInterstateBudget' + bridgeInterstateBudget);
    var year = $(button).parents('td').siblings('td')[0];
    var district = $(button).parents('td').siblings('td')[1];
    var bridgeinterstate = $(button).parents('td').siblings('td')[2];
    var bridgenoninterstate = $(button).parents('td').siblings('td')[3];
    var pavementinterstate = $(button).parents('td').siblings('td')[4];
    var pavementnoninterstate = $(button).parents('td').siblings('td')[5];

    $('#yearwork').val($.trim($(year).text()));
    $('#district').val($.trim($(district).text()));

    $('#bridgeinterstatebudget').val($.trim($(bridgeinterstate).text()));
    $('#bridgenoninterstatebudget').val($.trim($(bridgenoninterstate).text()));
    $('#pavementinterstatebudget').val($.trim($(pavementinterstate).text()));
    $('#pavementnoninterstatebudget').val($.trim($(pavementnoninterstate).text()));

    $('#bridgeinterstatebudgetHide').val(bridgeInterstateBudget);
    $('#bridgenoninterstatebudgetHide').val(bridgeNonInterstateBudget);
    $('#pavementinterstatebudgetHide').val(pavementInterstateBudget);
    $('#pavementnoninterstatebudgetHide').val(pavementNonInterstateBudget);
}
function resetform(form) {
    form.removeClass("was-validated");
    form.find(':input').filter('select').each(function () {
        if ($(this).is('[data-clear="yes"]')) {
            var firstOption = $(this).find("option:first");
            $(this).empty().append(firstOption);
        }
    });
}
function closemodal(id) {
    var model = document.getElementById(id);
    var modelobj = bootstrap.Modal.getOrCreateInstance(model);
    modelobj.hide();
}
$('#runbutton').click(async function () {
    var runbutton = document.getElementById("runbutton");
    var selectedscenario = $(runbutton).attr('current-id');
    CheckRunAvailability(selectedscenario);
});

function CheckRunAvailability(selectedscenario) {
    $.ajax
        ({
            type: 'POST',
            dataType: 'JSON',
            url: '/ProjectTreatments/CheckRunAvailAbilty',
            data: { scenarioId: selectedscenario },
            success: function (res) {
                if (res) {
                    $("#runAlert").modal("show");
                }
                else {
                    runscenario();
                }
            },
            error: function (res) {
                console.log(res)
            }

        });
}
$('#runbuttonAgain').click(async function () {
    $("#runAlert").modal("hide");
    await runscenario();
});

$('[data-bs-dismiss="modal"]').click(function () {
    var form = $(this).closest('form');

    resetform(form);
});
$('#createlibrary').submit(function (event) {
    event.preventDefault();
    event.stopPropagation();
    var form = document.getElementById('createlibrary');
    validatelibraryform(form);
});
$('#editlibrary').submit(function (event) {
    event.preventDefault();
    event.stopPropagation();
    var form = document.getElementById('editlibrary');
    if (!validateemptyfields(form))
        return;
    validatelibraryname(form);
});
$('#copylibrary').submit(function (event) {
    event.preventDefault();
    event.stopPropagation();
    var form = document.getElementById('copylibrary');
    if (!validateemptyfields(form))
        return;
    validatelibraryname(form);
});
$('#isemptylibrary,#librarywithtreatment').on('change', function () {
    if ($('#isemptylibrary').is(':checked')) {
        $('#libraryoptionparent :input').prop('disabled', true);
        $('#minyear').removeAttr('required');
        $('#maxyear').removeAttr('required');
    } else {
        $('#libraryoptionparent :input').removeAttr('disabled');
        $('#minyear').prop('required', true);
        $('#maxyear').prop('required', true);
    }
});
$('#assettype').on('change', function () {
    var value = $(this).val();
    if (value === 'B') {
        $('#bridgeidparent').show();
        $('#brkeyparent').show();
        $('#bridgeid').attr("required", true);
        $('#brkey').attr("required", true);
    } else {
        $('#bridgeidparent').hide();
        $('#brkeyparent').hide();
        $('#bridgeid').removeAttr('required');
        $('#brkey').removeAttr('required');
    }
});
$('#createtreatmentbtn').click(function () {
    $('#createtreatment').prop('action', '/Treatments/CreateTreatment');
    setformforcreation();
    $('#createtreatmentlabel').text('Create Treatment');
    $('#createtrtBtn').text('Create');
})
function setformforcreation() {
    $(`select[name="assettype"]`).prop('disabled', false).show();
    $('input[name="assettype"]').hide();
    $('input[name=district]').hide();
    $('input[name=county]').hide();
    $('input[name=route]').hide();
    $('input[name=section]').hide();
    $('#brkeyparent').hide();
    $('#bridgeidparent').hide();
    $('#district').show().prop('disabled', false);
    $('#county').show().prop('disabled', false);
    $('#route').show().prop('disabled', false);
    $('#section').show().prop('disabled', false);
    $(`[name="treatment"]`).prop('disabled', false);
    //$(`[name="benefit"]`).prop('disabled', false);
    $(`[name="risk"]`).prop('disabled', false);
    $(`[name="brkey"]`).prop('disabled', false);
    $(`[name="bridgeid"]`).prop('disabled', false);

    $(`[name="benefit"]`).val("");
    $(`[name="risk"]`).val("");
    $(`[name="minyear"]`).val("");
    $(`[name="cost"]`).val("");
    $(`[name="maxyear"]`).val("");
    $(`[name="indirectcostdesign"]`).val("");
    $(`[name="indirectcostrow"]`).val("");
    $(`[name="indirectcostutilities"]`).val(""); 
    $(`[name="indirectcostothers"]`).val("");
    $(`[name="treatmenttype"]`).val(""); 
    $(`[name="preferredyear"]`).val(""); 
    $(`[name="treatment"]`).val(""); 
    $(`[name="assettype"]`).val(""); 
    $(`[name="brkey"]`).val(""); 
    $(`[name="bridgeid"]`).val("");
    $(`[name="totalcost"]`).val("");
}
$('#createtreatment').submit(async function (event) {
    event.preventDefault();
    event.stopPropagation();
    var form = document.getElementById('createtreatment');
    if (form.checkValidity() && validateminmaxyears(form)) {
        var action = $('#createtrtBtn').text() === 'Create' ? 'Creating' : 'Updating';
        closemodal("createtreatmentdialog");
        displaynotification($('#createtreatmentlabel').text(), `${action} treatment please wait`, "P", false);
        var data = new FormData(this);
        try {
            const response = await fetch(this.action, { method: 'POST', body: data });
            if (response.ok) {
                var result = await response.text();
                displaynotification($('#createtreatmentlabel').text(), result, "S", true);
                setTimeout(() => { document.location.reload(true) }, 2000);
            } else {
                var message = await response.json();
                var list = createlistfromjson(message.value);
                displaynotification($('#createtreatmentlabel').text(), list, "E", true);
            }
        } catch (e) {
            console.log(e)
            displaynotification('Error', 'Unexpected Error occured while processing your request', "E", true);
        }
    }
    form.classList.add('was-validated');
});
$('#mapsform').submit(async function (event) {
    event.preventDefault();
    event.stopPropagation();
    var data = new FormData(this);
    console.log(this.scenario.value);
    if (this.scenario.value === null || this.scenario.value === undefined || this.scenario.value === '') {
        $('#scenarioslink').show();
        $("#maperror").css('visibility', 'visible');
        $('#generatebtn').prop('disabled', true);
        $("#errormessage").text('In order to generate a map, you have to select a scenario first.');
    } else {
        $("#maperror").css('visibility', 'collapsed');
        $('#scenarioslink').hide();
        try {
            $('#overlay').show();
            $('#generatebtn').prop('disabled', true);
            const response = await fetch(this.action, { method: 'POST', body: data });
            if (response.ok) {
                var result = await response.text();
                var parsedData = jQuery.parseJSON(result);
                $('#errormessage').hide();
                $('#mapframe').show();
                $('#mapframe').prop('src', parsedData.MapViewerUrl);
            } else {
                var message = await response.text();
                displaynotification("Error", message, "E", true);
                $("#maperror").css('visibility', 'visible');
                $('#errormessage').show();
                $("#errormessage").text('There is no data to show.');
                $('#mapframe').hide();
            }
        } catch (e) {
            displaynotification('Error', 'Unexpected Error occured while processing your request', "E", true);
            $("#maperror").css('visibility', 'visible');
            $('#errormessage').show();
            $("#errormessage").text('There is no data to show.');
            $('#mapframe').hide();
        } finally {
            $('#overlay').hide();
            $('#generatebtn').removeAttr('disabled');
        }
    }
});
function createlistfromjson(json) {
    if (Array.isArray(json)) {
        var list = $('<ol/>')
        list.addClass('list-group list-group-numbered')
        $.each(json, (index, value) => {
            list.append(`<li class="list-group-item d-flex align-items-start">${$.trim(value)}</li>`);
        });
        return list;
    }
    return json;
}
$('#iscommitted').on('change', function () {
    if ($(this).is(':checked')) {
        $('#pariorityorder').val('0').attr('disabled', true);
        $('#maxyear').val($('#preferredyear').val()).removeAttr('required').prop('disabled', true);
        $('#minyear').val($('#preferredyear').val()).removeAttr('required').prop('disabled', true);
    } else {
        $('#pariorityorder').val('4').removeAttr('disabled');
        $('#maxyear').prop('required', true).removeAttr('disabled');
        $('#minyear').prop('required', true).removeAttr('disabled');
        var assettype = $('#assettype').val();
        if ((assettype === 'B' || assettype === 'P') && isnumber($('#preferredyear').val())) {
            getdefaultslacks(assettype);
        }
    }
});
$('#preferredyear').on('change', function () {
    if ($('#iscommitted').is(':checked')) {
        $('#maxyear').val($(this).val());
        $('#minyear').val($(this).val());
    }
});
$('#scenarioslist').on("change", function () {
    var selectedscenario = (this).value;
    var selectedlibrary = document.getElementById("libraries").value;
    if (!isNaN(selectedscenario) && selectedscenario !== null && selectedscenario !== '') {
        document.location.href = `/RunScenario?id=${selectedlibrary}&scenid=${selectedscenario}`;
    }
});
function getselectedScenario() {
    var scenarioval = $('#scenario').val()
    return scenarioval === undefined ? -1 : scenarioval;
}
async function getdistricts(districtselector, scenarioId = -1) {
    var controller = document.location.pathname;
    try {
        var response = await $.ajax({ type: 'GET', dataType: 'JSON', url: '/Info/GetDistricts', data: { caller: controller, scenarioId: scenarioId } });
        $(districtselector).empty().append(new Option("Select District", ""));
        response.forEach((district) => {
            $(districtselector).append(new Option("District " + district, district));
        });
    } catch (error) {
        $(districtselector).empty().append(new Option("Select District", ""));
    }
}
async function getcounties(countyselector, district, scenarioId = -1) {
    try {
        debugger;
        if (!isnumber(district)) {
            $(countyselector).empty();
            $(countyselector).empty().append(new Option("Select County", ""));
            $("[data-filter=\"route\"]").empty();
            $("[data-filter=\"route\"]").empty().append(new Option("Select Route", ""));
            $("[data-filter=\"section\"]").empty();
            $("[data-filter=\"section\"]").empty().append(new Option("Select Treatment Type", ""));
            return;
        }
        var controller = document.location.pathname;
        var response = await $.ajax({ type: 'GET', dataType: 'JSON', url: '/Info/GetCounties', data: { caller: controller, scenarioId: scenarioId, district: district } });
        console.log(response);
        console.log(countyselector);
        $(countyselector).empty();
        $(countyselector).empty().append(new Option("Select County", ""));
        response.forEach((county) => {
            $(countyselector).append(new Option(county.countyFullName, county.countyId));
        });
    } catch (error) {
        $(countyselector).empty().append(new Option("Select County", ""));
    }
}
async function getroutes(selecteddistrict, selectedcounty, routeselector, scenarioId = -1) {
    debugger;
    var NewSelectedcounty = selectedcounty.split("-");
    selectedcounty = NewSelectedcounty[0];
    try {
        if (!isnumber(selecteddistrict) || !isnumber(selectedcounty)) {
            $("[data-filter=\"route\"]").empty();
            $("[data-filter=\"route\"]").empty().append(new Option("Select Route", ""));
            $("[data-filter=\"section\"]").empty();
            $("[data-filter=\"section\"]").empty().append(new Option("Select Treatment Type", ""));
            return;
        }
        var controller = document.location.pathname;
        if (selectedcounty == null) {
            var selectedcounty = document.getElementById('county').value;
            console.log(selectedcounty);
        }
        var response = await $.ajax({ type: 'GET', dataType: 'JSON', url: '/Info/GetRoutes', data: { caller: controller, scenarioId: scenarioId, district: selecteddistrict, county: selectedcounty } });
        $(routeselector).empty().append(new Option("Select Route", ""));
        response.forEach((route) => {
            const routeFormatted = route.toString().padStart(4, '0');
            $(routeselector).append(new Option(routeFormatted, route));
            //$(routeselector).append(new Option("Route " + route, route));
        })
    } catch (error) {
        $(routeselector).empty().append(new Option("Select Route", ""));
    }
}
async function getsections(selecteddistrict, selectedcounty, selectedroute, sectionselector, scenarioId = -1) {
    debugger;
    var NewSelectedcounty = selectedcounty.split("-");
    selectedcounty = NewSelectedcounty[0];
    try {
        if (!isnumber(selecteddistrict) && !isnumber(selectedcounty) && !isnumber(selectedroute)) {
            $("[data-filter=\"section\"]").empty();
            $("[data-filter=\"section\"]").empty().append(new Option("Select Treatment Type", ""));
            return;
        }
        var controller = document.location.pathname;
        if (controller == '/Outputs/Maps') {
            var response = await $.ajax({ type: 'GET', dataType: 'JSON', url: '/Info/GetTreatmentTypes', data: { caller: controller, scenarioId: scenarioId, district: selecteddistrict, county: selectedcounty, route: selectedroute } });
            $(sectionselector).empty().append(new Option("Select Treatment Type", ""));
            response.forEach((treatmentType) => {
                console.log(treatmentType);
                $(sectionselector).append(new Option(treatmentType, treatmentType));
            });
        }
        else {
            var response = await $.ajax({ type: 'GET', dataType: 'JSON', url: '/Info/GetSections', data: { caller: controller, scenarioId: scenarioId, district: selecteddistrict, county: selectedcounty, route: selectedroute } });
            $(sectionselector).empty().append(new Option("Select Section", ""));
            response.forEach((section) => {
                $(sectionselector).append(new Option(section, section));
            });
        }


    } catch (error) {
        $(sectionselector).empty().append(new Option("Select Section", ""));
    }
}
function checkfilesize() {
    var excelfile = $('#excelfile');
    var inputsize = excelfile[0].files[0].size;
    var fileSizeLimit = 128 * 1024 * 1024;

    var fileName = excelfile[0].files[0].name;
    var allowedExtensions = /(\.xls|\.xlsx)$/i;
    if (!allowedExtensions.test(fileName)) {
        excelfile.removeClass('is-valid');
        excelfile.addClass('is-invalid');
        excelfile.parent().parent().removeClass('was-validated');
        $("#excelvalidation").text('Please upload a valid Excel file with .xls or .xlsx extension.');
        return false;
    }

    if (inputsize > fileSizeLimit) {
        excelfile.removeClass('is-valid');
        excelfile.addClass('is-invalid');
        excelfile.parent().parent().removeClass('was-validated');
        $("#excelvalidation").text('the excel file must be under 128Mb in size');
        return false;
    }
    excelfile.removeClass('is-invalid');
    excelfile.addClass('is-valid');
    return true;
}
function importcandidatepoolfromscenario(importing, fromscenario, id, name) {
    if ((importing || fromscenario) && id === "") {
        console.log()
        displaynotification("Error", "there is no candidate pool attached to the selected scenario", "E", true);
    }
    else if (importing) {
        $("#candidatepool_name").val(name);
        $("#candidatepoolid").val(id);
        $("#importtreatment").modal("show");
        redirectToPage = true;
    }
}
$("#importtreatment").on("hidden.bs.modal", function () {
    if (redirectToPageWithImport == false) {
        if (redirectToPage) {
            document.location.href = '/Scenarios/Index';

        }
    }
});
$("#CreateNewLibraryForm").on("hidden.bs.modal", function () {
    console.log('hide');
});
$('#scenario').on('change', async function () {
    var scenarioId = $(this).val();
    if (isnumber(scenarioId)) {
        await getdistricts('[data-filter="district"]', scenarioId);
    }
});
$("#district").on('change', async function () {
    debugger;
    var selecteddistrict = document.getElementById("district").value;
    await getcounties('#county', selecteddistrict);
});
$("#county").on('change', async function () {
    var selectedcounty = document.getElementById("county").value;
    var selecteddistrict = document.getElementById("district").value;
    await getroutes(selecteddistrict, selectedcounty, '#route');
});
$("#route").on('change', async function () {
    var selectedroute = document.getElementById("route").value;
    var selectedcounty = document.getElementById("county").value;
    var selecteddistrict = document.getElementById("district").value;
    await getsections(selecteddistrict, selectedcounty, selectedroute, '#section');
});
$("#section").on('change', function () {
    var selectedsection = document.getElementById("section").value;
    var selectedroute = document.getElementById("route").value;
    var selectedcounty = document.getElementById("county").value;
    var selecteddistrict = document.getElementById("district").value;
    if (!isNaN(selecteddistrict) && selecteddistrict !== null && selecteddistrict !== '' && !isNaN(selectedcounty) && selectedcounty !== null && selectedcounty !== ''
        && !isNaN(selectedroute) && selectedroute !== null && selectedroute !== '' && selectedsection !== null && selectedsection !== '') {
        $.ajax({
            type: 'GET',
            dataType: 'JSON',
            url: '/Info/GetDirectionInterstate',
            data: { section: selectedsection, route: selectedroute, county: selectedcounty, district: selecteddistrict },
            success:
                function (response) {
                    $("#interstate").prop('checked', response.isInterstate);
                    $("#direction").prop('checked', response.direction);
                    if (response.isInterstate)
                        $("#interstateTxt").val("Yes");
                    else
                        $("#interstateTxt").val("No");
                    if (response.direction)
                        $("#directionTxt").val("Yes");
                    else
                        $("#directionTxt").val("No");
                },
            error:
                function (response) {
                    $("#interstate").prop('checked', false);
                    $("#direction").prop('checked', false);
                    $("#interstateTxt").val("No");
                    $("#directionTxt").val("No");
                }
        });
    } else {
        $("#interstate").prop('checked', false);
        $("#direction").prop('checked', false);
        $("#interstateTxt").val("No");
        $("#directionTxt").val("No");
    }
});
$('[data-filter="district"]').on('change', async function () {
    var selecteddistrict = $(this).val();
    var scenarioId = getselectedScenario();
    await getcounties('[data-filter="county"]', selecteddistrict, scenarioId);
});
$('[data-filter="county"]').on('change', async function () {
    var selectedcounty = $(this).val();
    var selecteddistrict = $('[data-filter="district"').val();
    var scenarioId = getselectedScenario();
    await getroutes(selecteddistrict, selectedcounty, '[data-filter="route"]', scenarioId);
});
$('[data-filter="route"]').on('change', async function () {
    var selectedroute = $(this).val();
    var selectedcounty = $('[data-filter="county"]').val();
    var selecteddistrict = $('[data-filter="district"]').val();
    var scenarioId = getselectedScenario();
    await getsections(selecteddistrict, selectedcounty, selectedroute, '[data-filter="section"]', scenarioId);
});
$('#scenarioparametersform').submit(function (event) {
    event.preventDefault();
    event.stopPropagation();
    var form = document.getElementById('scenarioparametersform');
    if (form.checkValidity()) {
        if ($('#parametervalue').is(':hidden')) {
            form.parametervalue.value = form.boolvalue.checked ? '1' : '0';
        }
        form.submit();
        //resetform($(form));
    }
    this.classList.add("was-validated");

})
function getQueryParam(param, defaultValue = undefined) {
    location.search.substr(1)
        .split("&")
        .some(function (item) {
            return item.split("=")[0].toLowerCase() == param && (defaultValue = item.split("=")[1], true)
        })
    return defaultValue
}
//$(async function getparams() {
//    if (document.location.pathname === '/Treatments' || document.location.pathname === '/Projects' || document.location.pathname === '/Maps' || document.location.pathname === '/ProjectTreatments') {
//        var district = getQueryParam('district', '');
//        var county = getQueryParam('county', '');
//        var route = getQueryParam('route', '');
//        var section = getQueryParam('section', '');
//        var year = getQueryParam('year', '');
//        var direction = getQueryParam('filterdirection', '');
//        var scenario = getQueryParam('scenarioid', -1);
//        if (scenario === -1) {
//           scenario = $('#scenario').val();
//        }
//        $(`[data-filter="year"] option[value="${year}"]`).prop('selected', true);
//        $(`[data-filter="direction"] option[value="${direction}"]`).prop('selected', 'selected');
//        await getdistricts('[data-filter="district"]', scenario);
//        $(`[data-filter="district"] option[value="${district}"]`).prop('selected', 'selected');
//        await getcounties('[data-filter="county"]', district,scenario);
//        $(`[data-filter="county"] option[value="${county}"]`).prop('selected', 'selected');
//        await getroutes(district, county, '[data-filter="route"]',scenario);
//        $(`[data-filter="route"] option[value="${route}"]`).prop('selected', 'selected');
//        await getsections(district, county, route, '[data-filter="section"]',scenario);
//        $(`[data-filter="section"] option[value="${section}"]`).prop('selected', 'selected');
//    }
//});
$('#projectsfilterform').submit(async function (event) {
    event.preventDefault();
    event.stopPropagation();
    try {
        var data = new FormData(document.getElementById('projectsfilterform'));
        var response = await fetch($(this).prop('action'), { method: 'POST', body: data });
        if (response.ok) {
            $('#projectsdataTable').DataTable().ajax.reload();
        }
    } catch (e) {
        displaynotification("Error", "an error occured while trying to applay the filter.", "E", true);
    }

});
$('#treatmentimportform').submit(async function (event) {
    debugger;
    event.preventDefault();
    event.stopPropagation();
    $('#excelvalidation').text('Field is required');
    this.classList.remove('was-validated');
    if (this.checkValidity() && checkfilesize()) {
        redirectToPageWithImport = true;
        closemodal('importtreatment')
        displaynotification("Importing Treatment", "Importing treatments from excel file please wait", "P", false);
        var data = new FormData(this);
        this.reset();
        try {
            const response = await fetch(this.action, { method: 'POST', body: data });
            if (response.ok) {

                displaynotification("Importing Treatment", "Treatments were imported successfuly", "S", true);
                if (redirectToPage) {
                    setTimeout(function () {
                        document.location.href = '/Scenarios/Index';
                    }, 3000);
                }
                else {
                    setTimeout(() => { document.location.href = '/CandidatePools'; }, 2000);
                }

            } else {
                var result = await response.json();
                var list = createlistfromjson(result.value);
                displaynotification("Importing Treatment", list, "E", false);
                if (redirectToPageWithImport) {
                    setTimeout(function () {
                        document.location.href = '/Scenarios/Index';
                    }, 3000);
                }
            }
        } catch (error) {
            displaynotification("Importing Treatment", "Unexpected error occured while trying to import treatments, please try again.", "E", true);
        }
    }
    this.classList.add('was-validated');
});
function loadselectedtreatment(treatment, id) {
    $.ajax({
        type: 'GET',
        dataType: 'JSON',
        url: '/Info/GetDirectionInterstate',
        data: { section: treatment.section, route: treatment.route, county: treatment.countyId, district: treatment.district },
        success:
            function (response) {
                $("#interstate").prop('checked', response.isInterstate);
                $("#direction").prop('checked', response.direction);
                if (response.isInterstate)
                    $("#interstateTxt").val("Yes");
                else
                    $("#interstateTxt").val("No");
                if (response.direction)
                    $("#directionTxt").val("Yes");
                else
                    $("#directionTxt").val("No");
            },
        error:
            function (response) {
                $("#interstate").prop('checked', false);
                $("#direction").prop('checked', false);
                $("#interstateTxt").val("No");
                $("#directionTxt").val("No");
            }
    });
    $(`[name="benefit"]`).prop('disabled', true);
    $(`[name="risk"]`).prop('disabled', true);
    $(`[name="treatmentid"]`).val(id);
    $(`[name="assettype"] option[value=${treatment.assetType}]`).prop('selected', true).parent().prop('disabled', true);
    $('input[name="assettype"]').show().val($(`[name="assettypeoptions"] option[value=${treatment.assetType}]`).text());
    $('[name="assettypeoptions"]').trigger('change');
    $('input[name=district]').show().val(treatment.district);
    $('input[name=county]').show().val(treatment.county);
    $('input[name=route]').show().val(treatment.route);
    $('input[name=section]').show().val(treatment.section);
    $('.modal-dialog #county').hide().prop('disabled', true);
    $('.modal-dialog #route').hide().prop('disabled', true);
    $('.modal-dialog #section').hide().prop('disabled', true);
    $(`.modal-dialog #district`).prop('disabled', true).hide();
    $(`[name="treatment"]`).val((treatment.treatment || treatment.treatmentType)).prop('disabled', true);
    $(`[name="preferredyear"] option[value=${treatment.preferredYear}]`).prop('selected', true);
    $(`[name="minyear"]`).val(treatment.minYear);
    $(`[name="maxyear"]`).val(treatment.maxYear);
    $(`[name="iscommitted"]`).prop('checked', treatment.isCommitted === true);
    $('[name="cost"]').val((treatment.cost || treatment.totalCost)?.toFixed(2));
    $(`[name="benefit"]`).val(treatment.benefit?.toFixed(2));
    $(`[name="risk"]`).val(treatment.risk?.toFixed(2));
    $(`[name="brkey"]`).val(treatment.brkey).prop('disabled', true);
    $(`[name="bridgeid"]`).val(treatment.bridgeId).prop('disabled', true);
    $(`[name="indirectcostdesign"]`).val(treatment.indirectCostDesign?.toFixed(2));
    $(`[name="indirectcostrow"]`).val(treatment.indirectCostRow?.toFixed(2));
    $(`[name="indirectcostutilities"]`).val(treatment.indirectCostUtilities?.toFixed(2));
    $(`[name="indirectcostothers"]`).val(treatment.indirectCostOther?.toFixed(2));
    $(`[name="pariorityorder"] option[value=${treatment.priorityOrder}]`).prop('selected', true);
    $(`[name="treatmenttype"] option[value=${treatment.userTreatmentTypeNo}]`).prop('selected', true);
    $(`[name="interstate"]`).prop('checked', treatment.interstate === 1);
    $(`[name="direction"]`).prop('checked', treatment.direction === 1);
}
async function editingtreatment() {
    try {
        $("#createtreatment").prop('action', '/Treatments/EditTreatment');
        $('#createtreatmentlabel').text('Update Treatment');
        $('#createtrtBtn').text('Update');
        var treatment = await $.ajax({ type: 'GET', dataType: 'JSON', url: '/Treatments/GetTreatment', data: { treatmentId: this.value } });
        loadselectedtreatment(treatment, this.value);
        //$('#preferredyear').val('2024');
        var model = document.getElementById("createtreatmentdialog");
        var formModel = bootstrap.Modal.getOrCreateInstance(model);

        //
        let total = 0;
        $(".costcalc").each(function () {
            let val = parseFloat($(this).val());
            if (!isNaN(val)) {
                total += val;
            }
        });
        $("#totalcost").val(total.toFixed(2));
        //


        formModel.show();
    } catch (error) {
        displaynotification("Error", "could not load the selected treatment", "E", true);
    }
}
$('#createscenario').submit(async function (event) {
    $('crtscenario').prop('disabled', true);
    event.preventDefault();
    event.stopPropagation();
    $(this).removeClass('was-validated');
    setfieldfeedback("Field is required", 'namefeedback', 'maxyearfeedback', 'minyearfeedback');
    if (this.checkValidity()) {
        var maxminyear = validateminmaxyears(this);
        if (!maxminyear)
            return;
        var hasnameuseed = await validatescenarioname($('#scenarioname'));
        $('crtscenario').prop('disabled', false);
        if (!hasnameuseed)
            return;
        var data = new FormData(this);
        closemodal('createscenariowrapper');
        resetform($(this));

        displaynotification('Create Scenario', `Creating scenario please wait`, "P", false);
        try {
            var response = await fetch(this.action, { method: 'POST', body: data });
            if (response.ok) {
                var result = await response.text();
                displaynotification('Create Scenario', result, "S", true);
                setTimeout(() => { document.location.reload(true) }, 2000);
            } else {
                var message = await response.json();
                var list = createlistfromjson(message.value);
                displaynotification('Create Scenario', list, "E", true);
            }
        } catch (e) {
            displaynotification('Error', 'Unexpected Error occured while processing your request', "E", true);
        }
    }
    $(this).addClass('was-validated');
});
async function validatescenarioname(scenariofield) {
    $('#createscenario').removeClass('was-validated');
    try {
        var response = await fetch(`Scenarios/CheckScenarioName?attemptedname=${scenariofield.val()}`);
        if (response.ok) {
            scenariofield.removeClass('is-invalid');
        } else {
            var text = await response.text()
            scenariofield.addClass('is-invalid');
            document.getElementById('namefeedback').innerText = text;
        }
        return response.ok;
    } catch (e) {
        return false;
    }
}
async function onedittreatmentproject(projectid) {
    try {
        var treatment = await $.ajax({ type: 'GET', dataType: 'JSON', url: '/ProjectTreatments/GetProjectTreatment', data: { projectTreatmentId: projectid } });
        loadselectedtreatment(treatment, projectid);
        $(`[name="projecttreatmentid"]`).val(projectid);
        var model = document.getElementById("editprojecttreatmentdialog");
        var formModel = bootstrap.Modal.getOrCreateInstance(model);
        formModel.show();
    } catch (error) {
        displaynotification("Error", "could not load the selected treatment", "E", true);
    }
}
$('#editprojecttreatment').submit(async function (event) {
    event.preventDefault();
    event.stopPropagation();
    var form = event.target;
    var validyears = validateminmaxyears(form);
    if (form.checkValidity() && validyears) {
        closemodal("editprojecttreatmentdialog");
        displaynotification("Update project treatment", `updating project treatment  please wait`, "P", false);
        var data = new FormData(form);
        try {
            const response = await fetch(form.action, { method: 'POST', body: data });
            if (response.ok) {
                var result = await response.text();
                displaynotification("Update project treatment", result, "S", true);
                setTimeout(() => { $('#projectsdataTable').DataTable().ajax.reload() }, 2000);
            } else {
                var message = await response.json();
                var list = createlistfromjson(message.value);
                displaynotification("Update project treatment", list, "E", true);
            }
        } catch (e) {
            displaynotification('Error', 'Unexpected Error occured while processing your request', "E", true);
        }
    }
    form.classList.add('was-validated');
});
$('#deleterecordform').submit(async function (event) {
    event.preventDefault();
    event.stopPropagation();
    var form = event.target;
    closemodal('deletealert');
    var data = new FormData(form);
    try {
        displaynotification("Delete Record", "Deleting the selected Record please wait", "P", false);
        const response = await fetch(form.action, { method: 'POST', body: data });
        if (response.ok) {
            var result = await response.text();
            displaynotification("Delete Record", result, "S", true);
            var datatableid = $(this).attr('data-table');
            if (datatableid === null || datatableid === undefined) {
                setTimeout(() => { document.location.reload(true) }, 2000);
            } else {
                setTimeout(() => { $(`#${datatableid}`).DataTable().ajax.reload() }, 2000);
            }
        } else {
            var message = await response.text();
            displaynotification("Error", message, "E", true);
        }
    } catch (e) {
        displaynotification('Error', 'Unexpected Error occured while Deleteing the selected record please try again', "E", true);
    }
})
function setfieldfeedback(message, ...feedbacks) {
    for (const feedback of feedbacks) {
        document.getElementById(feedback).innerText = message;
    }
}
function ondeletingprojecttreatment() {
    var form = document.getElementById('deleteform');
    form.treatmentid.value = this.value;
    form.projectid.value = getQueryParam('projectid', '');
}

function oneditinglibrary(value) {
    document.getElementById("editlibraryheader").innerText = "Edit Candidate Pool";
    var form = document.getElementById('editlibrary');
    form.action = '/CandidatePools/EditLibrary';
    form.submitbtn.innerText = 'Update';
    $.ajax(
        {
            type: 'POST',
            dataType: 'JSON',
            url: '/CandidatePools/GetLibrary',
            data: JSON.stringify(value),
            contentType: 'application/json',
            success:
                function (response) {
                    var form = document.getElementById('editlibrary');
                    form.libraryname.value = response.name;
                    form.librarydescription.value = response.description;
                    form.libraryId.value = response.candidatePoolId;
                    form.isshared.checked = response.isShared;
                    var model = document.getElementById('EditLibraryForm');
                    var formModel = bootstrap.Modal.getOrCreateInstance(model);
                    formModel.show();
                },
            error:
                function (response) {

                }
        });
}
function ondeletinglibrary(value) {
    var form = document.getElementById('deleteformtrue');
    form.libraryid.value = value;
    $("#deletealerttrue").modal("show");

}
function ondeletinglibrary(value) {
    var form = document.getElementById('deleteformfalse');
    form.libraryid.value = value;
    $("#deletealerfalse").modal("show");

}
function onsharinglibrary(value) {
    var form = document.getElementById('copylibrary');
    form.action = '/CandidatePools/CreateSharedLibrary';
    form.submitbtn.innerText = 'Create';
    form.libraryname.value = '';
    form.libraryId.value = value;
    var model = document.getElementById('CopyLibraryForm');
    var formModel = bootstrap.Modal.getOrCreateInstance(model);
    formModel.show();
}


function oncontextmenuclickedscenario(event, id) {
    var contextmenu = $('#contextmenu');
    var portheight = window.innerHeight;
    var contextheight = contextmenu.height();
    var ycor = event.clientY;
    var offset = (event.clientY + contextheight) - portheight;
    if (offset > 0) {
        ycor = ycor - offset;
    }
    contextmenu.css("display", "block")
        .css("left", `${event.clientX - 260}px`)
        .css("top", `${ycor}px`);
    var scenid = $(this).val();
    $('.menu a[href]').each(function () {
        var paramname = "scenid";
        if ($(this).attr("param")) {
            paramname = $(this).attr('param');
        }
        var url = new URL($(this).prop('href'));
        url.searchParams.set(paramname, scenid);
        $(this).prop("href", url.href);
    });
    $('.menu button').each(function () {
        $(this).attr('current-id', scenid);
    });
}


$(function () {
    $(document).on("click", function (e) {
        try {
            var contextMenuparent = $(e.target).is('[context-menu]');
            if (!contextMenuparent) {               
                var contextmenutreatment = document.getElementById('contextmenu');
                if (contextmenutreatment !== undefined || contextmenutreatment !== null) {
                    contextmenutreatment.style.display = 'none';
                }
            }

            var contextMenuparent = $(e.target).is('[context-menu-new]');
            if (!contextMenuparent) {
                var contextMenu = document.getElementById('contextmenunew');
                if (contextMenu !== undefined || contextMenu !== null) {
                    contextMenu.style.display = 'none';
                }
            }

        } catch (error) {
        }
    });
});
$('#filterform').on('submit', async function (event) {
    event.preventDefault();
    $('#filter').prop('disabled', true);
    displaynotification("Loading", "Loading charts please wait.", "P", false);
    try {
        var response = await fetch(event.target.action, { method: 'POST', body: new FormData(event.target) });
        if (response.ok) {
            var result = await response.json();
            var json = JSON.stringify(result);
            var chartdata = JSON.parse(json);
            if (chartdata === null || chartdata === undefined) {
                hidenotification();
                $('#chart').remove();
                $('.error_container p').text('there is no data show.');
                $('.error_container').show();
                return;
            }
            $('.error_container').hide();
            var chart = event.target.getAttribute("chart");
            if (chart === "needs") {
                initializeneedscharts(chartdata);
            } else {
                initializebudgetcharts(chartdata);
            }
            hidenotification();
        }
        else {
            var message = await response.text();
            displaynotification("Error", message, "E", true);
        }
    } catch (error) {
        displaynotification("Error", "Unexpected error occured while loading charts.", "E", true);
    }
    finally {
        $('#filter').prop('disabled', false);
    }
});
function initializeneedscharts(chartdata) {
    interstateValues = [];
    nonInterstateValues = [];
    for (var year in chartdata.seriesPoint) {
        if (chartdata.seriesPoint.hasOwnProperty(year)) {
            interstateValues.push(chartdata.seriesPoint[year].Interstate / 1000000);
            nonInterstateValues.push(chartdata.seriesPoint[year]['Non-Interstate'] / 1000000);
        }
    }
    $('#chart').remove();
    $('#chartcontainer').append('<canvas class="chartcanvas" id="chart"></canvas>');
    var ctx = document.getElementById('chart').getContext('2d');
    var myChart = new Chart(ctx, {
        type: 'bar',
        responsive: true,
        maintainAspectRatio: false,
        data: {
            labels: chartdata.labels,
            datasets: [
                {
                    label: 'Interstate',
                    backgroundColor: '#66cc94',
                    data: interstateValues,
                    stack: 'Stack 0',
                },
                {
                    label: 'Non-Interstate',
                    backgroundColor: '#6666cc',
                    data: nonInterstateValues,
                    stack: 'Stack 0',
                }
            ]
        },
        options: {
            scales: {
                x: {
                    display: true
                },
                y: {
                    display: true,
                    ticks: {
                        callback: function (value, index, values) {
                            return '$' + value + 'M';
                        }
                    }
                }
            }, plugins: {
                tooltip: {
                    enabled: false,
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        label: function (tooltipItem, data) {

                            var datasetLabel = tooltipItem.dataset.data[tooltipItem.dataIndex] || '';

                            return tooltipItem.dataset.label + ': $' + (tooltipItem.raw).toFixed(2) + 'M';
                        }
                    }
                }
            }
        }
    });
}
function initializebudgetcharts(chartdata) {
    interstateBridgeBudget = [];
    nonInterstateBridgeBudget = [];
    interstatePavementBudget = [];
    nonInterstatePavementBudget = [];
    for (var year in chartdata.seriesPoint) {
        if (chartdata.seriesPoint.hasOwnProperty(year)) {
            interstateBridgeBudget.push(chartdata.seriesPoint[year]['Bridge Interstate'] / 1000000);
            nonInterstateBridgeBudget.push(chartdata.seriesPoint[year]['Bridge Non-Interstate'] / 1000000);
            interstatePavementBudget.push(chartdata.seriesPoint[year]['Pavement Interstate'] / 1000000);
            nonInterstatePavementBudget.push(chartdata.seriesPoint[year]['Pavement Non-Interstate'] / 1000000);
        }
    }
    $('#chart').remove();
    $('#chartcontainer').append('<canvas class="chartcanvas" id="chart"></canvas>');
    var ctx = document.getElementById('chart').getContext('2d');
    var myChart = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: chartdata.labels,
            datasets: [
                {
                    label: 'Bridge Interstate',
                    backgroundColor: '#ff99ff',
                    data: interstateBridgeBudget,
                    stack: 'Stack 0',
                },
                {
                    label: 'Bridge Non-Interstate',
                    backgroundColor: '#66cccc',
                    data: nonInterstateBridgeBudget,
                    stack: 'Stack 0',
                },
                {
                    label: 'Pavement Interstate',
                    backgroundColor: '#0065ff',
                    data: interstatePavementBudget,
                    stack: 'Stack 0',
                },
                {
                    label: 'Pavement Non-Interstate',
                    backgroundColor: '#ffcc99',
                    data: nonInterstatePavementBudget,
                    stack: 'Stack 0',
                }
            ]
        },
        options: {
            scales: {
                x: {
                    display: true,
                },
                y: {
                    display: true,
                    ticks: {
                        callback: function (value, index, values) {
                            return '$' + value + 'M';
                        }
                    }
                }
            }, plugins: {
                tooltip: {
                    enabled: false,
                    mode: 'index',
                    intersect: false,
                    callbacks: {
                        label: function (tooltipItem, data) {

                            console.log(tooltipItem);
                            var datasetLabel = tooltipItem.dataset.data[tooltipItem.dataIndex] || '';

                            console.log(datasetLabel);
                            return tooltipItem.dataset.label + ': $' + (tooltipItem.raw).toFixed(2) + 'M';
                        }
                    }
                }
            }

        }
    });
}
