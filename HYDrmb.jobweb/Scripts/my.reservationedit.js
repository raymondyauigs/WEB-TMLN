var __bouncerGlobal = null;
function setupDate(dtclass, endDateSelector,urlattr) {
    var maxperiod = $(endDateSelector).val();
    var endDate = moment().add(maxperiod, "days").toDate();
    var holurl = $(dtclass).attr(urlattr);
    $(dtclass).bootstrapDP({
        orientation: "bottom",
        format: "dd/mm/yyyy",
        startDate: moment().toDate(),
        endDate: endDate,
        autoclose: true,
        todayHighlight: true,
        onClose: function () {
            //you can log $(this)
        },
    });

    if (holurl) {
        var year = moment(new Date()).format("YYYY");
        $.get(holurl + '/' + year + '/0', function (r) {
            if (r && r.length) {
                var hols = [];
                console.log(r);
                r.forEach(function (v, i) {
                    hols.push(moment(v, 'YYYY-MM-DD').format('DD/MM/YYYY'));
                });
                $(dtclass).bootstrapDP('setDatesHolidays', hols);
            }
        }
        );

    }
}

function setupSessionRange(rangeselector, intervalsattr, startselector, endselector, fromselector, tillselector,dateselector) {
    var intervals = $(rangeselector).parent().attr(intervalsattr).split(",");
    var startsession = moment($(startselector).val(), "DD/MM/YYYY HH:mm:ss").format("HH:mm");
    var endsession = moment($(endselector).val(), "DD/MM/YYYY HH:mm:ss").format("HH:mm");
    var sessionDate = moment($(startselector).val(), "DD/MM/YYYY HH:mm:ss").format("DD/MM/YYYY");
    //try debug intervals
    console.log(intervals);

    //the following is redundant because I have treated the start and end variables as time only
    $(dateselector).change(function (e) {
        var fromval = $(fromselector).val();
        var tillval = $(tillselector).val();
        $(startselector).val(moment($(e.target).val(), "DD/MM/YYYY").format("DD/MM/YYYY") + " " + fromval.slice(0, 2) + ":" + fromval.slice(3));
        $(endselector).val(moment($(e.target).val(), "DD/MM/YYYY").format("DD/MM/YYYY") + " " + tillval.slice(0, 2) + ":" + tillval.slice(3));
    })
    

    var rangeSession = new rSlider({
        target: rangeselector,
        values: intervals,
        range: true,
        set: [startsession, endsession],
        labels: false,

        onChange: function (vals) {
            sessionDate = moment($(dateselector).val(), "DD/MM/YYYY HH:mm:ss").format("DD/MM/YYYY");
            var startend = vals.split(",");
            $(startselector).val(sessionDate + " " + startend[0]);
            $(fromselector).val(startend[0].replace(":", ""));
            $(endselector).val(sessionDate + " " + startend[1]);
            $(tillselector).val(startend[1].replace(":", ""));
            console.log($(startselector).val(), $(endselector).val());

        },
    });
}


function setupSubmit(formselector) {
    $(formselector).submit(function (e) {
        e.preventDefault();
        //try to allow any api complete their refresh first.
        setTimeout(function () {
            $(formselector).off('submit');
            $(formselector).submit();

        }, 500);

    });
}
function setupRoomPic(roomselector, picselector) {
    $(roomselector).change(function (e) {
        var selected = $(this).find("option:selected");
        var selectedval = selected && selected.val();
        if (selectedval) {
            $(picselector).attr('placeshow', selectedval);
        }

    });
    $(roomselector).trigger('change');
};

$(document).ready(function () {
    themeLib.Core.setupTheme("#reservation-record-edit");

    setupDate(".datepicker", "input.rbperiod-val",'urlis');

    setupSessionRange(
        "#sessionrange",
        "intervals",
        'input[name*="SessionStart"]',
        'input[name*="SessionEnd"]',
        "input.from-val",
        "input.till-val",
        'input[name*="SessionDate"]',
        
    );
    uicontrolLib.Core.setupSectionShow("SessionType", null, null, "_");

    uicontrolLib.Core.setupAutoComplete(".autocp", "urlis");

    setupRoomPic("select[name*='RoomType']", ".roompicshow");

    setupSubmit('form');
});
