function setupDate(dtclass) {
    $(dtclass).bootstrapDP({
        orientation: "bottom",
        format: "dd/mm/yyyy",
        autoclose: true,
        todayHighlight: true,
        onClose: function () {
            this._trigger("changeDate");
        },
    });
}
function setupSearch(gridapi, srchselector, reserveurl, usertag, rowcountselector) {
    $(srchselector).click(function (e) {
        e.preventDefault();
        e.stopPropagation();
        gridapi.setDatasource(gridHelper.Core.createDataSource([], reserveurl, usertag, rowcountselector));
    });
}

function setupCommandClicks(newbtn) {
    $(newbtn).click(function (e) {
        e.preventDefault();
        e.stopPropagation();
        var editreserveurl = $(e.target).attr("urlis");
        window.location.assign(editreserveurl);
    });
}
function setupEditClick(edtbtnselector, editidattr) {
    if (!edtbtnselector) {
        edtbtnselector = ".edt-btn";
    }
    if (!editidattr) {
        editidattr = "edit-id";
    }

    $(edtbtnselector).off("click");

    var idvals = $(edtbtnselector).attr(editidattr);
    if (idvals && idvals.indexOf(":") > 0) {
        var idinfo = idvals.split(":");
        var today = new Date();
        today.setHours(0, 0, 0, 0);
        var ispast = moment(idinfo[1], "DD/MM/YYYY") < today;
        if (!ispast) {
            $(edtbtnselector).show();
            $(edtbtnselector).on("click", function (e) {
                var url = $(e.target).attr("urlis");
                window.location.replace(url + "/" + idinfo[0]);
            });
        }
        else {
            $(edtbtnselector).hide();
        }
    }
}

function enterDeletionAndEditEvent(cmdbtnselector, selectedidsattr, rmvbtnselector, enterrmvselector, edtbtnselector, editidattr) {
    if (!cmdbtnselector) {
        cmdbtnselector = ".command-buttons";
    }
    if (!rmvbtnselector) {
        rmvbtnselector = ".rmv-btn";
    }
    if (!edtbtnselector) {
        edtbtnselector = ".edt-btn";
    }
    if (!enterrmvselector) {
        enterrmvselector = ".enter-rmv-btn";
    }

    if (!selectedidsattr) {
        selectedidsattr = "selectedids";
    }

    if (!editidattr) {
        editidattr = "edit-id";
    }

    return function (selectedeventparams) {
        console.log("selection changed!");
        var reservations = selectedeventparams.api.getSelectedRows();
        var selectedIds = [];

        if (reservations && reservations.length) {
            var editinfo = reservations[0];
            reservations.forEach(function (v, i) {
                selectedIds.push(v["Id"]);
            });

            $(edtbtnselector).attr(editidattr, editinfo["Id"] + ":" + editinfo["ReservedDate"]);
            setupEditClick(edtbtnselector, editidattr);

            $(cmdbtnselector).attr(selectedidsattr, selectedIds.join("|"));
            $(rmvbtnselector).prop("disabled", false);
        } else {
            $(edtbtnselector).attr(editidattr, "");
            $(rmvbtnselector).prop("disabled", true);
        }
    };
}
function setupDeletionClicks(colHandle, rmvbtnselector, enterrmvselector, cmdbtnselector, selectedidsattr, searchbtnselector, urlattr) {
    if (!rmvbtnselector) {
        rmvbtnselector = ".rmv-btn";
    }
    if (!enterrmvselector) {
        enterrmvselector = ".enter-rmv-btn";
    }
    if (!cmdbtnselector) {
        cmdbtnselector = ".command-buttons";
    }
    if (!selectedidsattr) {
        selectedidsattr = "selectedids";
    }
    if (!searchbtnselector) {
        searchbtnselector = ".search-btn";
    }

    if (!urlattr) {
        urlattr = "urlis";
    }

    var listener = new window.keypress.Listener();
    listener.register_many([
        {
            keys: "esc",
            on_release: function (e) {
                $(enterrmvselector).removeClass("hidden");
                $(rmvbtnselector).addClass("hidden");
                colHandle.setColumnsVisible(["root"], false);

                $(searchbtnselector).trigger("click");

                return true;
            },
        },
    ]);

    $(enterrmvselector).off("click");
    $(enterrmvselector).on("click", function (e) {
        $(rmvbtnselector).removeClass("hidden");
        $(enterrmvselector).addClass("hidden");
        colHandle.setColumnsVisible(["root"], true);
    });
    $(rmvbtnselector).on("click", function (e) {
        var urlis = $(e.target).attr(urlattr);

        var selectedids = $(cmdbtnselector).attr(selectedidsattr);
        var del_idlist = selectedids && selectedids.split('|');
        alertLib.Core.askYesNo("Delete Reservation?", "Are you sure to delete selected reservations?", "No").done(function (r) {
            if (r.yes) {
                colHandle.setColumnsVisible(["root"], false);
                $(rmvbtnselector).addClass("hidden");
                $(enterrmvselector).removeClass("hidden");
                $.ajax({
                    url: urlis,
                    method: "post",
                    data: {
                        ids: del_idlist,
                        __RequestVerificationToken: $(rmvbtnselector).attr('forgery'),
                    },
                    success: function (ar) {
                        //debug remove reservations
                        console.log("result return for deletion", ar);
                        if (ar.message) {
                            alertLib.Core.alert("Delete Reservation Error!", ar.message, "OK");
                        } else {
                            alertLib.Core.alert("Selected Reservation(s) Removal!", "The deletion completed!", "OK");
                            $(searchbtnselector).trigger("click");
                        }
                    },
                });
            }
        });
    });
}
function displayReservationEvent(dispbtnselector, urlattr, selectedidattr) {
    if (!dispbtnselector) {
        dispbtnselector = ".disp-btn";
    }
    if (!urlattr) {
        urlattr = "urlis";
    }
    if (!selectedidattr) {
        selectedidattr = "selectedid";
    }

    return function (doubleclickparams) {
        var reservations = doubleclickparams.api.getSelectedRows();
        if (reservations && reservations.length) {
            var selectedid = reservations[0]["Id"];
            $(dispbtnselector).attr(selectedidattr, selectedid);
            var displayurl = $(dispbtnselector).attr(urlattr).trimEnd() + "/" + selectedid;
            window.location.assign(displayurl);
        }
    };
}

$(document).ready(function () {
    uicontrolLib.Core.setupUnderline();

    var gridholder = $("#basic")[0];
    var reserveurl = $("#basic").attr("reserveurlis");
    var usertag = $("#basic").attr("usertag");

    themeLib.Core.setupSelect("", "theme-select");

    var reservegrid = gridHelper.Core.createGrid(
        gridholder,
        [],
        reserveurl,
        usertag,
        "#rowcount",
        ["ReservedStartAt", "ReservedEndAt"],
        "skip-col",
        "hide-filter",
        "urlis",
        "autocp-",
        sortLib.Core.getSortandFilter,
        enterDeletionAndEditEvent(".command-buttons", "selectedids", ".rmv-btn", ".enter-rmv-btn", ".edt-btn", "edit-id"),
        displayReservationEvent(".disp-btn", "urlis", "selectedid"),
        "wantedprop",
        ".clearcontainer.origin",
        "div.columns span",
        "sorter",
        "options",
        "id",
        150
    );
    //debug book grid
    console.log("check book grid?", reservegrid.gridOptions.api, reservegrid.gridOptions.columnApi);

    setupCommandClicks(".edit-btn");
    setupDate(".datepicker input");
    setupSearch(reservegrid.gridOptions.api, ".search-btn", reserveurl, usertag, "#rowcount");
    $('button.rmv-btn').attr('forgery', $('#rmv-btn-forgery').val());
    $('#rmv-btn-forgery').remove();

    //setupDeletionClicks(colHandle, rmvbtnselector, enterrmvselector, cmdbtnselector, selectedidsattr, searchbtnselector, urlattr)
    setupDeletionClicks(reservegrid.gridOptions.columnApi, ".rmv-btn", ".enter-rmv-btn", ".command-buttons", "selectedids", ".search-btn", "urlis");
});
